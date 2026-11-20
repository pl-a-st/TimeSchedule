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

        /// <summary>Подключает заголовок, меню, кнопки и выбор темы к действующей главной форме.</summary>
        private void InitializeMainTheme()
        {
            ThemeManager.SetTheme(ThemePreferenceStore.Load());
            mainThemeInitializing = true;
            try { PlaceClientContentBelowTitleBar(); }
            finally { mainThemeInitializing = false; }
            mainThemeFont = (Font)Control.DefaultFont.Clone();
            calendarDateFont = new Font(Control.DefaultFont.FontFamily, 8f, FontStyle.Regular, GraphicsUnit.Point);
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
            bTnToDay.ThemeAccent = true;
            menuStrip1.Font = mainThemeFont;
            cBxSeetingsProjects.DrawMode = DrawMode.OwnerDrawFixed;
            cBxSeetingsProjects.DrawItem += DrawProjectItem;
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
            CenterMainToolbar();
            ThemeControlStyler.EnsureInPlaceFrame(cBxSeetingsProjects);
            ThemeControlStyler.EnsureInPlaceFrame(dateTimePicker1);
            ThemeControlStyler.EnsurePanelScrollBars(plMain);
            ThemeControlStyler.EnsureInputSkins(dateTimePicker1);
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
        private void MainToolbarResized(object sender, EventArgs e) { CenterMainToolbar(); }

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
            const int buttonHeight = 34;
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
            foreach (Control control in commands)
            {
                if (!control.Visible) continue;
                int height = control is ButtonForShedule ? buttonHeight : control.Height;
                control.SetBounds(x, y + (buttonHeight - height) / 2, control.Width, height);
                x += control.Width + gap;
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
            BackColor = palette.Window;
            ForeColor = palette.Text;
            menuStrip1.BackColor = palette.Window;
            menuStrip1.ForeColor = palette.Text;
            menuStrip1.Renderer = new ToolStripProfessionalRenderer(new MainThemeColorTable(palette));
            menuStrip1.Padding = new Padding(5, 1, 5, 1);
            menuStrip1.Invalidate();
            foreach (ToolStripItem item in menuStrip1.Items)
                ApplyMenuItemColors(item, palette);
            themeMenuItem.Checked = ThemeManager.Current == ThemeKind.Dark;
            cBxShowTask.BackColor = palette.Surface;
            cBxShowTask.ForeColor = palette.Text;
            plMain.BackColor = palette.Window;
            pBForLine.BackColor = palette.Window;
            plPersonButton.BackColor = palette.Window;
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

        /// <summary>Перекрашивает все пункты меню без изменения их событий и структуры.</summary>
        private static void ApplyMenuItemColors(ToolStripItem item, ThemePalette palette)
        {
            item.ForeColor = palette.Text;
            item.BackColor = palette.Window;
            if (item is ToolStripDropDownItem dropDown)
            {
                dropDown.DropDown.BackColor = palette.Surface;
                foreach (ToolStripItem child in dropDown.DropDownItems)
                    ApplyMenuItemColors(child, palette);
            }
        }


        /// <summary>Обновляет цвета уже созданных ячеек и растровой сетки без пересоздания задач.</summary>
        private void ApplyCalendarTheme()
        {
            ThemePalette palette = ThemeManager.Palette;
            if (Program.ListPersonButton != null)
                foreach (PersonButton person in Program.ListPersonButton.PersonButtons)
                    ApplyPersonTheme(person.Button);
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

        /// <summary>Применяет палитру к заголовку сотрудника без изменения события Click.</summary>
        private void ApplyPersonTheme(Button button)
        {
            ThemePalette palette = ThemeManager.Palette;
            button.BackColor = ThemeVisuals.PersonBackground;
            button.ForeColor = palette.Text;
            button.FlatAppearance.BorderSize = 0;
            button.Font = mainThemeFont;
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

        /// <summary>Рисует список проектов штатного ComboBox в цветах активной темы.</summary>
        private void DrawProjectItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= cBxSeetingsProjects.Items.Count) return;
            ThemePalette palette = ThemeManager.Palette;
            Color background = (e.State & DrawItemState.Selected) != 0
                ? palette.ButtonHover : palette.Surface;
            using (Brush brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, cBxSeetingsProjects.Items[e.Index].ToString(),
                cBxSeetingsProjects.Font, e.Bounds, palette.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
        }

        /// <summary>Снимает подписку на тему и освобождает созданный шрифт.</summary>
        private void MainThemeDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= MainThemeChanged;
            themeMenuItem.Click -= ToggleMainTheme;
            cBxSeetingsProjects.DrawItem -= DrawProjectItem;
            if (plMain.Parent != null) plMain.Parent.Resize -= MainToolbarResized;
            btnNewTask.VisibleChanged -= MainToolbarVisibilityChanged;
            Shown -= MainThemeShown;
            calendarDateFont.Dispose();
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
        }
    }
}
