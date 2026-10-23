using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class WallpaperController : IDisposable
    {
        private readonly Slideshow _slideshow;
        private readonly Timer _maintenance;
        private readonly Timer _coverTimer;
        private VlcPlayer _vlc;
        private WallpaperWindow _window;
        private AppSettings _settings;
        private bool _userPaused;
        private bool _autoPaused;
        private bool _sessionLocked;
        private bool _videoActive;
        private bool _disposed;
        private string _lastVideoError;
        private DateTime _lastAttachAttempt = DateTime.MinValue;
        private bool _desktopAttachFailed;
        private Bitmap _videoCover;
        private string _coverVideoPath;
        private int _coverAttempts;

        public event Action PreviewChanged;
        public event Action StateChanged;
        public event Action<string> Error;

        public WallpaperController(AppSettings settings)
        {
            _settings = settings.Clone();
            CreateWindow();
            _slideshow = new Slideshow();
            _slideshow.ImageChanged += OnSlideshowImage;
            _slideshow.StateChanged += RaiseState;
            _slideshow.Empty += delegate
            {
                if (_settings.Mode == "slideshow") RaiseState();
            };

            _maintenance = new Timer { Interval = _desktopAttachFailed ? 1000 : 4000 };
            _maintenance.Tick += delegate { Maintain(); };
            _maintenance.Start();

            _coverTimer = new Timer { Interval = 900 };
            _coverTimer.Tick += delegate { CaptureVideoCover(); };

            // At logon we may be waiting for WorkerW. Load VLC now so the
            // first frame is ready as soon as the desktop appears.
            if (_settings.Mode == "video" && File.Exists(_settings.VideoPath))
            {
                _vlc = new VlcPlayer();
                _vlc.Preload();
            }

            Apply(_settings, true);
        }

        public bool IsVideoActive { get { return _videoActive; } }
        public bool DesktopAttachFailed { get { return _desktopAttachFailed; } }
        public bool IsPaused { get { return _userPaused || _autoPaused || _sessionLocked; } }
        public bool IsUserPaused { get { return _userPaused; } }
        public string Mode { get { return _settings.Mode; } }
        public string CurrentVideoPath { get { return _settings.VideoPath; } }
        public string CurrentImagePath { get { return _slideshow.CurrentPath; } }
        public int ImageCount { get { return _slideshow.ImageCount; } }
        public Bitmap CurrentImage { get { return _slideshow.CurrentBitmap; } }
        public Bitmap VideoCover { get { return _videoCover; } }
        public long VideoLength { get { return _vlc != null ? _vlc.Length : 0; } }
        public long VideoPosition { get { return _vlc != null ? _vlc.Position : 0; } }

        private void CreateWindow()
        {
            _window = new WallpaperWindow();
            _window.CreateControl();
            _desktopAttachFailed = !_window.AttachToDesktop();
            _lastAttachAttempt = DateTime.UtcNow;
        }

        public void Apply(AppSettings settings, bool force)
        {
            if (_disposed) return;
            string previousMode = _settings.Mode;
            string previousFolder = _settings.ImageFolder;
            string previousVideo = _settings.VideoPath;
            bool previousLoop = _settings.Loop;
            _settings = settings.Clone();

            bool modeChanged = !string.Equals(previousMode, _settings.Mode, StringComparison.OrdinalIgnoreCase);
            bool folderChanged = !string.Equals(previousFolder, _settings.ImageFolder, StringComparison.OrdinalIgnoreCase);
            bool videoChanged = !string.Equals(previousVideo, _settings.VideoPath, StringComparison.OrdinalIgnoreCase);
            bool loopChanged = previousLoop != _settings.Loop;
            if (modeChanged) _userPaused = false;

            // A logon task can start us before Explorer has created WorkerW.
            // Do not create a visible video window during that interval; wait
            // for Maintain() to attach and re-apply.
            if (!_window.IsAttached)
            {
                _videoActive = false;
                _window.HideVideo();
                return;
            }

            if (_settings.Mode == "video")
            {
                if (!File.Exists(_settings.VideoPath))
                {
                    StopVideo();
                    ClearVideoCover();
                    ShowFallback();
                    RaiseState();
                    return;
                }

                _slideshow.SetPaused(true);
                if (modeChanged || force)
                {
                    _slideshow.StopAndClear();
                }
                if (force || modeChanged || videoChanged || loopChanged || !_videoActive || !_window.IsAttached)
                {
                    StartVideo();
                }
                else if (_vlc != null)
                {
                    _vlc.SetRate(_settings.PlaybackRate);
                    _vlc.SetVolume(_settings.Volume);
                    _vlc.SetLoop(_settings.Loop);
                    ApplyPauseState();
                }
                return;
            }

            if (_settings.Mode == "slideshow")
            {
                StopVideo();
                _window.HideVideo();
                if (force || modeChanged || folderChanged)
                {
                    _slideshow.Configure(_settings.ImageFolder, _settings.IntervalSeconds, _settings.Shuffle);
                }
                else
                {
                    _slideshow.SetInterval(_settings.IntervalSeconds);
                    _slideshow.SetShuffle(_settings.Shuffle);
                }
                _slideshow.SetPaused(IsPaused);
                RaiseState();
                return;
            }

            StopVideo();
            _slideshow.SetPaused(true);
            _slideshow.StopAndClear();
            ShowFallback();
            RaiseState();
        }

        private void StartVideo()
        {
            _videoActive = false;
            _lastVideoError = null;
            _coverTimer.Stop();
            _coverAttempts = 0;
            if (!string.Equals(_coverVideoPath, _settings.VideoPath, StringComparison.OrdinalIgnoreCase))
                ClearVideoCover();
            _window.ShowVideo();
            if (_vlc == null) _vlc = new VlcPlayer();
            _vlc.Started -= OnVideoStarted;
            _vlc.Started += OnVideoStarted;
            _vlc.PlaybackError -= OnVideoError;
            _vlc.PlaybackError += OnVideoError;
            _vlc.Ended -= OnVideoEnded;
            _vlc.Ended += OnVideoEnded;

            string error;
            bool ok = _vlc.Play(_settings.VideoPath, _window.VideoHandle,
                _settings.PlaybackRate, _settings.Volume, _settings.Loop, out error);
            if (!ok)
            {
                _lastVideoError = error;
                _videoActive = false;
                _window.HideVideo();
                _window.ShowFallback();
                RaiseError(error);
                RaiseState();
                return;
            }
            _videoActive = true;
            ApplyPauseState();
            RaiseState();
        }

        private void OnVideoStarted()
        {
            _videoActive = true;
            _window.ShowVideo();
            ApplyPauseState();
            _coverTimer.Stop();
            _coverAttempts = 0;
            _coverTimer.Start();
            RaiseState();
        }

        private void CaptureVideoCover()
        {
            _coverTimer.Stop();
            if (_disposed || _vlc == null || !_videoActive || string.IsNullOrEmpty(_settings.VideoPath))
                return;

            string temp = Path.Combine(Path.GetTempPath(), "DeskFlow-cover-" + Guid.NewGuid().ToString("N") + ".jpg");
            try
            {
                if (!_vlc.TakeSnapshot(temp) || !File.Exists(temp))
                {
                    if (_coverAttempts < 2)
                    {
                        _coverAttempts++;
                        _coverTimer.Start();
                    }
                    return;
                }
                _coverAttempts = 0;

                Bitmap cover = null;
                using (FileStream stream = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Image image = Image.FromStream(stream, true, true))
                {
                    cover = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);
                    using (Graphics g = Graphics.FromImage(cover))
                    {
                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        g.DrawImage(image, 0, 0, image.Width, image.Height);
                    }
                }

                Bitmap previous = _videoCover;
                _videoCover = cover;
                _coverVideoPath = _settings.VideoPath;
                Action handler = PreviewChanged;
                if (handler != null) handler();
                if (previous != null && !ReferenceEquals(previous, cover))
                {
                    try { previous.Dispose(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("读取视频封面失败: " + ex.Message);
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        private void ClearVideoCover()
        {
            _coverVideoPath = null;
            Bitmap previous = _videoCover;
            _videoCover = null;
            if (previous != null)
            {
                try { previous.Dispose(); } catch { }
            }
        }

        private void OnVideoError(string message)
        {
            _videoActive = false;
            _window.HideVideo();
            _window.ShowFallback();
            RaiseError(message);
            RaiseState();
        }

        private void OnVideoEnded()
        {
            RaiseState();
        }

        private void OnSlideshowImage(Bitmap bitmap)
        {
            _window.SetImage(bitmap);
            Action handler = PreviewChanged;
            if (handler != null) handler();
        }

        private void StopVideo()
        {
            _videoActive = false;
            if (_vlc != null)
            {
                try { _vlc.Stop(); } catch { }
                try { _vlc.Dispose(); } catch { }
                _vlc = null;
            }
            if (_window != null) _window.HideVideo();
        }

        private void ShowFallback()
        {
            if (_window != null) _window.ShowFallback();
            Action handler = PreviewChanged;
            if (handler != null) handler();
        }

        public void SetUserPaused(bool paused)
        {
            _userPaused = paused;
            ApplyPauseState();
            RaiseState();
        }

        public void TogglePause()
        {
            SetUserPaused(!_userPaused);
        }

        public void SetAutoPaused(bool paused)
        {
            if (_autoPaused == paused) return;
            _autoPaused = paused;
            ApplyPauseState();
            RaiseState();
        }

        public void SetSessionLocked(bool locked)
        {
            if (_sessionLocked == locked) return;
            _sessionLocked = locked;
            ApplyPauseState();
            RaiseState();
        }

        private void ApplyPauseState()
        {
            bool paused = IsPaused;
            if (_settings.Mode == "video" && _vlc != null)
            {
                _vlc.Pause(paused);
                if (!paused && _videoActive && _videoCover == null)
                {
                    _coverTimer.Stop();
                    _coverAttempts = 0;
                    _coverTimer.Start();
                }
            }
            if (_settings.Mode == "slideshow") _slideshow.SetPaused(paused);
        }

        public void NextImage()
        {
            if (_settings.Mode == "slideshow")
            {
                _userPaused = false;
                ApplyPauseState();
                _slideshow.Next();
                RaiseState();
            }
        }

        public void RefreshSlideshow()
        {
            if (_settings.Mode == "slideshow") _slideshow.Refresh();
        }

        public void ApplyVideoTuning()
        {
            if (_vlc == null) return;
            _vlc.SetRate(_settings.PlaybackRate);
            _vlc.SetVolume(_settings.Volume);
            _vlc.SetLoop(_settings.Loop);
            ApplyPauseState();
        }

        public void Reattach()
        {
            if (_disposed) return;
            try
            {
                if (!_window.IsHandleCreated || !NativeMethods.IsWindow(_window.Handle))
                {
                    Log.Info("壁纸窗口已丢失，正在重建。");
                    try { _window.Dispose(); } catch { }
                    CreateWindow();
                    Apply(_settings, true);
                }
                else
                {
                    _lastAttachAttempt = DateTime.UtcNow;
                    _desktopAttachFailed = !_window.AttachToDesktop();
                }
            }
            catch (Exception ex)
            {
                Log.Error("重新挂载壁纸窗口失败", ex);
            }
        }

        private void Maintain()
        {
            if (_disposed) return;
            try
            {
                if (!_window.IsHandleCreated || !NativeMethods.IsWindow(_window.Handle))
                {
                    Reattach();
                    return;
                }
                if (!_window.IsAttached)
                {
                    if ((DateTime.UtcNow - _lastAttachAttempt).TotalSeconds >= 1)
                    {
                        _lastAttachAttempt = DateTime.UtcNow;
                        _desktopAttachFailed = !_window.AttachToDesktop();
                        if (!_desktopAttachFailed)
                        {
                            _maintenance.Interval = 4000;
                            Apply(_settings, true);
                        }
                    }
                }
                else if (_desktopAttachFailed)
                {
                    _desktopAttachFailed = false;
                    _maintenance.Interval = 4000;
                    Apply(_settings, true);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("壁纸维护失败: " + ex.Message);
            }
        }

        private void RaiseState()
        {
            Action handler = StateChanged;
            if (handler != null) handler();
        }

        private void RaiseError(string message)
        {
            if (string.IsNullOrEmpty(message) || string.Equals(message, _lastVideoError, StringComparison.Ordinal))
            {
                Log.Warn("视频错误: " + (message ?? string.Empty));
            }
            else
            {
                _lastVideoError = message;
                Log.Warn("视频错误: " + message);
            }
            Action<string> handler = Error;
            if (handler != null) handler(message);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _maintenance.Stop();
            _maintenance.Dispose();
            _coverTimer.Stop();
            ClearVideoCover();
            _coverTimer.Dispose();
            try { _slideshow.Dispose(); } catch { }
            try { StopVideo(); } catch { }
            try
            {
                if (_window != null)
                {
                    _window.DetachFromDesktop();
                    _window.Dispose();
                }
            }
            catch { }
        }
    }
}
