using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Плоская матовая кнопка с единым оформлением для обеих тем.</summary>
    internal sealed class StyledButton : Button
    {
        /// <summary>Использовать акцентный фон вместо обычного.</summary>
        internal bool IsAccent { get; set; }
        /// <summary>Радиус закругления кнопки в логических пикселях.</summary>
        internal int CornerRadius { get; set; } = 4;
        /// <summary>Признак наведения указателя.</summary>
        private bool hovered;
        /// <summary>Признак удержания кнопки мышью.</summary>
        private bool pressed;

        /// <summary>Настраивает буферизацию и подписку на изменения палитры.</summary>
        internal StyledButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            TabStop = true;
            Font = Control.DefaultFont;
            ThemeManager.Changed += ThemeChanged;
        }

        /// <summary>Освобождает подписку на глобальную тему вместе с контролом.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) ThemeManager.Changed -= ThemeChanged;
            base.Dispose(disposing);
        }

        /// <summary>Перерисовывает кнопку при смене темы.</summary>
        private void ThemeChanged(object sender, EventArgs e)
        {
            if (!IsDisposed) Invalidate();
        }

        /// <summary>Обновляет состояние указателя для рисования.</summary>
        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        /// <summary>Обновляет состояние указателя для рисования.</summary>
        protected override void OnMouseLeave(EventArgs e) { hovered = false; pressed = false; Invalidate(); base.OnMouseLeave(e); }
        /// <summary>Обновляет состояние нажатия.</summary>
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) pressed = true; Invalidate(); base.OnMouseDown(e); }
        /// <summary>Обновляет состояние отпускания.</summary>
        protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
        /// <summary>Отображает отключённое состояние и фокус без изменения семантики Button.</summary>
        protected override void OnEnabledChanged(EventArgs e) { pressed = false; Invalidate(); base.OnEnabledChanged(e); }
        /// <summary>Перерисовывает клавиатурный фокус.</summary>
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        /// <summary>Перерисовывает клавиатурный фокус.</summary>
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        /// <summary>Рисует плоскую матовую поверхность, подпись и индикатор фокуса.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette colors = ThemeManager.Palette;
            Color fill = !Enabled ? colors.ButtonDisabled : pressed ? colors.ButtonPressed :
                hovered ? colors.ButtonHover : IsAccent ? colors.Accent : colors.Button;
            Color text = Enabled ? colors.Text : colors.MutedText;
            Rectangle bounds = new Rectangle(0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
            if (bounds.Width == 0 || bounds.Height == 0) return;
            DrawSurface(e.Graphics, bounds, Parent != null ? Parent.BackColor : colors.Window,
                fill, text, Text, Font, CornerRadius, Focused && ShowFocusCues, colors.Focus);
        }

        /// <summary>Рисует общую матовую поверхность кнопок, сохраняя совместимость с ButtonForShedule.</summary>
        /// <param name="graphics">Поверхность рисования.</param>
        /// <param name="bounds">Границы кнопки.</param>
        /// <param name="background">Цвет фона родительской поверхности.</param>
        /// <param name="fill">Цвет кнопки.</param>
        /// <param name="foreground">Цвет текста.</param>
        /// <param name="caption">Текст кнопки.</param>
        /// <param name="font">Шрифт подписи.</param>
        /// <param name="radius">Радиус скругления.</param>
        /// <param name="focused">Наличие клавиатурного фокуса.</param>
        /// <param name="focus">Цвет индикатора фокуса.</param>
        internal static void DrawSurface(Graphics graphics, Rectangle bounds, Color background,
            Color fill, Color foreground, string caption, Font font, int radius, bool focused, Color focus)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            graphics.Clear(background);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (GraphicsPath path = RoundedRectangle(bounds, Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2)))
            using (SolidBrush brush = new SolidBrush(fill))
                graphics.FillPath(brush, path);
            TextFormatFlags textFlags = TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            if (string.IsNullOrEmpty(caption) || caption.IndexOf('\n') < 0)
                textFlags |= TextFormatFlags.SingleLine;
            TextRenderer.DrawText(graphics, caption, font, bounds, foreground, textFlags);
            if (focused)
            {
                Rectangle focusBounds = Rectangle.Inflate(bounds, -3, -3);
                if (focusBounds.Width > 0 && focusBounds.Height > 0)
                    ControlPaint.DrawFocusRectangle(graphics, focusBounds, focus, fill);
            }
        }

        /// <summary>Создаёт границу скруглённого прямоугольника с ограниченным радиусом.</summary>
        private static GraphicsPath RoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); return path; }
            int diameter = radius * 2;
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
