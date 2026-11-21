using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>
    /// Отрисовывает общую рамку и полосы прокрутки поверх штатного TreeView.
    /// Пропускает мышь к дереву, не изменяет его HWND, узлы, выбор и положение прокрутки.
    /// </summary>
    internal sealed class StyledTreeViewOverlay : Control
    {
        /// <summary>Дерево, которому принадлежит штатная обработка мыши и прокрутки.</summary>
        private readonly TreeView tree;
        /// <summary>Защита от повторного Layout после изменения координат декоратора.</summary>
        private bool syncing;
        /// <summary>Последнее считанное состояние вертикальной прокрутки.</summary>
        private ScrollState vertical;
        /// <summary>Последнее считанное состояние горизонтальной прокрутки.</summary>
        private ScrollState horizontal;
        /// <summary>Признак инициализированного состояния полос.</summary>
        private bool hasSnapshot;
        /// <summary>Слежение за нативными сообщениями прокрутки без дополнительных таймеров.</summary>
        private readonly TreeScrollMessages scrollMessages;
        /// <summary>Признак перетаскивания вертикального ползунка в системном TreeView.</summary>
        private bool trackingVertical;
        /// <summary>Признак перетаскивания горизонтального ползунка в системном TreeView.</summary>
        private bool trackingHorizontal;
        /// <summary>Команда, обновляющая положение ползунка во время перетаскивания.</summary>
        private const int SbThumbTrack = 5;
        /// <summary>Сообщение завершения перетаскивания ползунка.</summary>
        private const int SbEndScroll = 8;
        /// <summary>Команда фиксации выбранного положения ползунка.</summary>
        private const int SbThumbPosition = 4;
        /// <summary>Сообщение нативной вертикальной прокрутки.</summary>
        private const int WmVScroll = 0x0115;
        /// <summary>Сообщение нативной горизонтальной прокрутки.</summary>
        private const int WmHScroll = 0x0114;
        /// <summary>Сообщение Windows для прозрачной обработки мыши.</summary>
        private const int WmNcHitTest = 0x0084;
        /// <summary>Передаёт указатель исходному дереву под декоратором.</summary>
        private static readonly IntPtr HitTransparent = new IntPtr(-1);
        /// <summary>Флаг запроса положения, страницы и диапазона прокрутки.</summary>
        private const uint SifAll = 0x17;
        /// <summary>Горизонтальная системная полоса.</summary>
        private const int SbHorz = 0;
        /// <summary>Вертикальная системная полоса.</summary>
        private const int SbVert = 1;

        /// <summary>Состояние одной нативной полосы прокрутки TreeView.</summary>
        private struct ScrollState : IEquatable<ScrollState>
        {
            internal int Minimum;
            internal int Maximum;
            internal int Position;
            internal uint Page;
            internal bool Show;

            /// <summary>Сравнивает только параметры, определяющие внешний вид ползунка.</summary>
            public bool Equals(ScrollState other)
            {
                return Minimum == other.Minimum && Maximum == other.Maximum &&
                    Position == other.Position && Page == other.Page && Show == other.Show;
            }
        }

        /// <summary>Win32-описание текущего состояния системного скроллбара.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ScrollInfo
        {
            public uint Size, Mask;
            public int Minimum, Maximum;
            public uint Page;
            public int Position, TrackPosition;
        }

        /// <summary>Подписывается на события штатной полосы без подмены алгоритма прокрутки.</summary>
        private sealed class TreeScrollMessages : NativeWindow
        {
            /// <summary>Владелец декоративного слоя.</summary>
            private readonly StyledTreeViewOverlay owner;

            /// <summary>Создаёт подписку на текущий дескриптор дерева.</summary>
            internal TreeScrollMessages(StyledTreeViewOverlay target) { owner = target; }

            /// <summary>Читает текущее промежуточное положение после штатной обработки сообщения.</summary>
            protected override void WndProc(ref Message message)
            {
                int code = message.Msg == WmVScroll || message.Msg == WmHScroll
                    ? (int)(message.WParam.ToInt64() & 0xFFFF) : -1;
                int kind = message.Msg;
                base.WndProc(ref message);
                if (code >= 0 && !owner.IsDisposed)
                    owner.NativeScrollProcessed(kind, code);
            }
        }

        /// <summary>Создаёт оформляющий слой без нового прокручиваемого контейнера.</summary>
        internal StyledTreeViewOverlay(TreeView source)
        {
            tree = source ?? throw new ArgumentNullException(nameof(source));
            scrollMessages = new TreeScrollMessages(this);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            tree.HandleCreated += TreeHandleCreated;
            tree.HandleDestroyed += TreeHandleDestroyed;
            tree.SizeChanged += TreeGeometryChanged;
            tree.LocationChanged += TreeGeometryChanged;
            tree.VisibleChanged += TreeGeometryChanged;
            tree.ParentChanged += TreeParentChanged;
            tree.GotFocus += TreeAppearanceChanged;
            tree.LostFocus += TreeAppearanceChanged;
            tree.AfterExpand += TreeNodesChanged;
            tree.AfterCollapse += TreeNodesChanged;
            tree.AfterSelect += TreeNodesChanged;
            tree.MouseWheel += TreeMouseWheel;
            tree.Disposed += TreeDisposed;
            ThemeManager.Changed += ThemeChanged;
            Application.Idle += ApplicationIdle;
            if (tree.IsHandleCreated) TreeHandleCreated(tree, EventArgs.Empty);
            AttachToParent();
        }

        /// <summary>Подключает наблюдение при создании или пересоздании HWND TreeView.</summary>
        private void TreeHandleCreated(object sender, EventArgs e)
        {
            if (scrollMessages.Handle != IntPtr.Zero) scrollMessages.ReleaseHandle();
            if (!tree.IsDisposed && tree.IsHandleCreated)
                scrollMessages.AssignHandle(tree.Handle);
            hasSnapshot = false;
        }

        /// <summary>Отключает наблюдение при уничтожении HWND без вмешательства в дерево.</summary>
        private void TreeHandleDestroyed(object sender, EventArgs e)
        {
            if (scrollMessages.Handle != IntPtr.Zero) scrollMessages.ReleaseHandle();
            trackingVertical = trackingHorizontal = false;
            hasSnapshot = false;
        }

        /// <summary>Синхронизирует декоративный ползунок с сообщениями SB_THUMBTRACK.</summary>
        private void NativeScrollProcessed(int message, int command)
        {
            bool tracking = command == SbThumbTrack;
            if (message == WmVScroll)
            {
                if (tracking) trackingVertical = true;
                else if (command == SbEndScroll || command == SbThumbPosition)
                    trackingVertical = false;
            }
            else if (message == WmHScroll)
            {
                if (tracking) trackingHorizontal = true;
                else if (command == SbEndScroll || command == SbThumbPosition)
                    trackingHorizontal = false;
            }
            RefreshScroll();
        }

        /// <summary>Считывает положение нативной полосы без изменения её состояния.</summary>
        private ScrollState ReadScroll(int bar)
        {
            if (!tree.IsHandleCreated || !tree.Scrollable) return default(ScrollState);
            var info = new ScrollInfo { Size = (uint)Marshal.SizeOf(typeof(ScrollInfo)), Mask = SifAll };
            if (!GetScrollInfo(tree.Handle, bar, ref info)) return default(ScrollState);
            return new ScrollState
            {
                Minimum = info.Minimum, Maximum = info.Maximum,
                Page = info.Page,
                Position = (bar == SbVert ? trackingVertical : trackingHorizontal)
                    ? info.TrackPosition : info.Position,
                Show = info.Page != 0 && info.Maximum - info.Minimum + 1 > info.Page
            };
        }

        /// <summary>Прикрепляет слой к родителю дерева без изменения иерархии TreeView.</summary>
        private void AttachToParent()
        {
            if (IsDisposed || tree.IsDisposed) return;
            if (!ReferenceEquals(Parent, tree.Parent))
            {
                if (Parent != null) Parent.Layout -= ParentLayout;
                if (tree.Parent == null) { Visible = false; return; }
                tree.Parent.Controls.Add(this);
                tree.Parent.Layout += ParentLayout;
            }
            SyncBounds();
        }

        /// <summary>Обновляет рамку и полосы при изменении размеров контейнера.</summary>
        private void ParentLayout(object sender, LayoutEventArgs e) { SyncBounds(); }
        /// <summary>Обновляет рамку и полосы при изменении дерева.</summary>
        private void TreeGeometryChanged(object sender, EventArgs e) { SyncBounds(); }
        /// <summary>Следует за деревом при смене контейнера.</summary>
        private void TreeParentChanged(object sender, EventArgs e) { AttachToParent(); }
        /// <summary>Обновляет состояние фокуса.</summary>
        private void TreeAppearanceChanged(object sender, EventArgs e) { Invalidate(); }
        /// <summary>Обновляет скроллбары после раскрытия, свёртывания или выбора узла.</summary>
        private void TreeNodesChanged(object sender, TreeViewEventArgs e) { RefreshScroll(); }
        /// <summary>Обновляет ползунок после прокрутки колесом.</summary>
        private void TreeMouseWheel(object sender, MouseEventArgs e) { RefreshScroll(); }
        /// <summary>Снимает визуальный слой после уничтожения дерева.</summary>
        private void TreeDisposed(object sender, EventArgs e) { Dispose(); }
        /// <summary>Перерисовывает рамку и полосы в новой теме.</summary>
        private void ThemeChanged(object sender, EventArgs e) { Invalidate(); }
        /// <summary>Отслеживает прокрутку стрелками, PgUp/PgDn и системной полосой.</summary>
        private void ApplicationIdle(object sender, EventArgs e) { RefreshScroll(); }

        /// <summary>Не занимает пространство узлов и не меняет расположение штатных контролов.</summary>
        private void SyncBounds()
        {
            if (syncing || IsDisposed || tree.IsDisposed || tree.Parent == null) return;
            syncing = true;
            try
            {
                if (Bounds != tree.Bounds) Bounds = tree.Bounds;
                if (Visible != tree.Visible) Visible = tree.Visible;
                RefreshScroll();
                if (Visible) BringToFront();
            }
            finally { syncing = false; }
        }

        /// <summary>Проверяет системные диапазоны и обновляет только изменившиеся ползунки.</summary>
        private void RefreshScroll()
        {
            if (IsDisposed || tree.IsDisposed || !tree.IsHandleCreated || !tree.Visible) return;
            ScrollState v = ReadScroll(SbVert), h = ReadScroll(SbHorz);
            if (!hasSnapshot || !v.Equals(vertical) || !h.Equals(horizontal))
            {
                vertical = v;
                horizontal = h;
                hasSnapshot = true;
                UpdateVisibleRegion();
                Invalidate();
            }
        }

        /// <summary>Ограничивает отрисовку рамкой и нативными областями полос прокрутки.</summary>
        private void UpdateVisibleRegion()
        {
            if (Width < 2 || Height < 2) return;
            int edge = tree.BorderStyle == BorderStyle.Fixed3D ? 2 : 1;
            using (var region = new Region(new Rectangle(0, 0, Width, edge)))
            {
                region.Union(new Rectangle(0, Height - edge, Width, edge));
                region.Union(new Rectangle(0, 0, edge, Height));
                region.Union(new Rectangle(Width - edge, 0, edge, Height));
                if (vertical.Show) region.Union(VerticalTrack());
                if (horizontal.Show) region.Union(HorizontalTrack());
                if (vertical.Show && horizontal.Show)
                {
                    Rectangle verticalTrack = VerticalTrack();
                    Rectangle horizontalTrack = HorizontalTrack();
                    region.Union(new Rectangle(verticalTrack.Left, horizontalTrack.Top,
                        verticalTrack.Width, horizontalTrack.Height));
                }
                Region previous = Region;
                Region = region.Clone();
                if (previous != null) previous.Dispose();
            }
        }

        /// <summary>Считывает ширину вертикального штатного скроллбара в текущем DPI Windows.</summary>
        private Rectangle VerticalTrack()
        {
            int edge = tree.BorderStyle == BorderStyle.Fixed3D ? 2 : 1;
            int width = SystemInformation.VerticalScrollBarWidth;
            int height = Height - edge * 2 - (horizontal.Show ? SystemInformation.HorizontalScrollBarHeight : 0);
            return new Rectangle(Width - edge - width, edge, width, Math.Max(1, height));
        }

        /// <summary>Считывает высоту горизонтального штатного скроллбара в текущем DPI Windows.</summary>
        private Rectangle HorizontalTrack()
        {
            int edge = tree.BorderStyle == BorderStyle.Fixed3D ? 2 : 1;
            int height = SystemInformation.HorizontalScrollBarHeight;
            int width = Width - edge * 2 - (vertical.Show ? SystemInformation.VerticalScrollBarWidth : 0);
            return new Rectangle(edge, Height - edge - height, Math.Max(1, width), height);
        }

        /// <summary>Рисует прозрачную для ввода общую рамку и две полосы в существующих областях.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 3 || Height < 3) return;
            ThemePalette palette = ThemeManager.Palette;
            e.Graphics.Clear(palette.Surface);
            if (vertical.Show) DrawTrack(e.Graphics, VerticalTrack(), vertical, false);
            if (horizontal.Show) DrawTrack(e.Graphics, HorizontalTrack(), horizontal, true);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = ThemeVisuals.InputPath(new Rectangle(0, 0, Width - 1, Height - 1)))
            using (Pen pen = new Pen(ThemeVisuals.InputBorderColor(tree.Enabled, tree.Focused)))
                e.Graphics.DrawPath(pen, path);
        }

        /// <summary>Вычисляет положение и длину декоративного ползунка по системному диапазону.</summary>
        private static void DrawTrack(Graphics graphics, Rectangle track, ScrollState state, bool horizontalAxis)
        {
            int length = horizontalAxis ? track.Width : track.Height;
            if (!state.Show || length <= 10) return;
            int full = Math.Max(1, state.Maximum - state.Minimum + 1);
            int page = Math.Max(1, Math.Min(full, (int)state.Page));
            int thumb = Math.Min(length - 4, Math.Max(22, (int)Math.Round((double)length * page / full)));
            int travel = Math.Max(0, length - thumb - 4);
            int maximumPosition = Math.Max(1, full - page);
            int start = 2 + (int)Math.Round((double)travel *
                Math.Max(0, Math.Min(state.Position - state.Minimum, maximumPosition)) / maximumPosition);
            Rectangle bounds = horizontalAxis
                ? new Rectangle(track.Left + start, track.Top + (track.Height - 7) / 2, thumb, 7)
                : new Rectangle(track.Left + (track.Width - 7) / 2, track.Top + start, 7, thumb);
            using (GraphicsPath path = ThemeVisuals.InputPath(bounds))
            using (SolidBrush brush = new SolidBrush(ThemeManager.Palette.ButtonHover))
                graphics.FillPath(brush, path);
        }

        /// <summary>Передаёт любое действие мыши дереву и его системным скроллбарам.</summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmNcHitTest) { m.Result = HitTransparent; return; }
            base.WndProc(ref m);
        }

        /// <summary>Освобождает подписки и графический декоратор без удержания TreeView.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.Idle -= ApplicationIdle;
                tree.HandleCreated -= TreeHandleCreated;
                tree.HandleDestroyed -= TreeHandleDestroyed;
                if (scrollMessages.Handle != IntPtr.Zero) scrollMessages.ReleaseHandle();
                ThemeManager.Changed -= ThemeChanged;
                tree.SizeChanged -= TreeGeometryChanged;
                tree.LocationChanged -= TreeGeometryChanged;
                tree.VisibleChanged -= TreeGeometryChanged;
                tree.ParentChanged -= TreeParentChanged;
                tree.GotFocus -= TreeAppearanceChanged;
                tree.LostFocus -= TreeAppearanceChanged;
                tree.AfterExpand -= TreeNodesChanged;
                tree.AfterCollapse -= TreeNodesChanged;
                tree.AfterSelect -= TreeNodesChanged;
                tree.MouseWheel -= TreeMouseWheel;
                tree.Disposed -= TreeDisposed;
                if (Parent != null) Parent.Layout -= ParentLayout;
            }
            base.Dispose(disposing);
        }

        /// <summary>Получает native-диапазон TreeView, включая автоскрываемые полосы.</summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetScrollInfo(IntPtr window, int bar, ref ScrollInfo info);
    }
}
