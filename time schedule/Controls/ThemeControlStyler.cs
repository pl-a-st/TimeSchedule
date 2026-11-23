using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Единое применение цветов и типографики к штатным WinForms-контролам без замены их поведения.</summary>
    internal static class ThemeControlStyler
    {
        /// <summary>Прокрутки, уже подключённые к существующим спискам.</summary>
        private static readonly ConditionalWeakTable<ListBox, StyledListScrollBar> listScrollBars =
            new ConditionalWeakTable<ListBox, StyledListScrollBar>();
        /// <summary>Поля текста, у которых однажды подключено обновление цветов при изменении доступности.</summary>
        private static readonly ConditionalWeakTable<TextBoxBase, object> themedTextInputs =
            new ConditionalWeakTable<TextBoxBase, object>();
        /// <summary>Оболочки, уже подключённые к существующим спискам и полям даты.</summary>
        private static readonly ConditionalWeakTable<Control, StyledInputFrame> inputFrames =
            new ConditionalWeakTable<Control, StyledInputFrame>();
        /// <summary>Списки, для которых установлена тема выделения без замены исходного ListBox.</summary>
        private static readonly ConditionalWeakTable<ListBox, object> styledListItems =
            new ConditionalWeakTable<ListBox, object>();
        /// <summary>Линии списков, установленные без замены их содержимого.</summary>
        private static readonly ConditionalWeakTable<ListBox, StyledListBoxOutline> listOutlines =
            new ConditionalWeakTable<ListBox, StyledListBoxOutline>();
        /// <summary>Рамки полей со сложной компоновкой, не изменяющие родителя контрола.</summary>
        private static readonly ConditionalWeakTable<Control, StyledFieldAdornment> inPlaceFrames =
            new ConditionalWeakTable<Control, StyledFieldAdornment>();

        /// <summary>Пара оформителей штатной прокрутки календарной панели.</summary>
        private sealed class PanelScrollBars
        {
            /// <summary>Горизонтальная системная полоса.</summary>
            internal readonly StyledPanelScrollBar Horizontal;
            /// <summary>Вертикальная системная полоса.</summary>
            internal readonly StyledPanelScrollBar Vertical;
            /// <summary>Покрытие системного пересечения двух полос прокрутки.</summary>
            internal readonly StyledPanelScrollCorner Corner;

            /// <summary>Создаёт единое оформление обеих полос и их пересечения.</summary>
            internal PanelScrollBars(ScrollableControl source)
            {
                Horizontal = new StyledPanelScrollBar(source, ScrollOrientation.HorizontalScroll);
                Vertical = new StyledPanelScrollBar(source, ScrollOrientation.VerticalScroll);
                Corner = new StyledPanelScrollCorner(source);
            }
        }

        /// <summary>Уже подключённая прокрутка исходных панелей.</summary>
        private static readonly ConditionalWeakTable<ScrollableControl, PanelScrollBars> panelScrollBars =
            new ConditionalWeakTable<ScrollableControl, PanelScrollBars>();

        /// <summary>Декоративные рамки и полосы прокрутки уже открытых TreeView.</summary>
        private static readonly ConditionalWeakTable<TreeView, StyledTreeViewOverlay> treeOverlays =
            new ConditionalWeakTable<TreeView, StyledTreeViewOverlay>();

        /// <summary>Списки, для которых уже зарегистрирована отрисовка раскрытой части.</summary>
        private static readonly ConditionalWeakTable<ComboBox, object> styledComboLists =
            new ConditionalWeakTable<ComboBox, object>();
        /// <summary>Единый владелец выпадающего окна и его подписок для всех режимов ComboBox.</summary>
        private static readonly ConditionalWeakTable<ComboBox, StyledComboPopupController> comboPopups =
            new ConditionalWeakTable<ComboBox, StyledComboPopupController>();

        /// <summary>Матовые поверхности полей даты, сохраняющие штатные события.</summary>
        private static readonly ConditionalWeakTable<DateTimePicker, StyledDatePickerSkin> datePickerSkins =
            new ConditionalWeakTable<DateTimePicker, StyledDatePickerSkin>();
        /// <summary>Матовые поверхности списков без свободного ввода.</summary>
        private static readonly ConditionalWeakTable<ComboBox, StyledComboBoxSkin> comboBoxSkins =
            new ConditionalWeakTable<ComboBox, StyledComboBoxSkin>();
        /// <summary>Стрелки штатных NumericUpDown.</summary>
        private static readonly ConditionalWeakTable<NumericUpDown, StyledNumericSpinner> numericSpinners =
            new ConditionalWeakTable<NumericUpDown, StyledNumericSpinner>();

        /// <summary>Оформляет существующее дерево контролов, сохраняя обработчики, значения и режимы ввода.</summary>
        /// <param name="root">Корневой контейнер.</param>
        /// <param name="font">Шрифт, ресурсом которого владеет вызывающая форма.</param>
        /// <param name="excluded">Контролы с семантическим цветом, которые нельзя перекрашивать.</param>
        internal static void Apply(Control root, Font font, Control excluded = null)
        {
            if (root == null || root.IsDisposed || root is StyledTitleBar || root is StyledThemeSwitch) return;
            ThemePalette palette = ThemeManager.Palette;
            if (font != null) root.Font = font;
            if (root == excluded) return;

            if (root is ComboBox comboBox)
                EnableComboList(comboBox);

            if (root is ButtonForShedule button)
            {
                button.UseTheme = true;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Invalidate();
            }
            else if (root is Button ordinaryButton)
            {
                ordinaryButton.UseVisualStyleBackColor = false;
                ordinaryButton.FlatStyle = FlatStyle.Flat;
                ordinaryButton.FlatAppearance.BorderSize = 0;
                ordinaryButton.BackColor = palette.Button;
                ordinaryButton.ForeColor = palette.Text;
            }
            else if (root is CheckBox checkBox)
            {
                ApplyChoiceTheme(checkBox, palette);
            }
            else if (root is RadioButton radioButton)
            {
                ApplyChoiceTheme(radioButton, palette);
            }
            else if (root is TextBoxBase textInput)
            {
                ApplyTextInputTheme(textInput);
            }
            else if (root is ComboBox || root is NumericUpDown || root is DateTimePicker ||
                root is ListBox || root is TreeView || root is ListView)
            {
                root.BackColor = palette.Surface;
                root.ForeColor = palette.Text;
                if (root is TreeView tree)
                    tree.LineColor = palette.Grid;
                if (root is DateTimePicker picker)
                {
                    picker.CalendarMonthBackground = palette.Surface;
                    picker.CalendarForeColor = palette.Text;
                    picker.CalendarTitleBackColor = palette.Button;
                    picker.CalendarTitleForeColor = palette.Text;
                    picker.CalendarTrailingForeColor = palette.MutedText;
                }
            }
            else if (root is Panel || root is Form || root is GroupBox || root is Label ||
                root is TabPage)
            {
                root.BackColor = palette.Window;
                root.ForeColor = palette.Text;
            }
            foreach (Control child in root.Controls)
                Apply(child, font, excluded);
            root.Invalidate();
        }

        /// <summary>Оформляет недоступное текстовое поле тем же приглушённым фоном, что и остальные заблокированные элементы.</summary>
        private static void ApplyTextInputTheme(TextBoxBase input)
        {
            if (!themedTextInputs.TryGetValue(input, out _))
            {
                themedTextInputs.Add(input, new object());
                input.EnabledChanged += TextInputEnabledChanged;
                input.Disposed += TextInputDisposed;
            }
            ApplyTextInputColors(input);
        }

        /// <summary>Синхронизирует текст и фон с доступностью поля и выбранной темой.</summary>
        private static void ApplyTextInputColors(TextBoxBase input)
        {
            if (input.IsDisposed) return;
            ThemePalette palette = ThemeManager.Palette;
            Color background = input.Enabled ? palette.Surface : palette.ButtonDisabled;
            Color foreground = input.Enabled ? palette.Text : ThemeVisuals.DisabledText;
            if (input.BackColor != background) input.BackColor = background;
            if (input.ForeColor != foreground) input.ForeColor = foreground;
            input.Invalidate();
        }

        /// <summary>Обновляет цвет при включении и отключении поля без изменения его значения.</summary>
        private static void TextInputEnabledChanged(object sender, EventArgs e)
        {
            ApplyTextInputColors((TextBoxBase)sender);
        }

        /// <summary>Освобождает подписки оформления при уничтожении текстового поля.</summary>
        private static void TextInputDisposed(object sender, EventArgs e)
        {
            var input = (TextBoxBase)sender;
            input.EnabledChanged -= TextInputEnabledChanged;
            input.Disposed -= TextInputDisposed;
            themedTextInputs.Remove(input);
        }

        /// <summary>Согласует цвета нативного флажка с палитрой, сохраняя его Checked и события.</summary>
        /// <param name="control">Существующий флажок или радиокнопка.</param>
        /// <param name="palette">Текущая палитра.</param>
        private static void ApplyChoiceTheme(ButtonBase control, ThemePalette palette)
        {
            control.UseVisualStyleBackColor = false;
            control.FlatStyle = FlatStyle.Flat;
            control.FlatAppearance.BorderColor = palette.Grid;
            control.FlatAppearance.CheckedBackColor = palette.ButtonHover;
            control.FlatAppearance.MouseDownBackColor = palette.ButtonPressed;
            control.FlatAppearance.MouseOverBackColor = palette.Window;
            control.BackColor = palette.Window;
            control.ForeColor = control.Enabled ? palette.Text : palette.MutedText;
        }

        /// <summary>
        /// Подключает общие рамки к штатным полям без изменения их типов, значений и событий.
        /// Пропускает табличную и потоковую компоновку, чтобы не изменить Designer-разметку.
        /// </summary>
        internal static void EnsureInputFrames(Control root)
        {
            if (root == null || root.IsDisposed) return;
            var fields = new List<Control>();
            CollectInputFields(root, fields);
            foreach (Control field in fields) EnsureInputFrame(field);
        }

        /// <summary>Подключает рамку без дублирования оформления DateTimePicker.</summary>
        internal static void EnsureInputFrame(Control field)
        {
            if (field == null || field.IsDisposed || field.Parent == null ||
                field.FindForm() is Form1 ||
                !field.Visible || field.Dock != DockStyle.None || field.Parent is FlowLayoutPanel ||
                field.Parent is TableLayoutPanel ||
                !(field is DateTimePicker || field is ComboBox || field is TextBox ||
                    field is NumericUpDown)) return;
            // У ComboBox должна быть только одна система оформления. Поверхность
            // DropDownList рисует StyledComboBoxSkin; свободный ввод оставляем
            // штатному полю с невмешивающейся в компоновку рамкой.
            ComboBox combo = field as ComboBox;
            if (combo != null)
            {
                if (combo.DropDownStyle != ComboBoxStyle.DropDownList)
                    EnsureInPlaceFrame(combo);
                return;
            }
            DateTimePicker picker = field as DateTimePicker;
            if (picker != null && !picker.ShowCheckBox && !picker.ShowUpDown)
                return;
            StyledInputFrame existing;
            if (inputFrames.TryGetValue(field, out existing)) return;
            StyledInputFrame frame;
            if (field is DateTimePicker) frame = new StyledDatePickerFrame((DateTimePicker)field);
            else if (field is TextBox) frame = new StyledPlainInputFrame((TextBox)field);
            else frame = new StyledPlainInputFrame((NumericUpDown)field);
            if (!frame.Attach()) { frame.Dispose(); return; }
            inputFrames.Add(field, frame);
            field.Disposed += InputDisposed;
        }

        /// <summary>
        /// Рисует общий контур на месте уже работающего поля, не изменяя его иерархию.
        /// Не подключает декоратор к ToolStrip: его коллекция Controls доступна только для чтения.
        /// </summary>
        internal static void EnsureInPlaceFrame(Control field)
        {
            if (field == null || field.IsDisposed || field.Parent == null ||
                field.Parent is ToolStrip ||
                !(field is DateTimePicker || field is ComboBox)) return;
            StyledFieldAdornment existing;
            if (inPlaceFrames.TryGetValue(field, out existing)) return;
            // Flat меняет только нативный контур редактируемого ComboBox,
            // не затрагивая режим ввода, Items и события изменения проекта.
            ComboBox editableCombo = field as ComboBox;
            if (editableCombo != null && editableCombo.FlatStyle != FlatStyle.Flat)
                editableCombo.FlatStyle = FlatStyle.Flat;
            var frame = new StyledFieldAdornment(field);
            inPlaceFrames.Add(field, frame);
            field.Disposed += InPlaceFieldDisposed;
        }

        /// <summary>Отвязывает визуальную рамку после уничтожения исходного поля.</summary>
        private static void InPlaceFieldDisposed(object sender, EventArgs e)
        {
            Control field = (Control)sender;
            inPlaceFrames.Remove(field);
            field.Disposed -= InPlaceFieldDisposed;
        }

        /// <summary>
        /// Добавляет только внешний вид двух системных скроллбаров панели.
        /// Исходный ScrollableControl остаётся владельцем значений и событий прокрутки.
        /// </summary>
        internal static void EnsurePanelScrollBars(ScrollableControl panel)
        {
            if (panel == null || panel.IsDisposed || panel.Parent == null ||
                panelScrollBars.TryGetValue(panel, out _)) return;
            panelScrollBars.Add(panel, new PanelScrollBars(panel));
            panel.Disposed += PanelDisposed;
        }

        /// <summary>Освобождает связь с декораторами после закрытия панели.</summary>
        private static void PanelDisposed(object sender, EventArgs e)
        {
            ScrollableControl panel = (ScrollableControl)sender;
            panelScrollBars.Remove(panel);
            panel.Disposed -= PanelDisposed;
        }

        /// <summary>
        /// Подключает матовые поверхности, сохраняя собственные события и значения контролов.
        /// Штатный DateTimePicker продолжает вызывать DropDown, CloseUp и ValueChanged.
        /// </summary>
        internal static void EnsureInputSkins(Control root)
        {
            if (root == null || root.IsDisposed) return;
            var fields = new List<Control>();
            CollectSkinnableFields(root, fields);
            foreach (Control field in fields)
            {
                if (field.IsDisposed || field.Parent == null) continue;
                var picker = field as DateTimePicker;
                if (picker != null && !picker.ShowCheckBox && !picker.ShowUpDown &&
                    !datePickerSkins.TryGetValue(picker, out _))
                {
                    datePickerSkins.Add(picker, new StyledDatePickerSkin(picker));
                    picker.Disposed += InputSkinDisposed;
                    continue;
                }
                var combo = field as ComboBox;
                if (combo != null && combo.DropDownStyle == ComboBoxStyle.DropDownList &&
                    !comboBoxSkins.TryGetValue(combo, out _))
                {
                    comboBoxSkins.Add(combo, new StyledComboBoxSkin(combo));
                    combo.Disposed += InputSkinDisposed;
                    continue;
                }
                var number = field as NumericUpDown;
                if (number != null && !numericSpinners.TryGetValue(number, out _))
                {
                    numericSpinners.Add(number, new StyledNumericSpinner(number));
                    number.Disposed += InputSkinDisposed;
                }
            }
        }

        /// <summary>Увеличивает только визуальную поверхность даты, сохраняя нативный размер и события.</summary>
        /// <param name="picker">Поле, декоративный слой которого уже создан.</param>
        /// <param name="height">Желаемая визуальная высота в пикселях.</param>
        internal static void SetDatePickerVisualHeight(DateTimePicker picker, int height)
        {
            if (picker == null || picker.IsDisposed) return;
            if (datePickerSkins.TryGetValue(picker, out StyledDatePickerSkin skin))
                skin.SetVisualHeight(height);
        }

        /// <summary>Собирает реальные поля, не обходя вновь добавленные декоративные контролы.</summary>
        private static void CollectSkinnableFields(Control root, List<Control> result)
        {
            if (root is StyledDatePickerSkin || root is StyledNumericSpinner ||
                root is StyledComboBoxSkin || root is StyledTitleBar) return;
            if (root is ComboBox || root is NumericUpDown || root is DateTimePicker)
                result.Add(root);
            foreach (Control child in root.Controls) CollectSkinnableFields(child, result);
        }

        /// <summary>Освобождает таблицы декораторов при уничтожении полей.</summary>
        private static void InputSkinDisposed(object sender, EventArgs e)
        {
            Control field = (Control)sender;
            if (field is DateTimePicker picker) datePickerSkins.Remove(picker);
            else if (field is ComboBox combo) comboBoxSkins.Remove(combo);
            else if (field is NumericUpDown number) numericSpinners.Remove(number);
            field.Disposed -= InputSkinDisposed;
        }

        /// <summary>Возвращает оформленную оболочку для совместной компоновки стандартных полей.</summary>
        internal static Control GetVisualControl(Control field)
        {
            if (field == null) return null;
            StyledInputFrame frame;
            return inputFrames.TryGetValue(field, out frame) && !frame.IsDisposed ? (Control)frame : field;
        }

        /// <summary>Находит стандартные поля, оставляя специализированные элементы без изменений.</summary>
        private static void CollectInputFields(Control root, List<Control> fields)
        {
            if (root is StyledTitleBar || root is StyledInputFrame) return;
            if (root is DateTimePicker || root is ComboBox || root is NumericUpDown ||
                root is TextBox) fields.Add(root);
            foreach (Control child in root.Controls) CollectInputFields(child, fields);
        }

        /// <summary>Завершает связь оформленного поля с его рамкой при уничтожении.</summary>
        private static void InputDisposed(object sender, EventArgs e)
        {
            var field = (Control)sender;
            inputFrames.Remove(field);
            field.Disposed -= InputDisposed;
        }

        /// <summary>Добавляет единый скроллбар к спискам после завершения первоначальной компоновки формы.</summary>
        /// <param name="root">Оформляемая форма или контейнер.</param>
        internal static void EnsureListScrollBars(Control root)
        {
            if (root == null || root.IsDisposed) return;
            var lists = new List<ListBox>();
            CollectLists(root, lists);
            foreach (ListBox list in lists)
            {
                if (list.IsDisposed || list.MultiColumn || list.Parent == null) continue;
                EnsureListItemTheme(list);
                StyledListScrollBar existing;
                if (!listScrollBars.TryGetValue(list, out existing))
                {
                    var scrollbar = new StyledListScrollBar(list);
                    listScrollBars.Add(list, scrollbar);
                    list.Disposed += ListDisposed;
                }
                StyledListBoxOutline outline;
                if (!listOutlines.TryGetValue(list, out outline))
                {
                    outline = new StyledListBoxOutline(list);
                    listOutlines.Add(list, outline);
                }
            }
        }

        /// <summary>
        /// Оформляет выбранные строки только у штатных однотипных списков: существующие
        /// OwnerDraw-списки и их DrawItem/MeasureItem остаются без изменений.
        /// </summary>
        /// <param name="list">Существующий ListBox со штатными данными и обработчиками.</param>
        private static void EnsureListItemTheme(ListBox list)
        {
            if (styledListItems.TryGetValue(list, out _) || list.DrawMode != DrawMode.Normal ||
                list.HorizontalScrollbar) return;
            int itemHeight = list.ItemHeight;
            list.DrawItem += DrawListItem;
            list.DrawMode = DrawMode.OwnerDrawFixed;
            list.ItemHeight = itemHeight;
            styledListItems.Add(list, new object());
        }

        /// <summary>Рисует выделение, не меняя выбранный индекс, табуляцию и прокрутку списка.</summary>
        private static void DrawListItem(object sender, DrawItemEventArgs e)
        {
            ListBox list = (ListBox)sender;
            ThemePalette palette = ThemeManager.Palette;
            bool selected = list.Enabled && (e.State & DrawItemState.Selected) != 0;
            Color background = !list.Enabled ? palette.ButtonDisabled :
                selected ? palette.ButtonHover : palette.Surface;
            using (SolidBrush brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);
            if (e.Index < 0 || e.Index >= list.Items.Count) return;

            Rectangle text = new Rectangle(e.Bounds.X + 4, e.Bounds.Y,
                Math.Max(0, e.Bounds.Width - 8), e.Bounds.Height);
            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            if (list.UseTabStops) flags |= TextFormatFlags.ExpandTabs;
            TextRenderer.DrawText(e.Graphics, list.GetItemText(list.Items[e.Index]), list.Font,
                text, list.Enabled ? palette.Text : palette.MutedText, flags);
            if ((e.State & DrawItemState.Focus) != 0 && list.Focused)
            {
                Rectangle focus = Rectangle.Inflate(e.Bounds, -1, -1);
                if (focus.Width > 1 && focus.Height > 1)
                    using (Pen pen = new Pen(palette.Focus))
                        e.Graphics.DrawRectangle(pen, focus);
            }
        }

        /// <summary>Собирает списки без изменения дерева контролов во время его обхода.</summary>
        private static void CollectLists(Control root, List<ListBox> lists)
        {
            if (root is ListBox list) lists.Add(list);
            foreach (Control child in root.Controls)
                CollectLists(child, lists);
        }

        /// <summary>Освобождает связанную полосу прокрутки одновременно со списком.</summary>
        private static void ListDisposed(object sender, EventArgs e)
        {
            var list = (ListBox)sender;
            if (styledListItems.TryGetValue(list, out _))
            {
                list.DrawItem -= DrawListItem;
                styledListItems.Remove(list);
            }
            StyledListScrollBar scrollbar;
            if (listScrollBars.TryGetValue(list, out scrollbar))
            {
                listScrollBars.Remove(list);
                scrollbar.Dispose();
            }
            StyledListBoxOutline outline;
            if (listOutlines.TryGetValue(list, out outline))
            {
                listOutlines.Remove(list);
                outline.Dispose();
            }
            list.Disposed -= ListDisposed;
        }

        /// <summary>
        /// Подключает к деревьям неинтерактивные рамки и полосы прокрутки.
        /// Собственные прокрутка, чекбоксы, разворачивание и выбор TreeView не изменяются.
        /// </summary>
        internal static void EnsureTreeViewOverlays(Control root)
        {
            if (root == null || root.IsDisposed) return;
            var trees = new List<TreeView>();
            CollectTrees(root, trees);
            foreach (TreeView tree in trees)
            {
                if (tree.IsDisposed || tree.Parent == null || treeOverlays.TryGetValue(tree, out _)) continue;
                treeOverlays.Add(tree, new StyledTreeViewOverlay(tree));
                tree.Disposed += TreeDisposed;
            }
        }

        /// <summary>Собирает исходные TreeView без изменения дочерних элементов во время обхода.</summary>
        private static void CollectTrees(Control root, List<TreeView> trees)
        {
            if (root is TreeView tree) trees.Add(tree);
            foreach (Control child in root.Controls) CollectTrees(child, trees);
        }

        /// <summary>Освобождает декоратор вместе с исходным деревом.</summary>
        private static void TreeDisposed(object sender, EventArgs e)
        {
            TreeView tree = (TreeView)sender;
            StyledTreeViewOverlay overlay;
            if (treeOverlays.TryGetValue(tree, out overlay))
            {
                treeOverlays.Remove(tree);
                overlay.Dispose();
            }
            tree.Disposed -= TreeDisposed;
        }

        /// <summary>
        /// Подключает оформление раскрытия однократно, не затрагивая существующий OwnerDraw,
        /// элементы, поведение при вводе и подписки SelectedIndexChanged.
        /// </summary>
        internal static void EnableComboList(ComboBox combo)
        {
            if (combo == null || combo.IsDisposed || styledComboLists.TryGetValue(combo, out _)) return;
            styledComboLists.Add(combo, new object());
            if (combo.DrawMode == DrawMode.Normal)
            {
                combo.DrawMode = DrawMode.OwnerDrawFixed;
                combo.DrawItem += DrawComboItem;
            }
            combo.DropDown += ComboDropDown;
            combo.Disposed += ComboDisposed;
            comboPopups.Add(combo, new StyledComboPopupController(combo));
            ApplyComboPopupColors(combo);
        }

        /// <summary>Открывает оформленный список независимо от режима ввода.</summary>
        internal static void ToggleComboPopup(ComboBox combo)
        {
            if (combo == null || combo.IsDisposed || !combo.Enabled) return;
            EnableComboList(combo);
            if (comboPopups.TryGetValue(combo, out StyledComboPopupController popup))
                popup.Toggle();
        }

        /// <summary>Учитывает раскрытие при прокрутке свёрнутого ComboBox колесом.</summary>
        internal static bool IsComboPopupOpen(ComboBox combo)
        {
            return combo != null && comboPopups.TryGetValue(combo, out StyledComboPopupController popup) &&
                popup.IsOpen;
        }

        /// <summary>Регистрирует подготовку данных, ранее выполнявшуюся событием DropDown.</summary>
        internal static void SetComboPopupPreparation(ComboBox combo, Action prepare)
        {
            if (combo == null || combo.IsDisposed) return;
            EnableComboList(combo);
            if (comboPopups.TryGetValue(combo, out StyledComboPopupController popup))
                popup.Prepare = prepare;
        }

        /// <summary>
        /// Сохраняет оформление нативного раскрытия для сторонних вызовов DroppedDown
        /// и автодополнения. Пользовательское раскрытие обслуживает StyledComboPopupController.
        /// </summary>
        private static void ComboDropDown(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            ApplyComboPopupColors(combo);
            StyledComboPopupScrollBar.ApplyNativeTheme(combo);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("TS-04 ComboDropDown: " + combo.Name +
                ", mode=" + combo.DrawMode + ", style=" + combo.DropDownStyle);
#endif
        }

        /// <summary>Согласует фон области раскрытия с активной палитрой.</summary>
        private static void ApplyComboPopupColors(ComboBox combo)
        {
            if (combo.IsDisposed) return;
            ThemePalette palette = ThemeManager.Palette;
            if (combo.BackColor != palette.Surface) combo.BackColor = palette.Surface;
            if (combo.ForeColor != palette.Text) combo.ForeColor = palette.Text;
            if (combo.DrawMode == DrawMode.Normal) combo.DrawMode = DrawMode.OwnerDrawFixed;
        }

        /// <summary>Отписывает только обработчики визуализации при уничтожении ComboBox.</summary>
        private static void ComboDisposed(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            combo.DrawItem -= DrawComboItem;
            combo.DropDown -= ComboDropDown;
            if (comboPopups.TryGetValue(combo, out StyledComboPopupController popup))
            {
                popup.Dispose();
                comboPopups.Remove(combo);
            }
            combo.Disposed -= ComboDisposed;
            styledComboLists.Remove(combo);
        }

        /// <summary>Закрашивает каждую строку раскрытого списка, включая выделение и фон.</summary>
        internal static void DrawComboItem(object sender, DrawItemEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            ThemePalette palette = ThemeManager.Palette;
            bool selected = combo.Enabled && (e.State & DrawItemState.Selected) != 0;
            Color background = !combo.Enabled ? palette.ButtonDisabled :
                selected ? palette.ButtonHover : palette.Surface;
            using (SolidBrush brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);
            if (e.Index < 0 || e.Index >= combo.Items.Count) return;
            Rectangle text = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top,
                Math.Max(0, e.Bounds.Width - 8), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), combo.Font,
                text, combo.Enabled ? palette.Text : palette.MutedText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            if ((e.State & DrawItemState.Focus) != 0 && (e.State & DrawItemState.NoFocusRect) == 0)
            {
                Rectangle focus = Rectangle.Inflate(e.Bounds, -1, -1);
                if (focus.Width > 1 && focus.Height > 1)
                    using (Pen pen = new Pen(palette.Focus)) e.Graphics.DrawRectangle(pen, focus);
            }
        }
    }
}
