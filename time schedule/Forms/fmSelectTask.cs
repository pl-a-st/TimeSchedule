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
    public partial class fmSelectTask : StyledWindowForm
    {
        /// <summary>Устанавливает порядок Tab и клавиши Enter/Esc для выбора задачи и отмены.</summary>
        public fmSelectTask()
        {
            InitializeComponent();
            tBxTargetTask.TabIndex = 0;
            lBxTasks.TabIndex = 1;
            btnSelectTask.TabIndex = 2;
            bTnCacel.TabIndex = 3;
            AcceptButton = btnSelectTask;
            CancelButton = bTnCacel;
        }

        private void btnSelectTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                ThemedMessageBox.Show("Не выбрана задача.");
                return;
            }
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
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
            this.TopMost = true;
            Program.Task = new Task();
            LoadLBxTasks();
        }
        private void LoadLBxTasks()
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
        }
        private void LoadLBxTasks(string targetTaskName)
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                if (task.Name.ToUpper().Contains(targetTaskName.ToUpper()) ||
                    task.Number.ToString().Contains(targetTaskName))
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
