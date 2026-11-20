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
            ThemeControlStyler.EnsureInputFrames(this);
            ThemeControlStyler.EnsureInputSkins(this);
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
