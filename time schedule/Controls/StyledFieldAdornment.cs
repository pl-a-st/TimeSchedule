using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Общая рамка существующего поля без смены родителя, координат и экземпляра контрола.
    /// Используется в компоновке, которая не допускает переноса полей в оболочки.
    /// </summary>
    internal sealed class StyledFieldAdornment : Control
    {
        /// <summary>Оформляемый штатный контрол, сохраняющий свои события и значения.</summary>
        private readonly Control field;
        /// <summary>Стрелка раскрытия для обычного ComboBox, если собственная оболочка не используется.</summary>
        private readonly ComboDropDownGlyph dropDownGlyph;
        /// <summary>Предотвращает рекурсивную компоновку.</summary>
        private bool synchronizing;

        /// <summary>Создаёт рамку над существующим полем без перемещения поля.</summary>
        /// <param name="target">Существующий ComboBox или DateTimePicker.</param>
        internal StyledFieldAdornment(Control target)
        {
            field = target ?? throw new ArgumentNullException(nameof(target));
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            if (field is ComboBox combo) dropDownGlyph = new ComboDropDownGlyph(combo);
            field.LocationChanged += FieldChanged;
            field.SizeChanged += FieldChanged;
            field.VisibleChanged += FieldChanged;
            field.EnabledChanged += FieldChanged;
            field.ParentChanged += FieldParentChanged;
            field.Enter += FieldFocusChanged;
            field.Leave += FieldFocusChanged;
            field.Disposed += FieldDisposed;
            ThemeManager.Changed += ThemeChanged;
            AttachToParent();
        }

        /// <summary>Подключает декоративные компоненты как соседние элементы, а не как родителей поля.</summary>
        private void AttachToParent()
        {
            if (field.IsDisposed || IsDisposed) return;
            if (!ReferenceEquals(Parent, field.Parent))
            {
                if (Parent != null) Parent.Layout -= ParentLayout;
                if (field.Parent == null) { Visible = false; return; }
                field.Parent.Controls.Add(this);
                field.Parent.Layout += ParentLayout;
            }
            if (dropDownGlyph != null) dropDownGlyph.AttachTo(field.Parent);
            SyncBounds();
        }

        /// <summary>Синхронизирует визуальную границу с положением исходного поля.</summary>
        private void SyncBounds()
        {
            if (synchronizing || IsDisposed || field.IsDisposed || field.Parent == null) return;
            synchronizing = true;
            try
            {
                Rectangle bounds = field.Bounds;
                if (Bounds != bounds) Bounds = bounds;
                if (Visible != field.Visible) Visible = field.Visible;
                if (Visible && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
                UpdateHitRegion();
                if (dropDownGlyph != null) dropDownGlyph.SyncWith(field);
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Оставляет доступной всю внутреннюю область штатного поля ввода.</summary>
        private void UpdateHitRegion()
        {
            if (Width < 3 || Height < 3) return;
            using (var border = new Region(new Rectangle(0, 0, Width, 1)))
            {
                border.Union(new Rectangle(0, Height - 1, Width, 1));
                border.Union(new Rectangle(0, 0, 1, Height));
                border.Union(new Rectangle(Width - 1, 0, 1, Height));
                int corner = Math.Min(ThemeVisuals.InputCornerRadius + 1, Math.Min(Width, Height));
                border.Union(new Rectangle(0, 0, corner, corner));
                border.Union(new Rectangle(Width - corner, 0, corner, corner));
                border.Union(new Rectangle(0, Height - corner, corner, corner));
                border.Union(new Rectangle(Width - corner, Height - corner, corner, corner));
                Region old = Region;
                Region = border.Clone();
                if (old != null) old.Dispose();
            }
        }

        /// <summary>Рисует контур из общей палитры без дополнительного фона и теней.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent == null ? palette.Window : Parent.BackColor);
            if (Width < 3 || Height < 3) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ThemeVisuals.InputPath(new Rectangle(0, 0, Width - 1, Height - 1)))
            using (Pen pen = new Pen(ThemeVisuals.InputBorderColor(field.Enabled, field.ContainsFocus)))
                e.Graphics.DrawPath(pen, path);
        }

        /// <summary>Обновляет декоратор после перемещения или смены состояния поля.</summary>
        private void FieldChanged(object sender, EventArgs e) { SyncBounds(); }
        /// <summary>Повторно подключает рамку при внешней смене контейнера.</summary>
        private void FieldParentChanged(object sender, EventArgs e) { AttachToParent(); }
        /// <summary>Обновляет расположение при компоновке соседних элементов.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { SyncBounds(); }
        /// <summary>Отображает фокус на внешней рамке.</summary>
        private void FieldFocusChanged(object sender, EventArgs e)
        {
            Invalidate();
            if (dropDownGlyph != null) dropDownGlyph.Invalidate();
        }
        /// <summary>Применяет активную палитру без смены значений поля.</summary>
        private void ThemeChanged(object sender, EventArgs e)
        {
            Invalidate();
            if (dropDownGlyph != null) dropDownGlyph.Invalidate();
        }
        /// <summary>Закрывает декоратор при уничтожении поля.</summary>
        private void FieldDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Снимает подписки при освобождении декоратора.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                field.LocationChanged -= FieldChanged;
                field.SizeChanged -= FieldChanged;
                field.VisibleChanged -= FieldChanged;
                field.EnabledChanged -= FieldChanged;
                field.ParentChanged -= FieldParentChanged;
                field.Enter -= FieldFocusChanged;
                field.Leave -= FieldFocusChanged;
                field.Disposed -= FieldDisposed;
                ThemeManager.Changed -= ThemeChanged;
                if (Parent != null) Parent.Layout -= ParentLayout;
                if (dropDownGlyph != null) dropDownGlyph.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>Матовая стрелка, расположенная над системной кнопкой ComboBox.</summary>
        private sealed class ComboDropDownGlyph : Control
        {
            /// <summary>Ширина зоны системной кнопки, перекрываемой матовой поверхностью.</summary>
            private const int ButtonWidth = 20;

            /// <summary>Список, который открывается исходным механизмом WinForms.</summary>
            private readonly ComboBox combo;
            /// <summary>Наведение влияет только на вид кнопки, не на поведение раскрытия.</summary>
            private bool hovered;

            /// <summary>Создаёт только визуальный элемент; сам список остаётся штатным.</summary>
            internal ComboDropDownGlyph(ComboBox target)
            {
                combo = target;
                TabStop = false;
                Cursor = Cursors.Hand;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            /// <summary>Подключает стрелку к текущему контейнеру списка.</summary>
            internal void AttachTo(Control parent)
            {
                if (parent != null && !ReferenceEquals(Parent, parent)) parent.Controls.Add(this);
            }

            /// <summary>Повторяет геометрию кнопки раскрытия исходного ComboBox.</summary>
            internal void SyncWith(Control field)
            {
                if (Parent == null) return;
                int width = Math.Min(ButtonWidth, field.Width);
                Rectangle bounds = new Rectangle(field.Right - width, field.Top + 1, width,
                    Math.Max(1, field.Height - 2));
                if (Bounds != bounds) Bounds = bounds;
                Visible = field.Visible;
                Enabled = field.Enabled;
                if (Visible && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
            }

            /// <summary>Рисует матовую стрелку без отдельного прямоугольного блока.</summary>
            protected override void OnPaint(PaintEventArgs e)
            {
                ThemePalette palette = ThemeManager.Palette;
                e.Graphics.Clear(Enabled ? palette.Surface : palette.ButtonDisabled);
                if (hovered && Enabled)
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(60, palette.ButtonHover)))
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                }
                using (Pen separator = new Pen(Color.FromArgb(95, palette.Grid)))
                    e.Graphics.DrawLine(separator, 0, 4, 0, Height - 5);
                Rectangle glyph = new Rectangle(1, 2, Math.Max(0, Width - 2),
                    Math.Max(0, Height - 4));
                ThemeVisuals.DrawDropDownArrow(e.Graphics, glyph, Enabled);
                DrawOuterBorder(e.Graphics);
            }

            /// <summary>
            /// Восстанавливает участок общей рамки поверх заливки системной кнопки.
            /// Контур строится в координатах исходного ComboBox, без смещения самого поля.
            /// </summary>
            /// <param name="graphics">Графический контекст области кнопки.</param>
            private void DrawOuterBorder(Graphics graphics)
            {
                Rectangle outline = new Rectangle(combo.Left - Left, combo.Top - Top,
                    combo.Width - 1, combo.Height - 1);
                if (outline.Width < 3 || outline.Height < 3) return;
                SmoothingMode previous = graphics.SmoothingMode;
                try
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = ThemeVisuals.InputPath(outline))
                    using (Pen border = new Pen(ThemeVisuals.InputBorderColor(Enabled, combo.ContainsFocus)))
                        graphics.DrawPath(border, path);
                }
                finally { graphics.SmoothingMode = previous; }
            }

            /// <summary>Подсвечивает только область стрелки, не меняя данные ComboBox.</summary>
            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                hovered = true;
                Invalidate();
            }

            /// <summary>Восстанавливает матовую поверхность при уходе указателя.</summary>
            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                hovered = false;
                Invalidate();
            }

            /// <summary>Открывает штатный список, оставляя его выбор и события прежними.</summary>
            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                if (combo.IsDisposed || !combo.Enabled) return;
                combo.Focus();
                combo.DroppedDown = !combo.DroppedDown;
            }
        }
    }
}
