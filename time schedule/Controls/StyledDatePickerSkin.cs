using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Тематическая поверхность DateTimePicker с собственным редактором текста.
    /// Исходный DateTimePicker остаётся владельцем Value, MinDate, MaxDate и событий.
    /// </summary>
    internal sealed class StyledDatePickerSkin : Control
    {
        /// <summary>Исходное поле с публичной семантикой DateTimePicker.</summary>
        private readonly DateTimePicker picker;
        /// <summary>Редактор даты с фоном и шрифтом из текущей палитры.</summary>
        private readonly TextBox editor;
        /// <summary>Сейчас пользователь вводит дату в тематическом редакторе.</summary>
        private bool editing;
        /// <summary>Открыт штатный календарь совместимого поля.</summary>
        private bool calendarOpen;
        /// <summary>Указатель находится над кнопкой календаря.</summary>
        private bool hoveringCalendar;
        /// <summary>Высота видимой поверхности без изменения исходного HWND.</summary>
        private int visualHeight;
        /// <summary>Защита от рекурсивной синхронизации с полем.</summary>
        private bool synchronizing;
        /// <summary>Подавляет обработку ухода фокуса при завершении сеанса ввода.</summary>
        private bool finishingEdit;
        /// <summary>Не завершает ввод при программной передаче фокуса дочернему редактору.</summary>
        private bool enteringEditor;
        /// <summary>Ожидается активация редактора после завершения штатной смены фокуса.</summary>
        private bool editorActivationQueued;
        /// <summary>Последнее подтверждённое значение, восстанавливаемое при отмене ввода.</summary>
        private DateTime valueBeforeEdit;
        /// <summary>Код нажатия кнопки мыши в стандартном поле даты.</summary>
        private const int WmLButtonDown = 0x0201;
        /// <summary>Код отпускания кнопки мыши в стандартном поле даты.</summary>
        private const int WmLButtonUp = 0x0202;
        /// <summary>Признак левой кнопки мыши для SendMessage.</summary>
        private const int MkLButton = 0x0001;

        /// <summary>Подключает оформленный ввод, сохраняя исходный DateTimePicker.</summary>
        /// <param name="source">Поле, владеющее датой и существующими обработчиками.</param>
        internal StyledDatePickerSkin(DateTimePicker source)
        {
            picker = source ?? throw new ArgumentNullException(nameof(source));
            visualHeight = picker.Height;
            TabStop = false;
            Cursor = Cursors.IBeam;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            editor = new TextBox
            {
                Name = "ThemedDateEditor",
                BorderStyle = BorderStyle.None,
                AutoSize = false,
                TabStop = false,
                Visible = false,
                ShortcutsEnabled = true
            };
            Controls.Add(editor);
            editor.KeyDown += EditorKeyDown;
            editor.PreviewKeyDown += EditorPreviewKeyDown;
            editor.Leave += EditorLeft;
            editor.MouseDown += EditorMouseDown;
            picker.LocationChanged += SourceChanged;
            picker.SizeChanged += SourceChanged;
            picker.VisibleChanged += SourceChanged;
            picker.EnabledChanged += SourceChanged;
            picker.ParentChanged += SourceParentChanged;
            picker.FontChanged += SourceChanged;
            picker.TextChanged += SourceChanged;
            picker.ValueChanged += SourceChanged;
            picker.Enter += SourceEntered;
            picker.Leave += SourceLeft;
            picker.DropDown += NativeDropDown;
            picker.CloseUp += NativeCloseUp;
            picker.Disposed += SourceDisposed;
            ThemeManager.Changed += ThemeChanged;
            Attach();
        }

        /// <summary>Размещает поле поверх исходного, не меняя его родителя и обработчики.</summary>
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

        /// <summary>Задаёт только визуальную высоту поля.</summary>
        /// <param name="height">Высота с учётом оформления и DPI.</param>
        internal void SetVisualHeight(int height)
        {
            visualHeight = Math.Max(picker.Height, height);
            Synchronize();
        }

        /// <summary>Синхронизирует рамку, редактор и тему с исходным контролом.</summary>
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
                if (Visible != picker.Visible) Visible = picker.Visible;
                Enabled = picker.Enabled;
                Font = picker.Font;
                ApplyEditorTheme();
                LayoutEditor();
                if (Visible && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Сохраняет единый фон текста и общего поля в обеих темах.</summary>
        private void ApplyEditorTheme()
        {
            ThemePalette palette = ThemeManager.Palette;
            editor.BackColor = Enabled ? palette.Surface : palette.ButtonDisabled;
            editor.ForeColor = Enabled ? palette.Text : ThemeVisuals.DisabledText;
            editor.Font = picker.Font;
        }

        /// <summary>Размещает текстовый ввод, оставляя видимой тематическую кнопку.</summary>
        private void LayoutEditor()
        {
            Rectangle button = CalendarButtonBounds;
            int width = Math.Max(1, button.Left - 11);
            int height = Math.Min(Math.Max(1, Font.Height + 5), Math.Max(1, Height - 6));
            editor.Bounds = new Rectangle(7, Math.Max(2, (Height - height) / 2), width, height);
        }

        /// <summary>Рисует постоянное оформление, не открывая поверхность Windows при фокусе.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent == null ? palette.Window : Parent.BackColor);
            if (Width <= 2 || Height <= 2) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle outer = new Rectangle(0, 0, Width - 1, Height - 1);
            Rectangle button = CalendarButtonBounds;
            Color surface = Enabled ? palette.Surface : palette.ButtonDisabled;
            using (GraphicsPath path = ThemeVisuals.InputPath(outer))
            using (SolidBrush brush = new SolidBrush(surface))
            {
                e.Graphics.FillPath(brush, path);
                if (hoveringCalendar && Enabled)
                {
                    GraphicsState state = e.Graphics.Save();
                    try
                    {
                        e.Graphics.SetClip(path);
                        using (var hover = new SolidBrush(Color.FromArgb(75, palette.ButtonHover)))
                            e.Graphics.FillRectangle(hover, button);
                    }
                    finally { e.Graphics.Restore(state); }
                }
                if (button.Left > 5)
                    using (var separator = new Pen(Enabled ? Color.FromArgb(85, palette.Grid) : ThemeVisuals.InputBorderColor(false, false)))
                        e.Graphics.DrawLine(separator, button.Left, 5, button.Left, Height - 6);
                using (var pen = new Pen(ThemeVisuals.InputBorderColor(Enabled,
                    editor.Focused || picker.ContainsFocus)))
                    e.Graphics.DrawPath(pen, path);
            }
            if (!editing)
            {
                Rectangle text = new Rectangle(6, 0, Math.Max(0, button.Left - 9), Height);
                TextRenderer.DrawText(e.Graphics, picker.Text, picker.Font, text,
                    Enabled ? palette.Text : ThemeVisuals.DisabledText,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            DrawCalendarIcon(e.Graphics, button, Enabled ? palette.Text : ThemeVisuals.DisabledText);
        }

        /// <summary>Область кнопки календаря, общая для рисования и обработки нажатий.</summary>
        private Rectangle CalendarButtonBounds
        {
            get
            {
                const int inset = 4;
                int width = Math.Min(20, Math.Max(0, Width - inset * 2));
                return new Rectangle(Math.Max(inset, Width - width - inset), 3,
                    width, Math.Max(0, Height - 6));
            }
        }

        /// <summary>Рисует пиктограмму календаря в принятой палитре.</summary>
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

        /// <summary>Начинает ввод с локальной даты без изменения Value до подтверждения.</summary>
        private void BeginEdit()
        {
            if (editing || !Enabled || picker.IsDisposed) return;
            valueBeforeEdit = picker.Value;
            editing = true;
            editor.Text = picker.Value.ToString("d", CultureInfo.CurrentCulture);
            editor.Visible = true;
            editor.BringToFront();
            enteringEditor = true;
            try
            {
                editor.Focus();
                editor.SelectAll();
            }
            finally { enteringEditor = false; }
            Invalidate();
        }

        /// <summary>Проверяет дату и применяет её через штатное свойство Value.</summary>
        /// <returns>True, если дата распознана и находится в допустимых пределах.</returns>
        private bool CommitEditorValue()
        {
            DateTime date;
            if (!DateTime.TryParse(editor.Text, CultureInfo.CurrentCulture,
                DateTimeStyles.AllowWhiteSpaces, out date))
                return false;
            DateTime candidate = date.Date.Add(valueBeforeEdit.TimeOfDay);
            if (candidate < picker.MinDate || candidate > picker.MaxDate) return false;
            picker.Value = candidate;
            valueBeforeEdit = picker.Value;
            return true;
        }

        /// <summary>Завершает ввод, оставляя недопустимую дату для исправления по Enter.</summary>
        /// <param name="commit">Подтвердить дату; false — отменить ввод без изменения Value.</param>
        /// <param name="cancelInvalid">При потере фокуса отказаться от недопустимого ввода.</param>
        /// <returns>True, если ввод завершён или уже был завершён.</returns>
        private bool FinishEdit(bool commit, bool cancelInvalid = false)
        {
            if (!editing || finishingEdit) return true;
            if (commit && !CommitEditorValue())
            {
                System.Media.SystemSounds.Beep.Play();
                if (!cancelInvalid) return false;
            }
            finishingEdit = true;
            try
            {
                editing = false;
                editor.Visible = false;
                Invalidate();
                return true;
            }
            finally { finishingEdit = false; }
        }

        /// <summary>Перехватывает клик до штатного поля для ввода или выбора даты.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !Enabled || picker.IsDisposed) return;
            if (CalendarButtonBounds.Contains(e.Location) && !picker.ShowUpDown)
            {
                if (FinishEdit(true)) OpenCalendar();
            }
            else BeginEdit();
        }

        /// <summary>Открывает ранее подтверждённый календарь StyledDateTimePicker.</summary>
        private void OpenCalendar()
        {
            StyledDateTimePicker themed = picker as StyledDateTimePicker;
            if (themed != null) { themed.ShowStyledCalendar(); return; }
            if (!picker.IsHandleCreated) return;
            int x = Math.Max(1, picker.ClientSize.Width - 9);
            int y = Math.Max(1, picker.ClientSize.Height / 2);
            IntPtr coordinates = (IntPtr)((y << 16) | (x & 0xffff));
            SendMessage(picker.Handle, WmLButtonDown, (IntPtr)MkLButton, coordinates);
            SendMessage(picker.Handle, WmLButtonUp, IntPtr.Zero, coordinates);
        }

        /// <summary>
        /// Планирует открытие редактора после завершения штатного Enter DateTimePicker.
        /// Не вызывает Focus синхронно внутри события смены фокуса WinForms.
        /// </summary>
        private void SourceEntered(object sender, EventArgs e)
        {
            if (finishingEdit || enteringEditor || editing || calendarOpen ||
                editorActivationQueued || picker.IsDisposed || !picker.IsHandleCreated) return;
            editorActivationQueued = true;
            picker.BeginInvoke((MethodInvoker)ActivateEditorAfterFocus);
        }

        /// <summary>Открывает редактор только если исходное поле сохранило фокус.</summary>
        private void ActivateEditorAfterFocus()
        {
            editorActivationQueued = false;
            if (IsDisposed || picker.IsDisposed || !picker.Focused ||
                finishingEdit || enteringEditor || editing || calendarOpen) return;
            BeginEdit();
        }

        /// <summary>Не убирает редактор, когда фокус переходит в его дочерний TextBox.</summary>
        private void SourceLeft(object sender, EventArgs e)
        {
            if (!enteringEditor && !editor.Focused && !calendarOpen) FinishEdit(true, true);
            Invalidate();
        }

        /// <summary>Оставляет Tab клавишей навигации формы, но обрабатывает её в редакторе.</summary>
        private void EditorPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter ||
                e.KeyCode == Keys.Escape || e.KeyCode == Keys.F4 ||
                (e.KeyCode == Keys.Down && e.Alt)) e.IsInputKey = true;
        }

        /// <summary>Подтверждает или отменяет дату и сохраняет штатную клавиатурную семантику.</summary>
        private void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (FinishEdit(true))
                {
                    StyledDateTimePicker themed = picker as StyledDateTimePicker;
                    themed?.NotifyEditorEnter();
                }
            }
            else if (e.KeyCode == Keys.Escape)
                FinishEdit(false);
            else if (e.KeyCode == Keys.Tab)
            {
                if (FinishEdit(true) && picker.Parent != null)
                    picker.Parent.SelectNextControl(picker, !e.Shift, true, true, true);
            }
            else if (e.KeyCode == Keys.F4 || (e.KeyCode == Keys.Down && e.Alt))
            {
                if (FinishEdit(true)) OpenCalendar();
            }
            else return;
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>Редактирование завершается при переходе фокуса в другой элемент формы.</summary>
        private void EditorLeft(object sender, EventArgs e)
        {
            if (!finishingEdit && !calendarOpen) FinishEdit(true, true);
            Invalidate();
        }

        /// <summary>Не передаёт нажатие дочернего редактора скрытому системному полю.</summary>
        private void EditorMouseDown(object sender, MouseEventArgs e) { Invalidate(); }

        /// <summary>Сохраняет тематическое поле после раскрытия штатного календаря.</summary>
        private void NativeDropDown(object sender, EventArgs e)
        {
            calendarOpen = true;
            FinishEdit(true, true);
            Invalidate();
        }

        /// <summary>Обновляет видимость поля после закрытия календаря.</summary>
        private void NativeCloseUp(object sender, EventArgs e)
        {
            calendarOpen = false;
            Invalidate();
        }

        /// <summary>Подсвечивает только кнопку при наведении.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hover = CalendarButtonBounds.Contains(e.Location);
            if (hover != hoveringCalendar)
            {
                hoveringCalendar = hover;
                Cursor = hover ? Cursors.Hand : Cursors.IBeam;
                Invalidate(CalendarButtonBounds);
            }
        }

        /// <summary>Снимает подсветку после ухода указателя.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveringCalendar = false;
            Cursor = Cursors.IBeam;
            Invalidate();
        }

        /// <summary>Синхронизирует отображение при изменении исходного поля.</summary>
        private void SourceChanged(object sender, EventArgs e) { Synchronize(); }
        /// <summary>Переподключается при смене родительского контейнера.</summary>
        private void SourceParentChanged(object sender, EventArgs e) { Attach(); }
        /// <summary>Учитывает штатную компоновку родительского контейнера.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { Synchronize(); }
        /// <summary>Обновляет палитру и редактор при переключении темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Synchronize(); }
        /// <summary>Удаляет оформление при освобождении исходного DateTimePicker.</summary>
        private void SourceDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Передаёт сообщение штатному полю только для календаря старого типа.</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        /// <summary>Освобождает все подписки и редактор вместе с исходным полем.</summary>
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
                picker.Leave -= SourceLeft;
                picker.DropDown -= NativeDropDown;
                picker.CloseUp -= NativeCloseUp;
                picker.Disposed -= SourceDisposed;
                ThemeManager.Changed -= ThemeChanged;
                if (Parent != null) Parent.Layout -= ParentLayout;
                editor.KeyDown -= EditorKeyDown;
                editor.PreviewKeyDown -= EditorPreviewKeyDown;
                editor.Leave -= EditorLeft;
                editor.MouseDown -= EditorMouseDown;
            }
            base.Dispose(disposing);
        }
    }
}
