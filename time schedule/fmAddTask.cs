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
    enum TypeStartWork
    {
        DateStart,
        TaskBefor
    }
    public enum CreateOrChange
    {
        Create,
        Change
    }
    public partial class fmAddTask : Form
    {
        Task thisTask = new Task();
        public fmAddTask()
        {
            
            InitializeComponent();
        }
        public CreateOrChange CreateOrChange
        { get; private set; }
        public void SetCreateOrChange (CreateOrChange createOrChange)
        {
            CreateOrChange = createOrChange;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (rBnWorksDay.Checked)
            {
                nUpDnCounWorkDay.Enabled = true;
                dTmTaskDateFinish.Enabled = false;
            }
            if (!rBnWorksDay.Checked)
            {
                nUpDnCounWorkDay.Enabled = false;
                dTmTaskDateFinish.Enabled = true;
            }
            WorkDaysDatesCalculate();
        }

        private void fmAddTask_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            foreach (Person person in Program.listPersons.Persons)
            {
                cmBxPerson.Items.Add(person.PersonFamaly);
            }
            foreach (int element in Enum.GetValues(typeof(TaskStatusRusEnum)))
            {
                cmBxTaskStatus.Items.Add((TaskStatusRusEnum)element);
            }
            bTnColor.FlatStyle = FlatStyle.Flat;
            if (CreateOrChange == CreateOrChange.Change)
                LoadFmAddTaskToCangeTask();
            if (CreateOrChange == CreateOrChange.Create)
                LoadFmAddTaskToCreateTask();
        }
        private void LoadFmAddTaskToCreateTask()
        {
            if (cmBxTaskStatus.Items.Count > 0)
                cmBxTaskStatus.Text = cmBxTaskStatus.Items[0].ToString();
            if (cmBxPerson.Items.Count > 0)
                cmBxPerson.Text = cmBxPerson.Items[0].ToString();
            rBnWorksDay.Checked = true;
            rBnDayStart.Checked = true;
            nUpDnPreviousTask.Enabled = false;
            tBxPreviousTask.Enabled = false;
            bTnColor.BackColor = Program.TaskColor;
        }
        private void LoadFmAddTaskToCangeTask()
        {
            nUpDnTaskNumber.Visible = true;
            nUpDnTaskNumber.Value = Program.Task.Number;
            lBlTaskNum.Visible = true;
            tBxTaskName.Text = Program.Task.Name;
            bTnColor.BackColor = Program.Task.Color;
            cmBxPerson.Text = Program.Task.PersonFamaly;
            cmBxTaskStatus.Text = ((TaskStatusRusEnum)Enum.Parse(typeof(TaskStatusEnum), Program.Task.Status.ToString(), true)).ToString();
            nUpDnPreviousTask.Value = Program.Task.TaskNumberAfter;
            rBnDayStart.Checked = true;
            nUpDnPrioirity.Value= Program.Task.Priority;
            if (nUpDnPreviousTask.Value>0)
            {
                foreach (Task task in Program.ListTasksAllPerson.Tasks)
                {
                    if (nUpDnPreviousTask.Value == task.Number)
                        tBxPreviousTask.Text = task.Name;
                }
                rBnPreviousTask.Checked = true;
            }
            rBnWorksDay.Checked = true;
            nUpDnCounWorkDay.Value = Program.Task.CountWorkingDays;
            dTmTaskDateStart.Value = Program.Task.DateStart;
            dTmTaskDateFinish.Value = Program.Task.DateFinish;
        }
        public void WorkDaysDatesCalculate()
        {
            if (rBnDayFinish.Checked)
            {
                thisTask = new Task(dTmTaskDateStart.Value.Date, dTmTaskDateFinish.Value);
                nUpDnCounWorkDay.Value = thisTask.CountWorkingDays;
            }
            if (rBnWorksDay.Checked)
            {
                thisTask = new Task(dTmTaskDateStart.Value.Date, Convert.ToInt32(nUpDnCounWorkDay.Value));
                dTmTaskDateFinish.Value = thisTask.DateFinish;
            }
        }
        private void dTTaskDateStart_ValueChanged(object sender, EventArgs e)
        {
            WorkDaysDatesCalculate();
        }
        private void taskDateFinishCalculate()
        {

        }

        private void nUpDnCounWorkDay_ValueChanged(object sender, EventArgs e)
        {
            WorkDaysDatesCalculate();
        }

        private void dTmTaskDateFinish_ValueChanged(object sender, EventArgs e)
        {
            WorkDaysDatesCalculate();
        }

        private void rBnDayStart_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rBnDayFinish_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rBtPreviousTask_CheckedChanged(object sender, EventArgs e)
        {
            if (rBnDayStart.Checked)
            {
                dTmTaskDateStart.Enabled = true;
                nUpDnPreviousTask.Value = 0;
                tBxPreviousTask.Text = string.Empty;
                btnPreviousTask.Enabled = false;
                nUpDnPreviousTask.Enabled = false;
                nUpDnPreviousTask.Enabled = false;
            }
            if (!rBnDayStart.Checked)
            {
                btnPreviousTask.Enabled = true;
                dTmTaskDateStart.Enabled = false;
            }
        }
        private void bTnColor_Click(object sender, EventArgs e)
        {
            colorDialog1.FullOpen = true;
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
            {
                bTnColor.BackColor = colorDialog1.Color;
                Program.TaskColor = colorDialog1.Color;
            }
        }

        private void btnCreateTask_Click(object sender, EventArgs e)
        {
            Task task = new Task();
            if (CreateOrChange == CreateOrChange.Create)
            {
                task = GetTaskForCreateChange(Program.ListTasksAllPerson.GetNextNumForTask());
                Program.ListTasksAllPerson.AddTask(task);
            }  
            if (CreateOrChange == CreateOrChange.Change)
            {
                task = GetTaskForCreateChange(Convert.ToInt32(nUpDnTaskNumber.Value));
                for(int i=0; i<Program.ListTasksAllPerson.Tasks.Count; i++)
                {
                    if (Program.ListTasksAllPerson.Tasks[i].Number == nUpDnTaskNumber.Value)
                    {
                        bool needToCheck = false;
                        if (Program.ListTasksAllPerson.Tasks[i].DateFinish != task.DateFinish)
                        {
                            needToCheck = true;
                        }
                        Program.ListTasksAllPerson.Tasks[i] = task;
                        if (needToCheck)
                            ChekTaskAfter(Program.ListTasksAllPerson.Tasks[i]);
                    }    
                }
            }
            Dals.WriteListProjectFileAppend(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
            this.Close();
        }
        private void ChekTaskAfter(Task task)
        {
            for (int i=0;i<Program.ListTasksAllPerson.Tasks.Count; i++)
            {
                if (Program.ListTasksAllPerson.Tasks[i].TaskNumberAfter==task.Number)
                {
                    Program.ListTasksAllPerson.Tasks[i].ChangeDatesCountDays(Task.GetDateFinish(task.DateFinish,2), Program.ListTasksAllPerson.Tasks[i].CountWorkingDays);
                    ChekTaskAfter(Program.ListTasksAllPerson.Tasks[i]);
                }
            }
        }
        private Task GetTaskForCreateChange(long numTask)
        {
            TaskStatusEnum taskStatus = (TaskStatusEnum)Enum.Parse(typeof(TaskStatusRusEnum), cmBxTaskStatus.Text, true);
            return  new Task
                (
                taskStatus,
                tBxTaskName.Text,
                cmBxPerson.Text,
                numTask,
                Convert.ToInt32(nUpDnPreviousTask.Value),
                dTmTaskDateStart.Value,
                dTmTaskDateFinish.Value,
                thisTask.CountDays,
                Convert.ToInt32(nUpDnCounWorkDay.Value),
                bTnColor.BackColor,
                Convert.ToInt32(nUpDnPrioirity.Value)
                );
        }
        private void cmBxPerson_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnPreviousTask_Click(object sender, EventArgs e)
        {
            fmSelectTask fmSelectTask = new fmSelectTask();
            fmSelectTask.ShowDialog();
            nUpDnPreviousTask.Value = Program.Task.Number;
            tBxPreviousTask.Text = Program.Task.Name;
            if (nUpDnPreviousTask.Value != 0)
            {
                dTmTaskDateStart.Value = Task.GetDateFinish(Program.Task.DateFinish,2);
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nUpDnCounWorkDay_KeyUp(object sender, KeyEventArgs e)
        {
            WorkDaysDatesCalculate();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
