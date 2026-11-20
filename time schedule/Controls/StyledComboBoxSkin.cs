using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Матовая поверхность существующего ComboBox в режиме DropDownList.
    /// Исходный список сохраняет элементы, выбор, поиск клавишами и обработчики.
    /// </summary>
    internal sealed class StyledComboBoxSkin : Control
    {
        /// <summary>Оригинальный список, который остаётся владельцем данных и событий.</summary>
        private readonly ComboBox combo;
        /// <summary>Предотвращает рекурсивную компоновку декоративного слоя.</summary>
        private bool synchronizing;
        /// <summary>Признак наведения на кнопку раскрытия.</summary>
        private bool hoverArrow;

        /// <summary>Создаёт поверхностный декоратор без смены родителя ComboBox.</summary>
        internal StyledComboBoxSkin(ComboBox source)
        {
            combo = source ?? throw new ArgumentNullException(nameof(source));
            TabStop = false;
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            combo.LocationChanged += SourceChanged;
            combo.SizeChanged += SourceChanged;
            combo.VisibleChanged += SourceChanged;
            combo.EnabledChanged += SourceChanged;
            combo.ParentChanged += SourceParentChanged;
            combo.SelectedIndexChanged += SourceChanged;
            combo.TextChanged += SourceChanged;
            combo.Enter += SourceChanged;
            combo.Leave += SourceChanged;
            combo.Disposed += SourceDisposed;
            ThemeManager.Changed += ThemeChanged;
            Attach();
        }

        /// <summary>Размещает декоративный слой рядом с исходным списком.</summary>
        private void Attach()
        {
            if (IsDisposed || combo.IsDisposed) return;
            if (Parent != null && !ReferenceEquals(Parent, combo.Parent)) Parent.Layout -= ParentLayout;
            if (combo.Parent == null) { Visible = false; return; }
            if (!ReferenceEquals(Parent, combo.Parent))
            {
                combo.Parent.Controls.Add(this);
                Parent.Layout += ParentLayout;
            }
            Synchronize();
        }

        /// <summary>Повторяет геометрию исходного списка без изменения его Bounds.</summary>
        private void Synchronize()
        {
            if (IsDisposed || combo.IsDisposed || combo.Parent == null) return;
            if (synchronizing) return;
            synchronizing = true;
            try
            {
                if (Bounds != combo.Bounds) Bounds = combo.Bounds;
                Visible = combo.Visible;
                Enabled = combo.Enabled;
                Font = combo.Font;
                if (Visible && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>
        /// Рисует одну общую матовую поверхность вместо нескольких накладных рамок.
        /// Исходный ComboBox остаётся владельцем списка, фокуса и событий выбора.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(Parent == null ? palette.Window : Parent.BackColor);
            Rectangle outer = new Rectangle(0, 0, Width - 1, Height - 1);
            if (outer.Width < 3 || outer.Height < 3) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle arrow = ArrowBounds;
            using (GraphicsPath path = ThemeVisuals.InputPath(outer))
            using (SolidBrush surface = new SolidBrush(Enabled ? palette.Surface : palette.ButtonDisabled))
            using (Pen border = new Pen(ThemeVisuals.InputBorderColor(Enabled, combo.ContainsFocus)))
            {
                e.Graphics.FillPath(surface, path);
                if (hoverArrow && Enabled)
                {
                    // Подсветка только внутри скруглённого поля, без квадратных углов.
                    GraphicsState state = e.Graphics.Save();
                    try
                    {
                        e.Graphics.SetClip(path);
                        using (SolidBrush hover = new SolidBrush(Color.FromArgb(75, palette.ButtonHover)))
                            e.Graphics.FillRectangle(hover, arrow);
                    }
                    finally { e.Graphics.Restore(state); }
                }
                if (arrow.Left > 5)
                {
                    using (Pen separator = new Pen(Color.FromArgb(85, palette.Grid)))
                        e.Graphics.DrawLine(separator, arrow.Left, 5, arrow.Left, Height - 6);
                }
                e.Graphics.DrawPath(border, path);
            }
            ThemeVisuals.DrawDropDownArrow(e.Graphics, arrow, Enabled);
            Rectangle text = new Rectangle(8, 0, Math.Max(0, arrow.Left - 12), Height);
            if (text.Width > 0)
                TextRenderer.DrawText(e.Graphics, combo.Text, combo.Font, text,
                    Enabled ? palette.Text : palette.MutedText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>Область раскрытия для любого размера исходного списка.</summary>
        private Rectangle ArrowBounds
        {
            get { return new Rectangle(Math.Max(4, Width - 24), 3,
                Math.Min(19, Math.Max(0, Width - 8)), Math.Max(0, Height - 6)); }
        }

        /// <summary>Раскрывает штатный ComboBox без модификации его данных и событий.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !combo.Enabled) return;
            combo.Focus();
            combo.DroppedDown = !combo.DroppedDown;
        }

        /// <summary>Сохраняет штатное изменение выбранной строки колесом мыши.</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!combo.Enabled || combo.DroppedDown || combo.Items.Count == 0) return;
            int step = e.Delta > 0 ? -1 : e.Delta < 0 ? 1 : 0;
            if (step == 0) return;
            int current = combo.SelectedIndex;
            int next = Math.Max(0, Math.Min(combo.Items.Count - 1, current + step));
            if (next != current) combo.SelectedIndex = next;
        }

        /// <summary>Отображает подсветку кнопки при наведении указателя.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool hover = ArrowBounds.Contains(e.Location);
            if (hover != hoverArrow) { hoverArrow = hover; Invalidate(ArrowBounds); }
        }

        /// <summary>Снимает подсветку при уходе указателя.</summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoverArrow = false;
            Invalidate();
        }

        /// <summary>Обновляет геометрию или текст вслед за оригиналом.</summary>
        private void SourceChanged(object sender, EventArgs e) { Synchronize(); }
        /// <summary>Переподключает декоративную поверхность при смене родителя.</summary>
        private void SourceParentChanged(object sender, EventArgs e) { Attach(); }
        /// <summary>Повторяет перемещение при изменении компоновки.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { Synchronize(); }
        /// <summary>Применяет новую палитру без изменения выбора.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }
        /// <summary>Удаляет слой вместе с исходным списком.</summary>
        private void SourceDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Снимает все подписки при освобождении.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                combo.LocationChanged -= SourceChanged;
                combo.SizeChanged -= SourceChanged;
                combo.VisibleChanged -= SourceChanged;
                combo.EnabledChanged -= SourceChanged;
                combo.ParentChanged -= SourceParentChanged;
                combo.SelectedIndexChanged -= SourceChanged;
                combo.TextChanged -= SourceChanged;
                combo.Enter -= SourceChanged;
                combo.Leave -= SourceChanged;
                combo.Disposed -= SourceDisposed;
                ThemeManager.Changed -= ThemeChanged;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
