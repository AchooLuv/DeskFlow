using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class MainForm : Form
    {
        private readonly TrayContext _context;
        private readonly AppSettings _settings;
        private readonly WallpaperController _controller;
        private readonly Panel _titleBar;
        private readonly Panel _body;
        private readonly Panel _sidebar;
        private readonly Panel _pageHost;
        private readonly LogoControl _logo;
        private readonly Label _brand;
        private readonly Label _brandSub;
        private readonly IconButton _minButton;
        private readonly IconButton _maxButton;
        private readonly IconButton _closeButton;
        private readonly NavButton _navSlideshow;
        private readonly NavButton _navVideo;
        private readonly NavButton _navSettings;
        private readonly RoundedPanel _statusCard;
        private readonly Label _statusTitle;
        private readonly Label _statusWallpaper;
        private readonly Label _statusTaskbar;
        private readonly Label _statusPerf;
        private readonly Label _version;
        private readonly SlideshowPage _slideshowPage;
        private readonly VideoPage _videoPage;
        private readonly SettingsPage _settingsPage;
        private readonly ToastControl _toast;
        private readonly Timer _uiTimer;
        private int _currentPage;
        private bool _attachWarned;

        public MainForm(TrayContext context, AppSettings settings, WallpaperController controller)
        {
            _context = context;
            _settings = settings;
            _controller = controller;

            Text = "DeskFlow";
            Icon = AppIcon.Get();
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1000, 660);
            MinimumSize = new Size(940, 620);
            BackColor = Theme.Background;
            DoubleBuffered = true;
            KeyPreview = true;
            Padding = new Padding(1);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw, true);

            _titleBar = new Panel { Dock = DockStyle.Top, Height = 54, BackColor = Theme.Background };
            _titleBar.Paint += TitleBarPaint;
            Controls.Add(_titleBar);

            _logo = new LogoControl { Location = new Point(18, 11), Width = 32, Height = 32 };
            _titleBar.Controls.Add(_logo);

            _brand = Ui.Label("DeskFlow", Theme.H3, Theme.Text, Theme.Background);
            _brand.SetBounds(60, 8, 160, 24);
            _titleBar.Controls.Add(_brand);

            _brandSub = Ui.Label("动态壁纸引擎", Theme.Tiny, Theme.TextDim, Theme.Background);
            _brandSub.SetBounds(60, 29, 160, 18);
            _titleBar.Controls.Add(_brandSub);

            _minButton = new IconButton { Glyph = "\uE921", Width = 40, Height = 32 };
            _minButton.Click += delegate { WindowState = FormWindowState.Minimized; };
            _titleBar.Controls.Add(_minButton);

            _maxButton = new IconButton { Glyph = "\uE922", Width = 40, Height = 32 };
            _maxButton.Click += delegate { ToggleMaximize(); };
            _titleBar.Controls.Add(_maxButton);

            _closeButton = new IconButton(true) { Glyph = "\uE8BB", Width = 40, Height = 32 };
            _closeButton.Click += delegate { Close(); };
            _titleBar.Controls.Add(_closeButton);

            _titleBar.MouseDown += TitleBarMouseDown;
            _brand.MouseDown += TitleBarMouseDown;
            _brandSub.MouseDown += TitleBarMouseDown;
            _titleBar.DoubleClick += delegate { ToggleMaximize(); };
            _brand.DoubleClick += delegate { ToggleMaximize(); };
            _brandSub.DoubleClick += delegate { ToggleMaximize(); };

            _body = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            Controls.Add(_body);
            _body.BringToFront();

            _sidebar = new Panel { Dock = DockStyle.Left, Width = 212, BackColor = Theme.Sidebar };
            _sidebar.Paint += SidebarPaint;
            _body.Controls.Add(_sidebar);

            _navSlideshow = new NavButton { Text = "图片轮播", Glyph = "\uE91B" };
            _navSlideshow.SetBounds(16, 18, 180, 46);
            _navSlideshow.Click += delegate { ShowPage(0); };
            _sidebar.Controls.Add(_navSlideshow);

            _navVideo = new NavButton { Text = "动态壁纸", Glyph = "\uE714" };
            _navVideo.SetBounds(16, 70, 180, 46);
            _navVideo.Click += delegate { ShowPage(1); };
            _sidebar.Controls.Add(_navVideo);

            _navSettings = new NavButton { Text = "设置", Glyph = "\uE713" };
            _navSettings.SetBounds(16, 122, 180, 46);
            _navSettings.Click += delegate { ShowPage(2); };
            _sidebar.Controls.Add(_navSettings);

            _statusCard = new RoundedPanel
            {
                Width = 180,
                Height = 122,
                Radius = 10f,
                FillColor = Theme.CardSoft,
                BorderColor = Theme.BorderSoft,
                BackColor = Theme.Sidebar
            };
            _sidebar.Controls.Add(_statusCard);

            _statusTitle = Ui.Label("运行状态", Theme.Tiny, Theme.TextMuted, Theme.CardSoft);
            _statusTitle.SetBounds(12, 10, 140, 18);
            _statusCard.Controls.Add(_statusTitle);

            _statusWallpaper = Ui.Label("壁纸  ·  未启用", Theme.Tiny, Theme.Text, Theme.CardSoft);
            _statusWallpaper.SetBounds(12, 34, 158, 18);
            _statusCard.Controls.Add(_statusWallpaper);

            _statusTaskbar = Ui.Label("任务栏  ·  完全透明", Theme.Tiny, Theme.TextMuted, Theme.CardSoft);
            _statusTaskbar.SetBounds(12, 56, 158, 18);
            _statusCard.Controls.Add(_statusTaskbar);

            _statusPerf = Ui.Label("全屏暂停  ·  开", Theme.Tiny, Theme.TextMuted, Theme.CardSoft);
            _statusPerf.SetBounds(12, 78, 158, 18);
            _statusCard.Controls.Add(_statusPerf);

            _version = Ui.Label("v1.0.8  ·  主显示器", Theme.Tiny, Theme.TextDim, Theme.Sidebar);
            _version.SetBounds(16, 0, 180, 18);
            _sidebar.Controls.Add(_version);

            _pageHost = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };
            _body.Controls.Add(_pageHost);
            _pageHost.BringToFront();

            _slideshowPage = new SlideshowPage(settings);
            _videoPage = new VideoPage(settings);
            _settingsPage = new SettingsPage(settings);
            _slideshowPage.Dock = DockStyle.Fill;
            _videoPage.Dock = DockStyle.Fill;
            _settingsPage.Dock = DockStyle.Fill;
            _pageHost.Controls.Add(_settingsPage);
            _pageHost.Controls.Add(_videoPage);
            _pageHost.Controls.Add(_slideshowPage);

            _slideshowPage.Changed += delegate { _context.OnSettingsChanged(); };
            _slideshowPage.TogglePauseRequested += delegate
            {
                _controller.TogglePause();
                RefreshUi();
            };
            _slideshowPage.NextRequested += delegate
            {
                _controller.NextImage();
                RefreshUi();
            };
            _slideshowPage.RefreshRequested += delegate
            {
                _controller.RefreshSlideshow();
                ShowToast("已刷新图片列表");
                RefreshUi();
            };
            _slideshowPage.ApplyRequested += delegate
            {
                if (_settings.Mode == "slideshow") return;
                _settings.Mode = "slideshow";
                _context.OnSettingsChanged();
                ShowToast("已切换到图片轮播");
                RefreshUi();
            };

            _videoPage.Changed += delegate { _context.OnSettingsChanged(); };
            _videoPage.TogglePauseRequested += delegate
            {
                _controller.TogglePause();
                RefreshUi();
            };
            _videoPage.RestartRequested += delegate
            {
                _controller.Apply(_settings, true);
                ShowToast("视频已重新开始");
                RefreshUi();
            };
            _videoPage.ApplyRequested += delegate
            {
                if (_settings.Mode == "video") return;
                _settings.Mode = "video";
                _context.OnSettingsChanged();
                ShowToast("已切换到动态壁纸");
                RefreshUi();
            };

            _settingsPage.Changed += delegate { _context.OnSettingsChanged(); };
            _settingsPage.ResetRequested += delegate
            {
                _slideshowPage.ApplySettingsToControls();
                _videoPage.ApplySettingsToControls();
                _context.OnSettingsChanged();
                ShowToast("已恢复默认设置");
            };

            _toast = new ToastControl();
            Controls.Add(_toast);
            _toast.BringToFront();

            _uiTimer = new Timer { Interval = 2000 };
            _uiTimer.Tick += delegate { if (Visible) UpdateSidebar(); };
            LayoutChrome();

            Shown += delegate
            {
                Log.Info("UI: dpi=" + DeviceDpi + " client=" + ClientSize + " bounds=" + Bounds +
                    " scaleDims=" + AutoScaleDimensions);
                CenterOnPrimaryScreen();
                _uiTimer.Start();
                ShowPage(Math.Max(0, Math.Min(2, _settings.LastPage)), false);
                RefreshUi();
                if (_controller.DesktopAttachFailed && !_attachWarned)
                {
                    _attachWarned = true;
                    ShowToast("壁纸窗口未能挂载到桌面，请重启 DeskFlow 或 Explorer 后重试。", true);
                }
            };
            VisibleChanged += delegate
            {
                if (Visible) _uiTimer.Start();
                else _uiTimer.Stop();
            };
            Resize += delegate
            {
                LayoutChrome();
                if (WindowState == FormWindowState.Maximized) _maxButton.Glyph = "\uE923";
                else _maxButton.Glyph = "\uE922";
            };
            FormClosing += MainFormClosing;
            KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape && _settings.MinimizeToTray)
                {
                    e.Handled = true;
                    Hide();
                }
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }

        public void RefreshUi()
        {
            if (IsDisposed) return;
            _slideshowPage.UpdateState(_controller);
            _videoPage.UpdateState(_controller);
            _settingsPage.ApplySettingsToControls();
            UpdateSidebar();
            if (WindowState == FormWindowState.Maximized) _maxButton.Glyph = "\uE923";
            else _maxButton.Glyph = "\uE922";
        }

        public void ShowToast(string message, bool error = false)
        {
            if (string.IsNullOrEmpty(message) || IsDisposed) return;
            _toast.ShowToast(message, error);
            _toast.Left = Math.Max(12, ClientSize.Width - _toast.Width - 22);
            _toast.Top = Math.Max(66, ClientSize.Height - _toast.Height - 18);
            _toast.BringToFront();
        }

        private void UpdateSidebar()
        {
            string mode = "未启用";
            if (_settings.Mode == "slideshow") mode = "图片轮播";
            else if (_settings.Mode == "video") mode = "动态壁纸";
            if (_controller.IsPaused && _settings.Mode != "none") mode += " · 已暂停";
            _statusWallpaper.Text = "壁纸  ·  " + mode;
            _statusWallpaper.ForeColor = _settings.Mode == "none" ? Theme.TextMuted : Theme.Text;
            _statusTaskbar.Text = "任务栏  ·  " + (_settings.TaskbarTransparent ? "完全透明" : "系统默认");
            _statusTaskbar.ForeColor = _settings.TaskbarTransparent ? Theme.Text : Theme.TextMuted;
            _statusPerf.Text = "全屏暂停  ·  " + (_settings.PauseFullscreen ? "开" : "关");
            _statusPerf.ForeColor = _settings.PauseFullscreen ? Theme.Text : Theme.TextMuted;
            _navSlideshow.Active = _currentPage == 0;
            _navVideo.Active = _currentPage == 1;
            _navSettings.Active = _currentPage == 2;
        }

        private void ShowPage(int index, bool activateMode = true)
        {
            _currentPage = index;
            _settings.LastPage = index;

            bool modeChanged = false;
            if (activateMode && _settings.SwitchModeOnNavClick)
            {
                if (index == 0 && _settings.Mode != "slideshow"
                    && !string.IsNullOrEmpty(_settings.ImageFolder)
                    && Directory.Exists(_settings.ImageFolder))
                {
                    _settings.Mode = "slideshow";
                    modeChanged = true;
                }
                else if (index == 1 && _settings.Mode != "video"
                    && !string.IsNullOrEmpty(_settings.VideoPath)
                    && File.Exists(_settings.VideoPath))
                {
                    _settings.Mode = "video";
                    modeChanged = true;
                }
            }

            _slideshowPage.Visible = index == 0;
            _videoPage.Visible = index == 1;
            _settingsPage.Visible = index == 2;
            if (index == 0) _slideshowPage.RefreshLayout();
            else if (index == 1) _videoPage.RefreshLayout();
            else _settingsPage.RefreshLayout();
            UpdateSidebar();
            if (modeChanged)
            {
                _context.OnSettingsChanged();
                ShowToast(index == 0 ? "已切换到图片轮播" : "已切换到动态壁纸");
            }
            _context.SaveSoon();
        }

        private void CenterOnPrimaryScreen()
        {
            if (WindowState != FormWindowState.Normal) return;
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(area.Left + Math.Max(0, (area.Width - Width) / 2),
                area.Top + Math.Max(0, (area.Height - Height) / 2));
        }

        private void LayoutChrome()
        {
            int width = _titleBar.Width;
            _closeButton.Location = new Point(width - 48, 11);
            _maxButton.Location = new Point(width - 90, 11);
            _minButton.Location = new Point(width - 132, 11);
            _statusCard.Location = new Point(16, Math.Max(200, _sidebar.Height - 166));
            _version.Location = new Point(16, _sidebar.Height - 26);
            if (_toast.Visible)
            {
                _toast.Left = Math.Max(12, ClientSize.Width - _toast.Width - 22);
                _toast.Top = Math.Max(66, ClientSize.Height - _toast.Height - 18);
            }
        }

        private void TitleBarPaint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Theme.BorderSoft, 1f))
                e.Graphics.DrawLine(pen, 0, _titleBar.Height - 1, _titleBar.Width, _titleBar.Height - 1);
        }

        private void SidebarPaint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Theme.BorderSoft, 1f))
                e.Graphics.DrawLine(pen, _sidebar.Width - 1, 0, _sidebar.Width - 1, _sidebar.Height);
        }

        private void TitleBarMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
        }

        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
                WindowState = FormWindowState.Maximized;
            }
            LayoutChrome();
        }

        private void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_context.IsExiting) return;
            if (_settings.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                e.Cancel = true;
                _context.Exit();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (NativeMethods.IsWindows11)
            {
                int preference = 2;
                try { NativeMethods.DwmSetWindowAttribute(Handle, 33, ref preference, sizeof(int)); }
                catch { }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Pen pen = new Pen(Theme.Border, 1f))
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_NCHITTEST && WindowState != FormWindowState.Maximized)
            {
                Point point = PointToClient(Cursor.Position);
                int edge = 7;
                bool left = point.X <= edge;
                bool right = point.X >= ClientSize.Width - edge;
                bool top = point.Y <= edge;
                bool bottom = point.Y >= ClientSize.Height - edge;
                if (left && top) { m.Result = (IntPtr)13; return; }
                if (right && top) { m.Result = (IntPtr)14; return; }
                if (left && bottom) { m.Result = (IntPtr)16; return; }
                if (right && bottom) { m.Result = (IntPtr)17; return; }
                if (left) { m.Result = (IntPtr)10; return; }
                if (right) { m.Result = (IntPtr)11; return; }
                if (top) { m.Result = (IntPtr)12; return; }
                if (bottom) { m.Result = (IntPtr)15; return; }
            }
            base.WndProc(ref m);
        }
    }
}
