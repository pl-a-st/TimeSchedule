using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Матовые кнопки штатного NumericUpDown. Значение, пределы, Increment,
    /// клавиатура и события остаются у исходного контрола.
    /// </summary>
    internal sealed class StyledNumericSpinner : Control
    {
        /// <summary>Оригинальное числовое поле.</summary>
        private readonly NumericUpDown number;
        /// <summary>Таймер повторения штатных команд при длительном нажатии.</summary>
        private readonly Timer repeatTimer;
        /// <summary>Предотвращает рекурсивную компоновку декоративного слоя.</summary>
        private bool synchronizing;
        /// <summary>Направление удерживаемой кнопки: вверх 1, вниз -1, нет 0.</summary>
        private int pressedDirection;
        /// <summary>Направление, на которое наведён указатель.</summary>
        private int hoveredDirection;
        /// <summary>Признак завершения начальной задержки автоповтора.</summary>
        private bool repeating;

        /// <summary>Создаёт стрелки без вмешательства в свойства NumericUpDown.</summary>
        internal StyledNumericSpinner(NumericUpDown target)
        {
            number = target ?? throw new ArgumentNullException(nameof(target));
            TabStop = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            repeatTimer = new Timer { Interval = 420 };
            repeatTimer.Tick += RepeatTick;
            number.SizeChanged += SourceChanged;
            number.LocationChanged += SourceChanged;
            number.ParentChanged += SourceParentChanged;
            number.VisibleChanged += SourceChanged;
            number.EnabledChanged += SourceChanged;
            number.Disposed += SourceDisposed;
            ThemeManager.Changed += ThemeChanged;
            Attach();
        }

        /// <summary>Размещает стрелки поверх штатной кнопочной области поля.</summary>
        private void Attach()
        {
            if (IsDisposed || number.IsDisposed) return;
            if (Parent != null && !ReferenceEquals(Parent, number.Parent)) Parent.Layout -= ParentLayout;
            if (number.Parent == null) { Visible = false; return; }
            if (!ReferenceEquals(Parent, number.Parent))
            {
                number.Parent.Controls.Add(this);
                Parent.Layout += ParentLayout;
            }
            Synchronize();
        }

        /// <summary>Сохраняет координаты исходного поля без изменения его размеров.</summary>
        private void Synchronize()
        {
            if (IsDisposed || number.IsDisposed || number.Parent == null) return;
            if (synchronizing) return;
            synchronizing = true;
            try
            {
                int width = Math.Min(18, number.Width);
                Rectangle desired = new Rectangle(number.Right - width, number.Top, width, number.Height);
                if (Bounds != desired) Bounds = desired;
                Visible = number.Visible;
                Enabled = number.Enabled;
                if (Visible && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Рисует две плоские стрелки и разделитель активной темы.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette colors = ThemeManager.Palette;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Enabled ? colors.Button : colors.ButtonDisabled);
            if (hoveredDirection != 0 && Enabled)
            {
                Rectangle hoverRect = hoveredDirection > 0
                    ? new Rectangle(0, 0, Width, Height / 2)
                    : new Rectangle(0, Height / 2, Width, Height - Height / 2);
                using (SolidBrush brush = new SolidBrush(colors.ButtonHover))
                    e.Graphics.FillRectangle(brush, hoverRect);
            }
            using (Pen pen = new Pen(Enabled ? colors.Grid : ThemeVisuals.InputBorderColor(false, false)))
                e.Graphics.DrawLine(pen, 0, Height / 2, Width, Height / 2);
            Color color = Enabled ? colors.Text : ThemeVisuals.DisabledText;
            DrawArrow(e.Graphics, Width / 2f, Height / 4f, true, color);
            DrawArrow(e.Graphics, Width / 2f, Height * 3f / 4f, false, color);
        }

        /// <summary>Рисует стрелку без зависимости от символов шрифта.</summary>
        private static void DrawArrow(Graphics graphics, float x, float y, bool up, Color color)
        {
            float dy = up ? 1.5f : -1.5f;
            using (Pen pen = new Pen(color, 1.2f))
            {
                graphics.DrawLine(pen, x - 3, y + dy, x, y - dy);
                graphics.DrawLine(pen, x, y - dy, x + 3, y + dy);
            }
        }

        /// <summary>Исполняет только штатные методы числового поля, сохраняя ValueChanged.</summary>
        private void Step(int direction)
        {
            if (!number.Enabled || number.IsDisposed) return;
            if (direction > 0) number.UpButton();
            else if (direction < 0) number.DownButton();
        }

        /// <summary>Реализует однократное изменение и повторение при удержании мыши.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !Enabled) return;
            pressedDirection = e.Y < Height / 2 ? 1 : -1;
            number.Focus();
            Step(pressedDirection);
            repeating = false;
            repeatTimer.Interval = 420;
            repeatTimer.Start();
        }

        /// <summary>Прекращает повтор при отпускании мыши.</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            StopRepeat();
            base.OnMouseUp(e);
        }

        /// <summary>Прекращает повтор при уходе указателя с кнопки.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            StopRepeat();
            hoveredDirection = 0;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <summary>Отображает наведение на верхнюю или нижнюю половину.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int direction = e.Y < Height / 2 ? 1 : -1;
            if (hoveredDirection != direction) { hoveredDirection = direction; Invalidate(); }
        }

        /// <summary>Повторяет штатный шаг со скоростью, близкой к системной.</summary>
        private void RepeatTick(object sender, EventArgs e)
        {
            if (pressedDirection == 0 || !Enabled ||
                (Control.MouseButtons & MouseButtons.Left) == 0) { StopRepeat(); return; }
            if (!repeating) { repeating = true; repeatTimer.Interval = 75; }
            Step(pressedDirection);
        }

        /// <summary>Сбрасывает состояние удержания.</summary>
        private void StopRepeat()
        {
            pressedDirection = 0;
            repeating = false;
            repeatTimer.Stop();
        }

        /// <summary>Синхронизирует размеры после изменения исходного поля.</summary>
        private void SourceChanged(object sender, EventArgs e) { Synchronize(); }
        /// <summary>Переносит декоратор вслед за полем при внешнем изменении родителя.</summary>
        private void SourceParentChanged(object sender, EventArgs e) { Attach(); }
        /// <summary>Пересчитывает положение при изменении компоновки.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { Synchronize(); }
        /// <summary>Перекрашивает кнопки при выборе темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }
        /// <summary>Освобождает слой вместе с исходным числовым полем.</summary>
        private void SourceDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Удаляет обработчики и освобождает таймер.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopRepeat();
                repeatTimer.Tick -= RepeatTick;
                repeatTimer.Dispose();
                number.SizeChanged -= SourceChanged;
                number.LocationChanged -= SourceChanged;
                number.ParentChanged -= SourceParentChanged;
                number.VisibleChanged -= SourceChanged;
                number.EnabledChanged -= SourceChanged;
                number.Disposed -= SourceDisposed;
                ThemeManager.Changed -= ThemeChanged;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
