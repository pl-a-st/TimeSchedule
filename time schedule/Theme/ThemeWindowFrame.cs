using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Единая прямоугольная рамка окна без изменения области дочерних элементов.</summary>
    internal static class ThemeWindowFrame
    {
        /// <summary>Атрибут DWM для управления формой углов окна.</summary>
        private const int DwmWindowCornerPreference = 33;
        /// <summary>Отключённое скругление Windows 11.</summary>
        private const int DoNotRound = 1;

        /// <summary>Определяет необходимость тонкой кромки обычного окна в обеих темах.</summary>
        internal static bool ShouldDraw(Form form)
        {
            return form.WindowState == FormWindowState.Normal &&
                form.ClientSize.Width > 2 && form.ClientSize.Height > 2;
        }

        /// <summary>Выключает скругление DWM без изменения работающей геометрии окон.</summary>
        internal static void Apply(Form form)
        {
            Region oldRegion = form.Region;
            if (oldRegion != null)
            {
                form.Region = null;
                oldRegion.Dispose();
            }
            if (form.IsHandleCreated)
            {
                int preference = DoNotRound;
                try
                {
                    DwmSetWindowAttribute(form.Handle, DwmWindowCornerPreference,
                        ref preference, Marshal.SizeOf(typeof(int)));
                }
                catch (DllNotFoundException) { }
                catch (EntryPointNotFoundException) { }
            }
            form.Invalidate();
        }

        /// <summary>Рисует внутренний контур поверх фона; основная кромка резервируется контейнером окна.</summary>
        internal static void Draw(Form form, Graphics graphics)
        {
            if (!ShouldDraw(form)) return;
            Rectangle bounds = form.ClientRectangle;
            bounds.Width -= 1;
            bounds.Height -= 1;
            if (bounds.Width < 1 || bounds.Height < 1) return;
            using (Pen pen = new Pen(ThemeManager.Palette.Grid))
                graphics.DrawRectangle(pen, bounds);
        }

        /// <summary>Запрашивает у DWM форму углов (если атрибут поддерживается системой).</summary>
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr window, int attribute,
            ref int value, int valueSize);
    }
}
