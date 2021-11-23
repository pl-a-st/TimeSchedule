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
    public partial class fmTasks : Form
    {
        public fmTasks()
        {
            InitializeComponent();
        }

        public void fmTasks_Load(object sender, EventArgs e)
        {
            LoadLBxTasks();
        }
        public ListBox RetunlBxTasks()
        {
            return lBxTasks;
        }
        private void LoadLBxTasks()
        {
            lBxTasks.Items.Clear();
            foreach(Task task in Program.ListTasksAllPerson.Tasks)
            {
                lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
        }
        private void LoadLBxTasks(string targetTaskName)
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                if (task.Name.ToUpper().Contains(targetTaskName.ToUpper()))
                    lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
        }
        private void btnNewTask_Click(object sender, EventArgs e)
        {
            fmAddTask fmAddTask = new fmAddTask();
            fmAddTask.SetCreateOrChange(CreateOrChange.Create);
            fmAddTask.ShowDialog();
            LoadLBxTasks();

        }

        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            DialogResult result = MessageBox.Show(
                "Вы уверены что хотите удалить задачу?",
                "Сообщение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                Program.fmMain.TopMost = true;
                this.TopMost = true;

                return;
            }
                
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                if (task.Number == Convert.ToInt32(lBxTasks.SelectedItem.ToString().Split('\t')[0]))
                {
                    Program.ListTasksAllPerson.Tasks.Remove(task);
                    break;
                }
                    
            }
            Dals.WriteListProjectFileAppend(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
            LoadLBxTasks();
        }

        private void btnChangeTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }

            fmAddTask fmAddTask = new fmAddTask();
            fmAddTask.GhangeNamebtnCreateTask("Изменить");
            fmAddTask.SetCreateOrChange(CreateOrChange.Change);
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                if (task.Number == Convert.ToInt32(lBxTasks.SelectedItem.ToString().Split('\t')[0]))
                {
                    Program.Task = task;
                    break;
                }
            }
            fmAddTask.ShowDialog();
            Dals.WriteListProjectFileAppend(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
            LoadLBxTasks();
        }
        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadLBxTasks(tBxTargetTask.Text);
        }
    }
}
