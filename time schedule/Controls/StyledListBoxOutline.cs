using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Общая плоская граница для ListBox. Не меняет родителя, данные,
    /// выделение и механизм прокрутки исходного списка.
    /// </summary>
    internal sealed class StyledListBoxOutline : Control
    {
        /// <summary>Оформляемый список, сохраняющий штатную обработку событий.</summary>
        private readonly ListBox list;
        /// <summary>Защита от повторной компоновки.</summary>
        private bool synchronizing;

        /// <summary>Создаёт декоратор поверх системной кромки без изменения ListBox.</summary>
        internal StyledListBoxOutline(ListBox target)
        {
            list = target ?? throw new ArgumentNullException(nameof(target));
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            list.Resize += ListGeometryChanged;
            list.LocationChanged += ListGeometryChanged;
            list.VisibleChanged += ListGeometryChanged;
            list.ParentChanged += ListParentChanged;
            list.EnabledChanged += ListGeometryChanged;
            list.Enter += ListFocusChanged;
            list.Leave += ListFocusChanged;
            list.Disposed += ListDisposed;
            ThemeManager.Changed += ThemeChanged;
            AttachToParent();
        }

        /// <summary>Добавляет границу в текущий контейнер списка.</summary>
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
                if (list.Parent == null) return;
                list.Parent.Controls.Add(this);
                list.Parent.Layout += ParentLayout;
            }
            SyncBounds();
        }

        /// <summary>Обновляет размеры при изменении контейнера.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { SyncBounds(); }

        /// <summary>Обновляет рамку после изменения позиции или видимости.</summary>
        private void ListGeometryChanged(object sender, EventArgs e) { SyncBounds(); }

        /// <summary>Перемещает декоратор вместе со списком.</summary>
        private void ListParentChanged(object sender, EventArgs e) { AttachToParent(); }

        /// <summary>Подсвечивает фокус из утверждённой палитры.</summary>
        private void ListFocusChanged(object sender, EventArgs e) { Invalidate(); }

        /// <summary>Перерисовывает рамку при переключении темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }

        /// <summary>Закрывает декоратор, когда исходный список уничтожается.</summary>
        private void ListDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Синхронизирует геометрию, не перестраивая содержимое ListBox.</summary>
        private void SyncBounds()
        {
            if (synchronizing || IsDisposed || list.IsDisposed || list.Parent == null) return;
            synchronizing = true;
            try
            {
                if (Bounds != list.Bounds) Bounds = list.Bounds;
                if (Visible != list.Visible) Visible = list.Visible;
                if (Visible) BringToFront();
                UpdateBorderRegion();
                Invalidate();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Оставляет интерактивной только узкую область рамки; весь список доступен мыши.</summary>
        private void UpdateBorderRegion()
        {
            if (Width < 2 || Height < 2) return;
            int edge = list.BorderStyle == BorderStyle.Fixed3D ? 2 : 1;
            int corner = Math.Min(ThemeVisuals.InputCornerRadius + 1, Math.Min(Width, Height));
            using (var area = new Region(new Rectangle(0, 0, Width, edge)))
            {
                area.Union(new Rectangle(0, Height - edge, Width, edge));
                area.Union(new Rectangle(0, 0, edge, Height));
                area.Union(new Rectangle(Width - edge, 0, edge, Height));
                area.Union(new Rectangle(0, 0, corner, corner));
                area.Union(new Rectangle(Width - corner, 0, corner, corner));
                area.Union(new Rectangle(0, Height - corner, corner, corner));
                area.Union(new Rectangle(Width - corner, Height - corner, corner, corner));
                Region old = Region;
                Region = area.Clone();
                if (old != null) old.Dispose();
            }
        }

        /// <summary>Перекрывает только системную рамку и рисует единую тонкую линию.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 2 || Height < 2) return;
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(list.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var outline = ThemeVisuals.InputPath(new Rectangle(0, 0, Width - 1, Height - 1)))
            using (var pen = new Pen(ThemeVisuals.InputBorderColor(list.Enabled, list.Focused)))
                e.Graphics.DrawPath(pen, outline);
        }

        /// <summary>Освобождает подписки и регион без удерживания закрытых окон.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Changed -= ThemeChanged;
                list.Resize -= ListGeometryChanged;
                list.LocationChanged -= ListGeometryChanged;
                list.VisibleChanged -= ListGeometryChanged;
                list.ParentChanged -= ListParentChanged;
                list.EnabledChanged -= ListGeometryChanged;
                list.Enter -= ListFocusChanged;
                list.Leave -= ListFocusChanged;
                list.Disposed -= ListDisposed;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
