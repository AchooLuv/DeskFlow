using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace DeskFlow
{
    internal sealed class Slideshow : IDisposable
    {
        private readonly Timer _timer;
        private readonly SynchronizationContext _sync;
        private readonly Random _random = new Random();
        private readonly List<string> _paths = new List<string>();
        private readonly List<int> _order = new List<int>();
        private string _folder = string.Empty;
        private int _cursor;
        private int _intervalSeconds = 30;
        private bool _shuffle;
        private bool _paused;
        private bool _disposed;
        private int _generation;

        public Bitmap CurrentBitmap { get; private set; }
        public string CurrentPath { get; private set; }
        public int ImageCount { get { return _paths.Count; } }

        public event Action<Bitmap> ImageChanged;
        public event Action StateChanged;
        public event Action Empty;

        public Slideshow()
        {
            _sync = SynchronizationContext.Current;
            _timer = new Timer { Interval = 30000 };
            _timer.Tick += delegate { Next(); };
        }

        public void Configure(string folder, int intervalSeconds, bool shuffle)
        {
            bool folderChanged = !string.Equals(_folder, folder ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            _folder = folder ?? string.Empty;
            _intervalSeconds = Math.Max(3, Math.Min(3600, intervalSeconds));
            _shuffle = shuffle;
            if (folderChanged || _paths.Count == 0)
            {
                Scan();
            }
            else if (CurrentBitmap == null)
            {
                // The slideshow was stopped while video mode was active. Reuse
                // the scanned list instead of requiring the folder to be picked
                // again.
                _paused = false;
                LoadCurrent();
                if (!_paused) StartTimer();
            }
            else if (!_paused)
            {
                StartTimer();
            }
            RaiseState();
        }

        public void Refresh()
        {
            Scan();
        }

        public void StopAndClear()
        {
            _generation++;
            _timer.Stop();
            _paused = true;
            ClearCurrent();
            RaiseState();
        }

        public void StopAndKeep()
        {
            _generation++;
            _timer.Stop();
            _paused = true;
            RaiseState();
        }

        private void Scan()
        {
            _generation++;
            _timer.Stop();
            _paths.Clear();
            _order.Clear();
            _cursor = 0;
            _paths.AddRange(ImageLoader.ScanFolder(_folder));
            BuildOrder();
            if (_paths.Count == 0)
            {
                ClearCurrent();
                RaiseEmpty();
                RaiseState();
                return;
            }
            LoadCurrent();
        }

        private void BuildOrder()
        {
            _order.Clear();
            for (int i = 0; i < _paths.Count; i++) _order.Add(i);
            if (_shuffle)
            {
                for (int i = _order.Count - 1; i > 0; i--)
                {
                    int j = _random.Next(i + 1);
                    int temp = _order[i];
                    _order[i] = _order[j];
                    _order[j] = temp;
                }
            }
            _cursor = 0;
        }

        public void Next()
        {
            if (_disposed || _paths.Count == 0) return;
            _timer.Stop();
            if (_order.Count == 0) BuildOrder();
            if (_cursor >= _order.Count)
            {
                if (_shuffle)
                {
                    string previous = CurrentPath;
                    BuildOrder();
                    if (_order.Count > 1 && previous != null)
                    {
                        int first = _order[0];
                        string firstPath = _paths[first];
                        if (string.Equals(firstPath, previous, StringComparison.OrdinalIgnoreCase))
                        {
                            int swap = _order[1];
                            _order[0] = swap;
                            _order[1] = first;
                        }
                    }
                }
                else
                {
                    _cursor = 0;
                }
            }
            int pathIndex = _order[Math.Min(_cursor, _order.Count - 1)];
            _cursor++;
            LoadCurrentByIndex(pathIndex);
        }

        private void LoadCurrent()
        {
            if (_paths.Count == 0) return;
            LoadCurrentByIndex(_order.Count > 0 ? _order[0] : 0);
            _cursor = 1;
        }

        private void LoadCurrentByIndex(int index)
        {
            if (index < 0 || index >= _paths.Count) return;
            string path = _paths[index];
            CurrentPath = path;
            RaiseState();
            int generation = ++_generation;
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            Task.Run(delegate
            {
                string error;
                Bitmap bitmap = ImageLoader.LoadForSize(path, width, height, out error);
                Post(delegate
                {
                    if (_disposed || generation != _generation)
                    {
                        if (bitmap != null) bitmap.Dispose();
                        return;
                    }
                    if (bitmap == null)
                    {
                        Log.Warn("轮播图片加载失败: " + path + " " + (error ?? string.Empty));
                        RaiseState();
                        // Skip a broken file after a short delay instead of blocking the rotation.
                        _timer.Interval = 1200;
                        if (!_paused) _timer.Start();
                        return;
                    }

                    Bitmap previous = CurrentBitmap;
                    CurrentBitmap = bitmap;
                    Action<Bitmap> handler = ImageChanged;
                    if (handler != null) handler(bitmap);
                    if (previous != null && !ReferenceEquals(previous, bitmap))
                    {
                        try { previous.Dispose(); } catch { }
                    }
                    if (!_paused) StartTimer();
                    RaiseState();
                });
            });
        }

        private void StartTimer()
        {
            _timer.Stop();
            _timer.Interval = Math.Max(3, _intervalSeconds) * 1000;
            if (!_paused && CurrentBitmap != null && _paths.Count > 0) _timer.Start();
        }

        public void SetPaused(bool paused)
        {
            if (_paused == paused) return;
            _paused = paused;
            if (paused) _timer.Stop();
            else StartTimer();
            RaiseState();
        }

        public void SetInterval(int seconds)
        {
            int normalized = Math.Max(3, Math.Min(3600, seconds));
            if (_intervalSeconds == normalized) return;
            _intervalSeconds = normalized;
            if (!_paused) StartTimer();
        }

        public void SetShuffle(bool shuffle)
        {
            if (_shuffle == shuffle) return;
            _shuffle = shuffle;
            if (_shuffle)
            {
                BuildOrder();
                _cursor = 0;
            }
            else
            {
                BuildOrder();
                _cursor = 0;
                if (CurrentPath != null)
                {
                    int current = _paths.FindIndex(delegate(string p)
                    {
                        return string.Equals(p, CurrentPath, StringComparison.OrdinalIgnoreCase);
                    });
                    if (current >= 0) _cursor = Math.Min(current + 1, _order.Count);
                }
            }
        }

        private void ClearCurrent()
        {
            CurrentPath = null;
            Bitmap previous = CurrentBitmap;
            CurrentBitmap = null;
            ImageChanged?.Invoke(null);
            if (previous != null) previous.Dispose();
        }

        private void RaiseState()
        {
            Action handler = StateChanged;
            if (handler != null) handler();
        }

        private void RaiseEmpty()
        {
            Action handler = Empty;
            if (handler != null) handler();
        }

        private void Post(Action action)
        {
            if (_sync != null) _sync.Post(delegate { try { action(); } catch { } }, null);
            else try { action(); } catch { }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _generation++;
            _timer.Stop();
            _timer.Dispose();
            if (CurrentBitmap != null)
            {
                try { CurrentBitmap.Dispose(); } catch { }
                CurrentBitmap = null;
            }
        }
    }
}
