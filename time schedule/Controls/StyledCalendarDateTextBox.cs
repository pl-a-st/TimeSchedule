using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Нередактируемый заголовок даты с вертикальным центрированием двух строк через штатный Win32 EDIT.</summary>
    public sealed class StyledCalendarDateTextBox : TextBox
    {
        /// <summary>Установка прямоугольника форматирования многострочного поля.</summary>
        private const int EmSetRect = 0x00B3;
        /// <summary>Текстовая область Win32 EDIT.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left, Top, Right, Bottom;
        }

        /// <summary>Создаёт неизменяемую ячейку, совместимую с прежним пулом TextBox.</summary>
        public StyledCalendarDateTextBox()
        {
            Multiline = true;
            ReadOnly = true;
            BorderStyle = BorderStyle.None;
            TextAlign = HorizontalAlignment.Center;
            WordWrap = false;
            TabStop = false;
        }

        /// <summary>Обновляет текстовую область после создания оконного дескриптора.</summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateTextArea();
        }

        /// <summary>Сохраняет центрирование при изменении размера ячейки.</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateTextArea();
        }

        /// <summary>Сохраняет центрирование при смене шрифта темы.</summary>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateTextArea();
        }

        /// <summary>Сохраняет центрирование при повторном использовании ячейки для другой даты.</summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            UpdateTextArea();
        }

        /// <summary>Устанавливает вертикальный отступ для двухстрочного текста, не заменяя штатное поведение TextBox.</summary>
        private void UpdateTextArea()
        {
            if (!IsHandleCreated || !Multiline || ClientSize.Width <= 2 || ClientSize.Height <= 2) return;
            int textHeight = Font.Height * 2;
            int top = Math.Max(0, (ClientSize.Height - textHeight) / 2);
            var rect = new NativeRect { Left = 1, Top = top,
                Right = ClientSize.Width - 1, Bottom = ClientSize.Height };
            SendMessage(Handle, EmSetRect, IntPtr.Zero, ref rect);
        }

        /// <summary>Устанавливает текстовый прямоугольник Win32 для многострочного EDIT.</summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr handle, int message,
            IntPtr wParam, ref NativeRect rectangle);
    }
}
