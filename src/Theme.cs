using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DeskFlow
{
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(12, 14, 19);
        public static readonly Color Sidebar = Color.FromArgb(15, 18, 24);
        public static readonly Color Card = Color.FromArgb(22, 26, 35);
        public static readonly Color CardHover = Color.FromArgb(28, 33, 44);
        public static readonly Color CardSoft = Color.FromArgb(18, 22, 30);
        public static readonly Color Border = Color.FromArgb(36, 42, 54);
        public static readonly Color BorderSoft = Color.FromArgb(29, 34, 44);
        public static readonly Color Text = Color.FromArgb(242, 245, 250);
        public static readonly Color TextMuted = Color.FromArgb(141, 150, 168);
        public static readonly Color TextDim = Color.FromArgb(96, 105, 122);
        public static readonly Color Accent = Color.FromArgb(86, 139, 255);
        public static readonly Color AccentHover = Color.FromArgb(110, 158, 255);
        public static readonly Color AccentSoft = Color.FromArgb(30, 45, 78);
        public static readonly Color Purple = Color.FromArgb(145, 112, 255);
        public static readonly Color Success = Color.FromArgb(65, 209, 154);
        public static readonly Color Warning = Color.FromArgb(242, 184, 75);
        public static readonly Color Danger = Color.FromArgb(255, 107, 107);
        public static readonly Color Black = Color.FromArgb(8, 10, 14);

        public static readonly string UiFamily;
        public static readonly string IconFamily;

        public static readonly Font H1;
        public static readonly Font H2;
        public static readonly Font H3;
        public static readonly Font Body;
        public static readonly Font BodyBold;
        public static readonly Font Small;
        public static readonly Font SmallBold;
        public static readonly Font Tiny;
        public static readonly Font Tiny2;
        public static readonly Font ButtonFont;
        public static readonly Font NavFont;
        public static readonly Font Icon12;
        public static readonly Font Icon16;
        public static readonly Font Icon24;

        static Theme()
        {
            UiFamily = PickFamily("Microsoft YaHei UI", "Microsoft YaHei", "Segoe UI");
            IconFamily = PickFamily("Segoe MDL2 Assets", "Segoe UI Symbol", "Segoe UI");

            H1 = new Font(UiFamily, 20f, FontStyle.Bold, GraphicsUnit.Point);
            H2 = new Font(UiFamily, 13.5f, FontStyle.Bold, GraphicsUnit.Point);
            H3 = new Font(UiFamily, 11f, FontStyle.Bold, GraphicsUnit.Point);
            Body = new Font(UiFamily, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            BodyBold = new Font(UiFamily, 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            Small = new Font(UiFamily, 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            SmallBold = new Font(UiFamily, 8.5f, FontStyle.Bold, GraphicsUnit.Point);
            Tiny = new Font(UiFamily, 7.8f, FontStyle.Regular, GraphicsUnit.Point);
            Tiny2 = new Font(UiFamily, 7.2f, FontStyle.Regular, GraphicsUnit.Point);
            ButtonFont = new Font(UiFamily, 9.2f, FontStyle.Regular, GraphicsUnit.Point);
            NavFont = new Font(UiFamily, 10f, FontStyle.Regular, GraphicsUnit.Point);
            Icon12 = new Font(IconFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
            Icon16 = new Font(IconFamily, 13f, FontStyle.Regular, GraphicsUnit.Point);
            Icon24 = new Font(IconFamily, 20f, FontStyle.Regular, GraphicsUnit.Point);
        }

        private static string PickFamily(params string[] names)
        {
            try
            {
                using (InstalledFontCollection fonts = new InstalledFontCollection())
                {
                    foreach (string wanted in names)
                    {
                        foreach (FontFamily family in fonts.Families)
                        {
                            if (string.Equals(family.Name, wanted, StringComparison.OrdinalIgnoreCase))
                                return family.Name;
                        }
                    }
                }
            }
            catch { }
            return "Segoe UI";
        }

        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = Math.Max(1f, radius * 2f);
            if (radius <= 0.1f)
            {
                path.AddRectangle(rect);
                return path;
            }
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            path.AddArc(rect.X, rect.Y, d, d, 180f, 90f);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270f, 90f);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0f, 90f);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
                g.FillPath(brush, path);
        }

        public static void DrawRounded(Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
                g.DrawPath(pen, path);
        }

        public static string Ellipsis(string text, Font font, int maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width <= maxWidth)
                return text;

            int low = 0;
            int high = text.Length;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                string candidate = text.Substring(0, mid).TrimEnd() + "…";
                if (TextRenderer.MeasureText(candidate, font, new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.NoPadding).Width <= maxWidth)
                    low = mid;
                else
                    high = mid - 1;
            }
            if (low <= 0) return "…";
            return text.Substring(0, low).TrimEnd() + "…";
        }

        public static Color Blend(Color a, Color b, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * amount),
                (int)(a.R + (b.R - a.R) * amount),
                (int)(a.G + (b.G - a.G) * amount),
                (int)(a.B + (b.B - a.B) * amount));
        }

        public static GraphicsPath Circle(RectangleF rect)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(rect);
            return path;
        }

        public static void DrawGlowEllipse(Graphics g, RectangleF rect, Color color)
        {
            using (GraphicsPath path = Circle(rect))
            using (PathGradientBrush brush = new PathGradientBrush(path))
            {
                brush.CenterColor = Color.FromArgb(110, color);
                brush.SurroundColors = new Color[] { Color.FromArgb(0, color) };
                g.FillEllipse(brush, rect);
            }
        }
    }
}
