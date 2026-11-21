using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Применяет поддерживаемую Windows тему к штатному списку ComboBox.
    /// Не создаёт второго HWND и не вмешивается в захват мыши или прокрутку.
    /// Внешний вид системного скроллбара зависит от версии и настроек Windows.
    /// </summary>
    internal static class StyledComboPopupScrollBar
    {
        /// <summary>Данные Win32 о дочерних окнах ComboBox.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ComboBoxInfo
        {
            internal int Size;
            internal NativeRect Item;
            internal NativeRect Button;
            internal int ButtonState;
            internal IntPtr Combo;
            internal IntPtr Edit;
            internal IntPtr List;
        }

        /// <summary>Прямоугольник Win32, необходимый структуре COMBOBOXINFO.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            internal int Left;
            internal int Top;
            internal int Right;
            internal int Bottom;
        }

        /// <summary>
        /// Оформляет уже существующий HWND списка. Не заменяет управление мышью,
        /// клавиатуру, подписки на события и сам список.
        /// </summary>
        /// <param name="combo">Исходный раскрывающийся список WinForms.</param>
        internal static void ApplyNativeTheme(ComboBox combo)
        {
            if (combo == null || combo.IsDisposed || !combo.IsHandleCreated) return;
            var info = new ComboBoxInfo { Size = Marshal.SizeOf(typeof(ComboBoxInfo)) };
            if (!GetComboBoxInfo(combo.Handle, ref info) || info.List == IntPtr.Zero)
            {
                Trace.TraceInformation("TS-04: HWND раскрытого списка ComboBox недоступен: " + combo.Name);
                return;
            }

            string requestedTheme = ThemeManager.Current == ThemeKind.Dark
                ? "DarkMode_Explorer" : "Explorer";
            int result = SetWindowTheme(info.List, requestedTheme, null);
            if (result < 0)
            {
                Trace.TraceInformation("TS-04: Windows не поддерживает тему списка " +
                    requestedTheme + " (HRESULT=0x" + result.ToString("X8") + ").");
            }
            ApplyPopupBorderColor(info.List);
        }

        /// <summary>
        /// Устанавливает нейтральную границу раскрытого окна средствами DWM Windows 11.
        /// Если Windows не поддерживает атрибут, сохраняет штатный список без изменений.
        /// </summary>
        /// <param name="listHandle">HWND существующего раскрытого списка.</param>
        private static void ApplyPopupBorderColor(IntPtr listHandle)
        {
            const int borderColorAttribute = 34;
            const int defaultBorderColor = -1;
            Color grid = ThemeManager.Palette.Grid;
            int color = ThemeManager.Current == ThemeKind.Dark
                ? grid.R | (grid.G << 8) | (grid.B << 16)
                : defaultBorderColor;
            int result = DwmSetWindowAttribute(listHandle, borderColorAttribute,
                ref color, Marshal.SizeOf(typeof(int)));
            if (result < 0)
                Trace.TraceInformation("TS-04: цвет границы раскрытого списка не поддерживается " +
                    "(HRESULT=0x" + result.ToString("X8") + ").");
        }

        /// <summary>Возвращает дескриптор стандартного раскрытого списка.</summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetComboBoxInfo(IntPtr hwnd, ref ComboBoxInfo info);

        /// <summary>Выбирает системный вариант темы без собственных окон и перерисовки.</summary>
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string appName, string idList);

        /// <summary>Запрашивает цвет не-клиентской рамки окна у системного DWM.</summary>
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute,
            ref int value, int valueSize);
    }

    /// <summary>Действие с фокусом, выполняемое только после завершения жизненного цикла popup.</summary>
    internal enum ComboPopupFocusAction { None, ReturnToOwner, NextControl, PreviousControl }

    /// <summary>
    /// Владеет тематическим раскрытием ComboBox в режимах DropDown и DropDownList.
    /// Сохраняет существующий экземпляр, свободный ввод и события выбора.
    /// </summary>
    internal sealed class StyledComboPopupController : IDisposable
    {
        /// <summary>Исходный ComboBox, владеющий текстом, элементами и выбранным индексом.</summary>
        private readonly ComboBox combo;
        /// <summary>Перехват F4 и Alt+Down в оконной процедуре штатного поля.</summary>
        private readonly KeyboardWindow keyboard;
        /// <summary>Единственное открытое тематическое окно данного ComboBox.</summary>
        private StyledComboDropDown popup;
        /// <summary>Закрытые окна, ожидающие безопасного освобождения после ToolStrip.Closed.</summary>
        private readonly System.Collections.Generic.List<StyledComboDropDown> closedPopups =
            new System.Collections.Generic.List<StyledComboDropDown>();
        /// <summary>Регистрация однократной очистки на следующем Application.Idle.</summary>
        private bool cleanupSubscribed;
        /// <summary>Отложенный переход фокуса после полного закрытия и освобождения popup.</summary>
        private ComboPopupFocusAction pendingFocus;
        /// <summary>Владелец меню, временно удерживаемого при раскрытии вложенного списка.</summary>
        private ToolStripDropDown ownerMenu;
        /// <summary>Исходный режим автоматического закрытия меню.</summary>
        private bool ownerMenuAutoClose;
        /// <summary>Закрыт ли вложенный список вследствие ухода пользователя из меню.</summary>
        private bool popupDismissedOutside;
        /// <summary>Не допускает повторное освобождение и открытие после Dispose.</summary>
        private bool disposed;

        /// <summary>Подготовка актуальных элементов, вызываемая перед пользовательским раскрытием.</summary>
        internal Action Prepare { get; set; }
        /// <summary>Открыт ли сейчас тематический список.</summary>
        internal bool IsOpen { get { return popup != null && !popup.IsDisposed; } }

        /// <summary>Подключает обработчики к существующему ComboBox однократно.</summary>
        /// <param name="source">Исходное поле ввода.</param>
        internal StyledComboPopupController(ComboBox source)
        {
            combo = source ?? throw new ArgumentNullException(nameof(source));
            keyboard = new KeyboardWindow(this);
            combo.KeyDown += ComboKeyDown;
            combo.HandleCreated += HandleCreated;
            combo.HandleDestroyed += HandleDestroyed;
            if (combo.IsHandleCreated) keyboard.Attach(combo.Handle);
        }

        /// <summary>Открывает или закрывает список по нажатию собственной стрелки.</summary>
        internal void Toggle()
        {
            if (disposed || combo.IsDisposed || !combo.Enabled ||
                combo.DropDownStyle == ComboBoxStyle.Simple) return;
            if (IsOpen) { popup.Close(); return; }
            Open();
        }

        /// <summary>Показывает один popup, не удаляя штатные обработчики ComboBox.</summary>
        private void Open()
        {
            if (disposed || IsOpen || combo.IsDisposed || !combo.Enabled ||
                combo.DropDownStyle == ComboBoxStyle.Simple) return;
            Prepare?.Invoke();
            if (combo.IsDisposed || !combo.Enabled || combo.Items.Count == 0) return;
            StyledComboDropDown next = new StyledComboDropDown(combo);
            KeepOwnerMenuOpen();
            combo.Focus();
            next.Closed += PopupClosed;
            next.FocusAfterCloseRequested += PopupFocusRequested;
            popup = next;
            try { next.ShowForCombo(); }
            catch (Exception ex) when (ex is InvalidOperationException ||
                ex is System.ComponentModel.Win32Exception || ex is ArgumentException)
            {
                Trace.TraceWarning("TS-04: ошибка тематического списка ComboBox: " + ex);
                next.Closed -= PopupClosed;
                next.FocusAfterCloseRequested -= PopupFocusRequested;
                popup = null;
                QueueForDisposal(next);
                RestoreOwnerMenu();
                MessageBox.Show("Не удалось открыть список. Повторите действие.",
                    "Выбор значения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Не позволяет родительскому меню закрыться, когда фокус переходит
        /// в отдельное тематическое окно раскрытого списка.
        /// </summary>
        private void KeepOwnerMenuOpen()
        {
            if (ownerMenu != null) return;
            for (Control parent = combo.Parent; parent != null; parent = parent.Parent)
            {
                ToolStripDropDown menu = parent as ToolStripDropDown;
                if (menu == null || !menu.Visible) continue;
                ownerMenu = menu;
                ownerMenuAutoClose = menu.AutoClose;
                popupDismissedOutside = false;
                menu.AutoClose = false;
                break;
            }
        }

        /// <summary>
        /// Немедленно восстанавливает закрытие родительского меню при закрытии списка.
        /// Освобождение самого списка остаётся отложенным до Application.Idle.
        /// </summary>
        private void RestoreOwnerMenu(bool selectionCommitted = false)
        {
            ToolStripDropDown menu = ownerMenu;
            if (menu == null) return;
            ownerMenu = null;
            bool closeMenu = selectionCommitted || popupDismissedOutside;
            popupDismissedOutside = false;
            if (menu.IsDisposed) return;
            menu.AutoClose = ownerMenuAutoClose;
            if (!closeMenu || !menu.Visible || !menu.IsHandleCreated) return;

            menu.BeginInvoke((MethodInvoker)delegate
            {
                if (!menu.IsDisposed && menu.Visible) menu.Close();
            });
        }

        /// <summary>Перехватывает клавиши до штатного раскрытия Windows, сохраняя прочий ввод.</summary>
        private void ComboKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F4 && !(e.KeyCode == Keys.Down && e.Alt)) return;
            if (disposed || !combo.Enabled || combo.DropDownStyle == ComboBoxStyle.Simple) return;
            if (!IsOpen) Open();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>Повторно подключается при пересоздании HWND.</summary>
        private void HandleCreated(object sender, EventArgs e) { keyboard.Attach(combo.Handle); }
        /// <summary>Снимает подкласс уничтожаемого HWND.</summary>
        private void HandleDestroyed(object sender, EventArgs e) { keyboard.Detach(); }

        /// <summary>Запоминает команду пользователя, но не меняет фокус внутри ToolStrip.</summary>
        private void PopupFocusRequested(ComboPopupFocusAction action)
        {
            if (!disposed) pendingFocus = action;
        }

        /// <summary>Открепляет закрытый popup без освобождения во время события Closed.</summary>
        private void PopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            StyledComboDropDown old = sender as StyledComboDropDown;
            if (old == null) return;
            old.Closed -= PopupClosed;
            old.FocusAfterCloseRequested -= PopupFocusRequested;
            if (ReferenceEquals(popup, old)) popup = null;
            popupDismissedOutside = e.CloseReason == ToolStripDropDownCloseReason.AppClicked ||
                e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange;
            QueueForDisposal(old);
            RestoreOwnerMenu(old.SelectionCommitted);
        }

        /// <summary>Планирует освобождение на следующем безопасном цикле сообщений.</summary>
        private void QueueForDisposal(StyledComboDropDown old)
        {
            if (old == null || old.IsDisposed) return;
            closedPopups.Add(old);
            if (cleanupSubscribed) return;
            cleanupSubscribed = true;
            Application.Idle += DisposeClosedPopups;
        }

        /// <summary>Освобождает ToolStripDropDown после завершения его обработчиков.</summary>
        private void DisposeClosedPopups(object sender, EventArgs e)
        {
            Application.Idle -= DisposeClosedPopups;
            cleanupSubscribed = false;
            StyledComboDropDown[] pending = closedPopups.ToArray();
            closedPopups.Clear();
            foreach (StyledComboDropDown old in pending)
            {
                if (old.IsDisposed) continue;
                try { old.Dispose(); }
                catch (InvalidOperationException ex)
                {
                    Trace.TraceWarning("TS-04: не удалось освободить закрытый список ComboBox: " + ex);
                }
            }
            if (!IsOpen) RestoreFocusAfterPopup();
        }

        /// <summary>
        /// Выполняет переход фокуса после освобождения ToolStrip и окончания его
        /// обработки сообщений, чтобы исключить возврат к ComboBox поверх редактора даты.
        /// </summary>
        private void RestoreFocusAfterPopup()
        {
            ComboPopupFocusAction action = pendingFocus;
            pendingFocus = ComboPopupFocusAction.None;
            if (action == ComboPopupFocusAction.None || disposed || IsOpen ||
                combo.IsDisposed || combo.Parent == null || !combo.Enabled || !combo.Visible)
                return;
            Form form = combo.FindForm();
            if (form == null || form.IsDisposed || !form.Visible || Form.ActiveForm != form)
                return;
            if (action == ComboPopupFocusAction.ReturnToOwner)
            {
                if (!form.ContainsFocus && combo.CanFocus) combo.Focus();
                return;
            }
            combo.Parent.SelectNextControl(combo,
                action == ComboPopupFocusAction.NextControl, true, true, true);
        }

        /// <summary>Снимает подписки и закрывает открытый список без удаления исходного ComboBox.</summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Prepare = null;
            pendingFocus = ComboPopupFocusAction.None;
            combo.KeyDown -= ComboKeyDown;
            combo.HandleCreated -= HandleCreated;
            combo.HandleDestroyed -= HandleDestroyed;
            keyboard.Detach();
            if (popup != null && !popup.IsDisposed) popup.Close();
            if (!IsOpen) RestoreOwnerMenu();
        }

        /// <summary>Подписка к текущему HWND для штатных горячих клавиш раскрытия.</summary>
        private sealed class KeyboardWindow : NativeWindow
        {
            /// <summary>Оформитель, которому передаются команды раскрытия.</summary>
            private readonly StyledComboPopupController owner;
            /// <summary>Сообщения клавиатуры стандартного окна Windows.</summary>
            private const int WmKeyDown = 0x0100, WmSysKeyDown = 0x0104;

            /// <summary>Связывает оконный перехват с текущим владельцем.</summary>
            internal KeyboardWindow(StyledComboPopupController source) { owner = source; }

            /// <summary>Подключает только существующий HWND ComboBox.</summary>
            internal void Attach(IntPtr handle)
            {
                if (handle == IntPtr.Zero || Handle == handle) return;
                Detach();
                try { AssignHandle(handle); }
                catch (Exception ex) when (ex is InvalidOperationException ||
                    ex is System.ComponentModel.Win32Exception)
                {
                    Trace.TraceWarning("TS-04: перехват клавиш ComboBox недоступен: " + ex);
                }
            }

            /// <summary>Убирает подкласс, не уничтожая исходный HWND.</summary>
            internal void Detach() { if (Handle != IntPtr.Zero) ReleaseHandle(); }

            /// <summary>Перенаправляет F4 и Alt+Down, не вмешиваясь в набор текста.</summary>
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmKeyDown || m.Msg == WmSysKeyDown)
                {
                    int key = m.WParam.ToInt32();
                    if (key == (int)Keys.F4 ||
                        (key == (int)Keys.Down &&
                         (m.Msg == WmSysKeyDown ||
                          (Control.ModifierKeys & Keys.Alt) == Keys.Alt)))
                    {
                        if (!owner.IsOpen) owner.Open();
                        m.Result = IntPtr.Zero;
                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }
    }

    /// <summary>
    /// Оформленный выпадающий список без системной рамки и нативного скроллбара.
    /// Значения и события выбора продолжают принадлежать исходному ComboBox.
    /// </summary>
    internal sealed class StyledComboDropDown : ToolStripDropDown
    {
        /// <summary>Исходный список с данными и SelectedIndex.</summary>
        private readonly ComboBox combo;
        /// <summary>Список с рисованием элементов и прокрутки.</summary>
        private readonly StyledComboPopupList view;
        /// <summary>Команда перехода фокуса, делегируемая владельцу popup после закрытия.</summary>
        internal event Action<ComboPopupFocusAction> FocusAfterCloseRequested;

        /// <summary>Подтверждён ли выбор пользователем до закрытия окна списка.</summary>
        internal bool SelectionCommitted { get; private set; }

        /// <summary>Создаёт окно выбора, не подменяя исходный ComboBox.</summary>
        /// <param name="source">Исходный ComboBox в режиме DropDown или DropDownList.</param>
        internal StyledComboDropDown(ComboBox source)
        {
            combo = source;
            int width = Math.Max(combo.Width, combo.DropDownWidth);
            int visibleRows = Math.Min(Math.Max(1, combo.MaxDropDownItems), combo.Items.Count);
            view = new StyledComboPopupList(combo, width, visibleRows);
            view.Accepted += AcceptItem;
            view.Cancelled += CancelSelection;
            view.NavigationRequested += NavigateAfterClose;
            var host = new ToolStripControlHost(view)
            {
                AutoSize = false,
                Size = view.Size,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };
            AutoSize = false;
            Padding = new Padding(1);
            Margin = Padding.Empty;
            DropShadowEnabled = true;
            AutoClose = true;
            BackColor = ThemeManager.Palette.Grid;
            Renderer = new NeutralComboPopupRenderer();
            Items.Add(host);
            Size = new Size(view.Width + 2, view.Height + 2);
            ThemeManager.Changed += ThemeChanged;
            Closed += PopupClosed;
        }

        /// <summary>Размещает список под исходным полем в границах рабочей области.</summary>
        internal void ShowForCombo()
        {
            Rectangle workingArea = Screen.FromControl(combo).WorkingArea;
            Point location = combo.PointToScreen(new Point(0, combo.Height));
            if (location.Y + Height > workingArea.Bottom)
                location.Y = combo.PointToScreen(Point.Empty).Y - Height;
            location.X = Math.Max(workingArea.Left,
                Math.Min(location.X, workingArea.Right - Width));
            location.Y = Math.Max(workingArea.Top,
                Math.Min(location.Y, workingArea.Bottom - Height));
            Show(location);
            view.Focus();
        }

        /// <summary>
        /// Завершает обработку ToolStrip до запуска существующих обработчиков выбора.
        /// Это особенно важно для наборов проектов, запускающих обновление календаря.
        /// </summary>
        private void AcceptItem(int index)
        {
            if (IsDisposed) return;
            SelectionCommitted = true;
            Close();
            if (combo.IsDisposed || !combo.IsHandleCreated) return;
            combo.BeginInvoke((MethodInvoker)delegate
            {
                if (!combo.IsDisposed && index >= 0 && index < combo.Items.Count)
                    combo.SelectedIndex = index;
            });
        }

        /// <summary>Закрывает список по Escape без синхронного возврата фокуса.</summary>
        private void CancelSelection()
        {
            if (IsDisposed) return;
            FocusAfterCloseRequested?.Invoke(ComboPopupFocusAction.ReturnToOwner);
            Close();
        }

        /// <summary>Откладывает переход по Tab до освобождения окна ToolStrip.</summary>
        private void NavigateAfterClose(bool forward)
        {
            if (IsDisposed) return;
            FocusAfterCloseRequested?.Invoke(forward ?
                ComboPopupFocusAction.NextControl : ComboPopupFocusAction.PreviousControl);
            Close();
        }

        /// <summary>
        /// Отключает акцентную системную рамку ToolStrip и использует
        /// тот же цвет, что и неактивная по фокусу рамка обычного ComboBox.
        /// </summary>
        private sealed class NeutralComboPopupRenderer : ToolStripProfessionalRenderer
        {
            /// <summary>Рисует одну нейтральную границу поверх штатной поверхности списка.</summary>
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                using (var pen = new Pen(ThemeVisuals.InputBorderColor(true, false)))
                    e.Graphics.DrawRectangle(pen, 0, 0,
                        e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
        }

        /// <summary>
        /// Перехватывает Escape и Tab, если ToolStrip не передал их дочернему списку.
        /// Переход фокуса выполняется после закрытия popup.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Escape)
            {
                CancelSelection();
                return true;
            }
            if (key == Keys.Tab)
            {
                NavigateAfterClose((keyData & Keys.Shift) == Keys.None);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Перекрашивает открытый список при переключении темы.</summary>
        private void ThemeChanged(object sender, EventArgs e)
        {
            BackColor = ThemeManager.Palette.Grid;
            view.Invalidate();
            Invalidate();
        }

        /// <summary>Снимает глобальную подписку сразу после закрытия popup.</summary>
        private void PopupClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            ThemeManager.Changed -= ThemeChanged;
        }

        /// <summary>Освобождает подписки даже при аварийном закрытии окна.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Changed -= ThemeChanged;
                Closed -= PopupClosed;
                view.Accepted -= AcceptItem;
                view.Cancelled -= CancelSelection;
                view.NavigationRequested -= NavigateAfterClose;
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Общий тематический список с собственной мышиной и клавиатурной прокруткой.</summary>
    internal sealed class StyledComboPopupList : Control
    {
        /// <summary>Исходный ComboBox как источник элементов и шрифта.</summary>
        private readonly ComboBox combo;
        /// <summary>Число полностью видимых строк.</summary>
        private readonly int visibleRows;
        /// <summary>Высота одной строки.</summary>
        private readonly int rowHeight;
        /// <summary>Позиция первой видимой строки.</summary>
        private int firstRow;
        /// <summary>Текущая строка клавиатурной навигации.</summary>
        private int activeRow;
        /// <summary>Признак перетаскивания ползунка.</summary>
        private bool draggingScroll;
        /// <summary>Начальная координата мыши при захвате ползунка.</summary>
        private int dragY;
        /// <summary>Начальный номер верхней строки при захвате ползунка.</summary>
        private int dragFirstRow;
        /// <summary>Ширина собственной полосы прокрутки.</summary>
        private const int ScrollWidth = 16;
        /// <summary>Подсветка ползунка только при наведении и перетаскивании.</summary>
        private bool hoverScroll;

        /// <summary>Событие выбора существующего элемента исходного ComboBox.</summary>
        internal event Action<int> Accepted;
        /// <summary>Событие отмены без изменения SelectedIndex.</summary>
        internal event Action Cancelled;
        /// <summary>Запрос перехода к следующему или предыдущему полю формы.</summary>
        internal event Action<bool> NavigationRequested;

        /// <summary>Создаёт список, соответствующий ограничениям числа строк ComboBox.</summary>
        internal StyledComboPopupList(ComboBox source, int width, int rows)
        {
            combo = source;
            visibleRows = Math.Max(1, rows);
            rowHeight = Math.Max(20, source.ItemHeight);
            Width = Math.Max(40, width);
            Height = visibleRows * rowHeight;
            Font = source.Font;
            AccessibleRole = AccessibleRole.List;
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable |
                ControlStyles.ResizeRedraw, true);
            activeRow = Math.Max(0, Math.Min(source.SelectedIndex, source.Items.Count - 1));
            EnsureVisible();
        }

        /// <summary>Нужно ли отображать собственную полосу прокрутки.</summary>
        private bool HasScroll { get { return combo.Items.Count > visibleRows; } }
        /// <summary>Максимальный индекс верхней видимой строки.</summary>
        private int MaximumFirst { get { return Math.Max(0, combo.Items.Count - visibleRows); } }
        /// <summary>Размер области прокрутки.</summary>
        private Rectangle ScrollBounds
        {
            get { return HasScroll ? new Rectangle(Width - ScrollWidth, 0, ScrollWidth, Height) : Rectangle.Empty; }
        }

        /// <summary>Вычисляет размер ползунка и его смещение относительно верхнего края.</summary>
        private Rectangle ThumbBounds
        {
            get
            {
                if (!HasScroll) return Rectangle.Empty;
                Rectangle track = ScrollBounds;
                int thumbHeight = Math.Max(20, (int)((long)visibleRows * track.Height / combo.Items.Count));
                thumbHeight = Math.Min(track.Height, thumbHeight);
                int travel = Math.Max(0, track.Height - thumbHeight);
                int top = MaximumFirst == 0 ? 0 : (int)((long)firstRow * travel / MaximumFirst);
                int thickness = Math.Min(8, track.Width - 2);
                return new Rectangle(track.Left + (track.Width - thickness) / 2,
                    top + 2, thickness, Math.Max(1, thumbHeight - 4));
            }
        }

        /// <summary>Отображает строки и неброский тематический скроллбар.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(palette.Surface);
            int contentWidth = Width - (HasScroll ? ScrollWidth : 0);
            for (int row = 0; row < visibleRows; row++)
            {
                int index = firstRow + row;
                if (index >= combo.Items.Count) break;
                Rectangle bounds = new Rectangle(0, row * rowHeight, contentWidth, rowHeight);
                if (index == activeRow)
                    using (var brush = new SolidBrush(palette.ButtonHover))
                        e.Graphics.FillRectangle(brush, bounds);
                Rectangle text = new Rectangle(5, bounds.Top, Math.Max(1, bounds.Width - 10), bounds.Height);
                TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[index]), Font,
                    text, combo.Enabled ? palette.Text : palette.MutedText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            if (HasScroll)
            {
                using (var trackBrush = new SolidBrush(palette.Surface))
                    e.Graphics.FillRectangle(trackBrush, ScrollBounds);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var thumbPath = ThemeVisuals.InputPath(ThumbBounds))
                using (var thumbBrush = new SolidBrush(draggingScroll || hoverScroll ?
                    palette.ButtonHover : palette.Button))
                    e.Graphics.FillPath(thumbBrush, thumbPath);
            }
        }

        /// <summary>Удерживает активную строку внутри видимой области.</summary>
        private void EnsureVisible()
        {
            firstRow = Math.Max(0, Math.Min(firstRow, MaximumFirst));
            if (activeRow < firstRow) firstRow = activeRow;
            if (activeRow >= firstRow + visibleRows) firstRow = activeRow - visibleRows + 1;
            firstRow = Math.Max(0, Math.Min(firstRow, MaximumFirst));
        }

        /// <summary>Передвигает верхнюю видимую строку в допустимых пределах.</summary>
        private void ScrollTo(int top)
        {
            int next = Math.Max(0, Math.Min(MaximumFirst, top));
            if (firstRow == next) return;
            firstRow = next;
            Invalidate();
        }

        /// <summary>Позволяет прокручивать список обычным колесом мыши.</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int steps = Math.Max(1, Math.Abs(e.Delta) / 120);
            ScrollTo(firstRow + (e.Delta > 0 ? -steps : steps) * 3);
        }

        /// <summary>Выбирает строку или начинает перемещение ползунка.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Focus();
            if (HasScroll && ScrollBounds.Contains(e.Location))
            {
                if (ThumbBounds.Contains(e.Location))
                {
                    draggingScroll = true;
                    dragY = e.Y;
                    dragFirstRow = firstRow;
                    Capture = true;
                }
                else ScrollTo(firstRow + (e.Y < ThumbBounds.Top ? -visibleRows : visibleRows));
                return;
            }
            int index = firstRow + e.Y / rowHeight;
            if (index >= 0 && index < combo.Items.Count)
            {
                activeRow = index;
                Invalidate();
            }
        }

        /// <summary>Подсвечивает строку под указателем или перемещает ползунок.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (draggingScroll)
            {
                int travel = Math.Max(1, ScrollBounds.Height - ThumbBounds.Height - 4);
                ScrollTo(dragFirstRow + (int)((long)(e.Y - dragY) * MaximumFirst / travel));
                return;
            }
            bool hoveringThumb = HasScroll && ThumbBounds.Contains(e.Location);
            if (hoveringThumb != hoverScroll)
            {
                hoverScroll = hoveringThumb;
                Invalidate(ScrollBounds);
            }
            if (HasScroll && ScrollBounds.Contains(e.Location)) return;
            if (e.Y < 0 || e.Y >= Height) return;
            int index = firstRow + e.Y / rowHeight;
            if (index >= 0 && index < combo.Items.Count && activeRow != index)
            {
                activeRow = index;
                Invalidate();
            }
        }

        /// <summary>Снимает подсветку ползунка после ухода указателя.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!hoverScroll) return;
            hoverScroll = false;
            Invalidate(ScrollBounds);
        }

        /// <summary>Подтверждает выбор строки, завершая только мышиную операцию.</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (draggingScroll)
            {
                draggingScroll = false;
                Capture = false;
                return;
            }
            if (e.Button == MouseButtons.Left && e.X >= 0 && e.X < Width &&
                (!HasScroll || !ScrollBounds.Contains(e.Location)) &&
                e.Y >= 0 && e.Y < Height && activeRow >= 0 && activeRow < combo.Items.Count)
                Accepted?.Invoke(activeRow);
        }

        /// <summary>Не позволяет системе ToolStrip перехватить навигационные команды.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Up || key == Keys.Down || key == Keys.PageUp ||
                key == Keys.PageDown || key == Keys.Home || key == Keys.End ||
                key == Keys.Enter || key == Keys.Escape || key == Keys.F4 ||
                key == Keys.Tab ||
                (key == Keys.Down && (keyData & Keys.Alt) == Keys.Alt))
            {
                OnKeyDown(new KeyEventArgs(keyData));
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Сохраняет стрелки и клавиши навигации внутри раскрытого списка.</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Up || key == Keys.Down || key == Keys.Left ||
                key == Keys.Right || key == Keys.PageUp || key == Keys.PageDown ||
                key == Keys.Home || key == Keys.End || key == Keys.Enter || key == Keys.Escape)
                return true;
            return base.IsInputKey(keyData);
        }

        /// <summary>Поддерживает стрелки, PageUp/Down, Home/End и подтверждение Enter.</summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int target = activeRow;
            switch (e.KeyCode)
            {
                case Keys.Up: target--; break;
                case Keys.Down: target++; break;
                case Keys.PageUp: target -= visibleRows; break;
                case Keys.PageDown: target += visibleRows; break;
                case Keys.Home: target = 0; break;
                case Keys.End: target = combo.Items.Count - 1; break;
                case Keys.Enter:
                    Accepted?.Invoke(activeRow);
                    e.SuppressKeyPress = true;
                    return;
                case Keys.Escape:
                case Keys.F4:
                    Cancelled?.Invoke();
                    e.SuppressKeyPress = true;
                    return;
                case Keys.Tab:
                    NavigationRequested?.Invoke(!e.Shift);
                    e.SuppressKeyPress = true;
                    return;
                default:
                    if (e.KeyCode == Keys.Down && e.Alt)
                    {
                        Cancelled?.Invoke();
                        e.SuppressKeyPress = true;
                    }
                    return;
            }
            activeRow = Math.Max(0, Math.Min(combo.Items.Count - 1, target));
            EnsureVisible();
            Invalidate();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>Поддерживает поиск первой подходящей строки по начальной букве.</summary>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (char.IsControl(e.KeyChar) || combo.Items.Count == 0) return;
            for (int offset = 1; offset <= combo.Items.Count; offset++)
            {
                int index = (activeRow + offset) % combo.Items.Count;
                if (!combo.GetItemText(combo.Items[index]).StartsWith(e.KeyChar.ToString(),
                    StringComparison.CurrentCultureIgnoreCase)) continue;
                activeRow = index;
                EnsureVisible();
                Invalidate();
                e.Handled = true;
                return;
            }
        }
    }

}
