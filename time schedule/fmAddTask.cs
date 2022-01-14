using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Drawing.Color;

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
        public fmAddTask(LoadRefreshForm loadRefreshForm)
        {
            
            InitializeComponent();
            thisloadRefreshForm = loadRefreshForm;
        }
        LoadRefreshForm thisloadRefreshForm;
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
        private void ControlToReadOnly(Control control)
        {
            if (control is Button || control is RadioButton)
                control.Enabled = false;
            if (control is TextBox)
                (control as TextBox).ReadOnly = true;
            if (control is NumericUpDown)
            {
                (control as NumericUpDown).ReadOnly = true;
                (control as NumericUpDown).Increment = 0;
            }
            if (control is DateTimePicker)
            {
                (control as DateTimePicker).Enabled = false;
            }
                

             
            if (control is ComboBox && control != cmBxTaskStatus)
            {
                (control as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                for (int i=0; i< (control as ComboBox).Items.Count;i++)
                {
                    if ((control as ComboBox).Items[i].ToString() != (control as ComboBox).Text)
                    {
                        (control as ComboBox).Items.RemoveAt(i);
                        i--;
                    }
                }   
            }
            if (control is GroupBox)
            {
                foreach (Control controlInGbx in (control as GroupBox).Controls)
                {
                    ControlToReadOnly(controlInGbx);
                }
            }
                
        }
        private void fmAddTask_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            DateForFixChange = dTmTaskDateFinish.Value.Date;
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
            {
                thisloadRefreshForm?.Invoke();
                LoadFmAddTaskToCangeTask();
            }
            if (CreateOrChange == CreateOrChange.Create)
                LoadFmAddTaskToCreateTask();
            if (Program.UserType != UserType.Admin)
            {
                foreach (Control control in this.Controls)
                {
                    ControlToReadOnly(control);
                }
                cmBxTaskStatus.Enabled = true;
                btnCreateTask.Enabled = true;
            }
        }
        private void LoadFmAddTaskToCreateTask()
        {
            if (cmBxTaskStatus.Items.Count > 0)
                cmBxTaskStatus.Text = cmBxTaskStatus.Items[0].ToString();
            if (cmBxPerson.Items.Count > 0)
                cmBxPerson.Text = cmBxPerson.Items[0].ToString();
            if (Program.Person.PersonFamaly != string.Empty)
                cmBxPerson.Text = Program.Person.PersonFamaly;
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
            dTmTaskDateStart.Value = Program.Task.DateStart.Date;
            dTmTaskDateFinish.Value = Program.Task.DateFinish.Date;
        }
        public void WorkDaysDatesCalculate()
        {
            if (rBnDayFinish.Checked)
            {
                thisTask = new Task(dTmTaskDateStart.Value.Date, dTmTaskDateFinish.Value.Date);
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
            
            Program.listNonWorkingDays.NonWorkDaysWrite(
                dTmTaskDateStart.Value.Date,
                dTmTaskDateFinish.Value.Date
                );
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
            Program.listNonWorkingDays.NonWorkDaysWrite(
                dTmTaskDateStart.Value.Date,
                dTmTaskDateFinish.Value.Date
                );
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
            ColorDialog colorDialog1 = new ColorDialog();
            colorDialog1.AllowFullOpen = true;
            colorDialog1.CustomColors = new int[] { 
                ToInt(Color.FromArgb(169,208,142)),
                ToInt(Color.FromArgb(248,203,173)),
                ToInt(Color.FromArgb(132,151,176)),
                ToInt(Color.FromArgb(165,165,165)),
                ToInt(Color.FromArgb(255,230,153)),
                ToInt(Color.FromArgb(226,239,218)),
                ToInt(Color.FromArgb(142,169,219)),
                ToInt(Color.FromArgb(255,217,102)),
                ToInt(Color.FromArgb(221,235,247)),
                };
            colorDialog1.SolidColorOnly = true;
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
            {
                bTnColor.BackColor = colorDialog1.Color;
                Program.TaskColor = colorDialog1.Color;
            }
        }
        static int ToInt(Color c)
        {
            return c.R + c.G * 0x100 + c.B * 0x10000;
        }
        private DateTime DateForFixChange;
        private void btnCreateTask_Click(object sender, EventArgs e)
        {
            Program.ListTasksAllPerson.Tasks.Clear();
            Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            Task task = new Task();
            if (CreateOrChange == CreateOrChange.Create)
            {
                task = GetTaskForCreateChange(Program.ListTasksAllPerson.GetNextNumForTask());
                Program.ListTasksAllPerson.AddTask(task);
            }  
            if (CreateOrChange == CreateOrChange.Change)
            {

                Program.listNonWorkingDays.NonWorkingDays.Sort();
                int ForFixt = (dTmTaskDateFinish.Value.Date - DateForFixChange).Days;
                Program.listNonWorkingDays.NonWorkDaysWrite(
                    dTmTaskDateStart.Value.Date,
                    Program.listNonWorkingDays.NonWorkingDays[Program.listNonWorkingDays.NonWorkingDays.Count-1].AddDays(ForFixt)); // навести порядок в этом говнокоде to do
                task = GetTaskForCreateChange(Convert.ToInt32(nUpDnTaskNumber.Value));
                for(int i=0; i<Program.ListTasksAllPerson.Tasks.Count; i++)
                {
                    if (Program.ListTasksAllPerson.Tasks[i].Number == nUpDnTaskNumber.Value)
                    {
                        bool needToCheck = false;
                        if (Program.ListTasksAllPerson.Tasks[i].DateFinish.Date != task.DateFinish.Date)
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
            thisloadRefreshForm?.Invoke();
            
            this.Close();
        }
        private void ChekTaskAfter(Task task)
        {
            for (int i=0;i<Program.ListTasksAllPerson.Tasks.Count; i++)
            {
                if (Program.ListTasksAllPerson.Tasks[i].TaskNumberAfter==task.Number)
                {
                    Program.ListTasksAllPerson.Tasks[i].ChangeDatesCountDays(
                        Task.GetDateFinish(task.DateFinish,2),
                        Program.ListTasksAllPerson.Tasks[i].CountWorkingDays
                        ); // Magic number 2 to do
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
                dTmTaskDateStart.Value.Date,
                dTmTaskDateFinish.Value.Date,
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
                dTmTaskDateStart.Value = Task.GetDateFinish(Program.Task.DateFinish.Date, 2);
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

        private void addPerson_Click(object sender, EventArgs e)
        {
            fmAddPerson fmAddPerson = new fmAddPerson();
            
            fmAddPerson.ShowDialog();
            
            cmBxPerson.Items.Clear();
            foreach (Person person in Program.listPersons.Persons)
            {
                cmBxPerson.Items.Add(person.PersonFamaly);
            }
            if (fmAddPerson.CreatePerson == CreatePerson.yes)
                cmBxPerson.Text = fmAddPerson.GetPersonName();
        }

        private void fmAddTask_FormClosed(object sender, FormClosedEventArgs e)
        {
          
        }

        private void btnCopyAnotherProject_Click(object sender, EventArgs e)
        {
            fmProjectCopy fmProjectCopy = new fmProjectCopy();
            fmProjectCopy.ShowDialog();
            if (fmProjectCopy.SetTBxAddress().Text == "" ||
                fmProjectCopy.SetTBxAddress().Text == null ||
                fmProjectCopy.ChoiceIsMade == ChoiceIsMade.no)
            {
                return;
            }
            string folderName = fmProjectCopy.SetTBxAddress().Text;
            string targetFolderName = "Проект";
            try
            {
                Program.ListTasksAllPerson.Tasks.Clear();
                Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
                folderName += "\\" + targetFolderName;
                Task task = new Task();
                ListTasks listTasks = new ListTasks();
                listTasks.SetTasksFromList(Dals.ReadListFromFile(folderName + "\\" + Constants.TASKS));
                List<string> listStringPersons = Dals.ReadListFromFile(folderName + "\\" + Constants.PERSONS);
                task = GetTaskForCreateChange(listTasks.GetNextNumForTask());
                ListPersons listPersons = new ListPersons();
                listPersons.SetPersonsFromList(
                    Dals.ReadListFromFile(folderName + "\\" + Constants.PERSONS),
                    listTasks.Tasks);
                string personFamaly = "Нераспределено";
                bool isPersonHas = false;
                foreach (Person person in listPersons.Persons)
                {
                    if (person.PersonFamaly== personFamaly)
                    {
                        isPersonHas = true;
                        break;
                    }
                }
                if(!isPersonHas)
                {
                    Person person = new Person(personFamaly);
                    listPersons.Persons.Add(person);
                }
                
                foreach (Person person in listPersons.Persons)
                {
                    if (person.PersonFamaly == task.PersonFamaly)
                    {
                        personFamaly = task.PersonFamaly;
                        break;
                    }
                }
                Dals.WriteListtFileAppend(
                    folderName + "\\" + Constants.PERSONS, 
                    listPersons.GetListForSave());
                task.SetPersonFamaly(personFamaly);
                const int NUBER_AFTER_FOR_COPY = 0;
                task.SetTaskNumberAfter(NUBER_AFTER_FOR_COPY);
                listTasks.AddTask(task);
                Dals.WriteListtFileAppend(folderName + "\\" + Constants.TASKS, listTasks.GetListForSave());
                MessageBox.Show("Задача успешно скопирована.");
                thisloadRefreshForm?.Invoke();

            }
            catch
            {
                MessageBox.Show("Не удалось произвести запись в файл: " + folderName + "\\" + Constants.TASKS);
            }
        }
    }
}
