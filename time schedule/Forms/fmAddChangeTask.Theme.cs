using System;
using System.Drawing;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Оформление редактора задач через общую систему тем без изменения логики редактирования.</summary>
    public partial class fmAddChangeTask
    {
        /// <summary>Редактор оформляется собственным адаптером с сохранением цвета задачи.</summary>
        protected override bool AutoThemeStandardControls { get { return false; } }

        /// <summary>Шрифт редактора, принадлежащий экземпляру формы.</summary>
        private Font editorThemeFont;
        /// <summary>Признак подключения к событиям темы.</summary>
        private bool editorThemeConnected;

        /// <summary>Оформляет поля после штатного создания и компоновки редактора.</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            HarmonizeEditorGeometry();
            ThemeControlStyler.EnsureInputFrames(this);
            ThemeControlStyler.EnsureInputSkins(this);
        }

        /// <summary>
        /// Отделяет поля дат от нижней линии группы и убирает пустую область под последней кнопкой.
        /// Размер измеряется после размещения содержимого под собственным заголовком окна.
        /// </summary>
        private void HarmonizeEditorGeometry()
        {
            const int borderSpace = 10;
            const int buttonGap = 9;
            const int bottomSpace = 14;
            nUpDnPreviousTask.Top = label6.Bottom + 5;
            rBnPreviousTask.Top = nUpDnPreviousTask.Top + 4;
            nUpDnCounWorkDay.Top = label5.Bottom + 5;
            rBnWorksDay.Top = nUpDnCounWorkDay.Top + 1;
            dTmTaskDateStart.Top = label3.Bottom + 6;
            dTmTaskDateFinish.Top = label4.Bottom + 6;
            rBnDayStart.Top = dTmTaskDateStart.Top;
            ArrangeFinishDateChoices();
            int requiredBottom = Math.Max(Math.Max(dTmTaskDateStart.Bottom, dTmTaskDateFinish.Bottom),
                Math.Max(rBnDayFinish.Bottom, rBnWorksDay.Bottom));
            int groupHeight = Math.Max(groupBox1.Height, requiredBottom + borderSpace);
            groupBox1.Height = groupHeight;
            groupBox2.Height = groupHeight;
            int groupBottom = Math.Max(groupBox1.Bottom, groupBox2.Bottom);
            btnCopyAnotherProject.Top = groupBottom + buttonGap;
            Control content = btnCopyAnotherProject.Parent;
            if (content == null) return;
            btnCopyAnotherProject.Left = Math.Max(0, (content.ClientSize.Width - btnCopyAnotherProject.Width) / 2);
            int desiredHeight = WindowChromeHeight + btnCopyAnotherProject.Bottom + bottomSpace;
            if (ClientSize.Height != desiredHeight)
                ClientSize = new Size(ClientSize.Width, desiredHeight);
        }

        /// <summary>
        /// Размещает варианты завершения рядом с подписями полей, не пересекая контур группы.
        /// При недостатке ширины переносит вариант даты ниже поля с сохранением полного текста.
        /// </summary>
        private void ArrangeFinishDateChoices()
        {
            const int horizontalSpace = 9;
            const int choiceGap = 8;
            int finishChoiceWidth = rBnDayFinish.PreferredSize.Width;
            int choiceLeft = groupBox2.ClientSize.Width - horizontalSpace - finishChoiceWidth;
            if (choiceLeft >= label4.Right + choiceGap)
            {
                rBnDayFinish.Location = new Point(choiceLeft, label4.Top);
            }
            else
            {
                rBnDayFinish.Location = new Point(dTmTaskDateFinish.Left,
                    dTmTaskDateFinish.Bottom + choiceGap);
            }

            int workChoiceLeft = Math.Max(label5.Right + choiceGap,
                groupBox2.ClientSize.Width - horizontalSpace - rBnWorksDay.PreferredSize.Width);
            if (workChoiceLeft + rBnWorksDay.PreferredSize.Width <= groupBox2.ClientSize.Width - horizontalSpace)
                rBnWorksDay.Location = new Point(workChoiceLeft, label5.Top);
        }

        /// <summary>Подключает единую тему и оформление раскрываемых списков.</summary>
        private void InitializeEditorTheme()
        {
            if (editorThemeConnected) return;
            editorThemeConnected = true;
            editorThemeFont = (Font)Control.DefaultFont.Clone();
            ThemeControlStyler.EnableComboList(cmBxPerson);
            ThemeControlStyler.EnableComboList(cmBxTaskStatus);
            ThemeManager.Changed += EditorThemeChanged;
            FormClosed += EditorThemeClosed;
            Disposed += EditorThemeDisposed;
            ApplyEditorTheme();
        }

        /// <summary>Оформляет поля и кнопки, не перекрашивая образец выбранного цвета задачи.</summary>
        private void ApplyEditorTheme()
        {
            if (IsDisposed) return;
            ThemeControlStyler.Apply(this, editorThemeFont, bTnColor);
            bTnColor.Font = editorThemeFont;
            bTnColor.Invalidate();
        }

        /// <summary>Обновляет внешний вид открытого редактора при смене темы.</summary>
        private void EditorThemeChanged(object sender, EventArgs e) { ApplyEditorTheme(); }

        /// <summary>Снимает подписку при закрытии даже при сохранённом экземпляре модальной формы.</summary>
        private void EditorThemeClosed(object sender, FormClosedEventArgs e)
        {
            ThemeManager.Changed -= EditorThemeChanged;
            editorThemeConnected = false;
        }

        /// <summary>Освобождает созданный шрифт и подписки жизненного цикла формы.</summary>
        private void EditorThemeDisposed(object sender, EventArgs e)
        {
            ThemeManager.Changed -= EditorThemeChanged;
            FormClosed -= EditorThemeClosed;
            Disposed -= EditorThemeDisposed;
            if (editorThemeFont != null)
            {
                editorThemeFont.Dispose();
                editorThemeFont = null;
            }
        }
    }
}
