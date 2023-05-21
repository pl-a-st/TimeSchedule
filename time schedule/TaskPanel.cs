using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace time_schedule
{
    public enum Access
    {
        ForUser,
        ForProgram
    }
    public class DateTimePickerAccess : DateTimePicker
    {
        public Access Access = Access.ForUser;
    }
    public class NudAccess : NumericUpDown
    {
        public Access Access = Access.ForUser;
    }
    public enum CheckStatus
    {
        Checked,
        MultyCheck,
        None
    }
    [Serializable]
    public class TaskPanel
    {
        public delegate void TaskSelecter();
        public event TaskSelecter Selected;
        public delegate void DateTimeChanger();
        public event DateTimeChanger NudTaskAfterChange;
        public event DateTimeChanger DateFinishAfterChange;

        public static int TaskPanelesCount;
        public static int TaskPaneleLastHeight;
        public CheckStatus CheckStatus = CheckStatus.None;
        public Task Task = new Task();
        public Panel PnlForTask = new Panel();
        public CheckBox ChkNedUse = new CheckBox();
        public Label LblTaskNumber = new Label();
        public TextBox TxtTaskName = new TextBox();
        public DateTimePickerAccess DateStart = new DateTimePickerAccess();
        public DateTimePickerAccess DateFinish = new DateTimePickerAccess();
        public NudAccess NudWorckworkingDaysCount = new NudAccess();
        public ComboBox CboPersons = new ComboBox();
        public NudAccess NudTaskAfter = new NudAccess();
        private int PaddingY = 5;
        private int PaddingX = 3;
        public TaskPanel TaskAfter;

        public TaskPanel()
        {


        }
        public TaskPanel(string name, int num, Point panelLocation)
        {
            TxtTaskName.Text = name;
            LblTaskNumber.Text = num.ToString();
            PnlForTask.Location = panelLocation;
            SetParametersForControls();
            SetSizeForControls();
            SetLocationForControls(
                new Control[]
               {LblTaskNumber, TxtTaskName, DateStart, DateFinish, NudWorckworkingDaysCount, CboPersons, NudTaskAfter}
                );
            PnlForTask.Controls.AddRange(
                new Control[]
               {LblTaskNumber, TxtTaskName, DateStart, DateFinish, NudWorckworkingDaysCount, CboPersons, NudTaskAfter}
                );
            TaskPanelesCount++;
            PnlForTask.Enter += PnlForTask_Enter;
            PnlForTask.Leave += PnlForTask_Leave;
            PnlForTask.MouseDown += PnlForTask_MouseDown;
            LblTaskNumber.Click += LblTaskNumber_Click;
            NudTaskAfter.ValueChanged += NudTaskAfter_ValueChanged;
            NudTaskAfter.KeyUp += NudTaskAfter_KeyUp;
           
            DateStart.ValueChanged += DateStart_ValueChanged;
            DateFinish.ValueChanged += DateFinish_ValueChanged;
            DateFinish.ValueChanged += DateFinish_ValueChangedInvoke;
            NudWorckworkingDaysCount.ValueChanged += NudWorckworkingDaysCount_ValueChanged;
            NudWorckworkingDaysCount.KeyUp += NudWorckworkingDaysCount_KeyUp;
        }

        private void NudWorckworkingDaysCount_KeyUp(object sender, KeyEventArgs e)
        {
            if (int.TryParse("" + NudWorckworkingDaysCount.Value + e, out int result))
            {
                NudWorckworkingDaysCount.Value = (int)result;
            }
        }

        private void NudTaskAfter_KeyUp(object sender, KeyEventArgs e)
        {
            if(int.TryParse("" + NudTaskAfter.Value + e, out int result))
            {
                NudTaskAfter.Value = (int)result;
            }
            
        }

        private void DateFinish_ValueChangedInvoke(object sender, EventArgs e)
        {
            DateFinishAfterChange.Invoke();
        }
        private void NudWorckworkingDaysCount_ValueChanged(object sender, EventArgs e)
        {
            if (NudWorckworkingDaysCount.Access == Access.ForProgram)
            {
                return;
            }
            Task task = new Task(DateStart.Value, (long)NudWorckworkingDaysCount.Value);
            DateFinish.Access = Access.ForProgram;
            DateFinish.Value = task.DateFinish;
            DateFinish.Access = Access.ForUser;
        }
        private void DateFinish_ValueChanged(object sender, EventArgs e)
        {
            if (DateFinish.Access == Access.ForProgram)
            {
                return;
            }
            Task task = new Task(DateStart.Value, DateFinish.Value);
            NudWorckworkingDaysCount.Access = Access.ForProgram;

            NudWorckworkingDaysCount.Value = task.CountWorkingDays;
            NudWorckworkingDaysCount.Access = Access.ForUser;
        }
        private void DateStart_ValueChanged(object sender, EventArgs e)
        {
            if (DateStart.Access == Access.ForProgram)
            {
                return;
            }
            Task task = new Task(DateStart.Value, (long)NudWorckworkingDaysCount.Value);
            DateFinish.Access = Access.ForProgram;
            DateFinish.Value = task.DateFinish;
            DateFinish.Access = Access.ForUser;
        }
        private void NudTaskAfter_ValueChanged(object sender, EventArgs e)
        {
            NudTaskAfterChange?.Invoke();
        }
        private void LblTaskNumber_Click(object sender, EventArgs e)
        {
            LblTaskNumber.Focus();
        }
        private void PnlForTask_MouseDown(object sender, MouseEventArgs e)
        {
            CheckStatus = CheckStatus.Checked;
            PnlForTask.BackColor = Color.FromArgb(red: 50, green: 50, blue: 50);
            if (Control.ModifierKeys != Keys.Control)
            {
                Selected?.Invoke(); ;
            }
        }
        private void PnlForTask_Click(object sender, EventArgs e)
        {

        }
        private void PnlForTask_Leave(object sender, EventArgs e)
        {


        }
        private void PnlForTask_Enter(object sender, EventArgs e)
        {
            CheckStatus = CheckStatus.Checked;
            PnlForTask.BackColor = Color.FromArgb(red: 50, green: 50, blue: 50);
            if (Control.ModifierKeys != Keys.Control)
            {
                Selected?.Invoke();
            }
        }
        private void SetLocationForControls(Control[] controls)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                if (i == 0)
                {
                    controls[i].Location = new Point(x: PaddingX, y: PaddingY);
                    continue;
                }
                controls[i].Location = new Point(
               x: controls[i - 1].Location.X + controls[i - 1].Width + PaddingX,
               y: PaddingY
               );
            }
            PnlForTask.Width = controls[controls.Length - 1].Location.X + controls[controls.Length - 1].Width + 2 * PaddingX;
        }
        private void SetSizeForControls()
        {
            PnlForTask.Height = TxtTaskName.Height + 2 * PaddingY;
            TaskPaneleLastHeight = PnlForTask.Height;
            LblTaskNumber.Width = 30;
            TxtTaskName.Width = 400;
            CboPersons.Width = 200;
            DateStart.Width = DateStart.Width * 2 / 3;
            DateFinish.Width = DateFinish.Width * 2 / 3;
            NudWorckworkingDaysCount.Width = 70;
            NudTaskAfter.Width = 45;
        }
        private void SetParametersForControls()
        {
            ChkNedUse.AutoSize = true;
            ChkNedUse.Checked = true;
            ChkNedUse.Visible = false;
            NudWorckworkingDaysCount.Value = 1;
            Task task = new Task(DateStart.Value, (long)NudWorckworkingDaysCount.Value);
            DateFinish.Value = task.DateFinish;
            NudWorckworkingDaysCount.Minimum = int.MinValue;
            LblTaskNumber.ForeColor = Color.White;
            PnlForTask.BorderStyle = BorderStyle.FixedSingle;
            PnlForTask.BackColor = Color.FromArgb(red: 150, green: 150, blue: 150);
        }
        public void SetTaskPanelNotChecked()
        {
            CheckStatus = CheckStatus.None;
            PnlForTask.BackColor = Color.FromArgb(red: 150, green: 150, blue: 150);
        }
    }
    [Serializable]
    public class PoolTasksPanel
    {
        public List<TaskPanel> TaskPanels = new List<TaskPanel>();
        public void AddTaskPanel()
        {
            TaskPanel taskPanel = new TaskPanel(
                "Пример",
                TaskPanels.Count + 1,
                new Point(x: 3, y: 3 + TaskPanel.TaskPaneleLastHeight * TaskPanels.Count - TaskPanels.Count)
                );
            taskPanel.Selected += TaskPanel_Notify;
            void TaskPanel_Notify()
            {
                SetNotCheckedForTaskPaneles(taskPanel);
            }
            TaskPanels.Add(taskPanel);
            taskPanel.NudTaskAfterChange += TaskPanel_NudTaskAfterChenge;
            void TaskPanel_NudTaskAfterChenge()
            {
                SetDateTimesForTaskPaneles(taskPanel);

            }
            taskPanel.DateFinishAfterChange += TaskPanel_DateFinishAfterChange;
            void TaskPanel_DateFinishAfterChange()
            {
                SetDateTimesStartForTaskPaneles(taskPanel);
            }
        }
        public void SetNumForPanels()
        {

            for (int i = 0; i < TaskPanels.Count; i++)
            {
                TaskPanels[i].LblTaskNumber.Text = string.Empty + (i + 1);
            }
            foreach (var panel in TaskPanels)
            {
                panel.NudTaskAfter.Access = Access.ForProgram;
                if (panel.TaskAfter!=null)
                {
                    panel.NudTaskAfter.Value = int.Parse(panel.TaskAfter.LblTaskNumber.Text);
                }
                panel.NudTaskAfter.Access = Access.ForUser;
            }
        }
        private void SetDateTimesStartForTaskPaneles(TaskPanel taskPanel)
        {
            foreach (TaskPanel task in TaskPanels)
            {
                if (task.NudTaskAfter.Value == decimal.Parse(taskPanel.LblTaskNumber.Text))
                {
                    task.DateStart.Value = taskPanel.DateFinish.Value.AddDays(1);
                }
            }
        }
        public void SetNewTaskPanels(ListTasks listTasks)
        {
            TaskPanels.Clear();
            foreach (Task task in listTasks.Tasks)
            {
                TaskPanel taskPanel = new TaskPanel(
                                task.Name,
                                (int)task.Number,
                                new Point(x: 3, y: 3 + TaskPanel.TaskPaneleLastHeight * TaskPanels.Count - TaskPanels.Count)
                                );
                taskPanel.Selected += TaskPanel_Notify;
                void TaskPanel_Notify()
                {
                    SetNotCheckedForTaskPaneles(taskPanel);
                }
                TaskPanels.Add(taskPanel);
            }
        }
        public ListTasks GetListTasks()
        {
            ListTasks listTasks = new ListTasks();

            foreach (TaskPanel taskPanel in TaskPanels)
            {

                listTasks.AddTask(new Task(
                    TaskStatus.New,
                    taskPanel.TxtTaskName.Text,
                    taskPanel.CboPersons.Text,
                    (long)Convert.ToInt32(taskPanel.LblTaskNumber.Text),
                    (long)taskPanel.NudTaskAfter.Value,
                    taskPanel.DateStart.Value,
                    taskPanel.DateFinish.Value,
                    (new Task(taskPanel.DateStart.Value, taskPanel.DateFinish.Value)).CountDays,
                    (new Task(taskPanel.DateStart.Value, taskPanel.DateFinish.Value)).CountWorkingDays,
                    Color.FromArgb(50, 50, 50),
                    1
                    ));
            }
            return listTasks;
        }
        public void SetNotCheckedForTaskPaneles(TaskPanel SelectedTaskPanel)
        {
            foreach (TaskPanel taskPanel in TaskPanels)
            {
                if (taskPanel == SelectedTaskPanel)
                {
                    continue;
                }
                taskPanel.SetTaskPanelNotChecked();
            }
        }
        public void SetDateTimesForTaskPaneles(TaskPanel selectedTaskPanel)
        {
            if (selectedTaskPanel.NudTaskAfter.Access == Access.ForProgram)
            {
                return;
            }
            if (selectedTaskPanel.NudTaskAfter.Value == 0)
            {
                return;
            }
            foreach (TaskPanel taskPanel in TaskPanels)
            {
                if (taskPanel == selectedTaskPanel)
                {
                    continue;
                }
                if (int.Parse(taskPanel.LblTaskNumber.Text) == (int)selectedTaskPanel.NudTaskAfter.Value)
                {
                    Task task = new Task(selectedTaskPanel.DateStart.Value, selectedTaskPanel.DateFinish.Value);
                    task.ChangeDatesAndCountDays(taskPanel.DateFinish.Value.AddDays(1), task.CountWorkingDays);

                    selectedTaskPanel.DateStart.Value = task.DateStart;
                    selectedTaskPanel.DateFinish.Value = task.DateFinish;
                    selectedTaskPanel.TaskAfter = taskPanel;
                    break;
                }

            }
        }
    }
}
