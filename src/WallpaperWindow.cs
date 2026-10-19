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
        private IntPtr _desktopParent;

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
            IntPtr progman;
            IntPtr workerw = NativeMethods.FindDesktopWorkerW(out progman);
            IntPtr target = workerw != IntPtr.Zero ? workerw : progman;
            if (target == IntPtr.Zero)
            {
                Log.Warn("未找到桌面 WorkerW / Progman，将使用普通窗口。");
                NativeMethods.ShowWindow(Handle, NativeMethods.SW_SHOWNOACTIVATE);
                return false;
            }

            _desktopParent = target;
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
            bool parented = NativeMethods.SetParent(Handle, target) != IntPtr.Zero;
            if (!parented)
            {
                Log.Warn("SetParent 失败，错误码 " + System.Runtime.InteropServices.Marshal.GetLastWin32Error()
                    + "；为避免遮挡桌面，壁纸窗口将保持隐藏。");
                NativeMethods.ShowWindow(Handle, NativeMethods.SW_HIDE);
                return false;
            }
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOP, bounds.Left, bounds.Top,
                bounds.Width, bounds.Height,
                NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_SHOWNOACTIVATE);
            Log.Info("壁纸窗口已挂载到桌面 (WorkerW=" + (workerw != IntPtr.Zero) + ", parent=" + NativeMethods.GetParent(Handle) + ")");
            return true;
        }

        public void DetachFromDesktop()
        {
            if (IsHandleCreated)
            {
                NativeMethods.SetParent(Handle, IntPtr.Zero);
            }
        }

        public void SetImage(Bitmap bitmap)
        {
            _image = bitmap;
            _videoHost.Visible = false;
            Invalidate();
        }

        public void ClearImage()
        {
            _image = null;
            Invalidate();
        }

        public void ShowVideo()
        {
            _videoHost.Visible = true;
            _videoHost.BringToFront();
            Invalidate();
        }

        public void HideVideo()
        {
            _videoHost.Visible = false;
            Invalidate();
        }

        public void ShowFallback()
        {
            _image = null;
            _videoHost.Visible = false;
            Invalidate();
        }

        public void RefreshAttachment()
        {
            if (!IsAttached)
            {
                AttachToDesktop();
            }
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
