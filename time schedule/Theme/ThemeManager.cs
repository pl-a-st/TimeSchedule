using System;
using System.Drawing;

namespace time_schedule
{
    /// <summary>Поддерживаемые визуальные темы TimeSchedule.</summary>
    internal enum ThemeKind { Light, Dark }

    /// <summary>Цветовые токены для единого оформления приложения.</summary>
    internal sealed class ThemePalette
    {
        /// <summary>Фон окна.</summary>
        internal Color Window { get; private set; }
        /// <summary>Фон вспомогательной панели.</summary>
        internal Color Surface { get; private set; }
        /// <summary>Цвет основного текста.</summary>
        internal Color Text { get; private set; }
        /// <summary>Цвет вторичного текста.</summary>
        internal Color MutedText { get; private set; }
        /// <summary>Матовая поверхность обычной кнопки.</summary>
        internal Color Button { get; private set; }
        /// <summary>Матовая поверхность при наведении.</summary>
        internal Color ButtonHover { get; private set; }
        /// <summary>Матовая поверхность при нажатии.</summary>
        internal Color ButtonPressed { get; private set; }
        /// <summary>Поверхность отключённой кнопки.</summary>
        internal Color ButtonDisabled { get; private set; }
        /// <summary>Цвет акцентной кнопки.</summary>
        internal Color Accent { get; private set; }
        /// <summary>Цвет подсветки клавиатурного фокуса.</summary>
        internal Color Focus { get; private set; }
        /// <summary>Цвет линий сетки (проверяется на календаре отдельно).</summary>
        internal Color Grid { get; private set; }

        private ThemePalette() { }

        /// <summary>Получает палитру без изменения бизнес-цветов задач.</summary>
        internal static ThemePalette For(ThemeKind kind)
        {
            if (kind == ThemeKind.Dark)
                return new ThemePalette
                {
                    Window = Color.FromArgb(30, 40, 55),
                    Surface = Color.FromArgb(36, 49, 67),
                    Text = Color.FromArgb(244, 248, 255),
                    MutedText = Color.FromArgb(190, 206, 226),
                    Button = Color.FromArgb(53, 72, 97),
                    ButtonHover = Color.FromArgb(65, 90, 121),
                    ButtonPressed = Color.FromArgb(42, 61, 88),
                    ButtonDisabled = Color.FromArgb(49, 56, 68),
                    Accent = Color.FromArgb(38, 102, 178),
                    Focus = Color.FromArgb(116, 182, 248),
                    Grid = Color.FromArgb(88, 107, 132)
                };
            return new ThemePalette
            {
                Window = Color.FromArgb(252, 253, 255),
                Surface = Color.FromArgb(244, 249, 255),
                Text = Color.FromArgb(25, 39, 60),
                MutedText = Color.FromArgb(71, 99, 133),
                Button = Color.FromArgb(208, 221, 238),
                ButtonHover = Color.FromArgb(197, 214, 234),
                ButtonPressed = Color.FromArgb(186, 206, 230),
                ButtonDisabled = Color.FromArgb(235, 240, 247),
                Accent = Color.FromArgb(178, 218, 252),
                Focus = Color.FromArgb(88, 158, 231),
                Grid = Color.FromArgb(159, 183, 213)
            };
        }
    }

    /// <summary>Управляет выбранной темой в текущем процессе; подключение форм выполняется поэтапно.</summary>
    internal static class ThemeManager
    {
        /// <summary>Текущая тема приложения.</summary>
        internal static ThemeKind Current { get; private set; } = ThemeKind.Light;
        /// <summary>Текущие цвета оформления.</summary>
        internal static ThemePalette Palette { get { return ThemePalette.For(Current); } }
        /// <summary>Уведомление о смене темы для подключённых контролов.</summary>
        internal static event EventHandler Changed;

        /// <summary>Устанавливает тему и уведомляет подписчиков однократно.</summary>
        internal static void SetTheme(ThemeKind kind)
        {
            if (Current == kind) return;
            Current = kind;
            EventHandler changed = Changed;
            if (changed != null) changed(null, EventArgs.Empty);
        }
    }
}
