using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Единый заголовок окна с кнопками управления, использующий общую палитру тем.</summary>
    internal sealed class StyledTitleBar : UserControl
    {
        /// <summary>Подпись окна.</summary>
        private readonly Label caption;
        /// <summary>Кнопка сворачивания окна.</summary>
        private readonly Button minimize;
        /// <summary>Кнопка развёртывания или восстановления окна.</summary>
        private readonly Button maximize;
        /// <summary>Кнопка закрытия окна.</summary>
        private readonly Button close;
        /// <summary>Единый переключатель тем, расположенный рядом с системными кнопками.</summary>
        private readonly StyledThemeSwitch themeSwitch;
        /// <summary>Окно, которому принадлежит заголовок.</summary>
        private readonly Form owner;
        /// <summary>Контейнер кнопок управления окном.</summary>
        private readonly FlowLayoutPanel buttons;
        /// <summary>Непрерывная линия под заголовком поверх всех дочерних элементов.</summary>
        private readonly Panel separator;

        /// <summary>Создаёт общий заголовок для указанной формы.</summary>
        /// <param name="window">Форма, состоянием которой управляют кнопки.</param>
        internal StyledTitleBar(Form window)
        {
            owner = window ?? throw new ArgumentNullException(nameof(window));
            Height = 40;
            Dock = DockStyle.Top;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            caption = new Label
            {
                AutoEllipsis = true, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(36, 0, 0, 0), Font = Control.DefaultFont
            };
            minimize = CreateButton(TitleBarIcon.Minimize, "Свернуть", (s, e) => owner.WindowState = FormWindowState.Minimized);
            maximize = CreateButton(TitleBarIcon.Maximize, "Развернуть", (s, e) => ToggleMaximized());
            close = CreateButton(TitleBarIcon.Close, "Закрыть", (s, e) => owner.Close());
            themeSwitch = new StyledThemeSwitch { Visible = false };

            buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right, Width = 120, Height = Height,
                WrapContents = false, FlowDirection = FlowDirection.LeftToRight,
                Margin = Padding.Empty, Padding = Padding.Empty
            };
            buttons.Controls.Add(themeSwitch);
            buttons.Controls.Add(minimize);
            buttons.Controls.Add(maximize);
            buttons.Controls.Add(close);
            Controls.Add(caption);
            Controls.Add(buttons);
            separator = new Panel { Height = 1, BackColor = ThemeManager.Palette.Grid };
            Controls.Add(separator);
            Resize += TitleBarResized;
            UpdateSeparatorBounds();
            caption.MouseDown += CaptionMouseDown;
            caption.DoubleClick += CaptionDoubleClick;
            caption.Paint += CaptionPaint;
            owner.TextChanged += OwnerTextChanged;
            owner.Resize += OwnerResized;
            ThemeManager.Changed += ThemeChanged;
            OwnerTextChanged(owner, EventArgs.Empty);
            OwnerResized(owner, EventArgs.Empty);
            ApplyTheme();
        }

        /// <summary>Поддерживает непрерывную линию на всю ширину при изменении размера заголовка.</summary>
        private void TitleBarResized(object sender, EventArgs e) { UpdateSeparatorBounds(); }

        /// <summary>Помещает линию поверх заголовка и кнопок, сохраняя боковые отступы.</summary>
        private void UpdateSeparatorBounds()
        {
            Rectangle bounds = new Rectangle(13, Height - 2, Math.Max(0, Width - 26), 1);
            if (separator.Bounds != bounds) separator.Bounds = bounds;
            if (Controls.GetChildIndex(separator) != 0) separator.BringToFront();
        }

        /// <summary>Восстанавливает непрерывную линию поверх дочерних элементов после перерасчёта Dock.</summary>
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (separator != null) UpdateSeparatorBounds();
        }

        /// <summary>Показывает переключатель темы только у окна, которому он разрешён.</summary>
        /// <param name="visible">Нужно ли отображать переключатель в заголовке.</param>
        internal void ConfigureThemeSwitch(bool visible)
        {
            themeSwitch.Visible = visible;
            UpdateButtonsWidth();
        }

        /// <summary>Показывает только действия, разрешённые для данного типа формы.</summary>
        internal void ConfigureWindowButtons(bool canMinimize, bool canMaximize)
        {
            minimize.Visible = canMinimize;
            maximize.Visible = canMaximize;
            UpdateButtonsWidth();
        }

        /// <summary>Синхронизирует ширину области управления с видимыми кнопками.</summary>
        private void UpdateButtonsWidth()
        {
            buttons.Width = (themeSwitch.Visible ? themeSwitch.Width : 0) +
                (minimize.Visible ? minimize.Width : 0) +
                (maximize.Visible ? maximize.Width : 0) + close.Width;
            buttons.PerformLayout();
            UpdateSeparatorBounds();
        }

        /// <summary>Отображает текущее состояние развёртывания.</summary>
        private void OwnerResized(object sender, EventArgs e)
        {
            TitleBarIconButton control = (TitleBarIconButton)maximize;
            control.IsRestore = owner.WindowState == FormWindowState.Maximized;
            maximize.AccessibleName = control.IsRestore ? "Восстановить" : "Развернуть";
        }

        /// <summary>Синхронизирует текст заголовка с текстом формы.</summary>
        private void OwnerTextChanged(object sender, EventArgs e) { caption.Text = owner.Text; }

        /// <summary>Рисует единую календарную пиктограмму слева от заголовка каждого окна.</summary>
        private void CaptionPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int x = 14, y = (caption.Height - 15) / 2;
            using (Pen pen = new Pen(ThemeManager.Palette.Focus, 1.5f))
            {
                e.Graphics.DrawRectangle(pen, x, y + 2, 15, 13);
                e.Graphics.DrawLine(pen, x, y + 6, x + 15, y + 6);
                e.Graphics.DrawLine(pen, x + 4, y, x + 4, y + 4);
                e.Graphics.DrawLine(pen, x + 11, y, x + 11, y + 4);
                e.Graphics.DrawLine(pen, x + 4, y + 10, x + 6, y + 10);
                e.Graphics.DrawLine(pen, x + 9, y + 10, x + 11, y + 10);
            }
        }

        /// <summary>Передаёт Windows начало перетаскивания над текстом заголовка.</summary>
        private void CaptionMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(owner.Handle, 0x00A1, (IntPtr)2, IntPtr.Zero);
        }

        /// <summary>Сохраняет штатное развёртывание двойным щелчком по заголовку.</summary>
        private void CaptionDoubleClick(object sender, EventArgs e) { ToggleMaximized(); }

        /// <summary>Переключает развёрнутое и обычное состояние окна.</summary>
        private void ToggleMaximized()
        {
            if (!maximize.Visible || !owner.MaximizeBox) return;
            owner.WindowState = owner.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        /// <summary>Создаёт кнопку с единообразной отрисовкой системного символа.</summary>
        private static Button CreateButton(TitleBarIcon icon, string accessibleName, EventHandler click)
        {
            var button = new TitleBarIconButton(icon)
            {
                AccessibleName = accessibleName, Width = 40, Height = 40,
                Margin = Padding.Empty, TabStop = true
            };
            button.Click += click;
            return button;
        }

        /// <summary>Тип действия кнопки заголовка.</summary>
        private enum TitleBarIcon { Minimize, Maximize, Close }

        /// <summary>Плоская кнопка с одинаковой толщиной линий и оформлением во всех состояниях.</summary>
        private sealed class TitleBarIconButton : Button
        {
            /// <summary>Изображаемое действие окна.</summary>
            private readonly TitleBarIcon icon;
            /// <summary>Показывать восстановление вместо развёртывания.</summary>
            private bool isRestore;
            /// <summary>Состояние наведения курсора.</summary>
            private bool hovered;

            /// <summary>Состояние пиктограммы развёртывания.</summary>
            internal bool IsRestore
            {
                get { return isRestore; }
                set { if (isRestore != value) { isRestore = value; Invalidate(); } }
            }

            /// <summary>Инициализирует управляемую GDI+ отрисовку.</summary>
            internal TitleBarIconButton(TitleBarIcon icon)
            {
                this.icon = icon;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
            }

            /// <summary>Обновляет состояние указателя.</summary>
            protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
            /// <summary>Обновляет состояние указателя.</summary>
            protected override void OnMouseLeave(EventArgs e) { hovered = false; Invalidate(); base.OnMouseLeave(e); }
            /// <summary>Перерисовывает клавиатурный фокус.</summary>
            protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
            /// <summary>Перерисовывает потерю фокуса.</summary>
            protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

            /// <summary>Рисует матовую поверхность и системный знак без зависимости от шрифта.</summary>
            protected override void OnPaint(PaintEventArgs e)
            {
                Color background = BackColor;
                e.Graphics.Clear(background);
                if (hovered)
                {
                    using (var brush = new SolidBrush(ThemeManager.Palette.Button))
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                }
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                float cx = Width / 2f, cy = Height / 2f;
                using (var pen = new Pen(ForeColor, 1.5f))
                {
                    if (icon == TitleBarIcon.Minimize)
                        e.Graphics.DrawLine(pen, cx - 6, cy, cx + 6, cy);
                    else if (icon == TitleBarIcon.Close)
                    {
                        e.Graphics.DrawLine(pen, cx - 5, cy - 5, cx + 5, cy + 5);
                        e.Graphics.DrawLine(pen, cx + 5, cy - 5, cx - 5, cy + 5);
                    }
                    else if (isRestore)
                    {
                        e.Graphics.DrawRectangle(pen, cx - 3, cy - 6, 10, 10);
                        e.Graphics.DrawRectangle(pen, cx - 7, cy - 2, 10, 10);
                    }
                    else e.Graphics.DrawRectangle(pen, cx - 6, cy - 6, 12, 12);
                }
                if (Focused && ShowFocusCues)
                    ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -5, -5));
            }
        }

        /// <summary>Определяет область, за которую Windows может перемещать окно.</summary>
        /// <param name="point">Точка в клиентских координатах заголовка.</param>
        internal bool IsDragArea(Point point)
        {
            return ClientRectangle.Contains(point) && point.X < Width - buttons.Width;
        }

        /// <summary>Перекрашивает заголовок при смене выбранной темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { ApplyTheme(); }

        /// <summary>Применяет только цвета централизованной палитры.</summary>
        private void ApplyTheme()
        {
            ThemePalette palette = ThemeManager.Palette;
            Color background = palette.Window;
            BackColor = background;
            caption.BackColor = background;
            caption.ForeColor = palette.Text;
            separator.BackColor = palette.Grid;
            foreach (Button button in new[] { minimize, maximize, close })
            {
                button.BackColor = background;
                button.ForeColor = palette.Text;

            }
            Invalidate(true);
        }

        /// <summary>Передаёт управление перетаскиванием системному менеджеру окон.</summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReleaseCapture();

        /// <summary>Отправляет оконное сообщение для штатного перетаскивания.</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);

        /// <summary>Отписывается от глобальной темы и событий формы при освобождении.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.Changed -= ThemeChanged;
                Resize -= TitleBarResized;
                owner.TextChanged -= OwnerTextChanged;
                owner.Resize -= OwnerResized;
                caption.MouseDown -= CaptionMouseDown;
                caption.DoubleClick -= CaptionDoubleClick;
                caption.Paint -= CaptionPaint;
            }
            base.Dispose(disposing);
        }
    }
}
