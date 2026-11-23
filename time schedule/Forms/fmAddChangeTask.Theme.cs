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
            AlignProjectField();
            AlignTaskNameField();
            PrepareWorkOptionFields();
            ThemeControlStyler.EnsureInputFrames(this);
            ThemeControlStyler.EnsureInputSkins(this);
            HarmonizeWorkOptionFields();
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
        /// Выравнивает внешнюю рамку поля проекта с левым краем групп, сохраняя
        /// расстояние до кнопки выбора и центрируя подпись над строкой.
        /// </summary>
        private void AlignProjectField()
        {
            if (tBxProjects.Parent == null || !ReferenceEquals(tBxProjects.Parent, button2.Parent)) return;
            const int frameInset = 2;
            const int buttonGap = 6;
            int visualLeft = groupBox1.Left;
            int visualRight = button2.Left - buttonGap;
            int visualWidth = visualRight - visualLeft;
            if (visualWidth <= frameInset * 2) return;
            tBxProjects.Left = visualLeft + frameInset;
            tBxProjects.Width = visualWidth - frameInset * 2;
            label9.Left = visualLeft + Math.Max(0, (visualWidth - label9.Width) / 2);
        }

        /// <summary>
        /// Выравнивает внешние границы приоритета со статусом и сохраняет
        /// одинаковый зазор между рамками названия работы и приоритета.
        /// </summary>
        private void AlignTaskNameField()
        {
            const int frameInset = 2;
            const int fieldGap = 6;
            if (tBxTaskName.Parent == null || nUpDnPrioirity.Parent == null || cmBxTaskStatus.Parent == null ||
                !ReferenceEquals(nUpDnPrioirity.Parent, cmBxTaskStatus.Parent) ||
                !ReferenceEquals(tBxTaskName.Parent, nUpDnPrioirity.Parent)) return;

            nUpDnPrioirity.Left = cmBxTaskStatus.Left + frameInset;
            nUpDnPrioirity.Width = Math.Max(1, cmBxTaskStatus.Width - frameInset * 2);
            int visualNameLeft = tBxTaskName.Left - frameInset;
            int visualNameWidth = cmBxTaskStatus.Left - fieldGap - visualNameLeft;
            if (visualNameWidth > 100 + frameInset * 2)
                tBxTaskName.Width = visualNameWidth - frameInset * 2;
        }

        /// <summary>
        /// Задаёт размеры полей до подключения декоративных рамок, чтобы NumericUpDown
        /// не переразмечал внутренний редактор после оформления.
        /// </summary>
        private void PrepareWorkOptionFields()
        {
            const int frameInset = 2;
            const int fieldInset = 6;
            const int optionGap = 8;
            const int rightInset = 8;
            int startOptionWidth = Math.Max(rBnPreviousTask.PreferredSize.Width, rBnDayStart.PreferredSize.Width);
            int finishOptionWidth = Math.Max(rBnWorksDay.PreferredSize.Width, rBnDayFinish.PreferredSize.Width);
            int availableStart = groupBox1.ClientSize.Width - fieldInset - optionGap - rightInset - startOptionWidth;
            int availableFinish = groupBox2.ClientSize.Width - fieldInset - optionGap - rightInset - finishOptionWidth;
            int fieldWidth = Math.Min(availableStart, availableFinish);
            if (fieldWidth < 90) return;

            nUpDnPreviousTask.Left = fieldInset + frameInset;
            nUpDnPreviousTask.Width = Math.Max(1, fieldWidth - frameInset * 2);
            nUpDnCounWorkDay.Left = fieldInset + frameInset;
            nUpDnCounWorkDay.Width = Math.Max(1, fieldWidth - frameInset * 2);
            dTmTaskDateStart.Left = fieldInset;
            dTmTaskDateStart.Width = fieldWidth;
            dTmTaskDateFinish.Left = fieldInset;
            dTmTaskDateFinish.Width = fieldWidth;
        }

        /// <summary>
        /// Завершает выравнивание после подключения рамок, не изменяя размеры
        /// уже оформленных NumericUpDown.
        /// </summary>
        private void HarmonizeWorkOptionFields()
        {
            const int optionGap = 8;
            Control previousDays = ThemeControlStyler.GetVisualControl(nUpDnPreviousTask);
            Control workingDays = ThemeControlStyler.GetVisualControl(nUpDnCounWorkDay);
            if (previousDays == null || workingDays == null) return;

            int visualHeight = Math.Max(previousDays.Height, workingDays.Height);
            ThemeControlStyler.SetDatePickerVisualHeight(dTmTaskDateStart, visualHeight);
            ThemeControlStyler.SetDatePickerVisualHeight(dTmTaskDateFinish, visualHeight);
            int optionLeft = previousDays.Right + optionGap;
            AlignWorkOption(rBnPreviousTask, previousDays, optionLeft);
            AlignWorkOption(rBnDayStart, dTmTaskDateStart, optionLeft);
            AlignWorkOption(rBnWorksDay, workingDays, optionLeft);
            AlignWorkOption(rBnDayFinish, dTmTaskDateFinish, optionLeft);
        }

        /// <summary>Выравнивает переключатель по вертикальному центру своего поля.</summary>
        /// <param name="option">Радиокнопка варианта начала или окончания работы.</param>
        /// <param name="field">Внешняя поверхность соответствующего поля.</param>
        /// <param name="left">Единая левая граница радиокнопки в группе.</param>
        private static void AlignWorkOption(RadioButton option, Control field, int left)
        {
            option.Left = left;
            option.Top = field.Top + (field.Height - option.Height) / 2;
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
