using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Группирует штатные элементы WinForms, рисуя спокойную рамку и заголовок
    /// одинаково в светлой и тёмной темах без изменения размеров и работы дочерних контролов.
    /// </summary>
    public sealed class StyledGroupBox : GroupBox
    {
        /// <summary>Доля цвета сетки в приглушённом контуре группы.</summary>
        private const float BorderMix = 0.72f;

        /// <summary>Включает буферизацию и собственную отрисовку контейнера.</summary>
        public StyledGroupBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>Обновляет подпись при изменении текста группы.</summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        /// <summary>
        /// Рисует скруглённый контур с разрывом под заголовок, используя ту же геометрию
        /// и не перекрывая дочерние поля, радиокнопки и списки.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            Color background = Parent != null ? Parent.BackColor : palette.Window;
            e.Graphics.Clear(background);
            if (ClientSize.Width <= 2 || ClientSize.Height <= 2) return;

            int top = Math.Max(1, Font.Height / 2);
            int right = ClientSize.Width - 1;
            int bottom = ClientSize.Height - 1;
            string caption = Text ?? string.Empty;
            Color frame = Blend(palette.Grid, background, BorderMix);
            SmoothingMode previous = e.Graphics.SmoothingMode;
            try
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = ThemeVisuals.InputPath(
                    Rectangle.FromLTRB(0, top, right, bottom)))
                using (Pen pen = new Pen(frame))
                    e.Graphics.DrawPath(pen, path);
            }
            finally { e.Graphics.SmoothingMode = previous; }

            if (caption.Length == 0) return;
            int available = Math.Max(1, right - 18);
            int labelWidth = TextRenderer.MeasureText(e.Graphics, caption, Font,
                new Size(available, Font.Height + 2),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix).Width;
            labelWidth = Math.Min(available, labelWidth);
            using (Brush cover = new SolidBrush(background))
                e.Graphics.FillRectangle(cover, 7, top - 1, Math.Max(1, Math.Min(right - 9, labelWidth + 8)), 3);
            Rectangle text = new Rectangle(9, 0, available, Font.Height + 2);
            TextRenderer.DrawText(e.Graphics, caption, Font, text,
                Enabled ? palette.Text : palette.MutedText,
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
                TextFormatFlags.VerticalCenter);
        }

        /// <summary>Смешивает цвета для менее контрастного контура группы.</summary>
        /// <param name="line">Цвет структурных линий темы.</param>
        /// <param name="background">Цвет поверхности контейнера.</param>
        /// <param name="weight">Доля структурного цвета от 0 до 1.</param>
        private static Color Blend(Color line, Color background, float weight)
        {
            return Color.FromArgb(
                (int)Math.Round(line.R * weight + background.R * (1f - weight)),
                (int)Math.Round(line.G * weight + background.G * (1f - weight)),
                (int)Math.Round(line.B * weight + background.B * (1f - weight)));
        }
    }
}
