using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    public partial class fmProjectCopy : Form
    {
        public fmProjectCopy()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            string folderName = string.Empty;
            string targetFolderName = "Проект";
            //try
            //{
            //    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            //    folderBrowserDialog.Description = "Выбирете папку проекта для копирования задачи";

            //    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
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
        }
        public ListProjects ListProjects
        { get; private set; } = new ListProjects();
        private void fmProjectCopy_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            ListProjects = new ListProjects(
                Dals.ReadListFromProjectFile(Constants.PROFECT_TO_COPY));
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
            ListProjects.Add(fmAddProject.Project);
            lBxProject.Items.Clear();
            foreach (Project project in ListProjects.Projects)
            {
                lBxProject.Items.Add(project.Name);
            }
            if (lBxProject.Items.Count>0)
                lBxProject.SelectedIndex = lBxProject.Items.Count-1;
            Dals.WriteListProjectFileAppend(
                Constants.PROFECT_TO_COPY,
                ListProjects.GetListForSave());
        }

        private void Cancel_Click(object sender, EventArgs e)
        {

        }
    }
}
