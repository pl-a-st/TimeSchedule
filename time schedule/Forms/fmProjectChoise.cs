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
    public partial class fmMainPathChoise : Form
    {
        public fmMainPathChoise()
        {
            InitializeComponent();
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
                MessageBox.Show("Не выбрана задача.");
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
                MessageBox.Show("Не выбрана задача.");
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
            Dals.WriteObjectToMainPathFile(Constants.PROJECT_TO_CHOOSE_BIN, ListProjects);
        }
        public PoolProjects ListProjects
        { get; private set; } = new PoolProjects();
        private void fmProjectCopy_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            string fullFileName = Dals.TakeUserPath(Constants.PROJECT_TO_CHOOSE_BIN);
            if (File.Exists(fullFileName)) {
                ListProjects = Dals.binReadUserPathFileToObject(ListProjects, fullFileName);
            }
            else {
                ListProjects = new PoolProjects(
                Dals.ReadListFromFile(Constants.PROJECT_TO_CHOOSE));
            }
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
