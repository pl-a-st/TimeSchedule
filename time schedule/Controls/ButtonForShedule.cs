using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    internal class ButtonForShedule : Button
    {
        /// <summary>Использовать общий матовый стиль вместо штатной отрисовки существующей кнопки.</summary>
        internal bool UseTheme { get; set; }
        /// <summary>Выделять кнопку акцентным цветом.</summary>
        internal bool ThemeAccent { get; set; }
        /// <summary>Рисовать прямоугольную кнопку в табличной области без скругления.</summary>
        internal bool ThemeSquareCorners { get; set; }
        /// <summary>Текущее наведение указателя в режиме оформления.</summary>
        private bool themeHover;
        /// <summary>Состояние нажатия в режиме оформления.</summary>
        private bool themePressed;

        /// <summary>Отслеживает наведение для оформления только при явном подключении темы.</summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            themeHover = true;
            if (UseTheme) Invalidate();
            base.OnMouseEnter(e);
        }

        /// <summary>Возвращает кнопке обычное состояние при уходе указателя.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            themeHover = false;
            themePressed = false;
            if (UseTheme) Invalidate();
            base.OnMouseLeave(e);
        }

        /// <summary>Отслеживает нажатие без изменения обработки Click.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) themePressed = true;
            if (UseTheme) Invalidate();
            base.OnMouseDown(e);
        }

        /// <summary>Сбрасывает состояние нажатия и сохраняет штатное событие Click.</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            themePressed = false;
            if (UseTheme) Invalidate();
            base.OnMouseUp(e);
        }

        /// <summary>Отрисовывает утверждённую кнопку только для явно подключённых форм.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!UseTheme) { base.OnPaint(e); return; }
            ThemePalette palette = ThemeManager.Palette;
            Color fill = !Enabled ? palette.ButtonDisabled : themePressed ? palette.ButtonPressed :
                themeHover ? palette.ButtonHover : ThemeAccent ? palette.Accent : palette.Button;
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            StyledButton.DrawSurface(e.Graphics, bounds,
                Parent != null ? Parent.BackColor : palette.Window, fill,
                Enabled ? palette.Text : palette.MutedText, Text, Font,
                ThemeSquareCorners ? 0 : 4, Focused && ShowFocusCues, palette.Focus);
        }
        
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            //this.BackColor = Color.FromArgb(90, 90, 90);
            //this.ForeColor = Color.FromArgb(240, 240, 240);
            if (IsParentsEnabled(this))
            {
                this.BackColor = Color.FromArgb(80, 80, 85);
                this.ForeColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                this.BackColor = Color.FromArgb(180, 180, 180);
                this.ForeColor = Color.FromArgb(250, 250, 250);   
            }
        }
        private bool IsParentsEnabled(Control control)
        {
            if (control.Enabled)
            {
                if (control.Parent != null)
                {
                    return IsParentsEnabled(control.Parent);
                }
                return true;
            }
            return false;
        }
        protected override void OnParentEnabledChanged(EventArgs e)
        {
            if (this.Parent.Enabled)
            {
                this.BackColor = Color.FromArgb(90, 90, 90);
                this.ForeColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                this.BackColor = Color.FromArgb(180, 180, 180);
                this.ForeColor = Color.FromArgb(250, 250, 250);
            }
            base.OnParentEnabledChanged(e);
        }
        protected override void OnEnabledChanged(EventArgs e)
        {
            if (this.Enabled)
            {
                this.BackColor = Color.FromArgb(90, 90, 90);
                this.ForeColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                this.BackColor = Color.FromArgb(180, 180, 180);
                this.ForeColor = Color.FromArgb(250, 250, 250);
            }
            base.OnEnabledChanged(e);
        }

    }
}
