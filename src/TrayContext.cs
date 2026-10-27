using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DeskFlow
{
    internal sealed class TrayContext : ApplicationContext
    {
        private readonly AppSettings _settings;
        private readonly WallpaperController _controller;
        private readonly NotifyIcon _tray;
        private readonly ContextMenuStrip _menu;
        private readonly ToolStripMenuItem _openItem;
        private readonly ToolStripMenuItem _pauseItem;
        private readonly ToolStripMenuItem _nextItem;
        private readonly ToolStripMenuItem _taskbarItem;
        private readonly ToolStripMenuItem _exitItem;
        private readonly Timer _timer;
        private readonly Timer _taskbarTimer;
        private readonly Timer _saveTimer;
        private readonly MessageWindow _messageWindow;
        private MainForm _form;
        private bool _exiting;
        private bool _fullscreenPaused;
        private bool _batteryPaused;
        private bool _shellOverlayOpen;
        private bool _shellOverlayApplied;
        private DateTime _shellOverlayApplyAt = DateTime.MinValue;
        private DateTime _lastBatteryCheck = DateTime.MinValue;

        public static readonly uint TaskbarCreatedMessage = NativeMethods.RegisterWindowMessage("TaskbarCreated");
        public static readonly uint ShowSettingsMessage = NativeMethods.RegisterWindowMessage("DeskFlow.ShowSettings");

        public bool IsExiting { get { return _exiting; } }

        public TrayContext(bool startHidden)
        {
            _settings = AppSettings.Load();
            TaskbarTransparency.Initialize(_settings.TaskbarTransparent);
            EnsureAutoStart();
            _controller = new WallpaperController(_settings);
            _controller.PreviewChanged += delegate
            {
                if (_form != null && !_form.IsDisposed && _form.Visible) _form.RefreshUi();
            };
            _controller.StateChanged += delegate
            {
                UpdateTray();
                if (_form != null && !_form.IsDisposed && _form.Visible) _form.RefreshUi();
            };
            _controller.Error += OnWallpaperError;

            _menu = new ContextMenuStrip();
            _menu.BackColor = Color.FromArgb(24, 28, 38);
            _menu.ForeColor = Theme.Text;
            _menu.ShowImageMargin = false;
            _menu.ShowCheckMargin = false;
            _menu.Padding = new Padding(4);
            _menu.Renderer = new DarkMenuRenderer();
            _menu.Font = Theme.Body;

            _openItem = CreateMenuItem("打开 DeskFlow", true);
            _openItem.Click += delegate { ShowSettings(); };
            _menu.Items.Add(_openItem);
            _menu.Items.Add(new ToolStripSeparator());

            _pauseItem = CreateMenuItem("暂停壁纸", false);
            _pauseItem.Click += delegate
            {
                _controller.TogglePause();
                UpdateTray();
                if (_form != null && !_form.IsDisposed && _form.Visible) _form.RefreshUi();
            };
            _menu.Items.Add(_pauseItem);

            _nextItem = CreateMenuItem("下一张图片", false);
            _nextItem.Click += delegate
            {
                _controller.NextImage();
                if (_form != null && !_form.IsDisposed && _form.Visible) _form.RefreshUi();
            };
            _menu.Items.Add(_nextItem);
            _menu.Items.Add(new ToolStripSeparator());

            _taskbarItem = CreateMenuItem("任务栏完全透明", false);
            _taskbarItem.Click += delegate
            {
                _settings.TaskbarTransparent = !_settings.TaskbarTransparent;
                OnSettingsChanged();
            };
            _menu.Items.Add(_taskbarItem);
            _menu.Items.Add(new ToolStripSeparator());

            _exitItem = CreateMenuItem("退出", false);
            _exitItem.Click += delegate { Exit(); };
            _menu.Items.Add(_exitItem);

            _tray = new NotifyIcon
            {
                Icon = AppIcon.Get(),
                Text = "DeskFlow · 动态壁纸",
                Visible = true,
                ContextMenuStrip = _menu
            };
            _tray.DoubleClick += delegate { ShowSettings(); };
            _tray.BalloonTipClicked += delegate { ShowSettings(); };
            _tray.BalloonTipClosed += delegate { _tray.Visible = true; };

            _messageWindow = new MessageWindow(this);
            _timer = new Timer { Interval = 2000 };
            _timer.Tick += delegate { OnTick(); };
            _timer.Start();

            // Start menu and shell overlays can reset the taskbar's accent
            // policy. Re-assert it independently of the slower UI timer.
            _taskbarTimer = new Timer { Interval = 250 };
            _taskbarTimer.Tick += delegate
            {
                if (_exiting) return;
                try
                {
                    if (IsShellOverlayOpen())
                    {
                        if (!_shellOverlayOpen)
                        {
                            _shellOverlayOpen = true;
                            _shellOverlayApplied = false;
                            _shellOverlayApplyAt = DateTime.UtcNow.AddMilliseconds(140);
                        }
                        if (!_shellOverlayApplied && DateTime.UtcNow >= _shellOverlayApplyAt)
                        {
                            TaskbarTransparency.Apply(true);
                            _shellOverlayApplied = true;
                        }
                        return;
                    }
                    if (_shellOverlayOpen)
                    {
                        _shellOverlayOpen = false;
                        _shellOverlayApplied = false;
                        TaskbarTransparency.Apply(true);
                    }
                    else
                    {
                        TaskbarTransparency.ApplyIfNeeded();
                    }
                }
                catch { }
            };
            _taskbarTimer.Start();

            _saveTimer = new Timer { Interval = 500 };
            _saveTimer.Tick += delegate
            {
                _saveTimer.Stop();
                _settings.Save();
            };

            SystemEvents.SessionSwitch += SystemEventsSessionSwitch;
            SystemEvents.PowerModeChanged += SystemEventsPowerModeChanged;

            UpdateTray();
            if (!startHidden) ShowSettings();
        }

        private ToolStripMenuItem CreateMenuItem(string text, bool bold)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.ForeColor = Theme.Text;
            item.Font = bold ? Theme.BodyBold : Theme.Body;
            item.Padding = new Padding(8, 3, 8, 3);
            return item;
        }

        public void ShowSettings()
        {
            if (_exiting) return;
            if (_form == null || _form.IsDisposed)
            {
                _form = new MainForm(this, _settings, _controller);
                _form.FormClosed += delegate { _form = null; };
            }
            if (!_form.Visible) _form.Show();
            if (_form.WindowState == FormWindowState.Minimized) _form.WindowState = FormWindowState.Normal;
            _form.ShowInTaskbar = true;
            _form.Activate();
            try { NativeMethods.SetForegroundWindow(_form.Handle); } catch { }
            _form.RefreshUi();
        }

        public void OnSettingsChanged()
        {
            TaskbarTransparency.SetEnabled(_settings.TaskbarTransparent);
            EnsureAutoStart();
            _controller.Apply(_settings, false);
            UpdateTray();
            if (_form != null && !_form.IsDisposed) _form.RefreshUi();
            SaveSoon();
        }

        public void SaveSoon()
        {
            if (_exiting) return;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void EnsureAutoStart()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) return;
                    if (_settings.AutoStart)
                    {
                        string command = "\"" + Application.ExecutablePath + "\" --autostart";
                        object current = key.GetValue("DeskFlow");
                        if (!string.Equals(current as string, command, StringComparison.OrdinalIgnoreCase))
                            key.SetValue("DeskFlow", command);
                    }
                    else
                    {
                        if (key.GetValue("DeskFlow") != null) key.DeleteValue("DeskFlow", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("更新开机启动失败: " + ex.Message);
            }
        }

        private void OnTick()
        {
            if (_exiting) return;

            bool fullscreen = _settings.PauseFullscreen && IsFullscreenAppActive();
            if (fullscreen != _fullscreenPaused)
            {
                _fullscreenPaused = fullscreen;
                _controller.SetAutoPaused(_fullscreenPaused || _batteryPaused);
            }

            if ((DateTime.UtcNow - _lastBatteryCheck).TotalSeconds >= 15)
            {
                _lastBatteryCheck = DateTime.UtcNow;
                bool battery = _settings.PauseOnBattery && SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline;
                if (battery != _batteryPaused)
                {
                    _batteryPaused = battery;
                    _controller.SetAutoPaused(_fullscreenPaused || _batteryPaused);
                }
            }

            UpdateTrayText();
        }

        private void UpdateTrayText()
        {
            string mode = _settings.Mode == "slideshow" ? "图片轮播" : (_settings.Mode == "video" ? "动态壁纸" : "未启用");
            if (_controller.IsPaused && _settings.Mode != "none") mode += " · 已暂停";
            string text = "DeskFlow · " + mode;
            if (text.Length > 63) text = text.Substring(0, 60) + "…";
            if (_tray.Text != text) _tray.Text = text;
        }

        private void UpdateTray()
        {
            _pauseItem.Text = _controller.IsPaused ? "继续壁纸" : "暂停壁纸";
            _nextItem.Enabled = _settings.Mode == "slideshow" && _controller.ImageCount > 0;
            _taskbarItem.Checked = _settings.TaskbarTransparent;
            UpdateTrayText();
        }

        private bool IsShellOverlayOpen()
        {
            try
            {
                IntPtr foreground = NativeMethods.GetForegroundWindow();
                if (foreground == IntPtr.Zero) return false;

                StringBuilder cls = new StringBuilder(256);
                NativeMethods.GetClassName(foreground, cls, cls.Capacity);
                if (string.Equals(cls.ToString(), "Windows.UI.Core.CoreWindow",
                    StringComparison.OrdinalIgnoreCase))
                    return true;

                uint pid;
                NativeMethods.GetWindowThreadProcessId(foreground, out pid);
                if (pid == 0) return false;
                string processName = Process.GetProcessById((int)pid).ProcessName;
                return string.Equals(processName, "StartMenuExperienceHost", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(processName, "SearchApp", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(processName, "SearchHost", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(processName, "ShellExperienceHost", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private bool IsFullscreenAppActive()
        {
            try
            {
                int state;
                if (NativeMethods.SHQueryUserNotificationState(out state) == 0)
                {
                    if (state == (int)NativeMethods.NotificationState.RunningD3DFullScreen ||
                        state == (int)NativeMethods.NotificationState.PresentationMode)
                        return true;
                }

                IntPtr foreground = NativeMethods.GetForegroundWindow();
                if (foreground == IntPtr.Zero) return false;

                uint pid;
                NativeMethods.GetWindowThreadProcessId(foreground, out pid);
                if (pid == (uint)Process.GetCurrentProcess().Id) return false;

                StringBuilder cls = new StringBuilder(256);
                NativeMethods.GetClassName(foreground, cls, cls.Capacity);
                string className = cls.ToString();
                if (className == "Progman" || className == "WorkerW" || className == "Shell_TrayWnd" ||
                    className == "Shell_SecondaryTrayWnd" || className == "Windows.UI.Core.CoreWindow")
                    return false;

                NativeMethods.RECT rect;
                if (!NativeMethods.GetWindowRect(foreground, out rect)) return false;
                Screen screen = Screen.FromHandle(foreground);
                Rectangle bounds = screen.Bounds;
                bool covers = rect.Left <= bounds.Left + 2 && rect.Top <= bounds.Top + 2 &&
                              rect.Right >= bounds.Right - 2 && rect.Bottom >= bounds.Bottom - 2;
                if (!covers) return false;

                int style = NativeMethods.GetWindowLong(foreground, NativeMethods.GWL_STYLE);
                if ((style & 0x40000000) != 0) return false; // WS_CHILD
                return NativeMethods.IsWindowVisible(foreground);
            }
            catch { return false; }
        }

        private void OnWallpaperError(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (_form != null && !_form.IsDisposed && _form.Visible)
            {
                _form.ShowToast(message, true);
            }
            else
            {
                try
                {
                    _tray.Visible = true;
                    _tray.ShowBalloonTip(3000, "DeskFlow", message, ToolTipIcon.Warning);
                }
                catch { }
            }
        }

        private void SystemEventsSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (_exiting) return;
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _controller.SetSessionLocked(true);
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _controller.SetSessionLocked(false);
                TaskbarTransparency.Apply(true);
            }
        }

        private void SystemEventsPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (_exiting) return;
            if (!_settings.PauseOnBattery) return;
            bool battery = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline;
            if (battery != _batteryPaused)
            {
                _batteryPaused = battery;
                _controller.SetAutoPaused(_fullscreenPaused || _batteryPaused);
            }
        }

        internal void OnTaskbarCreated()
        {
            Log.Info("检测到任务栏重建，正在恢复壁纸与任务栏样式。");
            TaskbarTransparency.Apply(true);
            _controller.Reattach();
        }

        public void Exit()
        {
            if (_exiting) return;
            _exiting = true;
            try { _timer.Stop(); } catch { }
            try { _taskbarTimer.Stop(); } catch { }
            try { _saveTimer.Stop(); } catch { }
            try { _settings.Save(); } catch { }
            SystemEvents.SessionSwitch -= SystemEventsSessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEventsPowerModeChanged;
            try { _controller.Dispose(); } catch { }
            try { TaskbarTransparency.Restore(); } catch { }
            try
            {
                if (_messageWindow != null) _messageWindow.DestroyHandle();
            }
            catch { }
            try
            {
                _tray.Visible = false;
                _tray.Dispose();
            }
            catch { }
            try
            {
                if (_form != null && !_form.IsDisposed)
                {
                    _form.Dispose();
                    _form = null;
                }
            }
            catch { }
            ExitThread();
        }

        public static void SignalExistingInstance()
        {
            NativeMethods.PostMessage(new IntPtr(0xFFFF), ShowSettingsMessage, IntPtr.Zero, IntPtr.Zero);
        }

        private sealed class MessageWindow : NativeWindow
        {
            private readonly TrayContext _owner;

            public MessageWindow(TrayContext owner)
            {
                _owner = owner;
                CreateParams cp = new CreateParams();
                cp.Caption = "DeskFlow.Message";
                cp.X = 0;
                cp.Y = 0;
                cp.Width = 0;
                cp.Height = 0;
                cp.Style = 0;
                cp.ExStyle = 0;
                CreateHandle(cp);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == (int)TaskbarCreatedMessage)
                {
                    _owner.OnTaskbarCreated();
                    return;
                }
                if (m.Msg == (int)ShowSettingsMessage)
                {
                    _owner.ShowSettings();
                    return;
                }
                base.WndProc(ref m);
            }
        }
    }

    internal sealed class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Color.FromArgb(24, 28, 38); } }
        public override Color ImageMarginGradientBegin { get { return Color.FromArgb(24, 28, 38); } }
        public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(24, 28, 38); } }
        public override Color ImageMarginGradientEnd { get { return Color.FromArgb(24, 28, 38); } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color MenuItemBorder { get { return Theme.AccentSoft; } }
        public override Color MenuItemSelected { get { return Theme.CardHover; } }
        public override Color MenuItemSelectedGradientBegin { get { return Theme.CardHover; } }
        public override Color MenuItemSelectedGradientEnd { get { return Theme.CardHover; } }
        public override Color SeparatorDark { get { return Theme.BorderSoft; } }
        public override Color SeparatorLight { get { return Theme.BorderSoft; } }
    }

    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable())
        {
            RoundedEdges = false;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? Theme.Text : (e.Item.Enabled ? Theme.TextMuted : Theme.TextDim);
            base.OnRenderItemText(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (Pen pen = new Pen(Theme.Border, 1f))
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }
    }
}
