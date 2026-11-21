using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Базовая форма с собственным заголовком и штатной обработкой границ окна Windows.</summary>
    public class StyledWindowForm : Form
    {
        /// <summary>Заголовок, оформляемый общей палитрой.</summary>
        private readonly StyledTitleBar titleBar;
        /// <summary>Контейнер исходных элементов, сохраняемый при перестройке компоновки.</summary>
        private Panel clientContent;
        /// <summary>Контейнеры формы, отслеживаемые для оформления полей, добавляемых после открытия.</summary>
        private readonly HashSet<Control> observedThemeContainers = new HashSet<Control>();
        /// <summary>Не допускает повторного подключения декораторов при вложенных событиях ControlAdded.</summary>
        private bool applyingAddedControlTheme;
        /// <summary>Защита от повторного изменения компоновки при пересчёте рамки.</summary>
        private bool updatingFrame;
        /// <summary>Признак доступности изменения размеров для текущего типа окна.</summary>
        private bool allowResize = true;
        /// <summary>Ширина области изменения размеров в физических пикселях.</summary>
        private const int ResizeBorder = 7;
        private const int WmNcHitTest = 0x0084;
        private const int WmNcCalcSize = 0x0083;
        private const int WmNcPaint = 0x0085;
        private const int WmNcActivate = 0x0086;
        private const int WmGetMinMaxInfo = 0x0024;
        private const int HtClient = 1;
        private const int HtCaption = 2;
        private const int HtLeft = 10;
        private const int HtRight = 11;
        private const int HtTop = 12;
        private const int HtTopLeft = 13;
        private const int HtTopRight = 14;
        private const int HtBottom = 15;
        private const int HtBottomLeft = 16;
        private const int HtBottomRight = 17;
        private const int WsCaption = 0x00C00000;
        private const int WsThickFrame = 0x00040000;
        private const int WsMinimizeBox = 0x00020000;
        private const int WsMaximizeBox = 0x00010000;
        private const int WsSysMenu = 0x00080000;
        private const int MonitorDefaultToNearest = 2;

        /// <summary>Оформляет только окно-потомок, не затрагивая остальные формы приложения.</summary>
        protected StyledWindowForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            titleBar = new StyledTitleBar(this);
            Controls.Add(titleBar);
            titleBar.BringToFront();
            ThemeManager.Changed += WindowThemeChanged;
            Disposed += WindowDisposed;
            ApplyWindowFrame();
        }

        /// <summary>Нужно ли размещать содержимое под заголовком автоматически после InitializeComponent.</summary>
        protected virtual bool AutoPlaceClientContent { get { return true; } }

        /// <summary>Высота собственного заголовка для позиционирования диалогов до первого показа.</summary>
        protected int WindowChromeHeight { get { return titleBar.Height; } }

        /// <summary>Показывать переключатель темы в заголовке (по умолчанию только у главной формы).</summary>
        protected virtual bool ShowThemeSwitch { get { return false; } }

        /// <summary>Оформлять ли стандартные элементы автоматически при открытии окна.</summary>
        protected virtual bool AutoThemeStandardControls { get { return true; } }

        /// <summary>Коллекция рабочих контролов независимо от размещения общего заголовка.</summary>
        protected Control.ControlCollection WindowContentControls
        {
            get { return clientContent != null ? clientContent.Controls : Controls; }
        }

        /// <summary>Централизованно подключает заголовок к наследующим формам до их обработчиков Load.</summary>
        protected override void OnLoad(EventArgs e)
        {
            titleBar.ConfigureThemeSwitch(ShowThemeSwitch);
            if (AutoPlaceClientContent && clientContent == null)
            {
                bool fixedWindow = FormBorderStyle == FormBorderStyle.FixedDialog ||
                    FormBorderStyle == FormBorderStyle.FixedSingle ||
                    FormBorderStyle == FormBorderStyle.Fixed3D ||
                    FormBorderStyle == FormBorderStyle.FixedToolWindow;
                allowResize = !fixedWindow;
                Size contentSize = ClientSize;
                FormBorderStyle = FormBorderStyle.None;
                ClientSize = contentSize;
                titleBar.ConfigureWindowButtons(MinimizeBox, MaximizeBox && allowResize);
                PlaceClientContentBelowTitleBar();
            }
            if (AutoThemeStandardControls)
                ThemeControlStyler.Apply(this, null);
            base.OnLoad(e);
        }

        /// <summary>Подключает оформление и декораторы полей после завершения первоначальной компоновки.</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RefreshStandardControlTheme();
            if (AutoThemeStandardControls)
                ObserveThemeContainers(this);
        }

        /// <summary>
        /// Подписывает контейнеры формы на появление вложенных полей без вмешательства в Designer.
        /// </summary>
        /// <param name="root">Контейнер с уже существующими или будущими элементами.</param>
        private void ObserveThemeContainers(Control root)
        {
            if (root == null || root.IsDisposed || root is StyledTitleBar ||
                root is StyledInputFrame) return;
            bool container = root is Form || root is Panel || root is GroupBox ||
                root is TabControl || root is TabPage || root is UserControl ||
                root.Controls.Count > 0;
            if (container && observedThemeContainers.Add(root))
            {
                root.ControlAdded += ThemeContainerControlAdded;
                root.ControlRemoved += ThemeContainerControlRemoved;
            }
            foreach (Control child in root.Controls)
                ObserveThemeContainers(child);
        }

        /// <summary>Снимает наблюдение с контейнера, удалённого из формы или перенесённого в другое окно.</summary>
        /// <param name="root">Удаляемый контейнер вместе с его дочерними элементами.</param>
        private void UnobserveThemeContainers(Control root)
        {
            if (root == null) return;
            if (observedThemeContainers.Remove(root))
            {
                root.ControlAdded -= ThemeContainerControlAdded;
                root.ControlRemoved -= ThemeContainerControlRemoved;
            }
            foreach (Control child in root.Controls)
                UnobserveThemeContainers(child);
        }

        /// <summary>Применяет единую тему и штатные адаптеры только к новому поддереву контролов.</summary>
        private void ThemeContainerControlAdded(object sender, ControlEventArgs e)
        {
            Control added = e.Control;
            if (added == null || added.IsDisposed || applyingAddedControlTheme ||
                IsDisposed || Disposing || !AutoThemeStandardControls ||
                added is StyledInputFrame || added is StyledFieldAdornment ||
                added is StyledDatePickerSkin || added is StyledComboBoxSkin ||
                added is StyledListScrollBar || added is StyledListBoxOutline ||
                added is StyledNumericSpinner || added is StyledTreeViewOverlay ||
                added is StyledPanelScrollBar || added is StyledPanelScrollCorner ||
                added is StyledTitleBar) return;
            applyingAddedControlTheme = true;
            try
            {
                ObserveThemeContainers(added);
                ThemeControlStyler.Apply(added, null);
                ThemeControlStyler.EnsureInputFrames(added);
                ThemeControlStyler.EnsureInputSkins(added);
                ThemeControlStyler.EnsureListScrollBars(added);
                ThemeControlStyler.EnsureTreeViewOverlays(added);
            }
            finally { applyingAddedControlTheme = false; }
        }

        /// <summary>Освобождает подписки на вложенный контейнер при его удалении из формы.</summary>
        private void ThemeContainerControlRemoved(object sender, ControlEventArgs e)
        {
            UnobserveThemeContainers(e.Control);
        }

        /// <summary>
        /// Применяет утверждённую тему к стандартным и добавленным после открытия полям.
        /// Повторные вызовы не создают вторые декораторы или подписки.
        /// </summary>
        protected void RefreshStandardControlTheme()
        {
            if (!AutoThemeStandardControls || IsDisposed || Disposing) return;
            ThemeControlStyler.Apply(this, null);
            if (!IsHandleCreated) return;
            ThemeControlStyler.EnsureInputFrames(this);
            ThemeControlStyler.EnsureInputSkins(this);
            ThemeControlStyler.EnsureListScrollBars(this);
            ThemeControlStyler.EnsureTreeViewOverlays(this);
        }

        /// <summary>Обновляет рамку и стандартные контролы уже открытого окна без их пересоздания.</summary>
        private void WindowThemeChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;
            ApplyWindowFrame();
            if (IsHandleCreated)
                RefreshStandardControlTheme();
        }

        /// <summary>Освобождает подписку на общую тему окна.</summary>
        private void WindowDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= WindowThemeChanged;
            foreach (Control container in observedThemeContainers)
            {
                container.ControlAdded -= ThemeContainerControlAdded;
                container.ControlRemoved -= ThemeContainerControlRemoved;
            }
            observedThemeContainers.Clear();
            Disposed -= WindowDisposed;
        }

        /// <summary>Обновляет внешнюю геометрию окна и сохраняет место для рамки.</summary>
        private void ApplyWindowFrame()
        {
            if (updatingFrame || titleBar == null || IsDisposed) return;
            updatingFrame = true;
            try
            {
                int inset = ThemeWindowFrame.ShouldDraw(this) ? 1 : 0;
                if (Padding.Left != inset || Padding.Top != inset ||
                    Padding.Right != inset || Padding.Bottom != inset)
                    Padding = new Padding(inset);
                Color frameBackground = inset != 0 ? ThemeManager.Palette.Grid : ThemeManager.Palette.Window;
                if (BackColor != frameBackground) BackColor = frameBackground;
                ThemeWindowFrame.Apply(this);
                if (clientContent != null && !clientContent.IsDisposed)
                    clientContent.Bounds = new Rectangle(inset, titleBar.Bottom,
                        Math.Max(0, ClientSize.Width - inset * 2),
                        Math.Max(0, ClientSize.Height - titleBar.Bottom - inset));
            }
            finally { updatingFrame = false; }
        }

        /// <summary>Сохраняет видимую кромку светлой формы при повторном применении цветов дочерней формой.</summary>
        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            ApplyWindowFrame();
        }

        /// <summary>Применяет системное скругление после создания дескриптора окна.</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyWindowFrame();
        }

        /// <summary>Обновляет скругление при изменении размеров и состояния окна.</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyWindowFrame();
        }

        /// <summary>Рисует внешнюю линию поверх рамочного отступа.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            ThemeWindowFrame.Draw(this, e.Graphics);
        }

        /// <summary>Помещает исходные элементы ниже заголовка без перекрытия и изменения их координат.</summary>
        protected void PlaceClientContentBelowTitleBar()
        {
            SuspendLayout();
            try
            {
                Size originalClientSize = ClientSize;
                var content = new Panel
                {
                    BackColor = ThemeManager.Palette.Window,
                    Bounds = new Rectangle(0, titleBar.Height, originalClientSize.Width, originalClientSize.Height),
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };
                content.SuspendLayout();
                try
                {
                    var existing = new Control[Controls.Count - 1];
                    int index = 0;
                    foreach (Control control in Controls)
                        if (!ReferenceEquals(control, titleBar)) existing[index++] = control;
                    foreach (Control control in existing)
                    {
                        Controls.Remove(control);
                        content.Controls.Add(control);
                    }
                    // Для главной формы с Canvas необходима фиксированная область содержимого:
                    // DockStyle.Fill иногда перекрывает заголовок из-за порядка dock-layout.
                    ClientSize = new Size(originalClientSize.Width, originalClientSize.Height + titleBar.Height);
                    Controls.Add(content);
                    clientContent = content;
                    content.Bounds = new Rectangle(Padding.Left, titleBar.Bottom,
                        Math.Max(0, ClientSize.Width - Padding.Horizontal),
                        Math.Max(0, ClientSize.Height - titleBar.Bottom - Padding.Bottom));
                    content.SendToBack();
                    titleBar.BringToFront();
                }
                finally
                {
                    content.ResumeLayout(true);
                }
                ThemeManager.Changed += ContentThemeChanged;
                content.Disposed += (sender, e) => ThemeManager.Changed -= ContentThemeChanged;
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        /// <summary>Обновляет только фон размещения элементов при переключении темы.</summary>
        private void ContentThemeChanged(object sender, EventArgs e)
        {
            if (clientContent != null && !clientContent.IsDisposed)
                clientContent.BackColor = ThemeManager.Palette.Window;
        }

        /// <summary>Сохраняет системные стили, необходимые для Snap, меню окна и изменения размеров.</summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.Style |= WsSysMenu;
                if (allowResize) parameters.Style |= WsThickFrame;
                else parameters.Style &= ~WsThickFrame;
                if (MinimizeBox) parameters.Style |= WsMinimizeBox;
                else parameters.Style &= ~WsMinimizeBox;
                if (MaximizeBox && allowResize) parameters.Style |= WsMaximizeBox;
                else parameters.Style &= ~WsMaximizeBox;
                parameters.Style &= ~WsCaption;
                return parameters;
            }
        }

        /// <summary>Передаёт перемещение и resize системе Windows с учётом собственной верхней панели.</summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmNcCalcSize || m.Msg == WmNcPaint)
            {
                m.Result = IntPtr.Zero;
                return;
            }
            if (m.Msg == WmNcActivate)
            {
                m.Result = (IntPtr)1;
                return;
            }
            if (m.Msg == WmGetMinMaxInfo)
            {
                base.WndProc(ref m);
                UpdateMaximizedBounds(m.LParam);
                return;
            }
            if (m.Msg == WmNcHitTest)
            {
                base.WndProc(ref m);
                if (m.Result != (IntPtr)HtClient && m.Result != IntPtr.Zero) return;
                Point point = PointToClient(ScreenPoint(m.LParam));
                int hit = GetHitTest(point);
                if (hit != HtClient) m.Result = (IntPtr)hit;
                return;
            }
            base.WndProc(ref m);
        }

        /// <summary>Определяет системную зону изменения размера или перетаскивания заголовка.</summary>
        private int GetHitTest(Point point)
        {
            if (allowResize && WindowState != FormWindowState.Maximized)
            {
                int border = Math.Max(ResizeBorder, (int)Math.Round(ResizeBorder * DeviceDpi / 96.0));
                bool left = point.X >= 0 && point.X < border;
                bool right = point.X < Width && point.X >= Width - border;
                bool top = point.Y >= 0 && point.Y < border;
                bool bottom = point.Y < Height && point.Y >= Height - border;
                if (left && top) return HtTopLeft;
                if (right && top) return HtTopRight;
                if (left && bottom) return HtBottomLeft;
                if (right && bottom) return HtBottomRight;
                if (left) return HtLeft;
                if (right) return HtRight;
                if (top) return HtTop;
                if (bottom) return HtBottom;
            }
            return titleBar.IsDragArea(titleBar.PointToClient(PointToScreen(point))) ? HtCaption : HtClient;
        }

        /// <summary>Преобразует координаты сообщения Windows с поддержкой отрицательных координат мониторов.</summary>
        private static Point ScreenPoint(IntPtr value)
        {
            long coordinates = value.ToInt64();
            return new Point(unchecked((short)(coordinates & 0xffff)), unchecked((short)((coordinates >> 16) & 0xffff)));
        }

        /// <summary>Устанавливает рабочую область текущего монитора для корректного развёртывания над панелью задач.</summary>
        private void UpdateMaximizedBounds(IntPtr data)
        {
            IntPtr monitor = MonitorFromWindow(Handle, MonitorDefaultToNearest);
            if (monitor == IntPtr.Zero) return;
            var info = new MonitorInfo { Size = Marshal.SizeOf(typeof(MonitorInfo)) };
            if (!GetMonitorInfo(monitor, ref info)) return;
            MinMaxInfo minMax = (MinMaxInfo)Marshal.PtrToStructure(data, typeof(MinMaxInfo));
            Rectangle screen = info.Monitor.ToRectangle();
            Rectangle work = info.Work.ToRectangle();
            minMax.MaxPosition = new NativePoint(work.Left - screen.Left, work.Top - screen.Top);
            minMax.MaxSize = new NativePoint(work.Width, work.Height);
            Marshal.StructureToPtr(minMax, data, false);
        }

        /// <summary>Точка системной геометрии окна.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X, Y;
            public NativePoint(int x, int y) { X = x; Y = y; }
        }

        /// <summary>Прямоугольник системной геометрии окна.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left, Top, Right, Bottom;
            public Rectangle ToRectangle() { return Rectangle.FromLTRB(Left, Top, Right, Bottom); }
        }

        /// <summary>Размеры и ограничения системного окна.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MinMaxInfo
        {
            public NativePoint Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize;
        }

        /// <summary>Монитор и доступная рабочая область.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int Size;
            public NativeRect Monitor, Work;
            public int Flags;
        }

        /// <summary>Выбирает монитор, на котором находится окно.</summary>
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr window, int flags);

        /// <summary>Получает геометрию экрана и его рабочей области.</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
    }
}
