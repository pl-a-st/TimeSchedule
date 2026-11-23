using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    public partial class fmMainPathChoise : StyledWindowForm
    {
        /// <summary>Сохраняет расположение подписи и связывает Enter/Esc с выбором проекта и отменой.</summary>
        public fmMainPathChoise()
        {
            InitializeComponent();
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            AcceptButton = btnSelect;
            CancelButton = Cancel;
        }

        /// <summary>Панель исходных элементов, за изменением размера которой следит компоновка.</summary>
        private Control projectChoiceContent;

        /// <summary>Выравнивает список, адрес и командную колонку после применения темы.</summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            projectChoiceContent = lBxProject.Parent;
            if (projectChoiceContent != null)
                projectChoiceContent.SizeChanged += ProjectChoiceContentSizeChanged;
            ArrangeProjectChoiceControls();
        }

        /// <summary>Снимает подписку при закрытии окна выбора проекта.</summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (projectChoiceContent != null)
                projectChoiceContent.SizeChanged -= ProjectChoiceContentSizeChanged;
            projectChoiceContent = null;
            base.OnFormClosed(e);
        }

        /// <summary>Обновляет расположение элементов при изменении размера рабочей области.</summary>
        private void ProjectChoiceContentSizeChanged(object sender, EventArgs e)
        {
            if (!IsDisposed && !Disposing) ArrangeProjectChoiceControls();
        }

        /// <summary>Создаёт единые отступы и ширину четырёх команд без изменения логики выбора.</summary>
        private void ArrangeProjectChoiceControls()
        {
            Control content = lBxProject.Parent;
            if (content == null || content.ClientSize.Width <= 0) return;
            const int margin = 12;
            const int gap = 7;
            int right = content.ClientSize.Width - margin;
            int buttonWidth = Math.Max(btnSelect.Width, 92);
            int buttonsLeft = right - buttonWidth;
            if (buttonsLeft - lBxProject.Left < 180) return;
            Button[] buttons = { btnSelect, btnAdd, btnDelete, Cancel };
            foreach (Button button in buttons)
            {
                button.Left = buttonsLeft;
                button.Width = buttonWidth;
            }
            btnSelect.Top = lBxProject.Top;
            btnAdd.Top = btnSelect.Bottom + gap;
            btnDelete.Top = btnAdd.Bottom + gap;
            Control address = ThemeControlStyler.GetVisualControl(tBxAddress);
            if (address == null) return;
            address.SetBounds(lBxProject.Left,
                Math.Max(btnDelete.Bottom + label1.Height + 2 * gap,
                    content.ClientSize.Height - margin - address.Height),
                Math.Max(1, right - lBxProject.Left), address.Height);
            label1.Left = lBxProject.Left;
            label1.Top = address.Top - label1.Height - 5;
            lBxProject.Width = Math.Max(1, buttonsLeft - gap - lBxProject.Left);
            lBxProject.Height = Math.Max(1, label1.Top - 10 - lBxProject.Top);
            Cancel.Top = lBxProject.Bottom - Cancel.Height;
        }

        public TextBox SetTBxAddress()
        {
            return tBxAddress;
        }
        public ChoiceIsMade ChoiceIsMade
        { get; private set; } = ChoiceIsMade.no;
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex == -1)
            {
                ThemedMessageBox.Show("Не выбран проект.");
                return;
            }
            ChoiceIsMade = ChoiceIsMade.yes;
            this.Close();
            //string folderName = string.Empty;
            //string targetFolderName = "Проект";
            //try
            //{
            //    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //    folderBrowserDialog.Description = "Выбирете папку проекта для копирования задачи";

            //    if (tBxAddress.Text!="")
            //    {
            //        Program.ListTasksAllPerson.Tasks.Clear();
            //        Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            //        folderName = folderBrowserDialog.SelectedPath + "\\" + targetFolderName;
            //        Task task = new Task();
            //        ListTasks listTasks = new ListTasks();

            //        listTasks.SetTasksFromList(Dals.ReadListFromFile(folderName + "\\" + Constants.TASKS));
            //        List<string> listStringPersons = Dals.ReadListFromFile(folderName + "\\" + Constants.PERSONS);
            //        task = GetTaskForCreateChange(listTasks.GetNextNumForTask());

            //        string personFamaly = "Нераспределено";
            //        foreach (string stirngPersones in listStringPersons)
            //        {
            //            if (stirngPersones.Split('\t')[0] == task.PersonFamaly)
            //            {
            //                personFamaly = task.PersonFamaly;
            //                break;
            //            }
            //        }
            //        task.SetPersonFamaly(personFamaly);
            //        listTasks.AddTask(task);
            //        Dals.WriteListtFileAppend(folderName + "\\" + Constants.TASKS, listTasks.GetListForSave());

            //        this.Close();
            //    }
            //}
            //catch
            //{
            //    MessageBox.Show("Не удалось произвести запись в файл: " + folderName + "\\" + Constants.TASKS);
            //}
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex == -1)
            {
                ThemedMessageBox.Show("Не выбран проект.");
                return;
            }
            ListProjects.Projects.RemoveAt(lBxProject.SelectedIndex);
            lBxProject.Items.Clear();
            foreach (Project project in ListProjects.Projects)
            {
                lBxProject.Items.Add(project.Name);
            }
            if (lBxProject.Items.Count > 0)
                lBxProject.SelectedIndex = lBxProject.Items.Count - 1;
            //Dals.WriteObjectToFile(
            //    Constants.PROGECT_TO_CHOOSE,
            //    ListProjects.GetListForSave());
            Dals.binWriteObjectToFile(ListProjects, Dals.TakeUserPath(Constants.PROJECT_TO_CHOOSE_BIN));
        }
        public PoolProjects ListProjects
        { get; private set; } = new PoolProjects();
        /// <summary>Загружает список ранее выбранных проектов из пользовательского каталога.</summary>
        private void fmProjectCopy_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            
            string projectsFile = Dals.TakeUserPath(Constants.PROJECT_TO_CHOOSE_BIN);
            if (File.Exists(projectsFile))
                ListProjects = Dals.binReadUserPathFileToObject(ListProjects, Constants.PROJECT_TO_CHOOSE_BIN);
            else if (File.Exists(Constants.PROJECT_TO_CHOOSE))
                ListProjects = new PoolProjects(Dals.ReadListFromFile(Constants.PROJECT_TO_CHOOSE));
            else
                ListProjects = new PoolProjects();
            foreach(Project project in ListProjects.Projects)
            {
                lBxProject.Items.Add(project.Name);
            }
        }

        private void lBxProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex != -1)
                tBxAddress.Text = ListProjects.Projects[lBxProject.SelectedIndex].Address;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            fmAddProject fmAddProject = new fmAddProject();
            fmAddProject.ShowDialog();
            if (fmAddProject.ChoiceIsMade == ChoiceIsMade.no)
                return;
            ListProjects.Add(fmAddProject.Project);
            lBxProject.Items.Clear();
            foreach (Project project in ListProjects.Projects)
            {
                if (project.Name!=null)
                lBxProject.Items.Add(project.Name);
            }
            if (lBxProject.Items.Count>0)
                lBxProject.SelectedIndex = lBxProject.Items.Count-1;
            //Dals.WriteListtFileAppend(
            //    Constants.PROJECT_TO_CHOOSE,
            //    ListProjects.GetListForSave());
            Dals.binWriteObjectToFile(ListProjects, Dals.TakeUserPath(Constants.PROJECT_TO_CHOOSE_BIN));
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
