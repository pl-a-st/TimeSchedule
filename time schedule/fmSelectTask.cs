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
    public partial class fmSelectTask : Form
    {
        public fmSelectTask()
        {
            InitializeComponent();
        }

        private void btnSelectTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                if (task.Number == Convert.ToInt32(lBxTasks.SelectedItem.ToString().Split('\t')[0]))
                {
                    Program.Task=task;
                    break;
                }
            }
            this.Close();
        }

        private void fmSelectTask_Load(object sender, EventArgs e)
        {
            Program.Task = new Task();
            LoadLBxTasks();
        }
        private void LoadLBxTasks()
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
        }
        private void LoadLBxTasks(string targetTaskName)
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                if(task.Name.Contains(targetTaskName))
                    lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
        }

        private void tBxTargetTask_TextChanged(object sender, EventArgs e)
        {
            LoadLBxTasks(tBxTargetTask.Text);
        }

        private void bTnCacel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
