using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Изолированный режим сравнения рисования полос задач и стандартных WinForms-кнопок.</summary>
    public partial class Form1
    {
        /// <summary>Включает экспериментальный холст по умолчанию только в Debug-сборке.</summary>
        private readonly bool canvasPilotEnabled = IsCanvasPilotEnabledByDefault();

        /// <summary>Повторно используемые кисти экспериментальной отрисовки.</summary>
        private readonly Dictionary<int, SolidBrush> canvasPilotBrushes = new Dictionary<int, SolidBrush>();

        /// <summary>Повторно используемые шрифты для отображения статусов задач.</summary>
        private readonly Dictionary<FontStyle, Font> canvasPilotFonts = new Dictionary<FontStyle, Font>();

        /// <summary>Количество рисований экспериментального холста после начала обновления.</summary>
        private int canvasPilotPaintEvents;

        /// <summary>Число сегментов, попавших в последнюю область перерисовки.</summary>
        private int canvasPilotLastSegments;

        /// <summary>Наибольшее измеренное время одного Paint экспериментального холста, мс.</summary>
        private double canvasPilotMaxPaintMs;

        /// <summary>Суммарное время Paint экспериментального холста после начала обновления, мс.</summary>
        private double canvasPilotTotalPaintMs;

        /// <summary>Отключает экспериментальное рисование после ошибки GDI+.</summary>
        private bool canvasPilotPaintFailed;

        /// <summary>Определяет режим рисования: экспериментальный в Debug, штатный в Release.</summary>
        private static bool IsCanvasPilotEnabledByDefault()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        /// <summary>Подключает альтернативную отрисовку к существующему PictureBox сетки без новых HWND.</summary>
        private void InitializeCanvasPilot()
        {
            if (canvasPilotEnabled)
            {
                pBForLine.Paint += PaintCanvasPilot;
                Trace.WriteLine("TimeSchedule canvas-pilot: mode=canvas-read-only interaction=disabled");
            }
#if DEBUG
            else
            {
                Trace.WriteLine("TimeSchedule canvas-pilot: mode=buttons interaction=normal");
            }
#endif
        }

        /// <summary>Сбрасывает показатели, не меняя существующие счетчики обновления.</summary>
        private void ResetCanvasPilotPaintMetrics()
        {
            canvasPilotPaintEvents = 0;
            canvasPilotLastSegments = 0;
            canvasPilotMaxPaintMs = 0;
            canvasPilotTotalPaintMs = 0;
        }

        /// <summary>Рисует геометрию уже построенных задач только в области Paint без создания Button.</summary>
        private void PaintCanvasPilot(object sender, PaintEventArgs e)
        {
            if (!canvasPilotEnabled || canvasPilotPaintFailed ||
                Program.ListTaskButtons == null)
                return;

            long started = Stopwatch.GetTimestamp();
            int segments = 0;
            try
            {
                Rectangle clip = e.ClipRectangle;
                foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
                {
                    Task task = taskButton.Task;
                    foreach (Rectangle planned in taskButton.CanvasPilotSegments)
                    {
                        Rectangle rectangle = new Rectangle(
                            planned.X + 1, planned.Y + 1, planned.Width - 2, planned.Height - 1);
                        if (rectangle.Width <= 0 || rectangle.Height <= 0 ||
                            !clip.IntersectsWith(rectangle))
                            continue;

                        e.Graphics.FillRectangle(GetCanvasPilotBrush(task.Color), rectangle);
                        TextRenderer.DrawText(e.Graphics, task.Name ?? string.Empty,
                            GetCanvasPilotFont(task.Status), rectangle,
                            GetCanvasPilotTextColor(task),
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                        segments++;
                    }
                }
            }
            catch (Exception ex) when (ex is ArgumentException ||
                ex is ObjectDisposedException || ex is ExternalException)
            {
                canvasPilotPaintFailed = true;
                Trace.WriteLine("TimeSchedule canvas-pilot paint failed: " + ex);
            }
            finally
            {
                double elapsedMs = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
                canvasPilotPaintEvents++;
                canvasPilotLastSegments = segments;
                canvasPilotTotalPaintMs += elapsedMs;
                canvasPilotMaxPaintMs = Math.Max(canvasPilotMaxPaintMs, elapsedMs);
            }
        }

        /// <summary>Возвращает одну кисть для каждого фактически использованного цвета.</summary>
        private SolidBrush GetCanvasPilotBrush(Color color)
        {
            int key = color.ToArgb();
            if (!canvasPilotBrushes.TryGetValue(key, out SolidBrush brush))
            {
                brush = new SolidBrush(color);
                canvasPilotBrushes.Add(key, brush);
            }
            return brush;
        }

        /// <summary>Воспроизводит оформление статусов, не создавая шрифт при каждом Paint.</summary>
        private Font GetCanvasPilotFont(TaskStatus status)
        {
            FontStyle style = FontStyle.Regular;
            if (status == TaskStatus.Closed)
                style = FontStyle.Strikeout;
            else if (status == TaskStatus.Active)
                style = FontStyle.Underline;
            else if (status == TaskStatus.To_approving)
                style = FontStyle.Italic | FontStyle.Underline;
            if (!canvasPilotFonts.TryGetValue(style, out Font font))
            {
                font = new Font(Control.DefaultFont, style);
                canvasPilotFonts.Add(style, font);
            }
            return font;
        }

        /// <summary>Применяет исходное правило красного текста просроченных задач.</summary>
        private static Color GetCanvasPilotTextColor(Task task)
        {
            return ((task.Status == TaskStatus.New && task.DateStart.Date < DateTime.Today) ||
                (task.DateFinish.Date < DateTime.Today && task.Status != TaskStatus.Closed))
                ? Color.DarkRed : SystemColors.ControlText;
        }

        /// <summary>Записывает сопоставимые показатели контролов, памяти и USER/GDI после обновления.</summary>
        private void LogCanvasPilotIdleMetrics()
        {
#if DEBUG
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    Trace.WriteLine("TimeSchedule canvas-pilot-result: mode=" +
                        (canvasPilotEnabled ? "canvas-read-only" : "buttons") +
                        " task-controls=" + (plMain.Controls.Count - 1) +
                        " total-main-controls=" + plMain.Controls.Count +
                        " gdi-handles=" + GetGuiResources(process.Handle, 0) +
                        " user-handles=" + GetGuiResources(process.Handle, 1) +
                        " private-memory-kb=" + process.PrivateMemorySize64 / 1024 +
                        " canvas-paint-events=" + canvasPilotPaintEvents +
                        " canvas-last-visible-segments=" + canvasPilotLastSegments +
                        " canvas-paint-total-ms=" + Math.Round(canvasPilotTotalPaintMs, 1) +
                        " canvas-paint-max-ms=" + Math.Round(canvasPilotMaxPaintMs, 1) +
                        " paint-failed=" + canvasPilotPaintFailed);
                }
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception ||
                ex is EntryPointNotFoundException || ex is DllNotFoundException)
            {
                Trace.WriteLine("TimeSchedule canvas-pilot resource diagnostics failed: " + ex);
            }
#endif
        }

        /// <summary>Освобождает шрифты, кисти и подписку экспериментального режима.</summary>
        private void DisposeCanvasPilot()
        {
            if (canvasPilotEnabled)
                pBForLine.Paint -= PaintCanvasPilot;
            foreach (SolidBrush brush in canvasPilotBrushes.Values)
                brush.Dispose();
            foreach (Font font in canvasPilotFonts.Values)
                font.Dispose();
            canvasPilotBrushes.Clear();
            canvasPilotFonts.Clear();
        }

        /// <summary>Получает счётчики оконных и графических ресурсов процесса Windows.</summary>
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr processHandle, int kind);
    }
}
