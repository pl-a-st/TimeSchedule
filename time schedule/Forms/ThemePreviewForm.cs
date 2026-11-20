using System;
using System.Drawing;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Изолированная визуальная проверка кнопок без запуска календаря и обращения к данным.</summary>
    internal sealed class ThemePreviewForm : StyledWindowForm
    {
        /// <summary>Надпись с названием текущей темы.</summary>
        private readonly Label themeTitle;

        /// <summary>Тестовое окно уже размещает элементы с учётом заголовка.</summary>
        protected override bool AutoPlaceClientContent { get { return false; } }

        /// <summary>Тестовое окно самостоятельно оформляет свои демонстрационные элементы.</summary>
        protected override bool AutoThemeStandardControls { get { return false; } }

        /// <summary>Создаёт тестовое окно для проверки графического слоя.</summary>
        internal ThemePreviewForm()
        {
            Text = "TimeSchedule — TS-04 проверка оформления";
            Font = new Font("Verdana", 9f, FontStyle.Regular, GraphicsUnit.Point);
            Size = new Size(680, 260);
            MinimumSize = new Size(570, 220);
            StartPosition = FormStartPosition.CenterScreen;
            themeTitle = new Label { Left = 25, Top = 67, Width = 440, Height = 26 };
            Controls.Add(themeTitle);
            AddButton("−5", 25, false, null);
            AddButton("Сегодня", 130, true, null);
            AddButton("+5", 235, false, null);
            AddButton("Обновить", 340, false, null);
            AddButton("Сменить тему", 445, false, (s, e) =>
                ThemeManager.SetTheme(ThemeManager.Current == ThemeKind.Light ? ThemeKind.Dark : ThemeKind.Light));
            ThemeManager.Changed += ThemeChanged;
            FormClosed += (s, e) => ThemeManager.Changed -= ThemeChanged;
            ApplyTheme();
        }

        /// <summary>Добавляет тестовую кнопку заданного типа.</summary>
        private void AddButton(string caption, int x, bool accent, EventHandler action)
        {
            StyledButton button = new StyledButton
            {
                Text = caption,
                Left = x,
                Top = 120,
                Width = caption == "Сменить тему" ? 160 : 95,
                Height = 36,
                IsAccent = accent
            };
            if (action != null) button.Click += action;
            Controls.Add(button);
        }

        /// <summary>Получает уведомление о переключении темы.</summary>
        private void ThemeChanged(object sender, EventArgs e) { ApplyTheme(); }

        /// <summary>Применяет только фон и подпись окна; компоненты получают токены самостоятельно.</summary>
        private void ApplyTheme()
        {
            BackColor = ThemeManager.Palette.Window;
            ForeColor = ThemeManager.Palette.Text;
            themeTitle.ForeColor = ThemeManager.Palette.Text;
            themeTitle.Text = ThemeManager.Current == ThemeKind.Dark ? "Тёмная тема" : "Светлая тема";
            Invalidate(true);
        }
    }
}
