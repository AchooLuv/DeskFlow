using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace DeskFlow
{
    internal sealed class LicenseForm : Form
    {
        private readonly TextBox _text;

        public LicenseForm()
        {
            Text = "开源许可";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(800, 580);
            BackColor = Theme.Background;
            ShowInTaskbar = false;
            KeyPreview = true;

            _text = new TextBox();
            _text.Multiline = true;
            _text.ReadOnly = true;
            _text.ScrollBars = ScrollBars.Both;
            _text.WordWrap = false;
            _text.BorderStyle = BorderStyle.None;
            _text.BackColor = Theme.CardSoft;
            _text.ForeColor = Theme.TextMuted;
            _text.Font = new Font("Consolas", 9f, FontStyle.Regular, GraphicsUnit.Point);
            _text.SetBounds(24, 74, ClientSize.Width - 48, ClientSize.Height - 98);
            Controls.Add(_text);

            IconButton close = new IconButton(true) { Glyph = "\uE8BB", Width = 40, Height = 32 };
            close.Location = new Point(ClientSize.Width - 52, 14);
            close.Click += delegate { Close(); };
            Controls.Add(close);

            KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) Close();
            };
            MouseDown += DragWindow;
            _text.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Escape) Close();
            };
            Shown += delegate { LoadText(); };
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

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
        }

        private void LoadText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DeskFlow 1.0.12");
            builder.AppendLine("Copyright © 2026 AchooLuv");
            builder.AppendLine();
            builder.AppendLine("DeskFlow is free software under the GNU General Public License v3.");
            builder.AppendLine("DeskFlow 以 GNU GPL v3 授权，源代码：https://github.com/AchooLuv/DeskFlow");
            builder.AppendLine();
            builder.AppendLine("============================================================");
            builder.AppendLine();
            builder.AppendLine(ReadResource("DeskFlow.THIRD-PARTY-NOTICES.txt"));
            builder.AppendLine();
            builder.AppendLine("============================================================");
            builder.AppendLine();
            builder.AppendLine(ReadResource("DeskFlow.LICENSE.txt"));
            _text.Text = builder.ToString();
            _text.Select(0, 0);
        }

        private static string ReadResource(string name)
        {
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                {
                    if (stream == null) return "(资源缺失: " + name + ")";
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Theme.Border, 1f))
                g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            TextRenderer.DrawText(g, "开源许可", Theme.H2, new Rectangle(24, 16, 300, 32), Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "DeskFlow · GPL v3 · VLC 3.0.21 · libwebp 1.6.0", Theme.Small,
                new Rectangle(24, 48, 500, 20), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
