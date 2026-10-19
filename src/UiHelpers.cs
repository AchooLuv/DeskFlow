using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeskFlow
{
    internal abstract class PageBase : UserControl
    {
        protected const int ContentMargin = 32;
        protected const int ContentTop = 102;

        public string HeaderTitle { get; set; }
        public string HeaderSubtitle { get; set; }

        protected PageBase()
        {
            HeaderTitle = string.Empty;
            HeaderSubtitle = string.Empty;
            Dock = DockStyle.Fill;
            BackColor = Theme.Background;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Background);
            TextRenderer.DrawText(g, HeaderTitle, Theme.H1, new Rectangle(ContentMargin, 8, Width - ContentMargin * 2, 48),
                Theme.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, HeaderSubtitle, Theme.Small, new Rectangle(ContentMargin, 58, Width - ContentMargin * 2, 22),
                Theme.TextMuted, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        public void RefreshLayout()
        {
            LayoutPage();
            Invalidate();
        }

        protected abstract void LayoutPage();
    }

    internal static class Ui
    {
        public static Label Label(string text, Font font, Color color, Color background)
        {
            Label label = new Label();
            label.Text = text ?? string.Empty;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = background;
            label.AutoSize = false;
            label.UseCompatibleTextRendering = false;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        public static RoundedPanel Card(Color background)
        {
            RoundedPanel panel = new RoundedPanel();
            panel.FillColor = Theme.Card;
            panel.BorderColor = Theme.Border;
            panel.BackColor = background;
            panel.Radius = 12f;
            return panel;
        }

        public static string FormatDuration(long milliseconds)
        {
            if (milliseconds <= 0) return "00:00";
            TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);
            if (time.TotalHours >= 1) return ((int)time.TotalHours).ToString("00") + ":" + time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
            return time.Minutes.ToString("00") + ":" + time.Seconds.ToString("00");
        }

        public static string FormatSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.0") + " KB";
            if (bytes < 1024L * 1024 * 1024) return (bytes / 1024.0 / 1024.0).ToString("0.0") + " MB";
            return (bytes / 1024.0 / 1024.0 / 1024.0).ToString("0.00") + " GB";
        }
    }
}
