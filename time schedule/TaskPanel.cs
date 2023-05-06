using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace time_schedule
{
    public enum CheckStatus
    {
        Checked,
        MultyCheck,
        None
    }
    [Serializable]
    public class TaskPanel
    {
        public static int TaskPanelesCount;
        public static int TaskPaneleLastHeight;
        public CheckStatus CheckStatus = CheckStatus.None;
        public Task Task = new Task();
        public Panel PnlForTask = new Panel();
        public CheckBox ChkNedUse = new CheckBox();
        public Label LblTaskNumber = new Label();
        public TextBox TxtTaskName = new TextBox();
        public DateTimePicker DateStart = new DateTimePicker();
        public DateTimePicker DateFinish = new DateTimePicker();
        public ComboBox CboPersons = new ComboBox();
        public NumericUpDown NudTaskAfter = new NumericUpDown();
        private int PaddingY = 5;
        private int PaddingX = 3;
        public TaskPanel(string name, int num, Point panelLocation)
        {
            TxtTaskName.Text = name;
            LblTaskNumber.Text = num.ToString();
            PnlForTask.Location = panelLocation;
            SetParametersForControls();
            SetSizeForControls();
            SetLocationForCOntrols();
            PnlForTask.Controls.AddRange(
                new Control[]
               {ChkNedUse, LblTaskNumber, TxtTaskName, TxtTaskName, DateStart, DateFinish, CboPersons, NudTaskAfter}
                );
            TaskPanelesCount++;
            PnlForTask.Enter += PnlForTask_Enter;
            PnlForTask.Leave += PnlForTask_Leave;
            PnlForTask.MouseDown += PnlForTask_MouseDown; ;
        }

        private void PnlForTask_MouseDown(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                CheckStatus = CheckStatus.MultyCheck;
                PnlForTask.BackColor = Color.FromArgb(red: 50, green: 50, blue: 50);
            }
        }

        private void PnlForTask_Click(object sender, EventArgs e)
        {
            
        }

        private void PnlForTask_Leave(object sender, EventArgs e)
        {
            CheckStatus = CheckStatus.None;
            PnlForTask.BackColor = Color.FromArgb(red: 150, green: 150, blue: 150);
        }

        private void PnlForTask_Enter(object sender, EventArgs e)
        {
            CheckStatus = CheckStatus.Checked;
            PnlForTask.BackColor = Color.FromArgb(red: 50, green: 50, blue: 50);
        }

        private void SetLocationForCOntrols()
        {
            ChkNedUse.Location = new Point(x: PaddingX, y: PaddingY);
            LblTaskNumber.Location = new Point(
                x: ChkNedUse.Location.X + ChkNedUse.PreferredSize.Width + PaddingX,
                y: PaddingY
                );
            TxtTaskName.Location = new Point(
                x: LblTaskNumber.Location.X + LblTaskNumber.Width + PaddingX,
                y: PaddingY
                );
            DateStart.Location = new Point(
                x: TxtTaskName.Location.X + TxtTaskName.Width + PaddingX,
                y: PaddingY
                );
            DateFinish.Location = new Point(
                x: DateStart.Location.X + DateStart.Width + PaddingX,
                y: PaddingY
                );
            CboPersons.Location = new Point(
                x: DateFinish.Location.X + DateFinish.PreferredSize.Width + PaddingX,
                y: PaddingY
                );
            NudTaskAfter.Location = new Point(
                x: CboPersons.Location.X + CboPersons.Width + PaddingX,
                y: PaddingY
                );

            PnlForTask.Width = CboPersons.Location.X + CboPersons.PreferredSize.Width + NudTaskAfter.Width + 2 * PaddingX;
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
            NudTaskAfter.Width = 40;

        }

        private void SetParametersForControls()
        {

            ChkNedUse.AutoSize = true;
            ChkNedUse.Checked = true;
            PnlForTask.BorderStyle = BorderStyle.FixedSingle;
            PnlForTask.BackColor = Color.FromArgb(red: 150, green: 150, blue: 150);
        }

        public TaskPanel()
        {


        }
    }
    [Serializable]
    public class PoolTasksPanel
    {
        public List<TaskPanel> TaskPanels = new List<TaskPanel>();
        public void SetNewTaskPanels(ListTasks listTasks)
        {
            TaskPanels.Clear();
            foreach(Task task in listTasks.Tasks)
            {
                TaskPanel taskPanel = new TaskPanel(
                                task.Name,
                                (int)task.Number,
                                new Point(x: 3, y: 3 + TaskPanel.TaskPaneleLastHeight * TaskPanels.Count - TaskPanels.Count)
                                );
                TaskPanels.Add(taskPanel);
            }
        }
        public ListTasks GetListTasks()
        {
            ListTasks listTasks = new ListTasks();
            foreach(TaskPanel taskPanel in TaskPanels)
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
    }
}
