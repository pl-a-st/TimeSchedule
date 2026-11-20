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
        /// <summary>Разделительная линия над командной группой главной формы.</summary>
        private Panel mainToolbarSeparator;

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
            Font = mainThemeFont;
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
            btnSetings.ThemeSquareCorners = true;
            bTnToDay.ThemeAccent = true;
            menuStrip1.Font = mainThemeFont;
            cBxSeetingsProjects.DrawMode = DrawMode.OwnerDrawFixed;
            cBxSeetingsProjects.DrawItem += ThemeControlStyler.DrawComboItem;
            ThemeManager.Changed += MainThemeChanged;
            Disposed += MainThemeDisposed;
            plMain.Parent.Resize += MainToolbarResized;
            btnNewTask.VisibleChanged += MainToolbarVisibilityChanged;
            Shown += MainThemeShown;
            CenterMainToolbar();
            ApplyMainTheme();
        }

        /// <summary>Завершает выравнивание после появления формы и её элементов.</summary>
        private void MainThemeShown(object sender, EventArgs e)
        {
            RestoreMainInput(cBxSeetingsProjects);
            RestoreMainInput(dateTimePicker1);
            AlignMainProjectRow();
            CenterMainToolbar();
            ThemeControlStyler.EnsureInPlaceFrame(cBxSeetingsProjects);
            ThemeControlStyler.EnsurePanelScrollBars(plMain);
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
        }

        /// <summary>Выравнивает проект относительно существующих кнопок без смены их родителей и обработчиков.</summary>
        private void AlignMainProjectRow()
        {
            if (plMain.Parent == null ||
                !ReferenceEquals(btnSetings.Parent, cBxSeetingsProjects.Parent) ||
                !ReferenceEquals(lblProjects.Parent, cBxSeetingsProjects.Parent)) return;
            int top = btnSetings.Top + (btnSetings.Height - cBxSeetingsProjects.Height) / 2;
            Point comboLocation = new Point(btnSetings.Right + 3, top);
            if (cBxSeetingsProjects.Location != comboLocation)
                cBxSeetingsProjects.Location = comboLocation;
            Point labelLocation = new Point(cBxSeetingsProjects.Right + 10,
                btnSetings.Top + (btnSetings.Height - lblProjects.Height) / 2);
            if (lblProjects.Location != labelLocation) lblProjects.Location = labelLocation;
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
            int y = host.ClientSize.Height - 70;
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
            BackColor = palette.Window;
            ForeColor = palette.Text;
            menuStrip1.BackColor = palette.Window;
            menuStrip1.ForeColor = palette.Text;
            MainThemeMenuRenderer menuRenderer = new MainThemeMenuRenderer(palette);
            menuStrip1.Renderer = menuRenderer;
            menuStrip1.Padding = new Padding(5, 1, 5, 1);
            menuStrip1.Invalidate();
            foreach (ToolStripItem item in menuStrip1.Items)
                ApplyMenuItemColors(item, palette, menuRenderer);
            ApplyCanvasPilotMenuTheme(palette);
            themeMenuItem.Checked = ThemeManager.Current == ThemeKind.Dark;
            cBxShowTask.BackColor = palette.Surface;
            cBxShowTask.ForeColor = palette.Text;
            plMain.BackColor = palette.Window;
            pBForLine.BackColor = palette.Window;
            plPersonButton.BackColor = palette.Window;
            AlignMainProjectRow();
            plForDate.BackColor = palette.Surface;
            ApplyCalendarTheme();
            lblProjects.ForeColor = palette.Text;
            cBxSeetingsProjects.BackColor = palette.Surface;
            cBxSeetingsProjects.ForeColor = palette.Text;
            cBxSeetingsProjects.Invalidate();
            dateTimePicker1.CalendarForeColor = palette.Text;
            dateTimePicker1.CalendarMonthBackground = palette.Surface;
            dateTimePicker1.CalendarTitleBackColor = palette.Button;
            dateTimePicker1.CalendarTitleForeColor = palette.Text;
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
            item.BackColor = palette.Window;
            if (!(item is ToolStripDropDownItem dropDown)) return;
            dropDown.DropDown.Renderer = renderer;
            dropDown.DropDown.BackColor = palette.Surface;
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
            if (plMain.Parent != null) plMain.Parent.Resize -= MainToolbarResized;
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

            /// <summary>Создаёт оформление меню с правильными цветами всех областей.</summary>
            internal MainThemeMenuRenderer(ThemePalette colors) : base(new MainThemeColorTable(colors))
            {
                palette = colors;
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
