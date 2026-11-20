using System;
using System.Drawing;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Закрывает системный квадрат в пересечении двух полос прокрутки календаря.
    /// Сохраняет штатные диапазоны, события и обработку мыши исходной панели.
    /// </summary>
    internal sealed class StyledPanelScrollCorner : Control
    {
        /// <summary>Исходная прокручиваемая панель, владеющая нативными полосами.</summary>
        private readonly ScrollableControl source;
        /// <summary>Защита от вложенного Layout при изменении размеров.</summary>
        private bool synchronizing;
        /// <summary>Код запроса области окна под указателем мыши.</summary>
        private const int WmNcHitTest = 0x0084;
        /// <summary>Признак, что мышь должна обрабатываться нижележащим окном.</summary>
        private static readonly IntPtr HtTransparent = new IntPtr(-1);

        /// <summary>Создаёт декоративное покрытие системного угла.</summary>
        /// <param name="target">Панель, у которой уже работают штатные полосы прокрутки.</param>
        internal StyledPanelScrollCorner(ScrollableControl target)
        {
            source = target ?? throw new ArgumentNullException(nameof(target));
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            source.SizeChanged += SourceGeometryChanged;
            source.LocationChanged += SourceGeometryChanged;
            source.VisibleChanged += SourceGeometryChanged;
            source.ParentChanged += SourceParentChanged;
            source.Disposed += SourceDisposed;
            ThemeManager.Changed += ThemeChanged;
            Application.Idle += ApplicationIdle;
            AttachToParent();
        }

        /// <summary>Подключает квадрат к контейнеру рядом с исходной панелью.</summary>
        private void AttachToParent()
        {
            if (IsDisposed || source.IsDisposed) return;
            if (!ReferenceEquals(Parent, source.Parent))
            {
                if (Parent != null) Parent.Layout -= ParentLayout;
                if (source.Parent == null) { Visible = false; return; }
                source.Parent.Controls.Add(this);
                source.Parent.Layout += ParentLayout;
            }
            SyncBounds();
        }

        /// <summary>Совмещает покрытие с системным пересечением двух полос.</summary>
        private void SyncBounds()
        {
            if (synchronizing || IsDisposed || source.IsDisposed || Parent == null) return;
            synchronizing = true;
            try
            {
                bool show = source.Visible && source.HorizontalScroll.Visible &&
                    source.VerticalScroll.Visible;
                if (Visible != show) Visible = show;
                if (!show) return;
                int width = SystemInformation.VerticalScrollBarWidth;
                int height = SystemInformation.HorizontalScrollBarHeight;
                Rectangle bounds = new Rectangle(source.Right - width, source.Bottom - height,
                    width, height);
                if (Bounds != bounds) Bounds = bounds;
                if (Parent.Controls.GetChildIndex(this) != 0) BringToFront();
            }
            finally { synchronizing = false; }
        }

        /// <summary>Рисует только цвет фона, не создавая третьего ползунка или рамки.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(ThemeManager.Palette.Window);
        }

        /// <summary>Не перехватывает штатные мышиные действия календарной панели.</summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmNcHitTest)
            {
                m.Result = HtTransparent;
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>Обновляет положение после перемещения исходной панели.</summary>
        private void SourceGeometryChanged(object sender, EventArgs e) { SyncBounds(); }
        /// <summary>Обновляет контейнер после изменения иерархии.</summary>
        private void SourceParentChanged(object sender, EventArgs e) { AttachToParent(); }
        /// <summary>Отслеживает изменения компоновки родительского окна.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { SyncBounds(); }
        /// <summary>Обновляет покрытие при штатной прокрутке и программных переходах.</summary>
        private void ApplicationIdle(object sender, EventArgs e) { SyncBounds(); }
        /// <summary>Перерисовывает только цвет покрытия после смены темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }
        /// <summary>Закрывает покрытие одновременно с его исходной панелью.</summary>
        private void SourceDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Отвязывает все внешние события при уничтожении покрытия.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.SizeChanged -= SourceGeometryChanged;
                source.LocationChanged -= SourceGeometryChanged;
                source.VisibleChanged -= SourceGeometryChanged;
                source.ParentChanged -= SourceParentChanged;
                source.Disposed -= SourceDisposed;
                ThemeManager.Changed -= ThemeChanged;
                Application.Idle -= ApplicationIdle;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
