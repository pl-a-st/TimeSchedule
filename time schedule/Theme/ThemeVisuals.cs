using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.CompilerServices;

namespace time_schedule
{
    /// <summary>Единые правила отображения календаря, отделённые от расчёта задач и расположения элементов.</summary>
    internal static class ThemeVisuals
    {
        /// <summary>Радиус скругления цветовой полосы в пикселях.</summary>
        internal const int TaskCornerRadius = 4;

        /// <summary>Единый радиус рамок полей ввода в пикселях.</summary>
        internal const int InputCornerRadius = 4;

        /// <summary>Доля цвета сетки в мягкой границе недоступного поля.</summary>
        private const float DisabledInputBorderGridWeight = 0.28f;

        /// <summary>Доля вторичного текста в тексте недоступных элементов.</summary>
        private const float DisabledTextWeight = 0.58f;

        /// <summary>Общая форма контуров полей и списков без объёмной системной рамки.</summary>
        internal static GraphicsPath InputPath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            int diameter = Math.Min(InputCornerRadius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter < 2) { path.AddRectangle(bounds); return path; }
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Определяет цвет рамки по доступности и фокусу, приглушая недоступные поля.</summary>
        /// <param name="enabled">Доступно ли поле для ввода.</param>
        /// <param name="focused">Находится ли клавиатурный фокус внутри поля.</param>
        internal static Color InputBorderColor(bool enabled, bool focused)
        {
            ThemePalette palette = ThemeManager.Palette;
            return !enabled ? BlendGridWithWindow(DisabledInputBorderGridWeight) :
                focused ? palette.Focus : palette.Grid;
        }

        /// <summary>Приглушённый текст и пиктограммы недоступных элементов обеих тем.</summary>
        internal static Color DisabledText
        {
            get
            {
                ThemePalette palette = ThemeManager.Palette;
                Color text = palette.MutedText;
                Color background = palette.Window;
                return Color.FromArgb(
                    (int)(text.R * DisabledTextWeight + background.R * (1f - DisabledTextWeight)),
                    (int)(text.G * DisabledTextWeight + background.G * (1f - DisabledTextWeight)),
                    (int)(text.B * DisabledTextWeight + background.B * (1f - DisabledTextWeight)));
            }
        }

        /// <summary>Рисует единую матовую стрелку раскрытия в существующей области кнопки.</summary>
        /// <param name="graphics">Контекст рисования пиктограммы.</param>
        /// <param name="bounds">Область кнопки в координатах контрола.</param>
        /// <param name="enabled">Состояние доступности списка.</param>
        internal static void DrawDropDownArrow(Graphics graphics, Rectangle bounds, bool enabled)
        {
            if (bounds.Width < 8 || bounds.Height < 8) return;
            SmoothingMode oldMode = graphics.SmoothingMode;
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int centerX = bounds.Left + bounds.Width / 2;
                int centerY = bounds.Top + bounds.Height / 2;
                using (var pen = new Pen(enabled ? ThemeManager.Palette.Text :
                    DisabledText, 1.4f))
                {
                    graphics.DrawLine(pen, centerX - 4, centerY - 2, centerX, centerY + 2);
                    graphics.DrawLine(pen, centerX, centerY + 2, centerX + 4, centerY - 2);
                }
            }
            finally { graphics.SmoothingMode = oldMode; }
        }

