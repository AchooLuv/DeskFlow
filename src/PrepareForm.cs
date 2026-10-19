using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class PrepareForm : Form
    {
        private readonly ThinProgressBar _progress;
        private readonly UiButton _retry;
        private readonly UiButton _exit;
        private string _status = "正在准备播放引擎…";
        private string _error;
        private bool _busy;
        private bool _allowClose;

        public PrepareForm()
        {
            Text = "DeskFlow";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 300);
            BackColor = Theme.Background;
            DoubleBuffered = true;
            ShowInTaskbar = true;
            MaximizeBox = false;
            MinimizeBox = false;

            _progress = new ThinProgressBar
            {
                Location = new Point(44, 196),
                Width = 432,
                Height = 6,
                Value = 0
            };
            Controls.Add(_progress);

            _retry = new UiButton
            {
                Text = "重试",
                Style = UiButtonStyle.Primary,
                Location = new Point(300, 244),
                Width = 82,
                Height = 36,
                Visible = false
            };
            _retry.Click += delegate { StartPrepare(); };
            Controls.Add(_retry);

            _exit = new UiButton
            {
                Text = "退出",
                Style = UiButtonStyle.Ghost,
                Location = new Point(392, 244),
                Width = 82,
                Height = 36,
                Visible = false
            };
            _exit.Click += delegate
            {
                _allowClose = true;
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(_exit);

            Shown += delegate { StartPrepare(); };
            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (_busy && !_allowClose) e.Cancel = true;
            };
            MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
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

        private async void StartPrepare()
        {
            if (_busy) return;
            _busy = true;
            _error = null;
            _status = "正在准备播放引擎…";
            _retry.Visible = false;
            _exit.Visible = false;
            _progress.Value = 0;
            Invalidate();

            Progress<RuntimeProgress> progress = new Progress<RuntimeProgress>(delegate(RuntimeProgress value)
            {
                _progress.Value = value.Percent;
                _status = value.Message;
                Invalidate();
            });

            string error = null;
            bool ok = await Task.Run(delegate
            {
                return Runtime.Prepare(progress, out error);
            });

            _busy = false;
            if (ok)
            {
                Runtime.DeleteOldPayloads();
                _status = "准备完成，正在启动…";
                _progress.Value = 100;
                Invalidate();
                await Task.Delay(260);
                _allowClose = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _error = string.IsNullOrEmpty(error) ? "播放引擎初始化失败。" : error;
                _status = "播放引擎准备失败";
                _retry.Visible = true;
                _exit.Visible = true;
                _retry.BringToFront();
                _exit.BringToFront();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Theme.Background);

            Theme.DrawGlowEllipse(g, new RectangleF(-120, -160, 420, 360), Theme.Accent);
            Theme.DrawGlowEllipse(g, new RectangleF(300, 130, 360, 300), Theme.Purple);

            RectangleF card = new RectangleF(1, 1, ClientSize.Width - 2, ClientSize.Height - 2);
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(248, 18, 22, 30)))
                Theme.FillRounded(g, brush, card, 16f);
            using (Pen pen = new Pen(Theme.Border, 1f))
                Theme.DrawRounded(g, pen, card, 16f);

            RectangleF logoRect = new RectangleF(44, 40, 56, 56);
            using (LinearGradientBrush brush = new LinearGradientBrush(logoRect, Theme.Accent, Theme.Purple, 45f))
            using (GraphicsPath path = Theme.RoundedRect(logoRect, 15f))
                g.FillPath(brush, path);
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                float cx = 72;
                float cy = 68;
                g.FillPolygon(brush, new PointF[]
                {
                    new PointF(cx - 6, cy - 12), new PointF(cx - 6, cy + 12), new PointF(cx + 14, cy)
                });
            }

            TextRenderer.DrawText(g, "DeskFlow", Theme.H1, new Rectangle(116, 40, 340, 34), Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "动态壁纸引擎", Theme.Small, new Rectangle(118, 72, 340, 20), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, "首次启动需要释放播放组件", Theme.BodyBold, new Rectangle(44, 124, 430, 24), Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "只会进行一次，约需数秒。之后启动无需等待。", Theme.Small,
                new Rectangle(44, 148, 430, 22), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, _status, Theme.Small, new Rectangle(44, 208, 432, 22),
                _error == null ? Theme.TextMuted : Theme.Danger,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            if (_error != null)
            {
                TextRenderer.DrawText(g, Theme.Ellipsis(_error, Theme.Tiny, 420), Theme.Tiny,
                    new Rectangle(44, 228, 420, 18), Theme.TextDim,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }

            TextRenderer.DrawText(g, "VLC 3.0.21  ·  libwebp 1.6.0", Theme.Tiny,
                new Rectangle(44, 272, 240, 18), Theme.TextDim,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
