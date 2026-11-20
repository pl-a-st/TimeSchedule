using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Подключает единое оформление к открывающимся формам без правок их Designer и бизнес-обработчиков.</summary>
    internal static class ThemeApplication
    {
        /// <summary>Формы, для которых уже установлена подписка на жизненный цикл.</summary>
        private static readonly HashSet<Form> registered = new HashSet<Form>();
        /// <summary>Признак однократной инициализации.</summary>
        private static bool started;

        /// <summary>Начинает наблюдение за окнами в штатном UI-потоке приложения.</summary>
        internal static void Start()
        {
            if (started) return;
            started = true;
            Application.Idle += RegisterOpenForms;
            ThemeManager.Changed += ApplyAll;
        }

        /// <summary>Подключает оформление к появившимся формам, не создавая новые таймеры.</summary>
        private static void RegisterOpenForms(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.IsDisposed || !registered.Add(form)) continue;
                form.FormClosed += UnregisterForm;
                form.Paint += DrawLightWindowBorder;
                form.Resize += WindowResized;
                if (!IsSpecialized(form)) ThemeControlStyler.Apply(form, null);
                form.Invalidate();
            }
        }

        /// <summary>Специализированные окна оформляются собственным адаптером той же палитры.</summary>
        private static bool IsSpecialized(Form form)
        {
            return form is Form1 || form is fmAddChangeTask ||
                form is ThemePreviewForm || form is TaskColorPaletteForm;
        }

        /// <summary>Применяет утверждённую тему к открытым обычным формам.</summary>
        private static void ApplyAll(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.IsDisposed) continue;
                if (!IsSpecialized(form)) ThemeControlStyler.Apply(form, null);
                form.Invalidate();
            }
        }

        /// <summary>Рисует тонкую плоскую рамку светлой формы, используя утверждённый цвет сетки.</summary>
        private static void DrawLightWindowBorder(object sender, PaintEventArgs e)
        {
            Form form = (Form)sender;
            if (form is StyledWindowForm || ThemeManager.Current != ThemeKind.Light ||
                form.WindowState == FormWindowState.Maximized ||
                form.ClientSize.Width < 2 || form.ClientSize.Height < 2) return;
            using (Pen pen = new Pen(ThemeManager.Palette.Grid, 1f))
                e.Graphics.DrawRectangle(pen, 0, 0,
                    form.ClientSize.Width - 1, form.ClientSize.Height - 1);
        }

        /// <summary>Обновляет рамку при изменении размера формы.</summary>
        private static void WindowResized(object sender, EventArgs e)
        {
            Form form = (Form)sender;
            if (!form.IsDisposed && ThemeManager.Current == ThemeKind.Light) form.Invalidate();
        }

        /// <summary>Снимает ссылку на закрытую форму, не удерживая её в памяти.</summary>
        private static void UnregisterForm(object sender, FormClosedEventArgs e)
        {
            var form = (Form)sender;
            form.FormClosed -= UnregisterForm;
            form.Paint -= DrawLightWindowBorder;
            form.Resize -= WindowResized;
            registered.Remove(form);
        }
    }
}
