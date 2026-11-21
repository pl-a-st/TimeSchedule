using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Матовая полоса прокрутки поверх системной полосы существующего ListBox.
    /// Не заменяет список и не вмешивается в его выделение, данные и обработчики.
    /// </summary>
    internal sealed class StyledListScrollBar : Control
    {
        /// <summary>Исходный список, сохраняющий свою прокрутку и события.</summary>
        private readonly ListBox list;
        /// <summary>Признак захвата мыши для перемещения ползунка.</summary>
        private bool dragging;
        /// <summary>Отступ указателя относительно верхней границы ползунка.</summary>
        private int dragOffset;
        /// <summary>Наведение на ползунок.</summary>
        private bool hovered;
        /// <summary>Защита синхронизации от повторного Layout.</summary>
        private bool synchronizing;

        /// <summary>Прикрепляет оформление к уже созданному ListBox без изменения его структуры.</summary>
        /// <param name="target">Действующий список WinForms.</param>
        internal StyledListScrollBar(ListBox target)
        {
            list = target ?? throw new ArgumentNullException(nameof(target));
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Width = SystemInformation.VerticalScrollBarWidth;
            TabStop = false;
            Cursor = Cursors.Hand;
            list.Resize += ListGeometryChanged;
            list.LocationChanged += ListGeometryChanged;
            list.VisibleChanged += ListGeometryChanged;
            list.ParentChanged += ListParentChanged;
            list.FontChanged += ListGeometryChanged;
            list.BackColorChanged += ListAppearanceChanged;
            list.SelectedIndexChanged += ListSelectionChanged;
            list.DataSourceChanged += ListGeometryChanged;
            list.MouseWheel += ListMouseWheel;
            list.KeyUp += ListKeyUp;
            list.Disposed += ListDisposed;
            ThemeManager.Changed += ThemeChanged;
            AttachToParent();
        }

        /// <summary>Количество полностью отображаемых строк списка.</summary>
        private int VisibleRows
        {
            get { return Math.Max(1, list.ClientSize.Height / Math.Max(1, list.ItemHeight)); }
        }

        /// <summary>Наибольший допустимый индекс верхней строки.</summary>
        private int MaximumTopIndex
        {
            get { return Math.Max(0, list.Items.Count - VisibleRows); }
        }

        /// <summary>Размещает декоратор справа, не меняя размеров ListBox и его полосы прокрутки.</summary>
        private void AttachToParent()
        {
            if (IsDisposed || list.IsDisposed) return;
            if (!ReferenceEquals(Parent, list.Parent))
            {
                if (Parent != null)
                {
                    Parent.Layout -= ParentLayout;
                    Parent.Controls.Remove(this);
                }
                if (list.Parent == null) { Visible = false; return; }
                list.Parent.Controls.Add(this);
                list.Parent.Layout += ParentLayout;
            }
            SyncBounds();
        }

        /// <summary>Обновляет положение и видимость при компоновке родительского контейнера.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { SyncBounds(); }

        /// <summary>Обновляет положение после изменения геометрии исходного списка.</summary>
        private void ListGeometryChanged(object sender, EventArgs e) { SyncBounds(); }

        /// <summary>Следует за списком при изменении его родителя.</summary>
        private void ListParentChanged(object sender, EventArgs e) { AttachToParent(); }

        /// <summary>Перерисовывает дорожку при изменении фонового цвета исходного списка.</summary>
        private void ListAppearanceChanged(object sender, EventArgs e) { Invalidate(); }

        /// <summary>Пересчитывает видимость и положение полосы после изменения выбора.</summary>
        private void ListSelectionChanged(object sender, EventArgs e) { SyncBounds(); }

        /// <summary>Перерисовывает декоративный ползунок после системной прокрутки мышью.</summary>
        private void ListMouseWheel(object sender, MouseEventArgs e) { ListSelectionChanged(sender, e); }

        /// <summary>Перерисовывает декоративный ползунок при клавиатурной навигации.</summary>
        private void ListKeyUp(object sender, KeyEventArgs e) { ListSelectionChanged(sender, e); }

        /// <summary>Закрывает декоратор вместе со списком.</summary>
        private void ListDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Перерисовывает существующий декоратор новой палитрой.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }

        /// <summary>Возвращает ширину штатной рамки, которую не должен закрывать ползунок.</summary>
        private int ListBorderWidth
        {
            get
            {
                if (list.BorderStyle == BorderStyle.Fixed3D) return SystemInformation.Border3DSize.Width;
                if (list.BorderStyle == BorderStyle.FixedSingle) return SystemInformation.BorderSize.Width;
                return 0;
            }
        }

        /// <summary>Синхронизирует полосу с фактическим положением и длиной списка.</summary>
        private void SyncBounds()
        {
            if (synchronizing || IsDisposed || list.IsDisposed || list.Parent == null) return;
            synchronizing = true;
            try
            {
                int border = ListBorderWidth;
                int scrollbarWidth = SystemInformation.VerticalScrollBarWidth;
                Rectangle rect = new Rectangle(list.Right - scrollbarWidth - border,
                    list.Top + border, scrollbarWidth,
                    Math.Max(1, list.Height - border * 2));
                if (Bounds != rect) Bounds = rect;
                bool shouldShow = list.Visible && list.Enabled && MaximumTopIndex > 0;
                if (Visible != shouldShow) Visible = shouldShow;
                if (shouldShow) BringToFront();
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Вычисляет размер ползунка по реальной доле видимых строк.</summary>
        private Rectangle ThumbBounds
        {
            get
            {
                int count = Math.Max(1, list.Items.Count);
                int trackHeight = Math.Max(0, ClientSize.Height);
                int thumbHeight = Math.Min(trackHeight,
                    Math.Max(24, (int)Math.Round((double)trackHeight * VisibleRows / count)));
                int travel = Math.Max(0, trackHeight - thumbHeight);
                int top = MaximumTopIndex == 0 ? 0 :
                    (int)Math.Round((double)travel * Math.Min(list.TopIndex, MaximumTopIndex) / MaximumTopIndex);
                int thumbWidth = Math.Max(6, SystemInformation.VerticalScrollBarWidth / 2);
                return new Rectangle(Math.Max(0, (Width - thumbWidth) / 2), top + 2,
                    Math.Min(Width, thumbWidth), Math.Max(1, thumbHeight - 4));
            }
        }

        /// <summary>Рисует плоскую дорожку и матовый ползунок средствами GDI+.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(list.BackColor);
            if (MaximumTopIndex == 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle thumb = ThumbBounds;
            using (var path = RoundedThumb(thumb))
            using (var brush = new SolidBrush(dragging || hovered ? palette.ButtonHover : palette.Button))
                e.Graphics.FillPath(brush, path);
        }

        /// <summary>Создаёт мягкие углы только для ползунка, не для всего окна.</summary>
        private static GraphicsPath RoundedThumb(Rectangle bounds)
        {
            var path = new GraphicsPath();
            int radius = Math.Min(4, Math.Min(bounds.Width, bounds.Height) / 2);
            if (radius < 1) { path.AddRectangle(bounds); return path; }
            int d = radius * 2;
            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Начинает перетаскивание либо перемещает список к выбранной позиции дорожки.</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || MaximumTopIndex == 0) return;
            Rectangle thumb = ThumbBounds;
            dragging = true;
            if (list.CanFocus) list.Focus();
            dragOffset = thumb.Contains(e.Location) ? e.Y - thumb.Top : thumb.Height / 2;
            Capture = true;
            ScrollTo(e.Y - dragOffset);
            Invalidate();
        }

        /// <summary>Прокручивает список пропорционально движению ползунка.</summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool nextHover = ThumbBounds.Contains(e.Location);
            if (nextHover != hovered) { hovered = nextHover; Invalidate(); }
            if (dragging) ScrollTo(e.Y - dragOffset);
        }

        /// <summary>Освобождает захват указателя после перетаскивания.</summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            dragging = false;
            Capture = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        /// <summary>Сохраняет штатное управление прокруткой колёсиком мыши.</summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (MaximumTopIndex == 0) return;
            int delta = Math.Max(1, SystemInformation.MouseWheelScrollLines);
            int direction = e.Delta > 0 ? -delta : delta;
            list.TopIndex = Math.Max(0, Math.Min(MaximumTopIndex, list.TopIndex + direction));
            Invalidate();
        }

        /// <summary>Сопоставляет положение указателя с индексом верхней строки.</summary>
        private void ScrollTo(int top)
        {
            int travel = ClientSize.Height - ThumbBounds.Height - 4;
            if (travel < 1 || MaximumTopIndex == 0) return;
            int index = (int)Math.Round((double)Math.Max(0, Math.Min(travel, top)) * MaximumTopIndex / travel);
            list.TopIndex = Math.Max(0, Math.Min(MaximumTopIndex, index));
            Invalidate();
        }

        /// <summary>Удаляет обработчики списка и глобальной темы при уничтожении декоратора.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Changed -= ThemeChanged;
                list.Resize -= ListGeometryChanged;
                list.LocationChanged -= ListGeometryChanged;
                list.VisibleChanged -= ListGeometryChanged;
                list.ParentChanged -= ListParentChanged;
                list.FontChanged -= ListGeometryChanged;
                list.BackColorChanged -= ListAppearanceChanged;
                list.SelectedIndexChanged -= ListSelectionChanged;
                list.DataSourceChanged -= ListGeometryChanged;
                list.MouseWheel -= ListMouseWheel;
                list.KeyUp -= ListKeyUp;
                list.Disposed -= ListDisposed;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
