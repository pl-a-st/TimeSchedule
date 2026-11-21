using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// DateTimePicker с самостоятельным оформленным календарём. Значение, ограничения,
    /// ручной ввод и события ValueChanged/DropDown/CloseUp принадлежат исходному контролу.
    /// </summary>
    public class StyledDateTimePicker : DateTimePicker
    {
        /// <summary>Окно открытого календаря; создаётся только на время раскрытия.</summary>
        private ToolStripDropDown calendarPopup;
        /// <summary>Панель, рисующая даты вместо нативного MonthCalendar.</summary>
        private StyledCalendarMonthView monthView;
        /// <summary>Закрытие календаря после уничтожения исходного поля.</summary>
        private bool disposingPicker;
        /// <summary>Закрытые окна, ожидающие безопасного освобождения после обработки сообщения Windows.</summary>
        private readonly List<ToolStripDropDown> closedPopups = new List<ToolStripDropDown>();
        /// <summary>Подписка на завершение текущего цикла оконных сообщений.</summary>
        private bool idleCleanupSubscribed;
        /// <summary>Сообщения клавиатуры и мыши обычного редактора дат.</summary>
        private const int WmLButtonDown = 0x0201, WmLButtonUp = 0x0202,
            WmKeyDown = 0x0100, WmSysKeyDown = 0x0104;

        /// <summary>Создаёт совместимый с дизайнером WinForms элемент даты.</summary>
        public StyledDateTimePicker() { }

        /// <summary>
        /// Передаёт подтверждение даты существующим обработчикам KeyUp исходного поля.
        /// В частности, главная форма использует Enter для перехода к выбранной дате.
        /// </summary>
        internal void NotifyEditorEnter()
        {
            if (!IsDisposed) base.OnKeyUp(new KeyEventArgs(Keys.Enter));
        }

        /// <summary>
        /// Перехватывает раскрытие календаря. Тематический ввод обслуживает
        /// StyledDatePickerSkin, остальные сообщения сохраняет DateTimePicker.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (!DesignMode && Enabled && !ShowUpDown &&
                (m.Msg == WmLButtonDown || m.Msg == WmLButtonUp))
            {
                int x = (short)((long)m.LParam & 0xffff);
                if (x >= ClientSize.Width - Math.Max(18, SystemInformation.VerticalScrollBarWidth))
                {
                    if (m.Msg == WmLButtonDown) ShowStyledCalendar();
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            if (!DesignMode && Enabled && !ShowUpDown &&
                (m.Msg == WmKeyDown || m.Msg == WmSysKeyDown))
            {
                int key = m.WParam.ToInt32();
                if (key == (int)Keys.F4 ||
                    (key == (int)Keys.Down && (ModifierKeys & Keys.Alt) != Keys.None))
                {
                    ShowStyledCalendar();
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Показывает тематический календарь без изменения существующих значений и
        /// подписок; повторное нажатие кнопки закрывает уже открытое окно.
        /// </summary>
        internal void ShowStyledCalendar()
        {
            if (DesignMode || disposingPicker || IsDisposed || !Enabled || ShowUpDown) return;
            if (calendarPopup != null)
            {
                if (!calendarPopup.IsDisposed) calendarPopup.Close();
                return;
            }
            var calendar = new StyledCalendarMonthView(this);
            var host = new ToolStripControlHost(calendar)
            {
                AutoSize = false,
                Size = calendar.Size,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            var popup = new ToolStripDropDown
            {
                AutoSize = false,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                AutoClose = true,
                DropShadowEnabled = true,
                Size = calendar.Size,
                BackColor = ThemeManager.Palette.Surface
            };
            popup.Items.Add(host);
            calendar.DateChosen += CalendarDateChosen;
            calendar.CancelRequested += CalendarCancelRequested;
            popup.Closed += PopupClosed;
            ThemeManager.Changed += PopupThemeChanged;
            calendarPopup = popup;
            monthView = calendar;
            Point screen = PointToScreen(new Point(0, Height));
            Rectangle screenBounds = Screen.FromControl(this).WorkingArea;
            if (screen.Y + popup.Height > screenBounds.Bottom)
                screen.Y = PointToScreen(Point.Empty).Y - popup.Height;
            screen.X = Math.Max(screenBounds.Left, Math.Min(screen.X,
                screenBounds.Right - popup.Width));
            screen.Y = Math.Max(screenBounds.Top, Math.Min(screen.Y,
                screenBounds.Bottom - popup.Height));
            try { popup.Show(screen); }
            catch (Exception ex) when (ex is InvalidOperationException ||
                ex is Win32Exception || ex is ArgumentException)
            {
                Trace.TraceWarning("TS-04: тематический календарь не открыт: " + ex);
                CloseFailedPopup(popup, calendar);
                MessageBox.Show("Не удалось открыть выбор даты. Можно ввести дату с клавиатуры.",
                    "Выбор даты", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            base.OnDropDown(EventArgs.Empty);
            calendar.Focus();
        }

        /// <summary>Убирает подписки после ошибки открытия до появления события Closed.</summary>
        private void CloseFailedPopup(ToolStripDropDown popup, StyledCalendarMonthView calendar)
        {
            ThemeManager.Changed -= PopupThemeChanged;
            calendar.DateChosen -= CalendarDateChosen;
            calendar.CancelRequested -= CalendarCancelRequested;
            popup.Closed -= PopupClosed;
            calendarPopup = null;
            monthView = null;
            QueueClosedPopup(popup);
        }

        /// <summary>Закрывает календарь при нажатии Escape.</summary>
        private void CalendarCancelRequested()
        {
            if (calendarPopup != null) calendarPopup.Close();
        }

        /// <summary>Сохраняет время исходной даты и штатные ограничения DateTimePicker.</summary>
        private void CalendarDateChosen(DateTime day)
        {
            DateTime candidate = day.Date.Add(Value.TimeOfDay);
            if (candidate < MinDate) candidate = MinDate;
            if (candidate > MaxDate) candidate = MaxDate;
            Value = candidate;
            if (calendarPopup != null) calendarPopup.Close();
        }

        /// <summary>Обновляет открытый календарь при переключении темы.</summary>
        private void PopupThemeChanged(object sender, EventArgs e)
        {
            if (calendarPopup == null) return;
            calendarPopup.BackColor = ThemeManager.Palette.Surface;
            if (monthView != null && !monthView.IsDisposed) monthView.Invalidate();
        }

        /// <summary>Снимает подписки и уведомляет существующих подписчиков CloseUp.</summary>
        private void PopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ToolStripDropDown old = calendarPopup;
            StyledCalendarMonthView view = monthView;
            calendarPopup = null;
            monthView = null;
            ThemeManager.Changed -= PopupThemeChanged;
            if (view != null)
            {
                view.DateChosen -= CalendarDateChosen;
                view.CancelRequested -= CalendarCancelRequested;
            }
            if (old != null)
            {
                old.Closed -= PopupClosed;
                QueueClosedPopup(old);
            }
            if (!disposingPicker && !IsDisposed) base.OnCloseUp(EventArgs.Empty);
        }

        /// <summary>
        /// Откладывает Dispose до выхода WinForms из ModalMenuFilter: обработчик Closed
        /// вызывается до окончания обработки закрывающего сообщения и не может уничтожать popup.
        /// </summary>
        /// <param name="popup">Уже закрытое окно, больше не участвующее в событиях выбора даты.</param>
        private void QueueClosedPopup(ToolStripDropDown popup)
        {
            if (popup == null || popup.IsDisposed) return;
            closedPopups.Add(popup);
            if (idleCleanupSubscribed) return;
            idleCleanupSubscribed = true;
            Application.Idle += DisposeClosedPopupsOnIdle;
        }

        /// <summary>Освобождает уже закрытые popup только после завершения текущего цикла сообщений.</summary>
        private void DisposeClosedPopupsOnIdle(object sender, EventArgs e)
        {
            Application.Idle -= DisposeClosedPopupsOnIdle;
            idleCleanupSubscribed = false;
            ToolStripDropDown[] pending = closedPopups.ToArray();
            closedPopups.Clear();
            foreach (ToolStripDropDown popup in pending)
            {
                if (popup.IsDisposed) continue;
                try { popup.Dispose(); }
                catch (InvalidOperationException ex)
                {
                    Trace.TraceWarning("TS-04: отложенное освобождение календаря не удалось: " + ex);
                }
            }
        }

        /// <summary>Не оставляет окно календаря живым после удаления поля или его HWND.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposingPicker = true;
                if (calendarPopup != null && !calendarPopup.IsDisposed)
                    calendarPopup.Close();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Небольшой полностью оформляемый календарь с переключением месяцев и лет,
    /// выбором даты мышью и клавиатурой, поддержкой локали и граничных дат.
    /// </summary>
    internal sealed class StyledCalendarMonthView : Control
    {
        /// <summary>Владелец значения и допустимого диапазона дат.</summary>
        private readonly DateTimePicker owner;
        /// <summary>Локализованный первый день недели и наименования месяцев.</summary>
        private readonly DateTimeFormatInfo dateFormat;
        /// <summary>Первый день показываемого месяца.</summary>
        private DateTime displayedMonth;
        /// <summary>Текущая дата для клавиатурной навигации.</summary>
        private DateTime focusedDate;
        /// <summary>Выбор месяца из сетки двенадцати месяцев.</summary>
        private bool monthSelection;
        /// <summary>Размеры элементов, рассчитанные по действующему шрифту и DPI.</summary>
        private readonly int cellWidth, cellHeight, headerHeight, weekdaysHeight, footerHeight;
        /// <summary>Внутренний отступ между содержимым и границей.</summary>
        private const int MarginSize = 8;
        /// <summary>Уведомление владельцу о выборе допустимой даты.</summary>
        internal event Action<DateTime> DateChosen;
        /// <summary>Запрос закрытия popup без изменения выбранного значения.</summary>
        internal event Action CancelRequested;

        /// <summary>Создаёт поверхность календаря, не добавляя элементов на каждый день.</summary>
        internal StyledCalendarMonthView(DateTimePicker source)
        {
            owner = source;
            Font = source.Font;
            dateFormat = (DateTimeFormatInfo)CultureInfo.CurrentCulture.DateTimeFormat.Clone();
            focusedDate = source.Value.Date;
            displayedMonth = new DateTime(focusedDate.Year, focusedDate.Month, 1);
            cellWidth = Math.Max(33, Font.Height * 2 + 7);
            cellHeight = Math.Max(26, Font.Height + 11);
            headerHeight = Math.Max(36, Font.Height + 20);
            weekdaysHeight = Math.Max(25, Font.Height + 9);
            footerHeight = Math.Max(28, Font.Height + 11);
            Size = new Size(MarginSize * 2 + cellWidth * 7,
                MarginSize * 2 + headerHeight + weekdaysHeight + cellHeight * 6 + footerHeight);
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        }

        /// <summary>Вычисляет допустимость целого дня с учётом MinDate и MaxDate.</summary>
        private bool DateAllowed(DateTime date)
        {
            return date.Date >= owner.MinDate.Date && date.Date <= owner.MaxDate.Date;
        }

        /// <summary>Проверяет наличие хотя бы одного допустимого дня в месяце.</summary>
        private bool MonthAllowed(DateTime month)
        {
            DateTime end = month.Year == 9999 && month.Month == 12
                ? DateTime.MaxValue : month.AddMonths(1).AddTicks(-1);
            return end >= owner.MinDate && month <= owner.MaxDate;
        }

        /// <summary>Переключает месяц без выхода за разрешённый диапазон.</summary>
        private void ChangeMonth(int amount)
        {
            if (amount == 0) return;
            DateTime candidate;
            try { candidate = displayedMonth.AddMonths(amount); }
            catch (ArgumentOutOfRangeException) { return; }
            if (!MonthAllowed(candidate)) return;
            displayedMonth = candidate;
            FocusMonth(candidate);
            Invalidate();
        }

        /// <summary>Переносит клавиатурный фокус в отображаемый месяц без изменения даты задачи.</summary>
        private void FocusMonth(DateTime month)
        {
            int day = Math.Min(focusedDate.Day, DateTime.DaysInMonth(month.Year, month.Month));
            DateTime date = new DateTime(month.Year, month.Month, day);
            if (date < owner.MinDate.Date) date = owner.MinDate.Date;
            if (date > owner.MaxDate.Date) date = owner.MaxDate.Date;
            focusedDate = date;
        }

        /// <summary>Меняет день клавиатурой, ограничивая его допустимыми датами.</summary>
        private void MoveFocus(int days)
        {
            DateTime candidate;
            try { candidate = focusedDate.AddDays(days); }
            catch (ArgumentOutOfRangeException) { return; }
            if (!DateAllowed(candidate)) return;
            focusedDate = candidate;
            displayedMonth = new DateTime(candidate.Year, candidate.Month, 1);
            Invalidate();
        }

        /// <summary>Вычисляет дату ячейки без переполнения DateTime на границах годов 1 и 9999.</summary>
        /// <param name="index">Номер ячейки календарной сетки, от 0 до 41.</param>
        /// <param name="date">Дата ячейки, если она представима в DateTime.</param>
        /// <returns>True для даты внутри диапазона DateTime.</returns>
        private bool TryGetGridDate(int index, out DateTime date)
        {
            int diff = ((int)displayedMonth.DayOfWeek - (int)dateFormat.FirstDayOfWeek + 7) % 7;
            long ticks = displayedMonth.Ticks + ((long)index - diff) * TimeSpan.TicksPerDay;
            if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
            {
                date = DateTime.MinValue;
                return false;
            }
            date = new DateTime(ticks);
            return true;
        }

        /// <summary>Выбирает день только внутри исходных MinDate и MaxDate.</summary>
        private void SelectDate(DateTime value)
        {
            if (DateAllowed(value)) DateChosen?.Invoke(value);
        }

        /// <summary>Рисует тематический заголовок, недели, даты и кнопку «Сегодня».</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(palette.Surface);
            Rectangle full = new Rectangle(0, 0, Width - 1, Height - 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ThemeVisuals.InputPath(full))
            using (Pen pen = new Pen(palette.Grid)) e.Graphics.DrawPath(pen, path);
            Rectangle header = new Rectangle(MarginSize, MarginSize, Width - MarginSize * 2, headerHeight);
            using (var brush = new SolidBrush(palette.Button)) e.Graphics.FillRectangle(brush, header);
            string caption = monthSelection ? displayedMonth.Year.ToString(CultureInfo.CurrentCulture) :
                displayedMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
            if (caption.Length > 0) caption = char.ToUpper(caption[0], CultureInfo.CurrentCulture) + caption.Substring(1);
            DrawText(e.Graphics, caption, header, palette.Text, true);
            Rectangle prev = new Rectangle(MarginSize, MarginSize, cellWidth, headerHeight);
            Rectangle next = new Rectangle(Width - MarginSize - cellWidth, MarginSize, cellWidth, headerHeight);
            DrawText(e.Graphics, "‹", prev, palette.Text, true);
            DrawText(e.Graphics, "›", next, palette.Text, true);
            int top = MarginSize + headerHeight;
            if (monthSelection)
            {
                DrawMonthChoices(e.Graphics, palette, top);
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    int index = ((int)dateFormat.FirstDayOfWeek + i) % 7;
                    string day = dateFormat.AbbreviatedDayNames[index];
                    Rectangle cell = new Rectangle(MarginSize + i * cellWidth, top,
                        cellWidth, weekdaysHeight);
                    DrawText(e.Graphics, day, cell, palette.MutedText, false);
                }
                top += weekdaysHeight;
                for (int i = 0; i < 42; i++)
                {
                    DateTime date;
                    if (!TryGetGridDate(i, out date)) continue;
                    Rectangle cell = new Rectangle(MarginSize + (i % 7) * cellWidth,
                        top + (i / 7) * cellHeight, cellWidth, cellHeight);
                    DrawDay(e.Graphics, cell, date, palette);
                }
            }
            Rectangle footer = new Rectangle(MarginSize, Height - MarginSize - footerHeight,
                Width - 2 * MarginSize, footerHeight);
            using (Pen line = new Pen(palette.Grid))
                e.Graphics.DrawLine(line, footer.Left, footer.Top, footer.Right, footer.Top);
            DrawText(e.Graphics, "Сегодня: " + DateTime.Today.ToString("dd.MM.yyyy"),
                footer, DateAllowed(DateTime.Today) ? palette.Text : palette.MutedText, false);
        }

        /// <summary>Рисует кнопку выбора одного из двенадцати месяцев текущего года.</summary>
        private void DrawMonthChoices(Graphics graphics, ThemePalette palette, int top)
        {
            int columnWidth = (Width - 2 * MarginSize) / 3;
            int rowHeight = (Height - top - footerHeight - MarginSize) / 4;
            for (int month = 1; month <= 12; month++)
            {
                DateTime candidate = new DateTime(displayedMonth.Year, month, 1);
                Rectangle cell = new Rectangle(MarginSize + ((month - 1) % 3) * columnWidth,
                    top + ((month - 1) / 3) * rowHeight, columnWidth, rowHeight);
                if (month == displayedMonth.Month)
                    using (var brush = new SolidBrush(palette.ButtonHover))
                        graphics.FillRectangle(brush, Rectangle.Inflate(cell, -2, -2));
                DrawText(graphics, dateFormat.AbbreviatedMonthNames[month - 1], cell,
                    MonthAllowed(candidate) ? palette.Text : palette.MutedText, false);
            }
        }

        /// <summary>Рисует отдельный день с подсветкой выбранной даты и текущего дня.</summary>
        private void DrawDay(Graphics graphics, Rectangle cell, DateTime date, ThemePalette palette)
        {
            if (date.Date == owner.Value.Date)
            {
                using (SolidBrush brush = new SolidBrush(palette.Accent))
                    graphics.FillRectangle(brush, Rectangle.Inflate(cell, -2, -2));
            }
            if (date.Date == focusedDate && Focused)
            {
                using (Pen pen = new Pen(palette.Focus))
                    graphics.DrawRectangle(pen, Rectangle.Inflate(cell, -2, -2));
            }
            if (date.Date == DateTime.Today)
            {
                using (Pen pen = new Pen(palette.Focus))
                    graphics.DrawRectangle(pen, Rectangle.Inflate(cell, -4, -4));
            }
            Color color = !DateAllowed(date) ? palette.MutedText :
                date.Month == displayedMonth.Month ? palette.Text : palette.MutedText;
            DrawText(graphics, date.Day.ToString(CultureInfo.CurrentCulture), cell, color, false);
        }

        /// <summary>Одинаковое центрирование подписей без изменения выбранных значений.</summary>
        private void DrawText(Graphics graphics, string text, Rectangle bounds, Color color, bool bold)
        {
            Font font = Font;
            Font boldFont = null;
            if (bold) { boldFont = new Font(Font, FontStyle.Bold); font = boldFont; }
            try
            {
                TextRenderer.DrawText(graphics, text, font, bounds, color,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            finally { if (boldFont != null) boldFont.Dispose(); }
        }

        /// <summary>Обрабатывает клики навигации и выбор дат в оформленной сетке.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Focus();
            int contentWidth = Width - 2 * MarginSize;
            if (e.Y >= MarginSize && e.Y < MarginSize + headerHeight)
            {
                if (e.X < MarginSize + cellWidth) ChangeMonth(monthSelection ? -12 : -1);
                else if (e.X >= Width - MarginSize - cellWidth) ChangeMonth(monthSelection ? 12 : 1);
                else { monthSelection = !monthSelection; Invalidate(); }
                return;
            }
            if (e.Y >= Height - MarginSize - footerHeight)
            {
                SelectDate(DateTime.Today);
                return;
            }
            int top = MarginSize + headerHeight;
            if (e.X < MarginSize || e.X >= MarginSize + contentWidth || e.Y < top) return;
            if (monthSelection)
            {
                int width = contentWidth / 3;
                int height = (Height - top - footerHeight - MarginSize) / 4;
                int column = Math.Min(2, (e.X - MarginSize) / Math.Max(1, width));
                int row = Math.Min(3, (e.Y - top) / Math.Max(1, height));
                int month = row * 3 + column + 1;
                DateTime candidate = new DateTime(displayedMonth.Year, month, 1);
                if (MonthAllowed(candidate))
                {
                    displayedMonth = candidate;
                    FocusMonth(candidate);
                    monthSelection = false;
                    Invalidate();
                }
                return;
            }
            top += weekdaysHeight;
            if (e.Y < top || e.Y >= top + cellHeight * 6) return;
            int x = Math.Min(6, (e.X - MarginSize) / cellWidth);
            int y = (e.Y - top) / cellHeight;
            DateTime clickedDate;
            if (TryGetGridDate(y * 7 + x, out clickedDate)) SelectDate(clickedDate);
        }

        /// <summary>Поддерживает клавиатурную навигацию календаря без нативного popup.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys code = keyData & Keys.KeyCode;
            if (code == Keys.Escape)
            {
                CancelRequested?.Invoke();
                return true;
            }
            if (code == Keys.Enter)
            {
                if (monthSelection) { monthSelection = false; Invalidate(); }
                else SelectDate(focusedDate);
                return true;
            }
            if (code == Keys.PageUp || code == Keys.PageDown)
            {
                ChangeMonth((code == Keys.PageUp ? -1 : 1) *
                    ((keyData & Keys.Control) == Keys.Control ? 12 : 1));
                return true;
            }
            if (!monthSelection)
            {
                int days = code == Keys.Left ? -1 : code == Keys.Right ? 1 :
                    code == Keys.Up ? -7 : code == Keys.Down ? 7 : 0;
                if (days != 0) { MoveFocus(days); return true; }
                if (code == Keys.Home) { MoveFocus(1 - focusedDate.Day); return true; }
                if (code == Keys.End)
                {
                    MoveFocus(DateTime.DaysInMonth(focusedDate.Year, focusedDate.Month) - focusedDate.Day);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
