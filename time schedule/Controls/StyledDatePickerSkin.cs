using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Матовая поверхность штатного DateTimePicker. Выбор даты выполняет сам DateTimePicker,
    /// поэтому сохраняются его события DropDown, CloseUp и ValueChanged.
    /// </summary>
    internal sealed class StyledDatePickerSkin : Control
    {
        /// <summary>Оформляемое поле, остающееся владельцем даты, ограничений и событий.</summary>
        private readonly DateTimePicker picker;
        /// <summary>Признак клавиатурного редактирования штатным контролом.</summary>
        private bool editing;
        /// <summary>Признак открытого штатного календаря.</summary>
        private bool calendarOpen;
        /// <summary>Признак наведения на область пиктограммы календаря.</summary>
        private bool hoveringCalendar;
        /// <summary>Визуальная высота оформленного поля; не меняет размер системного редактора.</summary>
        private int visualHeight;
        /// <summary>Предотвращает повторный вход при пересчёте геометрии.</summary>
        private bool synchronizing;

        /// <summary>Нажатие левой кнопки мыши в системном контроле.</summary>
        private const int WmLButtonDown = 0x0201;
        /// <summary>Отпускание левой кнопки мыши в системном контроле.</summary>
        private const int WmLButtonUp = 0x0202;
        /// <summary>Признак нажатой левой кнопки в параметре Windows-сообщения.</summary>
        private const int MkLButton = 0x0001;

        /// <summary>Создаёт оформление без переноса исходного поля и изменения его событий.</summary>
        /// <param name="source">Существующий DateTimePicker.</param>
        internal StyledDatePickerSkin(DateTimePicker source)
        {
            picker = source ?? throw new ArgumentNullException(nameof(source));
            visualHeight = picker.Height;
            TabStop = false;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            picker.LocationChanged += SourceChanged;
            picker.SizeChanged += SourceChanged;
            picker.VisibleChanged += SourceChanged;
            picker.EnabledChanged += SourceChanged;
            picker.ParentChanged += SourceParentChanged;
            picker.FontChanged += SourceChanged;
            picker.TextChanged += SourceChanged;
            picker.ValueChanged += SourceChanged;
            picker.Enter += SourceEntered;
            picker.KeyDown += NativeKeyDown;
            picker.Leave += SourceLeft;
            picker.DropDown += NativeDropDown;
            picker.CloseUp += NativeCloseUp;
            picker.Disposed += SourceDisposed;
            ThemeManager.Changed += ThemeChanged;
            Attach();
        }

        /// <summary>Размещает рисунок над штатным полем без изменения его родителя.</summary>
        private void Attach()
        {
            if (IsDisposed || picker.IsDisposed) return;
            if (Parent != null && !ReferenceEquals(Parent, picker.Parent))
                Parent.Layout -= ParentLayout;
            if (picker.Parent == null) { Visible = false; return; }
            if (!ReferenceEquals(Parent, picker.Parent))
            {
                picker.Parent.Controls.Add(this);
                Parent.Layout += ParentLayout;
            }
            Synchronize();
        }

        /// <summary>Задаёт высоту декоративной поверхности без изменения штатного ввода даты.</summary>
        /// <param name="height">Высота оформленного поля в пикселях.</param>
        internal void SetVisualHeight(int height)
        {
            visualHeight = Math.Max(picker.Height, height);
            Synchronize();
        }

        /// <summary>Следует за геометрией и видимостью штатного поля.</summary>
        private void Synchronize()
        {
            if (synchronizing || IsDisposed || picker.IsDisposed || picker.Parent == null) return;
            synchronizing = true;
            try
            {
                Rectangle bounds = picker.Bounds;
                if (visualHeight > bounds.Height)
                {
                    bounds.Y -= (visualHeight - bounds.Height) / 2;
                    bounds.Height = visualHeight;
                }
                if (Bounds != bounds) Bounds = bounds;
                bool show = picker.Visible && (!editing || calendarOpen);
                if (Visible != show) Visible = show;
                Enabled = picker.Enabled;
                Font = picker.Font;
                if (show && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Рисует матовое поле и пиктограмму, используя утверждённую палитру.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent == null ? palette.Window : Parent.BackColor);
            if (Width <= 2 || Height <= 2) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle outer = new Rectangle(0, 0, Width - 1, Height - 1);
            Color surface = Enabled ? palette.Surface : palette.ButtonDisabled;
            using (GraphicsPath path = ThemeVisuals.InputPath(outer))
            using (SolidBrush brush = new SolidBrush(surface))
                e.Graphics.FillPath(brush, path);
            Rectangle button = CalendarButtonBounds;
            using (SolidBrush brush = new SolidBrush(!Enabled ? palette.ButtonDisabled :
                hoveringCalendar ? palette.ButtonHover : palette.Button))
                e.Graphics.FillRectangle(brush, button);
            Color textColor = Enabled ? palette.Text : palette.MutedText;
            Rectangle text = new Rectangle(6, 0, Math.Max(0, button.Left - 9), Height);
            TextRenderer.DrawText(e.Graphics, picker.Text, picker.Font, text, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            DrawCalendarIcon(e.Graphics, button, textColor);
            using (GraphicsPath path = ThemeVisuals.InputPath(outer))
            using (Pen pen = new Pen(ThemeVisuals.InputBorderColor(Enabled, picker.ContainsFocus)))
                e.Graphics.DrawPath(pen, path);
        }

        /// <summary>Область кнопки раскрытия с внутренними отступами от рамки.</summary>
        private Rectangle CalendarButtonBounds
        {
            get
            {
                const int inset = 4;
                int buttonWidth = Math.Min(20, Math.Max(0, Width - inset * 2));
                return new Rectangle(Math.Max(inset, Width - buttonWidth - inset), 3,
                    buttonWidth, Math.Max(0, Height - 6));
            }
        }

        /// <summary>Рисует иконку календаря без зависимости от шрифтов Windows.</summary>
        private static void DrawCalendarIcon(Graphics graphics, Rectangle button, Color color)
        {
            int x = button.Left + (button.Width - 12) / 2;
            int y = button.Top + (button.Height - 12) / 2;
            using (Pen pen = new Pen(color, 1.2f))
            {
                graphics.DrawRectangle(pen, x + 1, y + 2, 10, 9);
                graphics.DrawLine(pen, x + 1, y + 5, x + 11, y + 5);
                graphics.DrawLine(pen, x + 4, y, x + 4, y + 3);
                graphics.DrawLine(pen, x + 8, y, x + 8, y + 3);
                graphics.DrawLine(pen, x + 4, y + 8, x + 5, y + 8);
                graphics.DrawLine(pen, x + 8, y + 8, x + 9, y + 8);
            }
        }

        /// <summary>Передаёт раскрытие штатному DateTimePicker; текст допускает обычный ввод.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !picker.Enabled || picker.IsDisposed) return;
            bool openCalendar = CalendarButtonBounds.Contains(e.Location) && !picker.ShowUpDown;
            editing = !openCalendar;
            Synchronize();
            picker.Focus();
            if (openCalendar && picker.IsHandleCreated)
                OpenNativeCalendar();
        }

        /// <summary>
        /// Посылает щелчок непосредственно штатной кнопке раскрытия.
        /// Событие CloseUp в этом случае генерирует исходный DateTimePicker, а не декоратор.
        /// </summary>
        private void OpenNativeCalendar()
        {
            int x = Math.Max(1, picker.ClientSize.Width - 9);
            int y = Math.Max(1, picker.ClientSize.Height / 2);
            IntPtr coordinates = (IntPtr)((y << 16) | (x & 0xffff));
            SendMessage(picker.Handle, WmLButtonDown, (IntPtr)MkLButton, coordinates);
            SendMessage(picker.Handle, WmLButtonUp, IntPtr.Zero, coordinates);
#if DEBUG
            if (!calendarOpen)
                System.Diagnostics.Debug.WriteLine("TS-04 DatePicker: штатное раскрытие не подтверждено, " + picker.Name);
#endif
        }

        /// <summary>Учитывает открытие именно штатного календаря и его жизненный цикл.</summary>
        private void NativeDropDown(object sender, EventArgs e)
        {
            calendarOpen = true;
            editing = true;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("TS-04 DatePicker DropDown: " + picker.Name);
#endif
            Synchronize();
        }

        /// <summary>Возвращает матовое оформление после штатного CloseUp.</summary>
        private void NativeCloseUp(object sender, EventArgs e)
        {
            calendarOpen = false;
            editing = false;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("TS-04 DatePicker CloseUp: " + picker.Name +
                ", value=" + picker.Value.ToString("yyyy-MM-dd"));
#endif
            Synchronize();
        }

        /// <summary>Сохраняет оформление при получении фокуса до начала ручного редактирования.</summary>
        private void SourceEntered(object sender, EventArgs e)
        {
            Synchronize();
        }

        /// <summary>Переключает поле в штатный режим для ввода и навигации с клавиатуры.</summary>
        private void NativeKeyDown(object sender, KeyEventArgs e)
        {
            if (calendarOpen || e.KeyCode == System.Windows.Forms.Keys.Tab) return;
            editing = true;
            Synchronize();
        }

        /// <summary>Возвращает оформленную поверхность после завершения редактирования.</summary>
        private void SourceLeft(object sender, EventArgs e)
        {
            if (calendarOpen) return;
            editing = false;
            Synchronize();
        }

        /// <summary>Подсвечивает кнопку при наведении.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hover = CalendarButtonBounds.Contains(e.Location);
            if (hover == hoveringCalendar) return;
            hoveringCalendar = hover;
            Invalidate(CalendarButtonBounds);
        }

        /// <summary>Сбрасывает подсветку при уходе указателя.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveringCalendar = false;
            Invalidate();
        }

        /// <summary>Обновляет рисунок при изменении исходного поля.</summary>
        private void SourceChanged(object sender, EventArgs e) { Synchronize(); }
        /// <summary>Переподключает рисунок при смене родителя исходного поля.</summary>
        private void SourceParentChanged(object sender, EventArgs e) { Attach(); }
        /// <summary>Обрабатывает штатную компоновку соседних элементов.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { Synchronize(); }
        /// <summary>Перекрашивает изображение после переключения темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Synchronize(); }
        /// <summary>Удаляет оформляющий слой вместе с исходным полем.</summary>
        private void SourceDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Передаёт синхронные сообщения мыши оконной процедуре DateTimePicker.</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        /// <summary>Снимает подписки без изменения подписчиков исходного DateTimePicker.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                picker.LocationChanged -= SourceChanged;
                picker.SizeChanged -= SourceChanged;
                picker.VisibleChanged -= SourceChanged;
                picker.EnabledChanged -= SourceChanged;
                picker.ParentChanged -= SourceParentChanged;
                picker.FontChanged -= SourceChanged;
                picker.TextChanged -= SourceChanged;
                picker.ValueChanged -= SourceChanged;
                picker.Enter -= SourceEntered;
                picker.KeyDown -= NativeKeyDown;
                picker.Leave -= SourceLeft;
                picker.DropDown -= NativeDropDown;
                picker.CloseUp -= NativeCloseUp;
                picker.Disposed -= SourceDisposed;
                ThemeManager.Changed -= ThemeChanged;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
