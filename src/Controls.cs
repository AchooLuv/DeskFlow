using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeskFlow
{
    internal class RoundedPanel : Panel
    {
        public float Radius { get; set; }
        public Color FillColor { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public RoundedPanel()
        {
            Radius = 12f;
            FillColor = Theme.Card;
            BorderColor = Theme.Border;
            BorderWidth = 1;
            BackColor = Theme.Background;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (SolidBrush brush = new SolidBrush(FillColor))
                Theme.FillRounded(g, brush, rect, Radius);
            if (BorderWidth > 0 && BorderColor.A > 0)
            {
                using (Pen pen = new Pen(BorderColor, BorderWidth))
                    Theme.DrawRounded(g, pen, rect, Radius);
            }
        }
    }

    internal enum UiButtonStyle
    {
        Primary,
        Secondary,
        Ghost,
        Soft,
        Danger
    }

    internal class UiButton : Control
    {
        private bool _hover;
        private bool _pressed;

        public UiButtonStyle Style { get; set; }
        public string Glyph { get; set; }
        public float Radius { get; set; }

        public UiButton()
        {
            Style = UiButtonStyle.Secondary;
            Radius = 8f;
            Height = 38;
            Cursor = Cursors.Hand;
            Font = Theme.ButtonFont;
            ForeColor = Theme.Text;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill;
            Color border;
            Color text;
            GetColors(out fill, out border, out text);

            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (SolidBrush brush = new SolidBrush(fill))
                Theme.FillRounded(g, brush, rect, Radius);
            if (border.A > 0)
            {
                using (Pen pen = new Pen(border, 1f))
                    Theme.DrawRounded(g, pen, rect, Radius);
            }

            bool hasGlyph = !string.IsNullOrEmpty(Glyph);
            int glyphWidth = hasGlyph ? 20 : 0;
            Size textSize = TextRenderer.MeasureText(Text ?? string.Empty, Font,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            int gap = hasGlyph && !string.IsNullOrEmpty(Text) ? 6 : 0;
            int total = glyphWidth + gap + textSize.Width;
            int x = Math.Max(4, (Width - total) / 2);

            if (hasGlyph)
            {
                TextRenderer.DrawText(g, Glyph, Theme.Icon14, new Rectangle(x, 0, glyphWidth, Height), text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                x += glyphWidth + gap;
            }
            if (!string.IsNullOrEmpty(Text))
            {
                TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, textSize.Width + 2, Height), text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        private void GetColors(out Color fill, out Color border, out Color text)
        {
            if (!Enabled)
            {
                fill = Theme.CardSoft;
                border = Theme.BorderSoft;
                text = Theme.TextDim;
                return;
            }

            switch (Style)
            {
                case UiButtonStyle.Primary:
                    fill = _pressed ? Theme.Blend(Theme.Accent, Color.Black, 0.15f) : (_hover ? Theme.AccentHover : Theme.Accent);
                    border = Color.Transparent;
                    text = Color.White;
                    break;
                case UiButtonStyle.Soft:
                    fill = _pressed ? Theme.Blend(Theme.AccentSoft, Color.Black, 0.1f) : (_hover ? Theme.Blend(Theme.AccentSoft, Theme.Accent, 0.12f) : Theme.AccentSoft);
                    border = Color.Transparent;
                    text = Theme.AccentHover;
                    break;
                case UiButtonStyle.Ghost:
                    fill = _pressed ? Theme.CardHover : (_hover ? Theme.Blend(Theme.CardHover, Color.White, 0.025f) : Color.Transparent);
                    border = Color.Transparent;
                    text = _hover ? Theme.Text : Theme.TextMuted;
                    break;
                case UiButtonStyle.Danger:
                    fill = _pressed ? Theme.Blend(Theme.CardHover, Theme.Danger, 0.16f) : (_hover ? Theme.Blend(Theme.CardHover, Theme.Danger, 0.1f) : Theme.CardSoft);
                    border = _hover ? Theme.Blend(Theme.Border, Theme.Danger, 0.35f) : Theme.BorderSoft;
                    text = Theme.Danger;
                    break;
                default:
                    fill = _pressed ? Theme.Card : (_hover ? Theme.CardHover : Theme.CardSoft);
                    border = _hover ? Theme.Blend(Theme.Border, Theme.Accent, 0.28f) : Theme.Border;
                    text = Theme.Text;
                    break;
            }
            if (_pressed && Style == UiButtonStyle.Secondary) fill = Theme.Card;
        }

        public static UiButton Create(string text, string glyph, UiButtonStyle style)
        {
            return new UiButton { Text = text, Glyph = glyph, Style = style };
        }
    }

    internal class IconButton : Control
    {
        private bool _hover;
        private bool _pressed;
        private readonly bool _closeStyle;

        public string Glyph { get; set; }

        public IconButton(bool closeStyle = false)
        {
            _closeStyle = closeStyle;
            Width = 38;
            Height = 32;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = false;
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (_hover)
            {
                Color fill = _closeStyle ? Color.FromArgb(190, 40, 46) : Theme.CardHover;
                if (_pressed) fill = Theme.Blend(fill, Color.Black, 0.15f);
                using (SolidBrush brush = new SolidBrush(fill))
                    Theme.FillRounded(g, brush, new RectangleF(2, 2, Width - 4, Height - 4), 7f);
            }
            Color color = _closeStyle && _hover ? Color.White : (_hover ? Theme.Text : Theme.TextMuted);
            TextRenderer.DrawText(g, Glyph ?? string.Empty, Theme.Icon12, ClientRectangle, color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    internal class NavButton : Control
    {
        private bool _hover;
        private bool _active;

        public string Glyph { get; set; }
        public bool Active
        {
            get { return _active; }
            set { _active = value; Invalidate(); }
        }

        public NavButton()
        {
            Height = 46;
            Cursor = Cursors.Hand;
            Font = Theme.NavFont;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = true;
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            if (_active)
            {
                using (SolidBrush brush = new SolidBrush(Theme.AccentSoft))
                    Theme.FillRounded(g, brush, rect, 9f);
                using (SolidBrush brush = new SolidBrush(Theme.Accent))
                    Theme.FillRounded(g, brush, new RectangleF(0, 12, 3, Height - 24), 1.5f);
            }
            else if (_hover)
            {
                using (SolidBrush brush = new SolidBrush(Theme.Card))
                    Theme.FillRounded(g, brush, rect, 9f);
            }

            Color iconColor = _active ? Theme.AccentHover : (_hover ? Theme.Text : Theme.TextMuted);
            Color textColor = _active ? Theme.Text : (_hover ? Theme.Text : Theme.TextMuted);
            TextRenderer.DrawText(g, Glyph ?? string.Empty, Theme.Icon16, new Rectangle(16, 0, 24, Height), iconColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, Text ?? string.Empty, Font, new Rectangle(50, 0, Width - 60, Height), textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }
    }

    internal class ToggleSwitch : Control
    {
        private bool _checked;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                OnCheckedChanged(EventArgs.Empty);
            }
        }

        public event EventHandler CheckedChanged;

        public ToggleSwitch()
        {
            Width = 44;
            Height = 24;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = true;
            AccessibleRole = AccessibleRole.CheckButton;
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            EventHandler handler = CheckedChanged;
            if (handler != null) handler(this, e);
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) { Checked = !Checked; e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF track = new RectangleF(1, (Height - 24) / 2f, Width - 2, 24);
            using (GraphicsPath path = Theme.RoundedRect(track, 12f))
            using (SolidBrush brush = new SolidBrush(_checked ? Theme.Accent : Color.FromArgb(58, 65, 80)))
                g.FillPath(brush, path);
            float knobX = _checked ? Width - 21 : 3;
            using (SolidBrush brush = new SolidBrush(_checked ? Color.White : Color.FromArgb(174, 182, 198)))
                g.FillEllipse(brush, knobX, (Height - 18) / 2f, 18, 18);
        }
    }

    internal class ModernSlider : Control
    {
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 50;
        private bool _dragging;
        private int _step = 1;

        public int Minimum { get { return _minimum; } set { _minimum = value; Value = _value; Invalidate(); } }
        public int Maximum { get { return _maximum; } set { _maximum = Math.Max(value, _minimum + 1); Value = _value; Invalidate(); } }
        public int Step { get { return _step; } set { _step = Math.Max(1, value); } }
        public int Value
        {
            get { return _value; }
            set
            {
                int clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (clamped == _value) return;
                _value = clamped;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged;

        public ModernSlider()
        {
            Height = 28;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = true;
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler != null) handler(this, e);
        }

        private void SetFromX(int x)
        {
            int left = 8;
            int right = Width - 8;
            float ratio = (float)(Math.Max(left, Math.Min(right, x)) - left) / Math.Max(1, right - left);
            int raw = _minimum + (int)Math.Round(ratio * (_maximum - _minimum));
            int stepped = _minimum + (int)Math.Round((double)(raw - _minimum) / _step) * _step;
            Value = stepped;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _dragging = true; SetFromX(e.X); Focus(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging) SetFromX(e.X);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _dragging = false;
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) { Value -= _step; e.Handled = true; }
            else if (e.KeyCode == Keys.Right) { Value += _step; e.Handled = true; }
            base.OnKeyDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int left = 8;
            int right = Width - 8;
            int cy = Height / 2;
            using (Pen pen = new Pen(Color.FromArgb(52, 59, 73), 4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, left, cy, right, cy);
            }

            float ratio = (float)(_value - _minimum) / Math.Max(1, _maximum - _minimum);
            float x = left + ratio * (right - left);
            using (Pen pen = new Pen(Theme.Accent, 4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, left, cy, x, cy);
            }
            using (SolidBrush brush = new SolidBrush(Color.White))
                g.FillEllipse(brush, x - 7, cy - 7, 14, 14);
            using (Pen pen = new Pen(Theme.Accent, 2f))
                g.DrawEllipse(pen, x - 6, cy - 6, 12, 12);
        }
    }

    internal class Chip : Control
    {
        private bool _selected;
        private bool _hover;

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; Invalidate(); }
        }

        public Chip()
        {
            Height = 28;
            Font = Theme.Small;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            TabStop = false;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            Color fill = _selected ? Theme.AccentSoft : (_hover ? Theme.CardHover : Theme.CardSoft);
            Color border = _selected ? Theme.Blend(Theme.Accent, Theme.Border, 0.45f) : Theme.BorderSoft;
            Color text = _selected ? Theme.AccentHover : Theme.TextMuted;
            using (SolidBrush brush = new SolidBrush(fill))
                Theme.FillRounded(g, brush, rect, 7f);
            using (Pen pen = new Pen(border, 1f))
                Theme.DrawRounded(g, pen, rect, 7f);
            TextRenderer.DrawText(g, Text ?? string.Empty, Font, ClientRectangle, text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    internal class NumberField : Control
    {
        private readonly TextBox _text;
        private int _minimum = 3;
        private int _maximum = 3600;
        private int _step = 1;
        private int _value = 30;
        private bool _suppress;
        private bool _hoverMinus;
        private bool _hoverPlus;

        public int Minimum { get { return _minimum; } set { _minimum = value; Value = _value; } }
        public int Maximum { get { return _maximum; } set { _maximum = Math.Max(value, _minimum + 1); Value = _value; } }
        public int Step { get { return _step; } set { _step = Math.Max(1, value); } }
        public int Value
        {
            get { return _value; }
            set
            {
                int next = Math.Max(_minimum, Math.Min(_maximum, value));
                if (next == _value && _text.Text == next.ToString()) return;
                _value = next;
                _suppress = true;
                _text.Text = _value.ToString();
                _suppress = false;
                Invalidate();
                OnValueChanged(EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged;

        public NumberField()
        {
            Width = 132;
            Height = 38;
            BackColor = Theme.Background;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            _text = new TextBox();
            _text.BorderStyle = BorderStyle.None;
            _text.BackColor = Theme.CardSoft;
            _text.ForeColor = Theme.Text;
            _text.Font = Theme.BodyBold;
            _text.TextAlign = HorizontalAlignment.Center;
            _text.Text = _value.ToString();
            _text.ShortcutsEnabled = true;
            _text.KeyDown += TextKeyDown;
            _text.LostFocus += delegate { CommitText(); };
            _text.GotFocus += delegate { _text.SelectAll(); };
            _text.TextChanged += delegate { if (!_suppress) Invalidate(); };
            Controls.Add(_text);
            LayoutText();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutText();
        }

        private void LayoutText()
        {
            if (_text == null) return;
            int side = 30;
            _text.SetBounds(side + 2, (Height - _text.Height) / 2, Math.Max(20, Width - side * 2 - 4), _text.Height);
        }

        protected virtual void OnValueChanged(EventArgs e)
        {
            EventHandler handler = ValueChanged;
            if (handler != null) handler(this, e);
        }

        private void CommitText()
        {
            int parsed;
            if (int.TryParse(_text.Text.Trim(), out parsed)) Value = parsed;
            else Value = _value;
            _text.Text = _value.ToString();
            Invalidate();
        }

        private void TextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CommitText();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Up) { Value += _step; e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Down) { Value -= _step; e.SuppressKeyPress = true; }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool left = e.X < 30;
            bool right = e.X > Width - 30;
            if (left != _hoverMinus || right != _hoverPlus)
            {
                _hoverMinus = left;
                _hoverPlus = right;
                Cursor = (left || right) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoverMinus = _hoverPlus = false;
            Cursor = Cursors.Default;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.X < 30) Value -= _step;
                else if (e.X > Width - 30) Value += _step;
                else { _text.Focus(); _text.SelectAll(); }
            }
            base.OnMouseDown(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (SolidBrush brush = new SolidBrush(Theme.CardSoft))
                Theme.FillRounded(g, brush, rect, 8f);
            using (Pen pen = new Pen(Theme.Border, 1f))
                Theme.DrawRounded(g, pen, rect, 8f);

            Color minusColor = _hoverMinus ? Theme.Text : Theme.TextMuted;
            Color plusColor = _hoverPlus ? Theme.Text : Theme.TextMuted;
            TextRenderer.DrawText(g, "-", Theme.H2, new Rectangle(0, 0, 30, Height), minusColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "+", Theme.H2, new Rectangle(Width - 30, 0, 30, Height), plusColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            using (Pen pen = new Pen(Theme.BorderSoft, 1f))
            {
                g.DrawLine(pen, 30, 9, 30, Height - 9);
                g.DrawLine(pen, Width - 30, 9, Width - 30, Height - 9);
            }
        }
    }

    internal class ThinProgressBar : Control
    {
        private int _value;

        public int Value
        {
            get { return _value; }
            set { _value = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        public ThinProgressBar()
        {
            Height = 6;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.CardSoft;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(38, 45, 58)))
                Theme.FillRounded(g, brush, new RectangleF(0, 0, Width, Height), Height / 2f);
            float w = Width * (_value / 100f);
            if (w > 0.5f)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(new RectangleF(0, 0, Math.Max(2, w), Height), Theme.Accent, Theme.Purple, 0f))
                    Theme.FillRounded(g, brush, new RectangleF(0, 0, Math.Max(2, w), Height), Height / 2f);
            }
        }
    }

    internal class LogoControl : Control
    {
        public LogoControl()
        {
            Width = 32;
            Height = 32;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            using (LinearGradientBrush brush = new LinearGradientBrush(rect, Theme.Accent, Theme.Purple, 45f))
            using (GraphicsPath path = Theme.RoundedRect(rect, Width * 0.27f))
                g.FillPath(brush, path);

            float cx = Width / 2f;
            float cy = Height / 2f;
            PointF[] triangle = new PointF[]
            {
                new PointF(cx - Width * 0.12f, cy - Height * 0.18f),
                new PointF(cx - Width * 0.12f, cy + Height * 0.18f),
                new PointF(cx + Width * 0.20f, cy)
            };
            using (SolidBrush brush = new SolidBrush(Color.White))
                g.FillPolygon(brush, triangle);
        }
    }

    internal class PreviewBox : Control
    {
        private Image _image;
        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private string _badge = string.Empty;
        private string _glyph = string.Empty;
        private Color _badgeColor = Theme.Success;
        private bool _videoMode;

        public Image PreviewImage
        {
            get { return _image; }
            set { _image = value; Invalidate(); }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value ?? string.Empty; Invalidate(); }
        }

        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value ?? string.Empty; Invalidate(); }
        }

        public string Badge
        {
            get { return _badge; }
            set { _badge = value ?? string.Empty; Invalidate(); }
        }

        public Color BadgeColor
        {
            get { return _badgeColor; }
            set { _badgeColor = value; Invalidate(); }
        }

        public bool VideoMode
        {
            get { return _videoMode; }
            set { _videoMode = value; Invalidate(); }
        }

        public string Glyph
        {
            get { return _glyph; }
            set { _glyph = value ?? string.Empty; Invalidate(); }
        }

        public PreviewBox()
        {
            BackColor = Theme.Background;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF outer = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            using (GraphicsPath clip = Theme.RoundedRect(outer, 12f))
            {
                g.SetClip(clip);
                using (LinearGradientBrush fallback = new LinearGradientBrush(ClientRectangle,
                    Color.FromArgb(28, 36, 54), Color.FromArgb(16, 20, 30), 35f))
                    g.FillRectangle(fallback, ClientRectangle);

                if (_image != null)
                {
                    try
                    {
                        float scale = Math.Max((float)Width / _image.Width, (float)Height / _image.Height);
                        float w = _image.Width * scale;
                        float h = _image.Height * scale;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(_image, (Width - w) / 2f, (Height - h) / 2f, w, h);
                    }
                    catch { }
                }
                else
                {
                    DrawPlaceholder(g);
                }

                using (LinearGradientBrush shade = new LinearGradientBrush(
                    new RectangleF(0, Height * 0.35f, Width, Height * 0.65f),
                    Color.FromArgb(0, 0, 0, 0), Color.FromArgb(210, 5, 7, 11), 90f))
                    g.FillRectangle(shade, 0, Height * 0.35f, Width, Height * 0.65f);
            }

            using (Pen pen = new Pen(Color.FromArgb(255, 52, 60, 78), 1f))
                Theme.DrawRounded(g, pen, outer, 12f);

            if (!string.IsNullOrEmpty(_badge))
            {
                Size badgeText = TextRenderer.MeasureText(_badge, Theme.SmallBold, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                int bw = badgeText.Width + 30;
                RectangleF pill = new RectangleF(14, 14, bw, 26);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, 10, 14, 20)))
                    Theme.FillRounded(g, brush, pill, 13f);
                using (SolidBrush dot = new SolidBrush(_badgeColor))
                    g.FillEllipse(dot, 24, 24, 6, 6);
                TextRenderer.DrawText(g, _badge, Theme.SmallBold, new Rectangle(34, 14, bw - 38, 26), Theme.Text,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
            }

            if (_image == null && _videoMode)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(160, 255, 255, 255)))
                using (GraphicsPath circle = Theme.Circle(new RectangleF(Width / 2f - 32, Height / 2f - 50, 64, 64)))
                    g.FillPath(brush, circle);
                using (SolidBrush brush = new SolidBrush(Theme.Black))
                {
                    float cx = Width / 2f;
                    float cy = Height / 2f - 18;
                    g.FillPolygon(brush, new PointF[]
                    {
                        new PointF(cx - 7, cy - 12), new PointF(cx - 7, cy + 12), new PointF(cx + 13, cy)
                    });
                }
            }

            int textWidth = Width - 32;
            string title = Theme.Ellipsis(_title, Theme.H3, textWidth);
            string subtitle = Theme.Ellipsis(_subtitle, Theme.Small, textWidth);
            int titleY = Height - (string.IsNullOrEmpty(subtitle) ? 38 : 54);
            if (!string.IsNullOrEmpty(title))
                TextRenderer.DrawText(g, title, Theme.H3, new Rectangle(16, titleY, textWidth, 24), Theme.Text,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            if (!string.IsNullOrEmpty(subtitle))
                TextRenderer.DrawText(g, subtitle, Theme.Small, new Rectangle(16, titleY + 24, textWidth, 20), Color.FromArgb(210, 226, 232, 242),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private void DrawPlaceholder(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(24, 255, 255, 255)))
            using (GraphicsPath path = Theme.Circle(new RectangleF(Width / 2f - 28, Height / 2f - 45, 56, 56)))
            {
                g.FillPath(brush, path);
            }
            TextRenderer.DrawText(g, string.IsNullOrEmpty(_glyph) ? "\uE91B" : _glyph, Theme.Icon24,
                new Rectangle(0, (int)(Height / 2f - 45), Width, 56), Theme.AccentHover,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    internal class SettingRow : Control
    {
        private readonly ToggleSwitch _toggle;
        private bool _hover;

        public string Title { get; set; }
        public string Description { get; set; }

        public bool Checked
        {
            get { return _toggle.Checked; }
            set { _toggle.Checked = value; }
        }

        public event EventHandler CheckedChanged;

        public SettingRow()
        {
            Height = 64;
            BackColor = Theme.Card;
            Title = string.Empty;
            Description = string.Empty;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _toggle = new ToggleSwitch();
            _toggle.CheckedChanged += delegate
            {
                Invalidate();
                EventHandler handler = CheckedChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            };
            Controls.Add(_toggle);
            Cursor = Cursors.Hand;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_toggle != null) _toggle.Location = new Point(Width - _toggle.Width - 2, (Height - _toggle.Height) / 2);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            _toggle.Checked = !_toggle.Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_hover)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(14, 255, 255, 255)))
                    g.FillRectangle(brush, ClientRectangle);
            }
            int textWidth = Math.Max(20, Width - _toggle.Width - 24);
            TextRenderer.DrawText(g, Theme.Ellipsis(Title, Theme.BodyBold, textWidth), Theme.BodyBold,
                new Rectangle(2, 10, textWidth, 22), Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, Theme.Ellipsis(Description, Theme.Small, textWidth), Theme.Small,
                new Rectangle(2, 31, textWidth, 22), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    internal class ToastControl : Control
    {
        private readonly Timer _timer;
        private string _text = string.Empty;
        private bool _error;

        public ToastControl()
        {
            Visible = false;
            Height = 38;
            Width = 220;
            BackColor = Theme.Background;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _timer = new Timer { Interval = 2400 };
            _timer.Tick += delegate { _timer.Stop(); Visible = false; };
        }

        public void ShowToast(string text, bool error = false)
        {
            _text = text ?? string.Empty;
            _error = error;
            using (Graphics g = CreateGraphics())
            {
                Size size = TextRenderer.MeasureText(g, _text, Theme.SmallBold, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                Width = size.Width + 42;
            }
            Visible = true;
            BringToFront();
            Invalidate();
            _timer.Stop();
            _timer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF rect = new RectangleF(0.5f, 0.5f, Width - 1, Height - 1);
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(245, 26, 31, 42)))
                Theme.FillRounded(g, brush, rect, 9f);
            using (Pen pen = new Pen(_error ? Theme.Blend(Theme.Danger, Theme.Border, 0.45f) : Theme.Border, 1f))
                Theme.DrawRounded(g, pen, rect, 9f);
            using (SolidBrush dot = new SolidBrush(_error ? Theme.Danger : Theme.Success))
                g.FillEllipse(dot, 14, Height / 2f - 3, 6, 6);
            TextRenderer.DrawText(g, _text, Theme.SmallBold, new Rectangle(26, 0, Width - 32, Height),
                _error ? Theme.Danger : Theme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }
    }
}

