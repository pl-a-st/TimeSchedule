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

        /// <summary>Рисует двухпозиционный переключатель с масштабированием пиктограмм по DPI.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent != null ? Parent.BackColor : palette.Window);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float scale = e.Graphics.DpiX / 96f;
            int sideInset = Math.Max(1, (int)Math.Round(5f * scale));
            int trackHeight = Math.Min(Height, Math.Max(2, (int)Math.Round(24f * scale)));
            Rectangle track = new Rectangle(sideInset, (Height - trackHeight) / 2,
                Math.Max(1, Width - 2 * sideInset), trackHeight);
            using (GraphicsPath trackPath = Capsule(track))
            using (SolidBrush background = new SolidBrush(hovered ? palette.ButtonHover : palette.Button))
            using (Pen outline = new Pen(Color.FromArgb(125, palette.Grid), scale))
            {
                e.Graphics.FillPath(background, trackPath);
                e.Graphics.DrawPath(outline, trackPath);
            }

            bool isDark = ThemeManager.Current == ThemeKind.Dark;
            int thumbInset = Math.Max(1, (int)Math.Round(2f * scale));
            int thumbSize = Math.Max(1, Math.Min(track.Height - 2 * thumbInset, (int)Math.Round(20f * scale)));
            Rectangle thumb = new Rectangle(isDark ? track.Right - thumbSize - thumbInset : track.Left + thumbInset,
                track.Top + thumbInset, thumbSize, thumbSize);
            using (GraphicsPath thumbPath = Capsule(thumb))
            using (SolidBrush thumbBrush = new SolidBrush(palette.Accent))
            {
                e.Graphics.FillPath(thumbBrush, thumbPath);
            }

            float cx = thumb.Left + thumb.Width / 2f;
            float cy = thumb.Top + thumb.Height / 2f;
            if (isDark) DrawMoon(e.Graphics, cx, cy, palette.Text, scale);
            else DrawSun(e.Graphics, cx, cy, palette.Text, scale);

            if (Focused && ShowFocusCues)
            {
                int inset = Math.Max(1, (int)Math.Round(scale));
                using (GraphicsPath focusPath = Capsule(Rectangle.Inflate(track, inset, inset)))
                using (Pen focusPen = new Pen(palette.Focus, scale))
                    e.Graphics.DrawPath(focusPen, focusPath);
            }
        }

        /// <summary>Рисует солнечную пиктограмму с масштабированием размера и толщины линий.</summary>
        private static void DrawSun(Graphics graphics, float cx, float cy, Color color, float scale)
        {
            using (Pen pen = new Pen(color, 1.3f * scale))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                graphics.DrawEllipse(pen, cx - 2.8f * scale, cy - 2.8f * scale,
                    5.6f * scale, 5.6f * scale);
                for (int i = 0; i < 8; i++)
                {
                    double angle = i * Math.PI / 4;
                    float dx = (float)Math.Cos(angle);
                    float dy = (float)Math.Sin(angle);
                    graphics.DrawLine(pen, cx + 4.8f * scale * dx, cy + 4.8f * scale * dy,
                        cx + 6.9f * scale * dx, cy + 6.9f * scale * dy);
                }
            }
        }

        /// <summary>Рисует полумесяц с сохранением пропорций при изменении DPI.</summary>
        private static void DrawMoon(Graphics graphics, float cx, float cy, Color color, float scale)
        {
            using (GraphicsPath moon = new GraphicsPath())
            using (SolidBrush brush = new SolidBrush(color))
            {
                moon.StartFigure();
                moon.AddBezier(cx + 2.9f * scale, cy - 5.4f * scale,
                    cx - 4.3f * scale, cy - 6.4f * scale,
                    cx - 6.4f * scale, cy + 4.7f * scale,
                    cx + 1.8f * scale, cy + 5.5f * scale);
                moon.AddBezier(cx + 1.8f * scale, cy + 5.5f * scale,
                    cx - 1.6f * scale, cy + 3.5f * scale,
                    cx - 1.6f * scale, cy - 2.1f * scale,
                    cx + 2.9f * scale, cy - 5.4f * scale);
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