        /// <summary>Возвращает полную дату и полное русское название дня недели.</summary>
        internal static string CalendarDate(DateTime date)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo("ru-RU");
            string day = date.ToString("dddd", culture);
            return date.ToString("dd.MM.yyyy", culture) + Environment.NewLine +
                char.ToUpper(day[0], culture) + day.Substring(1);
        }

        /// <summary>Фон заголовка исполнителя с контрастным разделением относительно календаря.</summary>
        internal static Color PersonBackground => ThemeManager.Palette.Button;

        /// <summary>Обычная сетка приглушена относительно утверждённого базового цвета Grid.</summary>
        internal static Color CalendarGrid => BlendGridWithWindow(0.50f);

        /// <summary>Начало недели заметнее обычного дня, но не перетягивает внимание с задач.</summary>
        internal static Color CalendarWeekSeparator => BlendGridWithWindow(0.84f);

        /// <summary>Мягкая двухпиксельная граница сотрудников, заметнее обычной сетки и слабее границы недели.</summary>
        internal static Color CalendarPersonSeparator => BlendGridWithWindow(0.58f);

        /// <summary>Общая тонкая граница шапки таблицы и правой грани кнопок сотрудников.</summary>
        internal static Color CalendarHeaderLine => BlendGridWithWindow(0.68f);

        /// <summary>Смешивает цвет сетки с поверхностью окна, не изменяя саму палитру темы.</summary>
        private static Color BlendGridWithWindow(float gridWeight)
        {
            Color grid = ThemeManager.Palette.Grid;
            Color surface = ThemeManager.Palette.Window;
            return Color.FromArgb((int)(grid.R * gridWeight + surface.R * (1 - gridWeight)),
                (int)(grid.G * gridWeight + surface.G * (1 - gridWeight)),
                (int)(grid.B * gridWeight + surface.B * (1 - gridWeight)));
        }

        /// <summary>Сине-графитовый цвет текста обычных задач в светлой теме.</summary>
        private static readonly Color lightTaskInk = Color.FromArgb(41, 57, 76);

        /// <summary>Глубокий чернильный цвет текста обычных задач в тёмной теме.</summary>
        private static readonly Color darkTaskInk = Color.FromArgb(20, 38, 56);

        /// <summary>Цветовые группы временной палитры задач в порядке их исходных оттенков.</summary>
        private static readonly float[] taskHueCenters = { 0f, 30f, 55f, 125f, 177f, 220f, 275f, 330f };

        /// <summary>Мягкие цвета задач для светлой темы; последний оттенок предназначен для серых задач.</summary>
        private static readonly Color[] lightTaskColors =
        {
            Color.FromArgb(248, 174, 177), Color.FromArgb(252, 195, 149),
            Color.FromArgb(255, 234, 165), Color.FromArgb(183, 236, 184),
            Color.FromArgb(170, 228, 226), Color.FromArgb(184, 216, 248),
            Color.FromArgb(220, 194, 245), Color.FromArgb(242, 202, 224),
            Color.FromArgb(188, 204, 225)
        };

        /// <summary>Приглушённые цвета задач для тёмной темы с сохранением различимости оттенков.</summary>
        private static readonly Color[] darkTaskColors =
        {
            Color.FromArgb(217, 139, 148), Color.FromArgb(227, 173, 127),
            Color.FromArgb(231, 207, 138), Color.FromArgb(165, 209, 153),
            Color.FromArgb(141, 208, 211), Color.FromArgb(139, 186, 229),
            Color.FromArgb(192, 166, 218), Color.FromArgb(225, 177, 202),
            Color.FromArgb(145, 167, 195)
        };

        /// <summary>Ступени светлой темы: пастельные варианты вплоть до утверждённого базового цвета.</summary>
        private static readonly float[] taskPaletteLightFades = { 0.60f, 0.40f, 0.20f, 0f };

        /// <summary>Четыре отчётливо различимые ступени тёмной темы с читаемым тёмным текстом.</summary>
        private static readonly float[] taskPaletteDarkFades = { 0.58f, 0.39f, 0.20f, 0f };

        /// <summary>Исходные RGB для записи в задачу, построенные один раз.</summary>
        private static readonly Color[,] taskPaletteLight = BuildTaskPalette(ThemeKind.Light);

        /// <summary>Соответствующие цветовые уровни тёмной темы.</summary>
        private static readonly Color[,] taskPaletteDark = BuildTaskPalette(ThemeKind.Dark);

        /// <summary>Индекс палитрового RGB для быстрого рисования больших календарей.</summary>
        private static readonly Dictionary<int, int> taskPaletteIndices = BuildTaskPaletteIndices();

        /// <summary>
        /// Готовит оттенки один раз. Осветление к светлой нейтральной поверхности
        /// сохраняет видимую градацию и читаемость текста в обеих темах.
        /// </summary>
        /// <param name="kind">Тема для построения цветовых полос.</param>
        private static Color[,] BuildTaskPalette(ThemeKind kind)
        {
            Color[] bases = kind == ThemeKind.Dark ? darkTaskColors : lightTaskColors;
            Color surface = ThemePalette.For(ThemeKind.Light).Window;
            var result = new Color[bases.Length, TaskPaletteToneCount];
            for (int family = 0; family < bases.Length; family++)
                for (int tone = 0; tone < TaskPaletteToneCount; tone++)
                {
                    float fade = (kind == ThemeKind.Dark ? taskPaletteDarkFades : taskPaletteLightFades)[tone];
                    Color color = bases[family];
                    result[family, tone] = Color.FromArgb(
                        (int)(color.R * (1f - fade) + surface.R * fade),
                        (int)(color.G * (1f - fade) + surface.G * fade),
                        (int)(color.B * (1f - fade) + surface.B * fade));
                }
            return result;
        }

        /// <summary>Сопоставляет палитровые RGB ступеням без изменения старых произвольных цветов.</summary>
        private static Dictionary<int, int> BuildTaskPaletteIndices()
        {
            var result = new Dictionary<int, int>();
            for (int family = 0; family < lightTaskColors.Length; family++)
                for (int tone = 0; tone < TaskPaletteToneCount; tone++)
                    result[taskPaletteLight[family, tone].ToArgb()] = family * TaskPaletteToneCount + tone;
            return result;
        }

        /// <summary>Число цветовых семейств, поддерживаемых выбором оттенка задачи.</summary>
        internal static int TaskPaletteFamilyCount => lightTaskColors.Length;

        /// <summary>Число ступеней насыщенности в каждом цветовом семействе.</summary>
        internal const int TaskPaletteToneCount = 4;

        /// <summary>Возвращает неизменяемый исходный RGB для выбранной ячейки палитры.</summary>
        /// <param name="family">Номер семейства в таблице цветов.</param>
        /// <param name="tone">Номер ступени, от наиболее светлой до базовой.</param>
        internal static Color TaskPaletteSourceColor(int family, int tone)
        {
            return TaskPaletteDisplayColor(family, tone, ThemeKind.Light);
        }

        /// <summary>Показывает палитровый оттенок в указанной теме без изменения глобальной темы.</summary>
        /// <param name="family">Номер семейства.</param>
        /// <param name="tone">Номер ступени.</param>
        /// <param name="kind">Тема предпросмотра.</param>
        internal static Color TaskPaletteDisplayColor(int family, int tone, ThemeKind kind)
        {
            if (family < 0 || family >= TaskPaletteFamilyCount ||
                tone < 0 || tone >= TaskPaletteToneCount)
                throw new ArgumentOutOfRangeException(family < 0 || family >= TaskPaletteFamilyCount
                    ? nameof(family) : nameof(tone));
            Color[,] variants = kind == ThemeKind.Dark ? taskPaletteDark : taskPaletteLight;
            return variants[family, tone];
        }

        /// <summary>Возвращает цвет календаря для текущей темы без изменения исходного RGB задачи.</summary>
        /// <param name="original">Исходный цвет задачи.</param>
        internal static Color TaskDisplayColor(Color original)
        {
            return TaskDisplayColorInTheme(original, ThemeManager.Current);
        }

        /// <summary>
        /// Вычисляет цвет задачи для заданной темы. Новые палитровые оттенки сохраняют
        /// различия насыщенности, а произвольные старые RGB продолжают использовать
        /// существующее сопоставление с ближайшим цветовым семейством.
        /// </summary>
        /// <param name="original">Исходный цвет задачи.</param>
        /// <param name="kind">Тема, в которой будет отображена задача.</param>
        internal static Color TaskDisplayColorInTheme(Color original, ThemeKind kind)
        {
            if (original.IsEmpty || original.A == 0) return original;
            if (original.A == 255 && taskPaletteIndices.TryGetValue(original.ToArgb(), out int index))
                return TaskPaletteDisplayColor(index / TaskPaletteToneCount,
                    index % TaskPaletteToneCount, kind);

            Color[] colors = kind == ThemeKind.Dark ? darkTaskColors : lightTaskColors;
            int nearest = colors.Length - 1;
            if (original.GetSaturation() >= 0.14f)
            {
                float hue = original.GetHue();
                float minDistance = float.MaxValue;
                for (int i = 0; i < taskHueCenters.Length; i++)
                {
                    float distance = Math.Abs(hue - taskHueCenters[i]);
                    distance = Math.Min(distance, 360f - distance);
                    if (distance < minDistance) { minDistance = distance; nearest = i; }
                }
            }
            return Color.FromArgb(original.A, colors[nearest]);
        }

        /// <summary>Исходное правило определения просрочки без изменения статусов и дат задачи.</summary>
        internal static bool IsTaskOverdue(Task task)
        {
            return (task.Status == TaskStatus.New && task.DateStart.Date < DateTime.Today) ||
                (task.DateFinish.Date < DateTime.Today && task.Status != TaskStatus.Closed);
        }

        /// <summary>Согласованный винный оттенок текста и знаков просрочки.</summary>
        private static readonly Color overdueTaskInk = Color.FromArgb(79, 42, 57);

        /// <summary>Выбирает цвет по теме для обычной задачи и сохраняет винный цвет просрочки.</summary>
        internal static Color TaskTextColor(Task task)
        {
            if (IsTaskOverdue(task)) return overdueTaskInk;
            return ThemeManager.Current == ThemeKind.Dark ? darkTaskInk : lightTaskInk;
        }

        /// <summary>Отступ между восклицательным знаком и текстом, не меняющий геометрию полосы.</summary>
        private const int TaskOverdueMarkerGap = 3;

        /// <summary>Отступ восклицательного знака от края полосы задачи.</summary>
        private const int TaskOverdueMarkerEdgeInset = 1;

        /// <summary>Масштаб независимого шрифта знака просрочки относительно шрифта задачи.</summary>
        private const float TaskOverdueMarkerFontScale = 1.2f;

        /// <summary>Начертание знака просрочки, не зависящее от оформления статуса задачи.</summary>
        private const FontStyle TaskOverdueMarkerFontStyle = FontStyle.Bold;

        /// <summary>Рассчитывает ширину места под увеличенный знак просрочки.</summary>
        /// <param name="font">Исходный шрифт подписи задачи.</param>
        private static int TaskOverdueMarkerWidth(Font font)
        {
            return Math.Max(8, (int)Math.Ceiling(font.Height * TaskOverdueMarkerFontScale * 0.65f));
        }

        /// <summary>Определяет занимаемые маркерами края полосы; на узких сегментах оставляет место тексту.</summary>
        /// <param name="band">Фактические границы отображаемого сегмента.</param>
        /// <param name="font">Шрифт подписи задачи, определяющий масштаб знака.</param>
        /// <param name="markStart">Нужен знак в начале сегмента.</param>
        /// <param name="markEnd">Нужен знак в конце сегмента.</param>
        /// <param name="left">Ширина области, занятой знаком слева.</param>
        /// <param name="right">Ширина области, занятой знаком справа.</param>
        private static void GetTaskOverdueMarkerInsets(Rectangle band, Font font,
            bool markStart, bool markEnd, out int left, out int right)
        {
            int reserved = TaskOverdueMarkerEdgeInset + TaskOverdueMarkerWidth(font) +
                TaskOverdueMarkerGap;
            left = markStart && band.Width >= reserved + 4 ? reserved : 0;
            right = markEnd && band.Width >= left + reserved + 4 ? reserved : 0;
        }

        /// <summary>Исключает области знаков просрочки из прямоугольника текста задачи.</summary>
        /// <param name="band">Фактические границы сегмента.</param>
        /// <param name="font">Шрифт подписи задачи, по которому рассчитывается размер знака.</param>
        /// <param name="markStart">Знак в начале сегмента.</param>
        /// <param name="markEnd">Знак в конце сегмента.</param>
        internal static Rectangle TaskOverdueTextBounds(Rectangle band, Font font,
            bool markStart, bool markEnd)
        {
            GetTaskOverdueMarkerInsets(band, font, markStart, markEnd,
                out int left, out int right);
            return new Rectangle(band.Left + left, band.Top,
                Math.Max(0, band.Width - left - right), band.Height);
        }

        /// <summary>Рисует винные знаки просрочки отдельным увеличенным шрифтом без подложки.</summary>
        /// <param name="graphics">Поверхность отображаемого календаря.</param>
        /// <param name="band">Границы сегмента задачи.</param>
        /// <param name="font">Шрифт подписи, определяющий семейство и базовый размер знака.</param>
        /// <param name="markStart">Знак в начале сегмента.</param>
        /// <param name="markEnd">Знак в конце сегмента.</param>
        internal static void DrawTaskOverdueMarkers(Graphics graphics, Rectangle band, Font font,
            bool markStart, bool markEnd)
        {
            GetTaskOverdueMarkerInsets(band, font, markStart, markEnd,
                out int left, out int right);
            if (left == 0 && right == 0) return;

            int markerWidth = TaskOverdueMarkerWidth(font);
            const TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
            using (Font markerFont = new Font(font.FontFamily,
                font.Size * TaskOverdueMarkerFontScale, TaskOverdueMarkerFontStyle,
                font.Unit, font.GdiCharSet, font.GdiVerticalFont))
            {
                if (left > 0)
                    TextRenderer.DrawText(graphics, "!", markerFont,
                        new Rectangle(band.Left + TaskOverdueMarkerEdgeInset,
                            band.Top, markerWidth, band.Height), overdueTaskInk, flags);
                if (right > 0)
                    TextRenderer.DrawText(graphics, "!", markerFont,
                        new Rectangle(band.Right - TaskOverdueMarkerEdgeInset - markerWidth,
                            band.Top, markerWidth, band.Height), overdueTaskInk, flags);
            }
        }

        /// <summary>Ширина видимого свободного промежутка от линии сетки до полосы задачи.</summary>
        private const int TaskGridClearance = 2;

        /// <summary>Ширина зазора по каждой стороне внутреннего недельного стыка одной задачи.</summary>
        private const int TaskWeekSplitClearance = 1;

        /// <summary>Вычисляет зазоры задачи с отдельным правилом для её внутренних недельных стыков.</summary>
        /// <param name="planned">Прямоугольник ячейки без отступов.</param>
        /// <param name="viewportX">Горизонтальное смещение календаря.</param>
        /// <param name="offsetY">Вертикальное смещение календаря или предпросмотра.</param>
        /// <param name="belowPersonSeparator">Над ячейкой расположен двухпиксельный разделитель исполнителей.</param>
        /// <param name="rightWeekSeparator">Справа проходит двухпиксельная граница начала недели.</param>
        /// <param name="fromPreviousWeek">Слева находится недельный сегмент этой же задачи.</param>
        /// <param name="intoNextWeek">Справа продолжается недельный сегмент этой же задачи.</param>
        internal static Rectangle TaskBandBounds(Rectangle planned, int viewportX, int offsetY,
            bool belowPersonSeparator, bool rightWeekSeparator,
            bool fromPreviousWeek, bool intoNextWeek)
        {
            int leftInset = (fromPreviousWeek ? TaskWeekSplitClearance : TaskGridClearance) + 1;
            int rightInset = (intoNextWeek ? TaskWeekSplitClearance : TaskGridClearance) +
                (rightWeekSeparator ? 1 : 0);
            int topInset = TaskGridClearance + (belowPersonSeparator ? 0 : 1);
            int bottomInset = TaskGridClearance;
            return new Rectangle(planned.X + leftInset - viewportX,
                planned.Y + topInset + offsetY,
                Math.Max(1, planned.Width - leftInset - rightInset),
                Math.Max(1, planned.Height - topInset - bottomInset));
        }

        /// <summary>Создаёт форму сегмента: скруглены только внешние границы всей задачи.</summary>
        internal static GraphicsPath TaskPath(Rectangle rect, bool roundLeft = true, bool roundRight = true)
        {
            var path = new GraphicsPath();
            if (rect.Width < 2 || rect.Height < 2) return path;
            int x = rect.Left, y = rect.Top, right = rect.Right - 1, bottom = rect.Bottom - 1;
            int radius = Math.Min(TaskCornerRadius, Math.Min(rect.Width - 1, rect.Height - 1) / 2);
            if (radius <= 1 || (!roundLeft && !roundRight))
            {
                path.AddRectangle(new Rectangle(x, y, rect.Width - 1, rect.Height - 1));
                return path;
            }
            int diameter = radius * 2;
            path.StartFigure();
            if (roundLeft) path.AddArc(x, y, diameter, diameter, 180, 90);
            else path.AddLine(x, y, x, y);
            if (roundRight) path.AddArc(right - diameter, y, diameter, diameter, 270, 90);
            else path.AddLine(right, y, right, y);
            if (roundRight) path.AddArc(right - diameter, bottom - diameter, diameter, diameter, 0, 90);
            else path.AddLine(right, bottom, right, bottom);
            if (roundLeft) path.AddArc(x, bottom - diameter, diameter, diameter, 90, 90);
            else path.AddLine(x, bottom, x, bottom);
            path.CloseFigure();
            return path;
        }

        /// <summary>Рисует сегмент с мягкими внешними углами и ровными внутренними торцами.</summary>
        /// <param name="graphics">Графический контекст холста календаря.</param>
        /// <param name="rect">Границы сегмента в координатах холста.</param>
        /// <param name="fill">Кисть текущего цвета отображения задачи.</param>
        /// <param name="outline">Необязательный контур временного предпросмотра.</param>
        /// <param name="roundLeft">Скруглять только левый конец всей задачи.</param>
        /// <param name="roundRight">Скруглять только правый конец всей задачи.</param>
        internal static void FillTask(Graphics graphics, Rectangle rect, Brush fill, Pen outline = null,
            bool roundLeft = true, bool roundRight = true)
        {
            if (rect.Width < 2 || rect.Height < 2) return;
            SmoothingMode previous = graphics.SmoothingMode;
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath shape = TaskPath(rect, roundLeft, roundRight))
                {
                    graphics.FillPath(fill, shape);
                    if (!roundLeft || !roundRight)
                    {
                        graphics.SmoothingMode = SmoothingMode.None;
                        FillSquareTaskCaps(graphics, rect, fill, roundLeft, roundRight);
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    }
                    if (outline != null) graphics.DrawPath(outline, shape);
                }
            }
            finally { graphics.SmoothingMode = previous; }
        }

        /// <summary>Делает только внутренние прямые торцы непрозрачными без изменения внешних скруглений.</summary>
        /// <param name="graphics">Графический контекст со сглаживанием в режиме None.</param>
        /// <param name="rect">Пиксельные границы сегмента.</param>
        /// <param name="fill">Кисть текущего отображаемого цвета задачи.</param>
        /// <param name="roundLeft">Скругляется ли левый внешний конец задачи.</param>
        /// <param name="roundRight">Скругляется ли правый внешний конец задачи.</param>
        private static void FillSquareTaskCaps(Graphics graphics, Rectangle rect, Brush fill,
            bool roundLeft, bool roundRight)
        {
            int height = Math.Max(0, rect.Height - 1);
            if (!roundLeft) graphics.FillRectangle(fill, rect.Left, rect.Top, 1, height);
            if (!roundRight) graphics.FillRectangle(fill, rect.Right - 1, rect.Top, 1, height);
        }

        /// <summary>Состояние концов сегмента, сохраняемое при изменении размера WinForms-кнопки.</summary>
        private sealed class ButtonCorners
        {
            internal bool Left;
            internal bool Right;
        }

        /// <summary>Состояния кнопок не удерживают закрытые формы или задачи в памяти.</summary>
        private static readonly ConditionalWeakTable<Button, ButtonCorners> buttonCorners =
            new ConditionalWeakTable<Button, ButtonCorners>();

        /// <summary>Оформляет сегмент кнопки; обработчик изменения размера добавляется только один раз.</summary>
        internal static void ApplyTaskButtonCorners(Button button, bool roundLeft = true, bool roundRight = true)
        {
            if (button == null) return;
            ButtonCorners corners;
            if (!buttonCorners.TryGetValue(button, out corners))
            {
                corners = new ButtonCorners();
                buttonCorners.Add(button, corners);
                button.SizeChanged += TaskButtonResized;
                button.Disposed += TaskButtonDisposed;
            }
            corners.Left = roundLeft;
            corners.Right = roundRight;
            UpdateButtonRegion(button, corners);
        }

        /// <summary>Пересчитывает геометрию сегмента при изменении его размеров.</summary>
        private static void TaskButtonResized(object sender, EventArgs e)
        {
            var button = (Button)sender;
            ButtonCorners corners;
            if (buttonCorners.TryGetValue(button, out corners)) UpdateButtonRegion(button, corners);
        }

        /// <summary>Удаляет обработчики, когда сегмент выводится из эксплуатации.</summary>
        private static void TaskButtonDisposed(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.SizeChanged -= TaskButtonResized;
            button.Disposed -= TaskButtonDisposed;
            buttonCorners.Remove(button);
        }

        /// <summary>Освобождает старый графический регион при изменении границ сегмента.</summary>
        private static void UpdateButtonRegion(Button button, ButtonCorners corners)
        {
            if (button.Width < 2 || button.Height < 2) return;
            using (GraphicsPath path = TaskPath(new Rectangle(0, 0, button.Width, button.Height),
                corners.Left, corners.Right))
            {
                Region oldRegion = button.Region;
                button.Region = new Region(path);
                if (oldRegion != null) oldRegion.Dispose();
            }
        }
    }
}
