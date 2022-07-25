using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        Change,
        ChangeToSelect
    }
    public partial class fmAddChangeTask : Form
    {
        Task thisTask = new Task();
        private DateTime FinishDateBeforeChange;
        LoadRefreshForm thisloadRefreshForm;
        public TreeProjects TreeProjects {
            get; private set;
        } = new TreeProjects();
        public CreateOrChange CreateOrChange 
        { get; private set; }
        public ClickButton ClickButton {
            get; private set;
        } = ClickButton.Cancel;
        public fmAddChangeTask(LoadRefreshForm loadRefreshForm)
        {
            
            InitializeComponent();
            thisloadRefreshForm = loadRefreshForm;
        }
        public NumericUpDown GetPriority() {
            return nUpDnPrioirity;
        }
        public ComboBox GetStatus() {
            return cmBxTaskStatus;
        }
        public ComboBox GetPerson() {
            return cmBxPerson;
        }
        public Button GetColor() {
            return bTnColor;
        }
        public TextBox GetProject() {
            return tBxProjects;
        }
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
        private void ControlToNotEnabled(Control control) {
            if (control is RadioButton)
                control.Enabled = false;
            if (control is TextBox)
                (control as TextBox).Enabled = false;
            if (control is NumericUpDown) {
                (control as NumericUpDown).Enabled = false;
            }
            if (control is DateTimePicker) {
                (control as DateTimePicker).Enabled = false;
            }
            if (control is ComboBox) {
                (control as ComboBox).Enabled = false;
            }
            if (control is GroupBox) {
                foreach (Control controlInGbx in (control as GroupBox).Controls) {
                    ControlToNotEnabled(controlInGbx);
                }
            }

        }
        private void fmAddTask_Load(object sender, EventArgs e)
        {
            //this.TopLevel = true;
            this.TopMost = true;
           
            foreach (int element in Enum.GetValues(typeof(TaskStatusRus))) {
                cmBxTaskStatus.Items.Add(((TaskStatusRus)element).ToString().Replace('_', ' '));
            }
            if (CreateOrChange != CreateOrChange.ChangeToSelect) {
                FinishDateBeforeChange = Program.ListTasksAllPersonToSave.GetMaxDateFinishTasks();
                foreach (Person person in Program.listPersons.Persons) {
                    cmBxPerson.Items.Add(person.PersonFamaly);
                }
                bTnColor.FlatStyle = FlatStyle.Flat;
                if (CreateOrChange == CreateOrChange.Change) {
                    thisloadRefreshForm?.Invoke();
                    LoadFmAddTaskToCangeTask();
                }
                if (CreateOrChange == CreateOrChange.Create)
                    LoadFmAddTaskToCreateTask();
                if (Program.UserType != UserType.Admin) {
                    foreach (Control control in this.Controls) {
                        ControlToReadOnly(control);
                    }
                    cmBxTaskStatus.Enabled = true;
                    btnCreateTask.Enabled = true;
                    btnCancel.Enabled = true;
                }
            }
            if (CreateOrChange == CreateOrChange.ChangeToSelect) {
                foreach (Control control in this.Controls) {
                    ControlToNotEnabled(control);
                }
                foreach (Person person in Program.listPersons.Persons) {
                    cmBxPerson.Items.Add(person.PersonFamaly);
                }
                TreeProjects.GetTreeFromFile();
                AddCheckBoxForEnabled(cmBxTaskStatus);
                AddCheckBoxForEnabled(nUpDnPrioirity);
                AddCheckBoxForEnabled(tBxProjects);
                AddCheckBoxForEnabled(cmBxPerson);
                AddCheckBoxForEnabled(bTnColor);
                bTnColor.Enabled = false;
                btnCopyAnotherProject.Enabled = false;
            }
        }

        private void AddCheckBoxForEnabled(Control control) {
            CheckBox checkBox = new CheckBox();
            checkBox.Text = string.Empty;
            
            checkBox.Width = 13;
            control.Width -= (checkBox.Width+5);
            checkBox.Location = new Point(
                control.Location.X + control.Width + 5,
                control.Location.Y);
            control.Parent.Controls.Add(checkBox);
            checkBox.MouseClick += CheckBox_MouseClick;
            void CheckBox_MouseClick(object sender, MouseEventArgs e) {
                if (checkBox.Checked) {
                    control.Enabled = true;
                }
                if (!checkBox.Checked) {
                    control.Enabled = false;
                }
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
            TreeProjects.GetTreeFromFile();
            
        }
        private void LoadFmAddTaskToCangeTask()
        {
            nUpDnTaskNumber.Visible = true;
            nUpDnTaskNumber.Value = Program.Task.Number;
            lBlTaskNum.Visible = true;
            tBxTaskName.Text = Program.Task.Name;
            bTnColor.BackColor = Program.Task.Color;
            cmBxPerson.Text = Program.Task.PersonFamaly;
            cmBxTaskStatus.Text = (
                (TaskStatusRus)Enum.Parse(
                    typeof(TaskStatus),
                    Program.Task.Status.ToString(),
                    true)
                ).ToString().Replace('_',' ');
            nUpDnPreviousTask.Value = Program.Task.TaskNumberAfter;
            rBnDayStart.Checked = true;
            nUpDnPrioirity.Value= Program.Task.Priority;
            if (nUpDnPreviousTask.Value>0)
            {
                foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
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
            TreeProjects.GetTreeFromTask(Program.Task);
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
        private void WriteNewNonWorkigDays() {
            Program.listNonWorkingDays.NonWorkingDays.Sort();
            int newCountDaysAfterChange = (dTmTaskDateFinish.Value.Date - FinishDateBeforeChange).Days;
            if (newCountDaysAfterChange < 0)
                return;
            const int DIFFRENCE_QUANTITY_LAST_INDEX = 1;
            int lastIndex = Program.listNonWorkingDays.NonWorkingDays.Count - DIFFRENCE_QUANTITY_LAST_INDEX;
            DateTime newMaxDateFinish = 
                Program.listNonWorkingDays.NonWorkingDays[lastIndex].AddDays(newCountDaysAfterChange);
            Program.listNonWorkingDays.NonWorkDaysWrite(FinishDateBeforeChange, newMaxDateFinish);
        }
        private void btnCreateTask_Click(object sender, EventArgs e)
        {
            ClickButton = ClickButton.Aplly;
            if (IsTBxTaskNameEmpty())
                return;
            Program.ListTasksAllPersonToSave.Tasks.Clear();
            string fullFileName = Dals.TakeMainPath(Constants.TASKS_BIN);
            if (File.Exists(fullFileName)){
                Program.ListTasksAllPersonToSave = Dals.binReadFileToObject(
                    Program.ListTasksAllPersonToSave, fullFileName);
            }
            else {
                Program.ListTasksAllPersonToSave.SetTasksFromList(Dals.ReadListFromMainPathFile(Constants.TASKS));
            }
            
            Task task = new Task();
            if (CreateOrChange == CreateOrChange.Create)
            {
                task = GetTaskForCreateChange(Program.ListTasksAllPersonToSave.GetNextNumForTask());
                task.GetTreeProjects().SetTreeViewProjects(TreeProjects.ListTreeNode);
                Program.ListTasksAllPersonToSave.AddTask(task);
            }  
            if (CreateOrChange == CreateOrChange.Change)
            {
                WriteNewNonWorkigDays();
                task = GetTaskForCreateChange(Convert.ToInt32(nUpDnTaskNumber.Value));
                task.GetTreeProjects().SetTreeViewProjects(TreeProjects.ListTreeNode);
                for (int i=0; i<Program.ListTasksAllPersonToSave.Tasks.Count; i++)
                {
                    if (Program.ListTasksAllPersonToSave.Tasks[i].Number == nUpDnTaskNumber.Value)
                    {
                        bool needToCheck = false;
                        if (Program.ListTasksAllPersonToSave.Tasks[i].DateFinish.Date != task.DateFinish.Date)
                        {
                            needToCheck = true;
                        }
                        Program.ListTasksAllPersonToSave.Tasks[i] = task;
                        if (needToCheck)
                            ChekTaskAfter(Program.ListTasksAllPersonToSave.Tasks[i]);
                    }    
                }
            }
            
            //Dals.WriteObjectToFile(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
            Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
            Dals.WriteObjectToMainPathFile(Constants.PERSONS_BIN, Program.listPersons);
            Program.fmMain.LoadRefreshForm(Statuses.ProgressBar.Use);
           
            this.Close();
        }
        private Boolean IsTBxTaskNameEmpty() {
            if (tBxTaskName.Text == "" && CreateOrChange!=CreateOrChange.ChangeToSelect) {
                MessageBox.Show(
                    "Использвание пустых наименований задач недопустимо!",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }
        private void ChekTaskAfter(Task task)
        {
            for (int i=0;i<Program.ListTasksAllPersonToSave.Tasks.Count; i++)
            {
                if (Program.ListTasksAllPersonToSave.Tasks[i].TaskNumberAfter==task.Number)
                {
                    Program.ListTasksAllPersonToSave.Tasks[i].ChangeDatesAndCountDays(
                        Task.GetDateFinish(task.DateFinish,2),
                        Program.ListTasksAllPersonToSave.Tasks[i].CountWorkingDays
                        ); // Magic number 2 to do
                    ChekTaskAfter(Program.ListTasksAllPersonToSave.Tasks[i]);
                }
            }
        }
        private Task GetTaskForCreateChange(long numTask)
        {
            TaskStatus taskStatus =
                (TaskStatus)Enum.Parse(
                                        typeof(TaskStatusRus),
                                        cmBxTaskStatus.Text.Replace(' ','_'),
                                        true
                                        );
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

        private void btnCopyAnotherMainPath_Click(object sender, EventArgs e)
        {
            fmMainPathChoise fmMainPathCopy = new fmMainPathChoise();
            fmMainPathCopy.ShowDialog();
            if (fmMainPathCopy.SetTBxAddress().Text == "" ||
                fmMainPathCopy.SetTBxAddress().Text == null ||
                fmMainPathCopy.ChoiceIsMade == ChoiceIsMade.no)
            {
                return;
            }
            string folderName = fmMainPathCopy.SetTBxAddress().Text;
            //string targetFolderName = "Проект";
            try
            {
                Program.ListTasksAllPersonToSave.Tasks.Clear();
                string fullFileName = Dals.TakeMainPath(Constants.TASKS_BIN);
                if (File.Exists(fullFileName)) {
                    Program.ListTasksAllPersonToSave = Dals.binReadFileToObject(
                        Program.ListTasksAllPersonToSave, fullFileName);
                }
                else {
                     Program.ListTasksAllPersonToSave.SetTasksFromList(
                    Dals.ReadListFromMainPathFile(Constants.TASKS));
                }
               
                
                Task task = new Task();
                ListTasks listTasks = new ListTasks();
                string textForError = "Для отображения задач необходимо создать хотя бы одну.";
                listTasks.SetTasksFromList(Dals.ReadListFromFile(folderName + "\\" + Constants.TASKS, textForError));
                textForError = "Не создано ни одного исполнителя.";
                List<string> listStringPersons = Dals.ReadListFromFile(folderName + "\\" + Constants.PERSONS, textForError);
                task = GetTaskForCreateChange(listTasks.GetNextNumForTask());
                ListPersons listPersons = new ListPersons();

                listPersons.SetPersonsFromList(
                    Dals.ReadListFromFile(folderName + "\\" + Constants.PERSONS, textForError),
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
                Dals.binWriteObjectToFile(
                    listPersons,
                    folderName + "\\" + Constants.PERSONS_BIN);
                task.SetPersonFamaly(personFamaly);
                const int NUBER_AFTER_FOR_COPY = 0;
                task.SetTaskNumberAfter(NUBER_AFTER_FOR_COPY);
                listTasks.AddTask(task);
                Dals.binWriteObjectToFile(listTasks, folderName + "\\" + Constants.TASKS_BIN);
                MessageBox.Show("Задача успешно скопирована.");
                Program.fmMain.LoadRefreshForm(Statuses.ProgressBar.Use);
            }
            catch
            {
                MessageBox.Show("Не удалось произвести запись в файл: " + folderName + "\\" + Constants.TASKS);
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            fmProjectTree fmProjectTree = new fmProjectTree();
            fmProjectTree.StartPosition = FormStartPosition.CenterParent;
            
            
            if (CreateOrChange == CreateOrChange.Change) {
                //TreeProjects.SetTreeViewProjects(Program.Task.TreeProjects.TreeViewProjects);
                fmProjectTree.SetTreeView(TreeProjects);//2c
                fmProjectTree.SetHasLoad(HasLoad.Yes);
                fmProjectTree.ShowDialog();
                if (fmProjectTree.ClickButton == ClickButton.Aplly) {
                    TreeProjects.SetTreeViewProjects(fmProjectTree.projectTreeView);
                    TreeProjects.SaveTree();
                }
                    
            }
            if(CreateOrChange == CreateOrChange.Create ||
                CreateOrChange == CreateOrChange.ChangeToSelect) {
                string fullFileName = Dals.TakeMainPath(Constants.PROJECTS_LIST);
                TreeProjects.GetTreeFromFile();

                fmProjectTree.SetTreeView(TreeProjects);
                fmProjectTree.SetHasLoad(HasLoad.Yes);
                fmProjectTree.ShowDialog();
                if (fmProjectTree.ClickButton == ClickButton.Aplly) {
                    TreeProjects.SetTreeViewProjects(fmProjectTree.projectTreeView);
                    TreeProjects.SaveTree();
                }
                    
            }
        }
    }
}
