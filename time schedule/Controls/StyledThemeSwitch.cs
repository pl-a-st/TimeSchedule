using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Единый двухпозиционный переключатель светлой и тёмной темы.</summary>
    internal sealed class StyledThemeSwitch : Control
    {
        /// <summary>Отслеживает наведение для компактной обратной связи.</summary>
        private bool hovered;

        /// <summary>Создаёт доступный клавиатуре переключатель с общей палитрой.</summary>
        internal StyledThemeSwitch()
        {
            Size = new Size(76, 40);
            Cursor = Cursors.Hand;
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
            ThemeManager.Changed += ThemeChanged;
            UpdateAccessibility();
        }

        /// <summary>Переключает тему только при прямом действии пользователя.</summary>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ThemeKind next = ThemeManager.Current == ThemeKind.Light ? ThemeKind.Dark : ThemeKind.Light;
            ThemeManager.SetTheme(next);
            ThemePreferenceStore.Save(next);
        }

        /// <summary>Разрешает переключение с клавиатуры клавишами пробел и Enter.</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            base.OnKeyDown(e);
        }

        /// <summary>Перерисовывает переключатель при наведении.</summary>
        protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
        /// <summary>Перерисовывает переключатель при уходе указателя.</summary>
        protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }
        /// <summary>Перерисовывает элемент при получении фокуса.</summary>
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        /// <summary>Перерисовывает элемент при потере фокуса.</summary>
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        /// <summary>Отображает обе темы отдельными матовыми сегментами с постоянными пиктограммами.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent != null ? Parent.BackColor : palette.Window);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle track = new Rectangle(3, (Height - 26) / 2, Width - 6, 26);
            using (GraphicsPath path = Capsule(track))
            using (SolidBrush brush = new SolidBrush(hovered ? palette.ButtonHover : palette.Surface))
                e.Graphics.FillPath(brush, path);

            int segmentWidth = (track.Width - 7) / 2;
            Rectangle light = new Rectangle(track.Left + 2, track.Top + 2, segmentWidth, track.Height - 4);
            Rectangle dark = new Rectangle(track.Right - segmentWidth - 2, track.Top + 2,
                segmentWidth, track.Height - 4);
            bool darkSelected = ThemeManager.Current == ThemeKind.Dark;
            DrawSegment(e.Graphics, light, !darkSelected, palette, true);
            DrawSegment(e.Graphics, dark, darkSelected, palette, false);

            if (Focused && ShowFocusCues)
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(track, 0, 2));
        }

        /// <summary>Рисует общую поверхность и одну из пиктограмм, не добавляя новой палитры.</summary>
        private static void DrawSegment(Graphics graphics, Rectangle bounds, bool selected,
            ThemePalette palette, bool sun)
        {
            using (GraphicsPath path = Capsule(bounds))
            using (SolidBrush brush = new SolidBrush(selected ? palette.Accent : palette.Button))
                graphics.FillPath(brush, path);

            Color icon = selected ? palette.Text : palette.MutedText;
            float x = bounds.Left + bounds.Width / 2f;
            float y = bounds.Top + bounds.Height / 2f;
            using (Pen pen = new Pen(icon, 1.4f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                if (sun)
                {
                    graphics.DrawEllipse(pen, x - 3.3f, y - 3.3f, 6.6f, 6.6f);
                    for (int i = 0; i < 8; i++)
                    {
                        double angle = i * Math.PI / 4;
                        float dx = (float)Math.Cos(angle);
                        float dy = (float)Math.Sin(angle);
                        graphics.DrawLine(pen, x + 5.4f * dx, y + 5.4f * dy,
                            x + 7.7f * dx, y + 7.7f * dy);
                    }
                }
                else
                {
                    graphics.DrawArc(pen, x - 6, y - 6, 12, 12, 47, 263);
                    graphics.DrawArc(pen, x - 1, y - 7, 9, 11, 76, 180);
                }
            }
        }

        /// <summary>Создаёт скруглённую матовую поверхность одного сегмента.</summary>
        private static GraphicsPath Capsule(Rectangle rect)
        {
            var path = new GraphicsPath();
            int diameter = rect.Height;
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 90, 180);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 180);
            path.CloseFigure();
            return path;
        }

        /// <summary>Синхронизирует доступное название с текущей темой.</summary>
        private void UpdateAccessibility()
        {
            AccessibleName = ThemeManager.Current == ThemeKind.Dark ?
                "Переключить на светлую тему" : "Переключить на тёмную тему";
            AccessibleRole = AccessibleRole.PushButton;
        }

        /// <summary>Реагирует на централизованную смену темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { UpdateAccessibility(); Invalidate(); }

        /// <summary>Освобождает подписку при уничтожении контрола.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) ThemeManager.Changed -= ThemeChanged;
            base.Dispose(disposing);
        }
    }
}
