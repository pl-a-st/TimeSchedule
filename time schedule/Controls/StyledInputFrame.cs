using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Общая плоская рамка штатного элемента ввода. Сохраняет исходный экземпляр,
    /// данные и обработчики, поэтому не подменяет поведение WinForms-контрола.
    /// </summary>
    internal abstract class StyledInputFrame : UserControl
    {
        /// <summary>Исходное поле, сохраняющее управление вводом и фокусом.</summary>
        private readonly Control input;

        /// <summary>Создаёт общую оболочку вокруг существующего поля.</summary>
        /// <param name="control">Оформляемое поле ввода.</param>
        protected StyledInputFrame(Control control)
        {
            input = control ?? throw new ArgumentNullException(nameof(control));
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Margin = Padding.Empty;
            Padding = Padding.Empty;
            BackColor = ThemeManager.Palette.Window;
            ThemeManager.Changed += ThemeChanged;
            input.Enter += InputFocusChanged;
            input.Leave += InputFocusChanged;
            input.EnabledChanged += InputFocusChanged;
            input.Disposed += InputDisposed;
        }

        /// <summary>Подключает рамку к исходному родителю, сохраняя исходные координаты поля.</summary>
        internal bool Attach()
        {
            Control originalParent = input.Parent;
            if (originalParent == null || originalParent.IsDisposed) return false;
            Rectangle originalBounds = input.Bounds;
            AnchorStyles originalAnchor = input.Anchor;
            int originalTabIndex = input.TabIndex;
            int originalIndex = originalParent.Controls.GetChildIndex(input);
            originalParent.SuspendLayout();
            SuspendLayout();
            try
            {
                Font = originalParent.Font;
                Bounds = new Rectangle(originalBounds.Left - 2, originalBounds.Top - 2,
                    originalBounds.Width + 4, originalBounds.Height + 4);
                Anchor = originalAnchor;
                originalParent.Controls.Add(this);
                originalParent.Controls.SetChildIndex(this, originalIndex);
                Controls.Add(input);
                input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                input.Location = new Point(2, 2);
                input.Width = originalBounds.Width;
                input.TabIndex = originalTabIndex;
                TabIndex = originalTabIndex;
            }
            finally
            {
                ResumeLayout(true);
                originalParent.ResumeLayout(true);
            }
            OnAttached();
            UpdateInputBounds();
            Invalidate();
            return true;
        }

        /// <summary>Позволяет специализированной оболочке завершить подключение к полю.</summary>
        protected virtual void OnAttached() { }

        /// <summary>Сохраняет размер поля при изменении размеров содержащей его панели.</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateInputBounds();
        }

        /// <summary>Центрирует однострочное поле внутри рамки, не меняя его высоту.</summary>
        private void UpdateInputBounds()
        {
            if (input == null || input.IsDisposed || input.Parent != this) return;
            int width = Math.Max(1, ClientSize.Width - 4);
            input.Width = width;
            if (input is TextBox text && text.Multiline)
                input.Height = Math.Max(1, ClientSize.Height - 4);
            input.Top = Math.Max(2, (ClientSize.Height - input.Height) / 2);
            input.Left = 2;
        }

        /// <summary>Рисует матовую рамку с радиусом четыре пикселя из общей палитры.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette colors = ThemeManager.Palette;
            e.Graphics.Clear(Parent == null ? colors.Window : Parent.BackColor);
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            if (bounds.Width < 2 || bounds.Height < 2) return;
            Color outline = !input.Enabled ? colors.MutedText :
                input.ContainsFocus ? colors.Focus : colors.Grid;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath shape = ThemeVisuals.InputPath(bounds))
            using (Pen pen = new Pen(outline))
                e.Graphics.DrawPath(pen, shape);
        }

        /// <summary>Обновляет границу при фокусе, блокировке и смене темы.</summary>
        private void InputFocusChanged(object sender, EventArgs e) { Invalidate(); }

        /// <summary>Не удерживает оболочку после уничтожения исходного поля.</summary>
        private void InputDisposed(object sender, EventArgs e) { if (!IsDisposed) Dispose(); }

        /// <summary>Обновляет рамку без изменения значения или режима ввода.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }

        /// <summary>Освобождает подписки и графические ресурсы оболочки.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Changed -= ThemeChanged;
                input.Enter -= InputFocusChanged;
                input.Leave -= InputFocusChanged;
                input.EnabledChanged -= InputFocusChanged;
                input.Disposed -= InputDisposed;
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>Общая рамка обычного TextBox и NumericUpDown без замены их экземпляров.</summary>
    internal sealed class StyledPlainInputFrame : StyledInputFrame
    {
        /// <summary>Сохраняемая строка ввода.</summary>
        private readonly TextBox text;
        /// <summary>Сохраняемое числовое поле.</summary>
        private readonly NumericUpDown number;

        /// <summary>Оформляет существующее текстовое поле.</summary>
        internal StyledPlainInputFrame(TextBox control) : base(control) { text = control; }

        /// <summary>Оформляет существующее числовое поле.</summary>
        internal StyledPlainInputFrame(NumericUpDown control) : base(control) { number = control; }

        /// <summary>Убирает двойную системную рамку, сохраняя режим ввода и значения.</summary>
        protected override void OnAttached()
        {
            if (text != null) text.BorderStyle = BorderStyle.None;
            if (number != null) number.BorderStyle = BorderStyle.None;
        }
    }

    /// <summary>Рамка существующего DateTimePicker с сохранением Value, календаря и событий.</summary>
    internal sealed class StyledDatePickerFrame : StyledInputFrame
    {
        /// <summary>Оформляет существующее поле даты без его замены.</summary>
        internal StyledDatePickerFrame(DateTimePicker picker) : base(picker) { }
    }

    /// <summary>Рамка существующего ComboBox с собственной матовой стрелкой раскрытия.</summary>
    internal sealed class StyledComboBoxFrame : StyledInputFrame
    {
        /// <summary>Оригинальный список с неизменными данными, событиями и клавиатурой.</summary>
        private readonly ComboBox combo;
        /// <summary>Кнопка, заменяющая только рисунок стандартной стрелки.</summary>
        private readonly DropDownGlyph glyph;

        /// <summary>Оформляет существующий список без изменения DropDownStyle.</summary>
        internal StyledComboBoxFrame(ComboBox control) : base(control)
        {
            combo = control;
            glyph = new DropDownGlyph { TabStop = false, Cursor = Cursors.Hand };
            glyph.Click += GlyphClick;
            combo.EnabledChanged += ComboEnabledChanged;
            glyph.Enabled = combo.Enabled;
            Controls.Add(glyph);
        }

        /// <summary>Закрепляет рисунок стрелки поверх штатной области раскрытия.</summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateGlyphBounds();
            glyph.BringToFront();
        }

        /// <summary>Сохраняет правильное положение стрелки при изменении ширины поля.</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateGlyphBounds();
        }

        /// <summary>Размещает стрелку строго внутри исходного ComboBox.</summary>
        private void UpdateGlyphBounds()
        {
            if (combo == null || glyph == null) return;
            int width = Math.Min(18, Math.Max(0, combo.Width));
            glyph.SetBounds(combo.Right - width, combo.Top, width, combo.Height);
        }

        /// <summary>Раскрывает штатный список без изменения порядка выбора или режима ввода.</summary>
        private void GlyphClick(object sender, EventArgs e)
        {
            if (!combo.Enabled) return;
            combo.Focus();
            combo.DroppedDown = !combo.DroppedDown;
        }

        /// <summary>Согласует состояние стрелки с доступностью исходного списка.</summary>
        private void ComboEnabledChanged(object sender, EventArgs e)
        {
            glyph.Enabled = combo.Enabled;
            glyph.Invalidate();
        }

        /// <summary>Освобождает обработчики вместе с общей оболочкой.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                combo.EnabledChanged -= ComboEnabledChanged;
                glyph.Click -= GlyphClick;
            }
            base.Dispose(disposing);
        }

        /// <summary>Матовая пиктограмма раскрытия, общая для редактируемых и обычных ComboBox.</summary>
        private sealed class DropDownGlyph : Control
        {
            /// <summary>Создаёт двойную буферизацию для гладкой отрисовки.</summary>
            internal DropDownGlyph()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                ThemeManager.Changed += ThemeChanged;
            }

            /// <summary>Рисует небольшую стрелку с матовой подложкой выбранной темы.</summary>
            protected override void OnPaint(PaintEventArgs e)
            {
                ThemePalette palette = ThemeManager.Palette;
                e.Graphics.Clear(Enabled ? palette.Button : palette.ButtonDisabled);
                ThemeVisuals.DrawDropDownArrow(e.Graphics, ClientRectangle, Enabled);
            }

            /// <summary>Обновляет пиктограмму при смене темы.</summary>
            private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }

            /// <summary>Не удерживает графическую кнопку подпиской после закрытия формы.</summary>
            protected override void Dispose(bool disposing)
            {
                if (disposing) ThemeManager.Changed -= ThemeChanged;
                base.Dispose(disposing);
            }
        }
    }
}
