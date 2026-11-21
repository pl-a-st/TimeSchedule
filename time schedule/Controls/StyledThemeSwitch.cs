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

        /// <summary>Рисует единый переключатель с движущимся ползунком вместо двух кнопок.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent != null ? Parent.BackColor : palette.Window);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle track = new Rectangle(5, (Height - 24) / 2, Width - 10, 24);
            using (GraphicsPath trackPath = Capsule(track))
            using (SolidBrush background = new SolidBrush(hovered ? palette.ButtonHover : palette.Button))
            using (Pen outline = new Pen(Color.FromArgb(125, palette.Grid), 1f))
            {
                e.Graphics.FillPath(background, trackPath);
                e.Graphics.DrawPath(outline, trackPath);
            }

            bool isDark = ThemeManager.Current == ThemeKind.Dark;
            const int thumbSize = 20;
            Rectangle thumb = new Rectangle(isDark ? track.Right - thumbSize - 2 : track.Left + 2,
                track.Top + 2, thumbSize, thumbSize);
            using (GraphicsPath thumbPath = Capsule(thumb))
            using (SolidBrush thumbBrush = new SolidBrush(palette.Accent))
            {
                e.Graphics.FillPath(thumbBrush, thumbPath);
            }

            float cx = thumb.Left + thumb.Width / 2f;
            float cy = thumb.Top + thumb.Height / 2f;
            if (isDark) DrawMoon(e.Graphics, cx, cy, palette.Text);
            else DrawSun(e.Graphics, cx, cy, palette.Text);

            if (Focused && ShowFocusCues)
            {
                using (GraphicsPath focusPath = Capsule(Rectangle.Inflate(track, 1, 1)))
                using (Pen focusPen = new Pen(palette.Focus, 1f))
                    e.Graphics.DrawPath(focusPen, focusPath);
            }
        }

        /// <summary>Рисует солнце внутри ползунка светлой темы.</summary>
        private static void DrawSun(Graphics graphics, float cx, float cy, Color color)
        {
            using (Pen pen = new Pen(color, 1.3f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                graphics.DrawEllipse(pen, cx - 2.8f, cy - 2.8f, 5.6f, 5.6f);
                for (int i = 0; i < 8; i++)
                {
                    double angle = i * Math.PI / 4;
                    float dx = (float)Math.Cos(angle);
                    float dy = (float)Math.Sin(angle);
                    graphics.DrawLine(pen, cx + 4.8f * dx, cy + 4.8f * dy,
                        cx + 6.9f * dx, cy + 6.9f * dy);
                }
            }
        }

        /// <summary>Рисует цельный контур полумесяца без пересечения дуг в верхней части.</summary>
        private static void DrawMoon(Graphics graphics, float cx, float cy, Color color)
        {
            using (GraphicsPath moon = new GraphicsPath())
            using (SolidBrush brush = new SolidBrush(color))
            {
                moon.StartFigure();
                moon.AddBezier(cx + 2.9f, cy - 5.4f, cx - 4.3f, cy - 6.4f,
                    cx - 6.4f, cy + 4.7f, cx + 1.8f, cy + 5.5f);
                moon.AddBezier(cx + 1.8f, cy + 5.5f, cx - 1.6f, cy + 3.5f,
                    cx - 1.6f, cy - 2.1f, cx + 2.9f, cy - 5.4f);
                moon.CloseFigure();
                graphics.FillPath(brush, moon);
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
