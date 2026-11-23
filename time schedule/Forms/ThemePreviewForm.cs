using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Изолированная проверка повторного использования темы на стандартных WinForms-полях.</summary>
    internal sealed class ThemePreviewForm : StyledWindowForm
    {
        /// <summary>Текущая выбранная тема.</summary>
        private readonly Label themeTitle;
        /// <summary>Счётчик полей, добавленных после открытия формы.</summary>
        private int dynamicFieldCount;
        /// <summary>Исходные динамические поля, удаляемые вместе с оформляющими оболочками.</summary>
        private readonly List<Control> dynamicFields = new List<Control>();

        /// <summary>Создаёт проверочную форму без доступа к календарю и рабочим данным.</summary>
        internal ThemePreviewForm()
        {
            Text = "TimeSchedule — TS-04 проверка оформления";
            Size = new Size(790, 650);
            MinimumSize = new Size(730, 600);
            StartPosition = FormStartPosition.CenterScreen;
            themeTitle = new Label { Left = 25, Top = 20, Width = 530, Height = 26 };
            Controls.Add(themeTitle);
            AddButton("−5", 25, false, null);
            AddButton("Сегодня", 130, true, null);
            AddButton("+5", 235, false, null);
            AddButton("Обновить", 340, false, null);
            AddButton("Сменить тему", 445, false, (s, e) =>
                ThemeManager.SetTheme(ThemeManager.Current == ThemeKind.Light ? ThemeKind.Dark : ThemeKind.Light));
            AddStandardControls();
            ThemeManager.Changed += ThemeChanged;
            Disposed += PreviewDisposed;
            ApplyTheme();
        }

        /// <summary>Добавляет кнопку с уже существующим тематическим стилем.</summary>
        private void AddButton(string caption, int x, bool accent, EventHandler action)
        {
            StyledButton button = new StyledButton
            {
                Text = caption,
                Left = x,
                Top = 64,
                Width = caption == "Сменить тему" ? 160 : 95,
                Height = 36,
                IsAccent = accent
            };
            if (action != null) button.Click += action;
            Controls.Add(button);
        }

        /// <summary>Создаёт стандартные поля, которые оформляет базовый StyledWindowForm.</summary>
        private void AddStandardControls()
        {
            AddLabel("Текстовое поле", 25, 116, 210);
            Controls.Add(new TextBox { Left = 25, Top = 139, Width = 210, Text = "Обычный текст" });

            AddLabel("Список с выбором", 265, 116, 210);
            var fixedCombo = new ComboBox
            {
                Left = 265, Top = 139, Width = 210,
                DropDownStyle = ComboBoxStyle.DropDownList, MaxDropDownItems = 7
            };
            AddComboItems(fixedCombo);
            Controls.Add(fixedCombo);

            AddLabel("Список с вводом", 505, 116, 210);
            var editableCombo = new ComboBox
            {
                Left = 505, Top = 139, Width = 210,
                DropDownStyle = ComboBoxStyle.DropDown, MaxDropDownItems = 7
            };
            AddComboItems(editableCombo);
            Controls.Add(editableCombo);

            AddLabel("Выбор даты", 25, 194, 210);
            Controls.Add(new StyledDateTimePicker
            {
                Left = 25, Top = 219, Width = 210, Format = DateTimePickerFormat.Short
            });

            AddLabel("Числовое поле", 265, 194, 210);
            Controls.Add(new NumericUpDown { Left = 265, Top = 219, Width = 140, Value = 5 });
            Controls.Add(new CheckBox { Left = 505, Top = 219, Width = 205,
                Text = "Активный параметр", Checked = true });
            Controls.Add(new TextBox { Left = 505, Top = 257, Width = 210,
                Text = "Недоступное поле", Enabled = false });

            AddLabel("Список с прокруткой", 25, 276, 210);
            var list = new ListBox { Left = 25, Top = 299, Width = 210, Height = 130 };
            for (int index = 1; index <= 15; index++) list.Items.Add("Исполнитель " + index);
            Controls.Add(list);

            AddLabel("Дерево", 265, 276, 210);
            var tree = new TreeView { Left = 265, Top = 299, Width = 210, Height = 130 };
            TreeNode group = tree.Nodes.Add("Проект");
            for (int index = 1; index <= 12; index++) group.Nodes.Add("Задача " + index);
            group.Expand();
            Controls.Add(tree);

            var addField = new StyledButton
            {
                Text = "Добавить поле",
                Left = 505, Top = 299, Width = 160, Height = 34
            };
            addField.Click += AddDynamicField;
            Controls.Add(addField);
            var removeField = new StyledButton
            {
                Text = "Удалить поле",
                Left = 505, Top = 339, Width = 160, Height = 34
            };
            removeField.Click += RemoveDynamicField;
            Controls.Add(removeField);

            var choices = new StyledGroupBox
            {
                Text = "Варианты выбора", Left = 25, Top = 455, Width = 450, Height = 86
            };
            choices.Controls.Add(new RadioButton
            {
                Text = "Первый", Left = 14, Top = 31, Width = 170, Checked = true
            });
            choices.Controls.Add(new RadioButton
            {
                Text = "Второй", Left = 190, Top = 31, Width = 170
            });
            Controls.Add(choices);
        }

        /// <summary>Создаёт подпись, использующую шрифт и оформление контейнера.</summary>
        private void AddLabel(string caption, int x, int y, int width)
        {
            Controls.Add(new Label
            {
                Left = x, Top = y, Width = width, Height = 20, Text = caption
            });
        }

        /// <summary>Готовит длинный список для проверки собственной и системной прокрутки.</summary>
        private static void AddComboItems(ComboBox combo)
        {
            for (int index = 1; index <= 15; index++) combo.Items.Add("Вариант " + index);
            combo.SelectedIndex = 0;
        }

        /// <summary>Проверяет оформление поля, добавленного после события Shown.</summary>
        private void AddDynamicField(object sender, EventArgs e)
        {
            if (dynamicFieldCount >= 4) return;
            dynamicFieldCount++;
            Control field;
            switch (dynamicFieldCount)
            {
                case 1:
                    field = new TextBox { Width = 210, Text = "Новое поле" };
                    break;
                case 2:
                    var combo = new ComboBox
                    {
                        Width = 210, DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    AddComboItems(combo);
                    field = combo;
                    break;
                case 3:
                    field = new StyledDateTimePicker
                    {
                        Width = 210, Format = DateTimePickerFormat.Short
                    };
                    break;
                default:
                    var list = new ListBox { Width = 210, Height = 64 };
                    for (int index = 1; index <= 12; index++)
                        list.Items.Add("Динамический пункт " + index);
                    field = list;
                    break;
            }
            field.Left = 505;
            field.Top = 382 + (dynamicFieldCount - 1) * 34;
            WindowContentControls.Add(field);
            dynamicFields.Add(field);
        }

        /// <summary>Удаляет последний динамический контрол вместе с его визуальной оболочкой.</summary>
        private void RemoveDynamicField(object sender, EventArgs e)
        {
            if (dynamicFields.Count == 0) return;
            int last = dynamicFields.Count - 1;
            Control source = dynamicFields[last];
            dynamicFields.RemoveAt(last);
            Control visual = ThemeControlStyler.GetVisualControl(source);
            if (visual != null && !visual.IsDisposed)
            {
                if (visual.Parent != null) visual.Parent.Controls.Remove(visual);
                visual.Dispose();
            }
            dynamicFieldCount--;
        }

        /// <summary>Обновляет подпись после переключения темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { ApplyTheme(); }

        /// <summary>Освобождает подписку даже при уничтожении ещё не показанной формы.</summary>
        private void PreviewDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= ThemeChanged;
            Disposed -= PreviewDisposed;
        }

        /// <summary>Обновляет подпись; стандартные поля обслуживает базовая форма.</summary>
        private void ApplyTheme()
        {
            themeTitle.Text = ThemeManager.Current == ThemeKind.Dark ? "Тёмная тема" : "Светлая тема";
            themeTitle.ForeColor = ThemeManager.Palette.Text;
            Invalidate(true);
        }
    }
}
