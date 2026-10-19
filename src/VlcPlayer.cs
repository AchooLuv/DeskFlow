using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class VlcPlayer : IDisposable
    {
        private const int EventPlaying = 0x104;
        private const int EventPaused = 0x105;
        private const int EventStopped = 0x106;
        private const int EventEndReached = 0x109;
        private const int EventEncounteredError = 0x10A;

        private IntPtr _instance;
        private IntPtr _media;
        private IntPtr _player;
        private IntPtr _eventManager;
        private LibVlcEventCallback _callback;
        private readonly SynchronizationContext _sync;
        private double _rate = 1.0;
        private int _volume = 30;
        private bool _loop = true;
        private bool _disposed;
        private bool _started;

        public event Action Started;
        public event Action<string> PlaybackError;
        public event Action Ended;

        public bool IsInitialized
        {
            get { return _instance != IntPtr.Zero; }
        }

        public VlcPlayer()
        {
            _sync = SynchronizationContext.Current;
        }

        public static string Version
        {
            get
            {
                try
                {
                    IntPtr ptr = libvlc_get_version();
                    return ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(ptr);
                }
                catch { return string.Empty; }
            }
        }

        private void EnsureInstance()
        {
            if (_instance != IntPtr.Zero) return;
            Log.Info("VLC: 检查运行库");
            if (!File.Exists(Path.Combine(Runtime.VlcDir, "libvlc.dll")))
                throw new FileNotFoundException("缺少 libvlc.dll，请重新运行 DeskFlow 以释放播放组件。");

            NativeMethods.SetDllDirectory(Runtime.VlcDir);
            Environment.SetEnvironmentVariable("VLC_PLUGIN_PATH", Runtime.PluginsDir);
            string dll = Path.Combine(Runtime.VlcDir, "libvlc.dll");
            if (NativeMethods.LoadLibrary(dll) == IntPtr.Zero)
                throw new InvalidOperationException("无法加载 libvlc.dll (错误码 " + Marshal.GetLastWin32Error() + ")");
            Log.Info("VLC: libvlc.dll 已加载");

            List<string> args = new List<string>
            {
                "--intf=dummy",
                "--quiet",
                "--no-interact",
                "--no-video-title-show",
                "--no-osd",
                "--no-stats",
                "--no-snapshot-preview",
                "--no-metadata-network-access",
                "--no-sub-autodetect-file",
                "--no-spu",
                "--no-plugins-cache",
                "--no-mouse-events",
                "--no-keyboard-events",
                "--avcodec-hw=any"
            };

            List<IntPtr> allocated = new List<IntPtr>();
            IntPtr[] argv = new IntPtr[args.Count];
            try
            {
                for (int i = 0; i < args.Count; i++)
                {
                    IntPtr ptr = AllocUtf8(args[i]);
                    allocated.Add(ptr);
                    argv[i] = ptr;
                }
                _instance = libvlc_new(argv.Length, argv);
            }
            finally
            {
                foreach (IntPtr ptr in allocated) Marshal.FreeHGlobal(ptr);
            }

            Log.Info("VLC: libvlc_new -> " + _instance);
            if (_instance == IntPtr.Zero)
            {
                string message = LastError();
                throw new InvalidOperationException("VLC 初始化失败" + (string.IsNullOrEmpty(message) ? "。" : ": " + message));
            }
            Log.Info("VLC 已初始化: " + Version);
        }

        public bool Play(string path, IntPtr hwnd, double rate, int volume, bool loop, out string error)
        {
            error = null;
            if (_disposed) { error = "播放器已关闭。"; return false; }
            try
            {
                EnsureInstance();
                Log.Info("VLC: 开始播放 " + path);
                StopInternal(true);
                _rate = Math.Max(0.25, Math.Min(2.0, rate));
                _volume = Math.Max(0, Math.Min(100, volume));
                _loop = loop;
                _started = false;

                _media = libvlc_media_new_path(_instance, path);
                Log.Info("VLC: media -> " + _media);
                if (_media == IntPtr.Zero)
                    throw new InvalidOperationException("无法打开视频文件。");
                libvlc_media_add_option(_media, ":no-video-title-show");
                if (_loop) libvlc_media_add_option(_media, ":input-repeat=65535");

                _player = libvlc_media_player_new_from_media(_media);
                Log.Info("VLC: player -> " + _player);
                if (_player == IntPtr.Zero)
                    throw new InvalidOperationException("无法创建视频播放器。");

                libvlc_media_player_set_hwnd(_player, hwnd);
                Log.Info("VLC: hwnd set");
                AttachEvents();
                Log.Info("VLC: events attached");
                if (libvlc_media_player_play(_player) == -1)
                    throw new InvalidOperationException("视频开始播放失败。");
                Log.Info("VLC: play called");

                ApplyTuning();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.Error("视频播放失败: " + path, ex);
                StopInternal(true);
                return false;
            }
        }

        private void AttachEvents()
        {
            _eventManager = libvlc_media_player_event_manager(_player);
            if (_eventManager == IntPtr.Zero) return;
            _callback = OnVlcEvent;
            int[] events = { EventPlaying, EventPaused, EventStopped, EventEndReached, EventEncounteredError };
            foreach (int evt in events)
                libvlc_event_attach(_eventManager, evt, _callback, IntPtr.Zero);
        }

        private void DetachEvents()
        {
            if (_eventManager == IntPtr.Zero || _callback == null) return;
            int[] events = { EventPlaying, EventPaused, EventStopped, EventEndReached, EventEncounteredError };
            foreach (int evt in events)
            {
                try { libvlc_event_detach(_eventManager, evt, _callback, IntPtr.Zero); }
                catch { }
            }
            _eventManager = IntPtr.Zero;
            _callback = null;
        }

        private void OnVlcEvent(IntPtr evt, IntPtr userData)
        {
            if (_disposed || evt == IntPtr.Zero) return;
            int type;
            try { type = Marshal.ReadInt32(evt); }
            catch { return; }

            if (type == EventPlaying)
            {
                ApplyTuning();
                if (!_started)
                {
                    _started = true;
                    Raise(Started);
                }
            }
            else if (type == EventEndReached)
            {
                if (_loop && _player != IntPtr.Zero && !_disposed)
                {
                    try
                    {
                        libvlc_media_player_set_time(_player, 0);
                        libvlc_media_player_play(_player);
                    }
                    catch { }
                }
                else
                {
                    Raise(Ended);
                }
            }
            else if (type == EventEncounteredError)
            {
                RaiseError("视频解码失败，请确认文件没有损坏。");
            }
        }

        private void Raise(Action action)
        {
            if (action == null) return;
            if (_sync != null) _sync.Post(delegate { try { action(); } catch { } }, null);
            else try { action(); } catch { }
        }

        private void RaiseError(string message)
        {
            Action<string> handler = PlaybackError;
            if (handler == null) return;
            if (_sync != null) _sync.Post(delegate { try { handler(message); } catch { } }, null);
            else try { handler(message); } catch { }
        }

        public void SetRate(double rate)
        {
            _rate = Math.Max(0.25, Math.Min(2.0, rate));
            if (_player != IntPtr.Zero)
            {
                try { libvlc_media_player_set_rate(_player, (float)_rate); }
                catch { }
            }
        }

        public void SetVolume(int volume)
        {
            _volume = Math.Max(0, Math.Min(100, volume));
            if (_player != IntPtr.Zero)
            {
                try { libvlc_audio_set_volume(_player, _volume); }
                catch { }
            }
        }

        public void SetLoop(bool loop)
        {
            _loop = loop;
        }

        private void ApplyTuning()
        {
            if (_player == IntPtr.Zero) return;
            try { libvlc_media_player_set_rate(_player, (float)_rate); } catch { }
            try { libvlc_audio_set_volume(_player, _volume); } catch { }
        }

        public void Pause(bool paused)
        {
            if (_player == IntPtr.Zero) return;
            try { libvlc_media_player_set_pause(_player, paused ? 1 : 0); }
            catch { }
        }

        public long Length
        {
            get
            {
                if (_player == IntPtr.Zero) return 0;
                try { return libvlc_media_player_get_length(_player); }
                catch { return 0; }
            }
        }

        public long Position
        {
            get
            {
                if (_player == IntPtr.Zero) return 0;
                try { return libvlc_media_player_get_time(_player); }
                catch { return 0; }
            }
        }

        public bool IsPlaying
        {
            get
            {
                if (_player == IntPtr.Zero) return false;
                try { return libvlc_media_player_is_playing(_player) != 0; }
                catch { return false; }
            }
        }

        public void Stop()
        {
            StopInternal(true);
        }

        private void StopInternal(bool release)
        {
            if (_player != IntPtr.Zero)
            {
                DetachEvents();
                try { libvlc_media_player_stop(_player); } catch { }
                if (release)
                {
                    try { libvlc_media_player_release(_player); } catch { }
                    _player = IntPtr.Zero;
                }
            }
            if (_media != IntPtr.Zero)
            {
                try { libvlc_media_release(_media); } catch { }
                _media = IntPtr.Zero;
            }
            _started = false;
        }

        private static IntPtr AllocUtf8(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text + "\0");
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return ptr;
        }

        private static string LastError()
        {
            try
            {
                IntPtr ptr = libvlc_errmsg();
                return ptr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(ptr);
            }
            catch { return string.Empty; }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopInternal(true);
            if (_instance != IntPtr.Zero)
            {
                try { libvlc_release(_instance); } catch { }
                _instance = IntPtr.Zero;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LibVlcEventCallback(IntPtr evt, IntPtr userData);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libvlc_new(int argc, IntPtr[] argv);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_release(IntPtr instance);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libvlc_get_version();

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libvlc_errmsg();

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libvlc_media_new_path(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_media_release(IntPtr media);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_media_add_option(IntPtr media, [MarshalAs(UnmanagedType.LPUTF8Str)] string option);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_media_player_release(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_media_player_set_hwnd(IntPtr player, IntPtr hwnd);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libvlc_media_player_play(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_media_player_stop(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_media_player_set_pause(IntPtr player, int doPause);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libvlc_media_player_set_rate(IntPtr player, float rate);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libvlc_media_player_is_playing(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long libvlc_media_player_get_length(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long libvlc_media_player_get_time(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libvlc_media_player_set_time(IntPtr player, long time);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libvlc_audio_set_volume(IntPtr player, int volume);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr libvlc_media_player_event_manager(IntPtr player);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int libvlc_event_attach(IntPtr eventManager, int eventType, LibVlcEventCallback callback, IntPtr userData);

        [DllImport("libvlc.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void libvlc_event_detach(IntPtr eventManager, int eventType, LibVlcEventCallback callback, IntPtr userData);
    }
}
