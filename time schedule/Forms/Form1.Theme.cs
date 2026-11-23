using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Подключение общей темы к главной форме без изменения календарной бизнес-логики.</summary>
    public partial class Form1
    {
        /// <summary>Переключатель темы доступен только в главном окне программы.</summary>
        protected override bool ShowThemeSwitch { get { return true; } }

        /// <summary>Календарь использует собственный тематический адаптер.</summary>
        protected override bool AutoThemeStandardControls { get { return false; } }

        /// <summary>Защищает календарь от обновления при первичном переносе контролов под заголовок.</summary>
        private bool mainThemeInitializing;

        /// <summary>Пункт меню для переключения пользовательской темы.</summary>
        private ToolStripMenuItem themeMenuItem;
        /// <summary>Единый шрифт управляемых элементов главной формы.</summary>
        private Font mainThemeFont;
        /// <summary>Шрифт полной даты и дня недели, читаемый при стандартной ширине столбца.</summary>
        private Font calendarDateFont;
        /// <summary>Шрифт даты нижней навигации с сохранённым согласованным размером.</summary>
        private Font mainToolbarDateFont;
        /// <summary>Высота визуальных элементов нижней панели команд.</summary>
        private const int MainToolbarControlHeight = 34;
        /// <summary>Краевой пиксель, учитываемый при отрисовке правой и нижней границ кнопки.</summary>
        private const int ProjectButtonPaintInset = 1;
        /// <summary>Смещение календарного блока, создающее воздух слева и сверху.</summary>
        private const int CalendarWorkspaceOffset = 12;
        /// <summary>Минимальный отступ календарной области от правого края рабочей панели.</summary>
        private const int CalendarWorkspaceRightMargin = 16;
        /// <summary>Дополнительный воздух справа, когда вертикальной полосы прокрутки нет.</summary>
        private const int CalendarNoVerticalScrollExtraMargin = 12;
        /// <summary>Минимальный промежуток между календарём и линией нижней панели команд.</summary>
        private const int CalendarToolbarGap = 24;
        /// <summary>Предотвращает повторное изменение геометрии во время Resize.</summary>
        private bool fittingCalendarToPeople;
        /// <summary>Разделительная линия над командной группой главной формы.</summary>
        private Panel mainToolbarSeparator;
        /// <summary>Одна нижняя линия видимой сетки перед полосой горизонтальной прокрутки.</summary>
        private Panel mainCalendarGridEndLine;
        /// <summary>Правая граница рабочей сетки перед вертикальным скроллбаром.</summary>
        private Panel mainCalendarGridRightLine;
        /// <summary>Левая граница сетки, продолженная до верхней линии календарных дат.</summary>
        private Panel mainCalendarGridLeftLine;
        /// <summary>Верхняя граница только над областью календарной сетки.</summary>
        private Panel mainCalendarGridTopLine;
        /// <summary>Скрывает за пределами сетки лишнюю ширину фона шапки дат.</summary>
        private Panel mainCalendarHeaderEdge;

        /// <summary>Подключает заголовок, меню, кнопки и выбор темы к действующей главной форме.</summary>
        private void InitializeMainTheme()
        {
            ThemeManager.SetTheme(ThemePreferenceStore.Load());
            mainThemeInitializing = true;
            try { PlaceClientContentBelowTitleBar(); }
            finally { mainThemeInitializing = false; }
            mainThemeFont = (Font)Control.DefaultFont.Clone();
            calendarDateFont = new Font(Control.DefaultFont.FontFamily, 8f, FontStyle.Regular, GraphicsUnit.Point);
            mainToolbarDateFont = new Font(Control.DefaultFont.FontFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
            dateTimePicker1.Font = mainToolbarDateFont;
            dateTimePicker1.Width = Math.Max(dateTimePicker1.Width, 220);
            mainToolbarSeparator = new Panel { Height = 1, TabStop = false };
            plMain.Parent.Controls.Add(mainToolbarSeparator);
            mainCalendarGridEndLine = new Panel { Height = 1, TabStop = false, Enabled = false };
            plMain.Parent.Controls.Add(mainCalendarGridEndLine);
            mainCalendarGridRightLine = new Panel { Width = 1, TabStop = false, Enabled = false };
            plMain.Parent.Controls.Add(mainCalendarGridRightLine);
            mainCalendarGridLeftLine = new Panel { Width = 1, TabStop = false, Enabled = false };
            plMain.Parent.Controls.Add(mainCalendarGridLeftLine);
            mainCalendarGridTopLine = new Panel { Height = 1, TabStop = false, Enabled = false };
            plMain.Parent.Controls.Add(mainCalendarGridTopLine);
            mainCalendarHeaderEdge = new Panel { TabStop = false, Enabled = false };
            plMain.Parent.Controls.Add(mainCalendarHeaderEdge);
            Font = mainThemeFont;
            InsetMainCalendarWorkspace();
            themeMenuItem = new ToolStripMenuItem("Тёмная тема") { Visible = false };
            themeMenuItem.Click += ToggleMainTheme;
            menuStrip1.Items.Add(themeMenuItem);
            foreach (ButtonForShedule button in new[]
            {
                button1, button2, btnSetings, minusFiveDay, addFiveDays, bTnToDay, NewRefresh, btnNewTask
            })
            {
                button.UseTheme = true;
                button.Font = mainThemeFont;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
            }
            button1.ThemeSquareCorners = true;
            button1.Paint += PaintMainProjectHeaderEdge;
            btnSetings.ThemeSquareCorners = true;
            bTnToDay.ThemeAccent = true;
            menuStrip1.Font = mainThemeFont;
            cBxSeetingsProjects.DrawMode = DrawMode.OwnerDrawFixed;
            cBxSeetingsProjects.DrawItem += ThemeControlStyler.DrawComboItem;
            InitializeMainMenuComboTheme();
            ThemeManager.Changed += MainThemeChanged;
            Disposed += MainThemeDisposed;
            plMain.Parent.Resize += MainToolbarResized;
            plMain.SizeChanged += MainCalendarBoundsChanged;
            plMain.ClientSizeChanged += MainCalendarBoundsChanged;
            plMain.VisibleChanged += MainCalendarVisibilityChanged;
            plForDate.SizeChanged += MainCalendarBoundsChanged;
            plPersonButton.SizeChanged += MainCalendarBoundsChanged;
            btnNewTask.VisibleChanged += MainToolbarVisibilityChanged;
            Shown += MainThemeShown;
            CenterMainToolbar();
            ApplyMainTheme();
        }

        /// <summary>Сдвигает существующие части календаря без переподчинения контролов и потери якорей.</summary>
        private void InsetMainCalendarWorkspace()
        {
            Control host = plMain.Parent;
            if (host == null || !ReferenceEquals(host, plForDate.Parent) ||
                !ReferenceEquals(host, plPersonButton.Parent) ||
                !ReferenceEquals(host, btnSetings.Parent) ||
                !ReferenceEquals(host, button1.Parent)) return;

            host.SuspendLayout();
            try
            {
                int offset = CalendarWorkspaceOffset;
                int additionalWidth = Math.Min(offset,
                    Math.Max(0, host.ClientSize.Width - plMain.Right - CalendarWorkspaceRightMargin));
                plMain.SetBounds(plMain.Left + offset, plMain.Top + offset,
                    Math.Max(1, plMain.Width - offset + additionalWidth),
                    Math.Max(1, plMain.Height - offset));
                plForDate.SetBounds(plForDate.Left + offset, plForDate.Top + offset,
                    plMain.Width, plForDate.Height);
                plPersonButton.SetBounds(plPersonButton.Left + offset,
                    plPersonButton.Top + offset, plPersonButton.Width,
                    Math.Max(1, plPersonButton.Height - offset));
                button1.Location = new Point(button1.Left + offset, button1.Top + offset);
                btnSetings.Location = new Point(btnSetings.Left + offset, btnSetings.Top + offset);
                cBxSeetingsProjects.Location = new Point(
                    cBxSeetingsProjects.Left + offset, cBxSeetingsProjects.Top + offset);
                lblProjects.Location = new Point(lblProjects.Left + offset, lblProjects.Top + offset);
            }
            finally { host.ResumeLayout(true); }
        }

        /// <summary>
        /// Оформляет фильтр задач как обычный тематический список внутри панели,
        /// совместимой с ToolStripControlHost и исходным обработчиком выбора.
        /// </summary>
        private void InitializeMainMenuComboTheme()
        {
            cBxShowTask.FlatStyle = FlatStyle.Flat;
            cBxShowTask.AutoSize = false;
            cBxShowTask.Width = 158;
            cBxShowTask.Margin = new Padding(8, 3, 8, 5);
            cBxShowTask.ApplyTheme();
        }

        /// <summary>Завершает выравнивание после появления формы и её элементов.</summary>
        private void MainThemeShown(object sender, EventArgs e)
        {
            RestoreMainInput(cBxSeetingsProjects);
            RestoreMainInput(dateTimePicker1);
            AlignMainProjectRow();
            CenterMainToolbar();
            ThemeControlStyler.EnsureInPlaceFrame(cBxSeetingsProjects);
            ThemeControlStyler.SetComboPopupPreparation(cBxSeetingsProjects,
                () => cBxSeetingsProjects_DropDown(cBxSeetingsProjects, EventArgs.Empty));
            ThemeControlStyler.EnsurePanelScrollBars(plMain);
            UpdateMainCalendarGridEndLine();
            ThemeControlStyler.EnsureInputSkins(dateTimePicker1);
            ThemeControlStyler.SetDatePickerVisualHeight(dateTimePicker1, MainToolbarControlHeight);
        }

        /// <summary>
        /// Возвращает исходное поле в рабочий контейнер главной формы, если ранее
        /// установленная декоративная оболочка нарушила его отображение.
        /// </summary>
        /// <param name="field">Штатное поле выбора проекта или даты.</param>
        private void RestoreMainInput(Control field)
        {
            Control content = plMain.Parent;
            if (content == null || field == null || field.IsDisposed) return;
            Control previousParent = field.Parent;
#if DEBUG
            System.Diagnostics.Debug.WriteLine("TS-04 MainInput before: " + field.Name +
                ", parent=" + (previousParent == null ? "null" : previousParent.GetType().Name) +
                ", visible=" + field.Visible + ", bounds=" + field.Bounds);
#endif
            if (!ReferenceEquals(previousParent, content))
            {
                Point location = previousParent == null
                    ? field.Location : content.PointToClient(field.PointToScreen(Point.Empty));
                content.Controls.Add(field);
                field.Location = location;
                StyledInputFrame frame = previousParent as StyledInputFrame;
                if (frame != null) frame.Dispose();
            }
            field.Anchor = ReferenceEquals(field, dateTimePicker1) ? AnchorStyles.Bottom :
                AnchorStyles.Top | AnchorStyles.Left;
            field.Visible = true;
            field.BringToFront();
            if (ReferenceEquals(field, cBxSeetingsProjects))
            {
                field.Location = new Point(btnSetings.Right + 3,
                    Math.Max(menuStrip1.Bottom + 3, btnSetings.Top +
                        (btnSetings.Height - field.Height) / 2));
                menuStrip1.BringToFront();
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine("TS-04 MainInput after: " + field.Name +
                ", parent=" + field.Parent.GetType().Name +
                ", visible=" + field.Visible + ", bounds=" + field.Bounds);
#endif
        }

        /// <summary>Обновляет расположение команд при изменении рабочей области.</summary>
        private void MainToolbarResized(object sender, EventArgs e)
        {
            AlignMainProjectRow();
            CenterMainToolbar();
            FitMainCalendarToPeople();
        }

        /// <summary>
        /// Определяет видимую высоту сетки по строкам исполнителей, ограничивая её
        /// нижней панелью. Поддерживает более свободный правый край при отсутствии
        /// вертикальной полосы, не меняя модель задач или прокрутку.
        /// </summary>
        private void FitMainCalendarToPeople()
        {
            Control host = plMain.Parent;
            if (fittingCalendarToPeople || IsDisposed || Disposing || host == null ||
                pBForLine == null || pBForLine.IsDisposed ||
                FormReadyToBeAddedControl != Statuses.FormReadyToBeAddedControl.Ready)
                return;

            fittingCalendarToPeople = true;
            host.SuspendLayout();
            try
            {
                int maxBottom = mainToolbarSeparator != null && !mainToolbarSeparator.IsDisposed
                    ? mainToolbarSeparator.Top - CalendarToolbarGap
                    : host.ClientSize.Height - 110;
                int availableHeight = Math.Max(1, maxBottom - plMain.Top);
                int scrollHeight = SystemInformation.HorizontalScrollBarHeight;
                int contentHeight = Math.Max(Constants.ROW_HIGHT, pBForLine.Height);
                int intendedHeight = Math.Min(availableHeight, contentHeight + scrollHeight);
                bool verticalScrollNeeded = contentHeight + scrollHeight > availableHeight;
                int rightMargin = CalendarWorkspaceRightMargin +
                    (verticalScrollNeeded ? 0 : CalendarNoVerticalScrollExtraMargin);
                int intendedWidth = Math.Max(1, host.ClientSize.Width - plMain.Left - rightMargin);

                if (plMain.Height != intendedHeight) plMain.Height = intendedHeight;
                if (plMain.Width != intendedWidth) plMain.Width = intendedWidth;
                if (plForDate.Width != intendedWidth) plForDate.Width = intendedWidth;
                UpdateMainCalendarGridEndLine();
            }
            finally
            {
                host.ResumeLayout(true);
                fittingCalendarToPeople = false;
            }
        }

        /// <summary>
        /// Совмещает кнопку «Проекты» с верхней линией шапки календаря и правой
        /// границей списка исполнителей, сохраняя двухпиксельный промежуток до первой строки.
        /// </summary>
        private void AlignMainProjectRow()
        {
            if (plMain.Parent == null ||
                !ReferenceEquals(btnSetings.Parent, cBxSeetingsProjects.Parent) ||
                !ReferenceEquals(lblProjects.Parent, cBxSeetingsProjects.Parent)) return;
            if (button1.Left != plPersonButton.Left)
                button1.Left = plPersonButton.Left;
            // Тематическая отрисовка ButtonForShedule оставляет один пиксель у правого края.
            int projectWidth = Math.Max(1, GetPersonButtonWith()) + ProjectButtonPaintInset;
            int projectTop = Math.Max(0, plForDate.Top - 1);
            int projectHeight = Math.Max(1, plPersonButton.Top - projectTop -
                Constants.MIN_ROW_HIGHT + ProjectButtonPaintInset);
            Rectangle projectBounds = new Rectangle(plPersonButton.Left, projectTop, projectWidth, projectHeight);
            if (button1.Bounds != projectBounds) button1.Bounds = projectBounds;
            int top = btnSetings.Top + (btnSetings.Height - cBxSeetingsProjects.Height) / 2;
            Point comboLocation = new Point(btnSetings.Right + 3, top);
            if (cBxSeetingsProjects.Location != comboLocation)
                cBxSeetingsProjects.Location = comboLocation;
            Point labelLocation = new Point(cBxSeetingsProjects.Right + 10,
                btnSetings.Top + (btnSetings.Height - lblProjects.Height) / 2);
            if (lblProjects.Location != labelLocation) lblProjects.Location = labelLocation;
        }

        /// <summary>Обновляет обрамление календаря после изменения размеров составляющих панелей.</summary>
        private void MainCalendarBoundsChanged(object sender, EventArgs e)
        {
            UpdateMainCalendarGridEndLine();
        }

        /// <summary>Восстанавливает все линии сетки после скрытия и повторного показа календаря.</summary>
        private void MainCalendarVisibilityChanged(object sender, EventArgs e)
        {
            UpdateMainCalendarGridEndLine();
        }

        /// <summary>Выравнивает границы календаря по общей геометрии рабочей сетки.</summary>
        private void UpdateMainCalendarGridEndLine()
        {
            if (mainCalendarGridEndLine == null || mainCalendarGridEndLine.IsDisposed ||
                mainCalendarGridRightLine == null || mainCalendarGridRightLine.IsDisposed ||
                mainCalendarGridLeftLine == null || mainCalendarGridLeftLine.IsDisposed ||
                mainCalendarGridTopLine == null || mainCalendarGridTopLine.IsDisposed ||
                mainCalendarHeaderEdge == null || mainCalendarHeaderEdge.IsDisposed ||
                plMain.IsDisposed || plMain.Parent == null) return;

            int gridRight = plMain.Left + plMain.ClientSize.Width - 1;
            int gridBottom = plMain.Top + plMain.ClientSize.Height - 1;
            int headerTop = Math.Max(0, plForDate.Top - 1);
            int gridLeft = plMain.Left;
            bool visible = plMain.Visible && plMain.ClientSize.Width > 0 &&
                plMain.ClientSize.Height > 0;

            Rectangle bottomLine = new Rectangle(plMain.Left, gridBottom,
                Math.Max(0, gridRight - plMain.Left + 1), 1);
            bool bottomVisible = visible && plMain.HorizontalScroll.Visible;
            if (mainCalendarGridEndLine.Bounds != bottomLine)
                mainCalendarGridEndLine.Bounds = bottomLine;
            mainCalendarGridEndLine.Visible = bottomVisible;
            if (bottomVisible) mainCalendarGridEndLine.BringToFront();

            Rectangle rightLine = new Rectangle(gridRight, headerTop,
                1, Math.Max(0, gridBottom - headerTop + 1));
            if (mainCalendarGridRightLine.Bounds != rightLine)
                mainCalendarGridRightLine.Bounds = rightLine;
            mainCalendarGridRightLine.Visible = visible;
            if (visible) mainCalendarGridRightLine.BringToFront();

            Rectangle leftLine = new Rectangle(gridLeft, headerTop,
                1, Math.Max(0, gridBottom - headerTop + 1));
            if (mainCalendarGridLeftLine.Bounds != leftLine)
                mainCalendarGridLeftLine.Bounds = leftLine;
            mainCalendarGridLeftLine.Visible = visible;
            if (visible) mainCalendarGridLeftLine.BringToFront();

            Rectangle topLine = new Rectangle(gridLeft, headerTop,
                Math.Max(0, gridRight - gridLeft + 1), 1);
            if (mainCalendarGridTopLine.Bounds != topLine)
                mainCalendarGridTopLine.Bounds = topLine;
            mainCalendarGridTopLine.Visible = visible;
            if (visible) mainCalendarGridTopLine.BringToFront();

            int personnelHeight = Math.Max(1, gridBottom - plPersonButton.Top + 1);
            if (plPersonButton.Height != personnelHeight)
                plPersonButton.Height = personnelHeight;

            Rectangle headerExcess = new Rectangle(gridRight + 1, plForDate.Top,
                Math.Max(0, plForDate.Right - gridRight - 1), plForDate.Height);
            if (mainCalendarHeaderEdge.Bounds != headerExcess)
                mainCalendarHeaderEdge.Bounds = headerExcess;
            mainCalendarHeaderEdge.Visible = visible && headerExcess.Width > 0;
            if (mainCalendarHeaderEdge.Visible) mainCalendarHeaderEdge.BringToFront();
        }

        /// <summary>Перерасполагает кнопки при изменении доступности создания задачи.</summary>
        private void MainToolbarVisibilityChanged(object sender, EventArgs e) { CenterMainToolbar(); }

        /// <summary>Выравнивает нижние команды по центру, сохраняя их обработчики и порядок.</summary>
        private void CenterMainToolbar()
        {
            Control host = plMain.Parent;
            if (host == null) return;
            Control[] commands = { minusFiveDay, bTnToDay, addFiveDays,
                dateTimePicker1, NewRefresh, btnNewTask };
            const int gap = 9;
            const int buttonHeight = MainToolbarControlHeight;
            int count = 0, totalWidth = 0;
            foreach (Control control in commands)
            {
                if (!control.Visible) continue;
                totalWidth += control.Width;
                count++;
            }
            if (count == 0) return;
            int x = Math.Max(8, (host.ClientSize.Width - totalWidth - gap * (count - 1)) / 2);
            int y = host.ClientSize.Height - 56;
            int groupLeft = x;
            foreach (Control control in commands)
            {
                if (!control.Visible) continue;
                int height = control is ButtonForShedule ? buttonHeight : control.Height;
                control.SetBounds(x, y + (buttonHeight - height) / 2, control.Width, height);
                x += control.Width + gap;
            }
            if (mainToolbarSeparator != null && !mainToolbarSeparator.IsDisposed)
            {
                int right = x - gap;
                Rectangle line = new Rectangle(groupLeft, Math.Max(0, y - 15),
                    Math.Max(0, Math.Min(right, host.ClientSize.Width - 8) - groupLeft), 1);
                if (mainToolbarSeparator.Bounds != line) mainToolbarSeparator.Bounds = line;
                mainToolbarSeparator.BringToFront();
            }
        }

        /// <summary>Переключает тему без загрузки задач или перестроения интерактивного Canvas.</summary>
        private void ToggleMainTheme(object sender, EventArgs e)
        {
            ThemeKind kind = ThemeManager.Current == ThemeKind.Light ? ThemeKind.Dark : ThemeKind.Light;
            ThemeManager.SetTheme(kind);
            ThemePreferenceStore.Save(kind);
        }

        /// <summary>Обновляет оформление при смене общей темы.</summary>
        private void MainThemeChanged(object sender, EventArgs e) { ApplyMainTheme(); }

        /// <summary>Оформляет только общие поверхности и элементы навигации.</summary>
        private void ApplyMainTheme()
        {
            ThemePalette palette = ThemeManager.Palette;
            if (mainToolbarSeparator != null) mainToolbarSeparator.BackColor = palette.Grid;
            if (mainCalendarGridEndLine != null && !mainCalendarGridEndLine.IsDisposed)
                mainCalendarGridEndLine.BackColor = ThemeVisuals.CalendarGrid;
            if (mainCalendarGridRightLine != null && !mainCalendarGridRightLine.IsDisposed)
                mainCalendarGridRightLine.BackColor = ThemeVisuals.CalendarHeaderLine;
            if (mainCalendarGridLeftLine != null && !mainCalendarGridLeftLine.IsDisposed)
                mainCalendarGridLeftLine.BackColor = ThemeVisuals.CalendarHeaderLine;
            if (mainCalendarGridTopLine != null && !mainCalendarGridTopLine.IsDisposed)
                mainCalendarGridTopLine.BackColor = ThemeVisuals.CalendarHeaderLine;
            if (mainCalendarHeaderEdge != null && !mainCalendarHeaderEdge.IsDisposed)
                mainCalendarHeaderEdge.BackColor = palette.Window;
            BackColor = palette.Window;
            ForeColor = palette.Text;
            menuStrip1.BackColor = palette.Window;
            menuStrip1.ForeColor = palette.Text;
            MainThemeMenuRenderer menuRenderer = new MainThemeMenuRenderer(palette, true);
            menuStrip1.Renderer = menuRenderer;
            menuStrip1.Padding = new Padding(5, 1, 5, 1);
            menuStrip1.Invalidate();
            foreach (ToolStripItem item in menuStrip1.Items)
                ApplyMenuItemColors(item, palette, menuRenderer);
            ApplyCanvasPilotMenuTheme(palette);
            themeMenuItem.Checked = ThemeManager.Current == ThemeKind.Dark;
            cBxShowTask.BackColor = palette.Surface;
            cBxShowTask.ForeColor = palette.Text;
            cBxShowTask.ApplyTheme();
            plMain.BackColor = palette.Window;
            pBForLine.BackColor = palette.Window;
            plPersonButton.BackColor = palette.Window;
            AlignMainProjectRow();
            plForDate.BackColor = palette.Surface;
            ApplyCalendarTheme();
            UpdateMainCalendarGridEndLine();
            lblProjects.ForeColor = palette.Text;
            cBxSeetingsProjects.BackColor = palette.Surface;
            cBxSeetingsProjects.ForeColor = palette.Text;
            cBxSeetingsProjects.Invalidate();
            dateTimePicker1.CalendarForeColor = palette.Text;
            dateTimePicker1.CalendarMonthBackground = palette.Surface;
            dateTimePicker1.CalendarTitleBackColor = palette.Button;
            dateTimePicker1.CalendarTitleForeColor = palette.Text;
            dateTimePicker1.CalendarTrailingForeColor = palette.MutedText;
            dateTimePicker1.ForeColor = palette.Text;
            dateTimePicker1.BackColor = palette.Surface;
            dateTimePicker1.Invalidate();
            foreach (ButtonForShedule button in new[]
            {
                button1, button2, btnSetings, minusFiveDay, addFiveDays, bTnToDay, NewRefresh, btnNewTask
            }) button.Invalidate();
            menuStrip1.Invalidate();
        }

        /// <summary>Согласует обычные и вложенные меню, не меняя содержимое и обработчики.</summary>
        /// <param name="item">Пункт меню и его потомки.</param>
        /// <param name="palette">Активная палитра.</param>
        /// <param name="renderer">Общий оформитель раскрывающихся меню.</param>
        private static void ApplyMenuItemColors(ToolStripItem item, ThemePalette palette,
            MainThemeMenuRenderer renderer)
        {
            item.ForeColor = item.Enabled ? palette.Text : palette.MutedText;
            item.BackColor = item.Owner is ToolStripDropDown ? palette.Surface : palette.Window;
            if (!(item is ToolStripDropDownItem dropDown)) return;
            dropDown.DropDown.Renderer = renderer;
            dropDown.DropDown.BackColor = palette.Surface;
            if (dropDown.DropDown is ToolStripDropDownMenu menu)
            {
                bool hasImages = false, hasChecks = false;
                foreach (ToolStripItem child in menu.Items)
                {
                    hasImages |= child.Image != null;
                    if (child is ToolStripMenuItem option)
                        hasChecks |= option.Checked || option.CheckOnClick;
                }
                menu.ShowImageMargin = hasImages;
                menu.ShowCheckMargin = hasChecks;
                menu.Padding = new Padding(4, 3, 4, 3);
            }
            foreach (ToolStripItem child in dropDown.DropDownItems)
                ApplyMenuItemColors(child, palette, renderer);
        }


        /// <summary>Обновляет цвета уже созданных ячеек и растровой сетки без пересоздания задач.</summary>
        private void ApplyCalendarTheme()
        {
            ThemePalette palette = ThemeManager.Palette;
            if (Program.ListPersonButton != null)
                foreach (PersonButton person in Program.ListPersonButton.PersonButtons)
                    ApplyPersonTheme(person.Button);
            ApplyTaskButtonTheme();
            if (Program.PoolTextBox != null)
                foreach (DateTextBox date in Program.PoolTextBox.ListTextBoxes)
                    if (date.HasCreatedTextBox)
                        ApplyDateTheme(date);
            if (pBForLine.Image != null && Bmp != null &&
                ReferenceEquals(pBForLine.Image, Bmp) && !calendarVirtualViewport)
            {
                using (Graphics graphics = Graphics.FromImage(Bmp))
                    graphics.Clear(palette.Window);
                LoadHorizontLine();
                DrowVerticalLines();
            }
            pBForLine.Invalidate();
            plForDate.Invalidate();
            plPersonButton.Invalidate();
        }

        /// <summary>Обновляет только цвета резервных кнопок задач при переключении темы.</summary>
        private static void ApplyTaskButtonTheme()
        {
            if (Program.ListTaskButtons == null) return;
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
            {
                Color background = ThemeVisuals.TaskDisplayColor(taskButton.Task.Color);
                Color foreground = ThemeVisuals.TaskTextColor(taskButton.Task);
                foreach (Button button in taskButton.Buttons)
                {
                    if (button.IsDisposed) continue;
                    button.BackColor = background;
                    button.ForeColor = foreground;
                }
            }
        }

        /// <summary>Применяет палитру к заголовку сотрудника без изменения события Click.</summary>
        private void ApplyPersonTheme(Button button)
        {
            ThemePalette palette = ThemeManager.Palette;
            button.UseVisualStyleBackColor = false;
            button.BackColor = ThemeVisuals.PersonBackground;
            button.ForeColor = palette.Text;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = palette.ButtonHover;
            button.FlatAppearance.MouseDownBackColor = palette.ButtonPressed;
            button.Font = mainThemeFont;
            button.Paint -= PaintPersonTableCell;
            button.Paint += PaintPersonTableCell;
            button.Invalidate();
        }

        /// <summary>Отделяет строки сотрудников теми же линиями, что и календарь, без изменения кнопок.</summary>
        private static void PaintPersonTableCell(object sender, PaintEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Width < 2 || button.Height < 2) return;
            // Горизонтальную границу уже рисует календарная сетка; второй контур
            // на кнопке сотрудника создавал ложную двойную линию.
            using (Pen right = new Pen(ThemeVisuals.CalendarHeaderLine))
                e.Graphics.DrawLine(right, button.Width - 1, 0,
                    button.Width - 1, button.Height - 1);
        }

        /// <summary>Завершает кнопку проектов одной линией календарной шапки.</summary>
        private static void PaintMainProjectHeaderEdge(object sender, PaintEventArgs e)
        {
            Control header = sender as Control;
            if (header == null || header.Width < 2 || header.Height < 2) return;
            using (var pen = new Pen(ThemeVisuals.CalendarHeaderLine))
                e.Graphics.DrawLine(pen, 0, header.Height - 1,
                    header.Width - 1, header.Height - 1);
        }

        /// <summary>Применяет утверждённую палитру к существующему контекстному меню задач Canvas.</summary>
        /// <param name="palette">Палитра активной темы.</param>
        private void ApplyCanvasPilotMenuTheme(ThemePalette palette)
        {
            if (canvasPilotTaskMenu == null || canvasPilotTaskMenu.IsDisposed) return;
            MainThemeMenuRenderer renderer = new MainThemeMenuRenderer(palette);
            canvasPilotTaskMenu.Renderer = renderer;
            canvasPilotTaskMenu.BackColor = palette.Surface;
            canvasPilotTaskMenu.ForeColor = palette.Text;
            canvasPilotTaskMenu.ShowImageMargin = false;
            canvasPilotTaskMenu.ShowCheckMargin = false;
            if (mainThemeFont != null) canvasPilotTaskMenu.Font = mainThemeFont;
            foreach (ToolStripItem item in canvasPilotTaskMenu.Items)
                ApplyCanvasPilotMenuItemColors(item, palette, renderer);
            canvasPilotTaskMenu.Invalidate();
        }

        /// <summary>Оформляет вложенные пункты, сохраняя события, статусы и права доступа.</summary>
        private void ApplyCanvasPilotMenuItemColors(ToolStripItem item, ThemePalette palette,
            MainThemeMenuRenderer renderer)
        {
            item.ForeColor = item.Enabled ? palette.Text : palette.MutedText;
            item.BackColor = palette.Surface;
            if (!(item is ToolStripDropDownItem dropDown)) return;
            dropDown.DropDown.Renderer = renderer;
            dropDown.DropDown.BackColor = palette.Surface;
            if (dropDown.DropDown is ToolStripDropDownMenu menu)
            {
                menu.ShowImageMargin = false;
                menu.ShowCheckMargin = ReferenceEquals(item, canvasPilotStatusMenuItem);
            }
            foreach (ToolStripItem child in dropDown.DropDownItems)
                ApplyCanvasPilotMenuItemColors(child, palette, renderer);
        }

        /// <summary>Оформляет рабочую дату, включая акцент на сегодняшнем дне.</summary>
        private void ApplyDateTheme(DateTextBox date)
        {
            ThemePalette palette = ThemeManager.Palette;
            TextBox box = date.TextBox;
            box.BackColor = date.Date == DateTime.Today ? palette.Accent : palette.Surface;
            box.ForeColor = palette.Text;
            box.BorderStyle = BorderStyle.None;
            box.Font = calendarDateFont;
            box.Text = ThemeVisuals.CalendarDate(date.Date);
        }

        /// <summary>Использует общий цвет сетки из единого источника.</summary>
        private static Color CalendarGridColor => ThemeVisuals.CalendarGrid;

        /// <summary>Снимает подписку на тему и освобождает созданный шрифт.</summary>
        private void MainThemeDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= MainThemeChanged;
            themeMenuItem.Click -= ToggleMainTheme;
            cBxSeetingsProjects.DrawItem -= ThemeControlStyler.DrawComboItem;
            if (plMain.Parent != null)
            {
                plMain.Parent.Resize -= MainToolbarResized;
            }
            plMain.SizeChanged -= MainCalendarBoundsChanged;
            plMain.ClientSizeChanged -= MainCalendarBoundsChanged;
            plMain.VisibleChanged -= MainCalendarVisibilityChanged;
            plForDate.SizeChanged -= MainCalendarBoundsChanged;
            plPersonButton.SizeChanged -= MainCalendarBoundsChanged;
            button1.Paint -= PaintMainProjectHeaderEdge;
            btnNewTask.VisibleChanged -= MainToolbarVisibilityChanged;
            Shown -= MainThemeShown;
            calendarDateFont.Dispose();
            mainToolbarDateFont.Dispose();
            mainThemeFont.Dispose();
        }

        /// <summary>Цвета стандартного WinForms-меню из единой палитры.</summary>
        private sealed class MainThemeColorTable : ProfessionalColorTable
        {
            /// <summary>Активная палитра меню.</summary>
            private readonly ThemePalette palette;
            /// <summary>Создаёт палитру меню.</summary>
            internal MainThemeColorTable(ThemePalette colors) { palette = colors; UseSystemColors = false; }
            /// <summary>Фон выпадающего меню.</summary>
            public override Color ToolStripDropDownBackground => palette.Surface;
            /// <summary>Фон верхней панели меню.</summary>
            public override Color MenuStripGradientBegin => palette.Window;
            /// <summary>Фон верхней панели меню.</summary>
            public override Color MenuStripGradientEnd => palette.Window;
            /// <summary>Фон выделенного пункта.</summary>
            public override Color MenuItemSelected => palette.ButtonHover;
            /// <summary>Фон активного пункта.</summary>
            public override Color MenuItemSelectedGradientBegin => palette.ButtonHover;
            /// <summary>Фон активного пункта.</summary>
            public override Color MenuItemSelectedGradientEnd => palette.ButtonHover;
            /// <summary>Фон открытого пункта.</summary>
            public override Color MenuItemPressedGradientBegin => palette.ButtonPressed;
            /// <summary>Фон открытого пункта.</summary>
            public override Color MenuItemPressedGradientMiddle => palette.ButtonPressed;
            /// <summary>Фон открытого пункта.</summary>
            public override Color MenuItemPressedGradientEnd => palette.ButtonPressed;
            /// <summary>Цвет рамок меню.</summary>
            public override Color MenuBorder => palette.Grid;
            /// <summary>Граница выделенного пункта.</summary>
            public override Color MenuItemBorder => palette.Grid;
            /// <summary>Граница полос меню и подменю.</summary>
            public override Color ToolStripBorder => palette.Grid;
            /// <summary>Тёмная линия разделителя меню.</summary>
            public override Color SeparatorDark => palette.Grid;
            /// <summary>Светлая линия разделителя меню.</summary>
            public override Color SeparatorLight => palette.Surface;
            /// <summary>Начало области отметок и иконок.</summary>
            public override Color ImageMarginGradientBegin => palette.Surface;
            /// <summary>Середина области отметок и иконок.</summary>
            public override Color ImageMarginGradientMiddle => palette.Surface;
            /// <summary>Конец области отметок и иконок.</summary>
            public override Color ImageMarginGradientEnd => palette.Surface;
            /// <summary>Фон выбранного пункта с отметкой.</summary>
            public override Color CheckBackground => palette.ButtonHover;
            /// <summary>Фон отметки при наведении.</summary>
            public override Color CheckSelectedBackground => palette.ButtonPressed;
            /// <summary>Фон отметки при нажатии.</summary>
            public override Color CheckPressedBackground => palette.ButtonPressed;
        }

        /// <summary>Единый рендерер стрелок и текста обычных и контекстных меню.</summary>
        private sealed class MainThemeMenuRenderer : ToolStripProfessionalRenderer
        {
            /// <summary>Палитра стрелок раскрытия и отключённых пунктов.</summary>
            private readonly ThemePalette palette;
            /// <summary>Разрешает оформление строк только главного меню, не затрагивая контекстные меню задач.</summary>
            private readonly bool drawMainMenuSelection;

            /// <summary>Создаёт оформление меню с правильными цветами всех областей.</summary>
            internal MainThemeMenuRenderer(ThemePalette colors, bool drawMainSelection = false)
                : base(new MainThemeColorTable(colors))
            {
                palette = colors;
                drawMainMenuSelection = drawMainSelection;
            }

            /// <summary>Оформляет выбранные пункты меню без системной градиентной рамки.</summary>
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (!drawMainMenuSelection || !(e.Item is ToolStripMenuItem) ||
                    e.Item.Width <= 1 || e.Item.Height <= 1)
                {
                    base.OnRenderMenuItemBackground(e);
                    return;
                }
                bool submenu = e.Item.Owner is ToolStripDropDown;
                Color background = e.Item.Selected && e.Item.Enabled
                    ? palette.ButtonHover : submenu ? palette.Surface : palette.Window;
                Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
                using (SolidBrush brush = new SolidBrush(background))
                    e.Graphics.FillRectangle(brush, bounds);
                if (e.Item.Selected && e.Item.Enabled)
                {
                    Rectangle border = Rectangle.Inflate(bounds, -1, -1);
                    using (Pen pen = new Pen(palette.Grid))
                        e.Graphics.DrawRectangle(pen, border);
                }
            }

            /// <summary>Рисует стрелки подменю читаемыми в обеих темах.</summary>
            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = e.Item.Enabled ? palette.Text : palette.MutedText;
                base.OnRenderArrow(e);
            }

            /// <summary>Использует цвета темы также для выключенных пунктов.</summary>
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Enabled ? palette.Text : palette.MutedText;
                base.OnRenderItemText(e);
            }
        }
    }
}
