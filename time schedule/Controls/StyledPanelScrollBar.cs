using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Визуальное покрытие штатной полосы прокрутки ScrollableControl.
    /// Возвращает управление мышью оригинальному окну; значения и события прокрутки не заменяет.
    /// </summary>
    internal sealed class StyledPanelScrollBar : Control
    {
        /// <summary>Панель с исходной системной полосой прокрутки.</summary>
        private readonly ScrollableControl source;
        /// <summary>Направление отображаемой полосы.</summary>
        private readonly ScrollOrientation orientation;
        /// <summary>Защита от рекурсивного Layout.</summary>
        private bool synchronizing;
        /// <summary>Контрольное состояние для перерисовки после программных переходов.</summary>
        private ScrollSnapshot snapshot;
        /// <summary>Код проверки области мыши, требующий передачу событии нижележащему окну.</summary>
        private const int WmNcHitTest = 0x0084;
        /// <summary>Результат пропуска событий мыши к штатному скроллбару.</summary>
        private static readonly IntPtr HtTransparent = new IntPtr(-1);

        /// <summary>Параметры исходного скроллбара, существенные для его внешнего вида.</summary>
        private struct ScrollSnapshot : IEquatable<ScrollSnapshot>
        {
            internal int Value;
            internal int Maximum;
            internal int LargeChange;
            internal bool Visible;
            /// <summary>Проверяет, изменилось ли состояние прокрутки.</summary>
            public bool Equals(ScrollSnapshot other)
            {
                return Value == other.Value && Maximum == other.Maximum &&
                    LargeChange == other.LargeChange && Visible == other.Visible;
            }
        }

        /// <summary>Создаёт декоративный слой без вмешательства в синхронизацию рабочих панелей.</summary>
        /// <param name="target">Панель, которой продолжает управлять Windows.</param>
        /// <param name="axis">Горизонтальная или вертикальная прокрутка.</param>
        internal StyledPanelScrollBar(ScrollableControl target, ScrollOrientation axis)
        {
            source = target ?? throw new ArgumentNullException(nameof(target));
            orientation = axis;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            source.Scroll += SourceScroll;
            source.SizeChanged += SourceGeometryChanged;
            source.LocationChanged += SourceGeometryChanged;
            source.ParentChanged += SourceParentChanged;
            source.VisibleChanged += SourceGeometryChanged;
            source.Disposed += SourceDisposed;
            ThemeManager.Changed += ThemeChanged;
            Application.Idle += ApplicationIdle;
            AttachToParent();
        }

        /// <summary>Получает штатные параметры прокрутки без изменения их значений.</summary>
        private ScrollProperties Properties
        {
            get { return orientation == ScrollOrientation.HorizontalScroll
                ? (ScrollProperties)source.HorizontalScroll : source.VerticalScroll; }
        }

        /// <summary>Считывает состояние штатного механизма прокрутки.</summary>
        private ScrollSnapshot ReadSnapshot()
        {
            ScrollProperties scroll = Properties;
            return new ScrollSnapshot
            {
                Value = scroll.Value,
                Maximum = scroll.Maximum,
                LargeChange = Math.Max(1, scroll.LargeChange),
                Visible = scroll.Visible
            };
        }

        /// <summary>Размещает слой поверх существующей системной полосы как соседний контрол.</summary>
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

        /// <summary>Синхронизирует координаты и видимость со штатным скроллбаром.</summary>
        private void SyncBounds()
        {
            if (synchronizing || source.IsDisposed || IsDisposed || Parent == null) return;
            synchronizing = true;
            try
            {
                ScrollSnapshot current = ReadSnapshot();
                int verticalWidth = SystemInformation.VerticalScrollBarWidth;
                int horizontalHeight = SystemInformation.HorizontalScrollBarHeight;
                Rectangle bounds = orientation == ScrollOrientation.HorizontalScroll
                    ? new Rectangle(source.Left, source.Bottom - horizontalHeight,
                        Math.Max(1, source.Width - (source.VerticalScroll.Visible ? verticalWidth : 0)),
                        horizontalHeight)
                    : new Rectangle(source.Right - verticalWidth, source.Top, verticalWidth,
                        Math.Max(1, source.Height - (source.HorizontalScroll.Visible ? horizontalHeight : 0)));
                if (Bounds != bounds) Bounds = bounds;
                bool show = source.Visible && current.Visible && current.Maximum > current.LargeChange - 1;
                if (Visible != show) Visible = show;
                if (Visible && Parent.Controls.GetChildIndex(this) != 0) BringToFront();
                if (!snapshot.Equals(current)) { snapshot = current; Invalidate(); }
            }
            finally { synchronizing = false; }
        }

        /// <summary>Рисует матовую дорожку и ползунок, используя только палитру темы.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(palette.Window);
            if (!snapshot.Visible) return;
            bool horizontal = orientation == ScrollOrientation.HorizontalScroll;
            int track = horizontal ? Width : Height;
            if (track < 8) return;
            int length = Math.Max(1, snapshot.Maximum + 1);
            int large = Math.Min(length, Math.Max(1, snapshot.LargeChange));
            int thumbLength = Math.Min(track - 2, Math.Max(24,
                (int)Math.Round((double)(track - 2) * large / length)));
            int travel = Math.Max(0, track - 2 - thumbLength);
            int maximumValue = Math.Max(0, snapshot.Maximum - large + 1);
            int offset = maximumValue == 0 ? 0 :
                (int)Math.Round((double)travel * Math.Max(0, Math.Min(snapshot.Value, maximumValue)) / maximumValue);
            int thickness = horizontal ? Math.Min(8, Height - 2) : Math.Min(8, Width - 2);
            Rectangle thumb = horizontal
                ? new Rectangle(1 + offset, (Height - thickness) / 2, thumbLength, thickness)
                : new Rectangle((Width - thickness) / 2, 1 + offset, thickness, thumbLength);
            if (thumb.Width <= 0 || thumb.Height <= 0) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ThemeVisuals.InputPath(thumb))
            using (SolidBrush brush = new SolidBrush(palette.Button))
                e.Graphics.FillPath(brush, path);
        }

        /// <summary>
        /// Не перехватывает действия мыши: нативный ScrollableControl сохраняет
        /// перетаскивание, колесо, клавиатуру и вызовы существующих Scroll-обработчиков.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmNcHitTest)
            {
                m.Result = HtTransparent;
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>Обновляет отображение после системной прокрутки.</summary>
        private void SourceScroll(object sender, ScrollEventArgs e) { SyncBounds(); }
        /// <summary>Пересчитывает положение при изменении размеров исходной панели.</summary>
        private void SourceGeometryChanged(object sender, EventArgs e) { SyncBounds(); }
        /// <summary>Подключает декоративный слой после изменения контейнера.</summary>
        private void SourceParentChanged(object sender, EventArgs e) { AttachToParent(); }
        /// <summary>Сохраняет положение при компоновке родителя.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { SyncBounds(); }
        /// <summary>Учитывает программную прокрутку, не требующую событий Scroll.</summary>
        private void ApplicationIdle(object sender, EventArgs e) { SyncBounds(); }
        /// <summary>Обновляет цвета после переключения темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }
        /// <summary>Освобождает декоратор вместе с исходной панелью.</summary>
        private void SourceDisposed(object sender, EventArgs e) { Dispose(); }

        /// <summary>Освобождает подписки, не удерживая окно после его закрытия.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                source.Scroll -= SourceScroll;
                source.SizeChanged -= SourceGeometryChanged;
                source.LocationChanged -= SourceGeometryChanged;
                source.ParentChanged -= SourceParentChanged;
                source.VisibleChanged -= SourceGeometryChanged;
                source.Disposed -= SourceDisposed;
                ThemeManager.Changed -= ThemeChanged;
                Application.Idle -= ApplicationIdle;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }
    }
}
