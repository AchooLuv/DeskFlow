using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class WallpaperWindow : Form
    {
        private readonly Panel _videoHost;
        private Bitmap _image;

        public WallpaperWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Black;
            DoubleBuffered = true;
            Bounds = Screen.PrimaryScreen.Bounds;

            _videoHost = new Panel();
            _videoHost.Dock = DockStyle.Fill;
            _videoHost.BackColor = Color.Black;
            _videoHost.Visible = false;
            Controls.Add(_videoHost);

            SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW;
                cp.ExStyle &= ~NativeMethods.WS_EX_APPWINDOW;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }

        public IntPtr VideoHandle
        {
            get
            {
                IntPtr handle = _videoHost.Handle;
                return handle;
            }
        }

        public bool IsAttached
        {
            get
            {
                if (!IsHandleCreated || !NativeMethods.IsWindow(Handle)) return false;
                IntPtr parent = NativeMethods.GetParent(Handle);
                return parent != IntPtr.Zero && NativeMethods.IsWindow(parent);
            }
        }

        public bool AttachToDesktop()
        {
            if (!IsHandleCreated) CreateHandle();
            NativeMethods.DesktopTarget desktop = NativeMethods.FindDesktopTarget();
            // On Windows 10 the wallpaper is painted by WorkerW. Falling back to
            // Progman makes hiding the window reveal a black desktop, which is
            // exactly the startup/switch/exit black screen. Wait for WorkerW.
            IntPtr target = desktop.Raised ? desktop.Progman : desktop.WorkerW;
            if (target == IntPtr.Zero)
            {
                Log.Warn("未找到可用的桌面 WorkerW/Progman，壁纸窗口保持隐藏并等待重试。"
                    + " progman=" + desktop.Progman + " workerw=" + desktop.WorkerW
                    + " raised=" + desktop.Raised);
                NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
                return false;
            }

            Log.Info("桌面挂载目标: progman=" + desktop.Progman + " workerw=" + desktop.WorkerW
                + " defview=" + desktop.DefView + " raised=" + desktop.Raised + " target=" + target);
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
            int oldStyle = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_STYLE);
            NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_STYLE,
                (oldStyle & ~NativeMethods.WS_POPUP) | NativeMethods.WS_CHILD);

            if (desktop.Raised)
            {
                int exStyle = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE,
                    exStyle | NativeMethods.WS_EX_LAYERED);
                NativeMethods.SetLayeredWindowAttributes(Handle, 0, 255, NativeMethods.LWA_ALPHA);
            }

            bool parented = NativeMethods.SetParent(Handle, target) != IntPtr.Zero;
            if (!parented)
            {
                Log.Warn("SetParent 失败，错误码 " + System.Runtime.InteropServices.Marshal.GetLastWin32Error()
                    + "；为避免遮挡桌面，壁纸窗口将保持隐藏。");
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_STYLE, oldStyle);
                NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
                return false;
            }

            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            IntPtr insertAfter = desktop.Raised && desktop.DefView != IntPtr.Zero
                ? desktop.DefView
                : NativeMethods.HWND_TOP;
            NativeMethods.SetWindowPos(Handle, insertAfter, bounds.Left, bounds.Top,
                bounds.Width, bounds.Height, NativeMethods.SWP_NOACTIVATE);
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
            Log.Info("壁纸窗口已挂载到桌面 (raised=" + desktop.Raised + ", parent=" + NativeMethods.GetParent(Handle) + ")");
            if (_image != null || _videoHost.Visible) Reveal();
            return true;
        }

        public void UpdateDesktopBounds()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            Bounds = bounds;
            if (!IsAttached) return;
            NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOP,
                bounds.Left, bounds.Top, bounds.Width, bounds.Height,
                NativeMethods.SWP_NOACTIVATE);
        }

        public void DetachFromDesktop()
        {
            if (IsHandleCreated)
            {
                IntPtr parent = NativeMethods.GetParent(Handle);
                HideWindow();
                NativeMethods.SetParent(Handle, IntPtr.Zero);
                // Ask Explorer to repaint the real wallpaper immediately, especially
                // when the old parent was Progman rather than WorkerW.
                if (parent != IntPtr.Zero && NativeMethods.IsWindow(parent))
                {
                    NativeMethods.RedrawWindow(parent, IntPtr.Zero, IntPtr.Zero,
                        NativeMethods.RDW_INVALIDATE | NativeMethods.RDW_ERASE |
                        NativeMethods.RDW_ALLCHILDREN | NativeMethods.RDW_UPDATENOW);
                }
                IntPtr progman = NativeMethods.FindWindow("Progman", null);
                if (progman != IntPtr.Zero)
                {
                    NativeMethods.RedrawWindow(progman, IntPtr.Zero, IntPtr.Zero,
                        NativeMethods.RDW_INVALIDATE | NativeMethods.RDW_ERASE |
                        NativeMethods.RDW_ALLCHILDREN | NativeMethods.RDW_UPDATENOW);
                }
            }
        }

        public void SetImage(Bitmap bitmap)
        {
            _image = bitmap;
            _videoHost.Visible = false;
            if (bitmap != null) Reveal();
            else HideWindow();
            Invalidate();
        }

        public void ShowVideo()
        {
            _videoHost.Visible = true;
            _videoHost.BringToFront();
            Reveal();
            Invalidate();
        }

        public void HideVideo()
        {
            _videoHost.Visible = false;
            if (_image == null) HideWindow();
            Invalidate();
        }

        public void ShowFallback()
        {
            _image = null;
            _videoHost.Visible = false;
            HideWindow();
            Invalidate();
        }

        private void Reveal()
        {
            if (!IsHandleCreated || !IsAttached) return;
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_SHOWNOACTIVATE);
        }

        private void HideWindow()
        {
            if (IsHandleCreated) NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)NativeMethods.MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Painting is fully handled by OnPaint to avoid a desktop flash.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_image != null)
            {
                try
                {
                    g.DrawImageUnscaled(_image, 0, 0);
                    return;
                }
                catch { }
            }

            Rectangle bounds = ClientRectangle;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, bounds.Width), Math.Max(1, bounds.Height)),
                Color.FromArgb(19, 25, 38), Color.FromArgb(9, 11, 16), 40f))
            {
                g.FillRectangle(brush, bounds);
            }

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(18, Theme.Accent)))
                g.FillEllipse(glow, bounds.Width * 0.55f, -bounds.Height * 0.25f, bounds.Width * 0.55f, bounds.Width * 0.55f);
            using (SolidBrush glow = new SolidBrush(Color.FromArgb(12, Theme.Purple)))
                g.FillEllipse(glow, -bounds.Width * 0.15f, bounds.Height * 0.58f, bounds.Width * 0.5f, bounds.Width * 0.5f);

            int logo = Math.Max(36, bounds.Height / 16);
            RectangleF logoRect = new RectangleF(bounds.Width / 2f - logo / 2f,
                bounds.Height / 2f - logo - 16, logo, logo);
            using (LinearGradientBrush brush = new LinearGradientBrush(logoRect, Theme.Accent, Theme.Purple, 45f))
            using (GraphicsPath path = Theme.RoundedRect(logoRect, logo * 0.27f))
                g.FillPath(brush, path);
            using (SolidBrush white = new SolidBrush(Color.White))
            {
                float cx = logoRect.X + logo / 2f;
                float cy = logoRect.Y + logo / 2f;
                g.FillPolygon(white, new PointF[]
                {
                    new PointF(cx - logo * 0.1f, cy - logo * 0.16f),
                    new PointF(cx - logo * 0.1f, cy + logo * 0.16f),
                    new PointF(cx + logo * 0.18f, cy)
                });
            }

            using (Font titleFont = new Font(Theme.UiFamily, Math.Max(12f, bounds.Height / 70f), FontStyle.Bold, GraphicsUnit.Point))
            {
                TextRenderer.DrawText(g, "DeskFlow", titleFont,
                    new Rectangle(0, (int)(bounds.Height / 2f + 10), bounds.Width, 40), Color.FromArgb(205, 225, 232, 245),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding);
            }
            TextRenderer.DrawText(g, "选择图片文件夹或视频，让桌面流动起来", Theme.Small,
                new Rectangle(0, (int)(bounds.Height / 2f + 48), bounds.Width, 24), Color.FromArgb(145, 158, 175, 196),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding);
        }
    }
}
