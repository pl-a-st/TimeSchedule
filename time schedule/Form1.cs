using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    

    public partial class Form1 : Form
    {
        public Form1()
        {
            Program.fmMain = this;
            InitializeComponent();
            plMain.MouseWheel += CalendarTasks_MouseWheel;
            CalendarTasks.MouseWheel += CalendarTasks_MouseWheel1;
            plMain.ClientSizeChanged += PlMain_ClientSizeChanged;
        }
        VScrollBar myScrollBar = new VScrollBar();
        private void PlMain_ClientSizeChanged(object sender, EventArgs e)
        {
            panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }

        private void CalendarTasks_MouseWheel1(object sender, MouseEventArgs e)
        {
            try
            {
                panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                int test = plMain.VerticalScroll.Value;
                plPeraonButton.VerticalScroll.Value = test;
            }
            catch
            {
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        public int GetPersonButtonWith()
        {
            return plMain.Location.X - plPeraonButton.Location.X;
        }
        public void LoadRefreshForm(ref Panel plPersonButton,ref Panel plMain, ref DataGridView calendarTasks, ref DataGridView dateTabl)
        {
            for (int i = 0; i < plMain.Controls.Count; i++)
            {
                if (plMain.Controls[i] is Button)
                {
                    plMain.Controls.Remove(plMain.Controls[i]);
                    i--;
                }

            }
            for (int i = 0; i < plPeraonButton.Controls.Count; i++)
            {
                if (plPeraonButton.Controls[i] is Button)
                {
                    plPeraonButton.Controls.Remove(plPeraonButton.Controls[i]);
                    i--;
                }

            }
            while (CalendarTasks.RowCount > 1)
            {
                CalendarTasks.Rows.RemoveAt(0);
            }
            while (CalendarTasks.ColumnCount > 1)
            {
                CalendarTasks.Columns.RemoveAt(0);
            }
            while (DateTable.ColumnCount > 1)
            {
                DateTable.Columns.RemoveAt(0);
            }
            Program.ListTasksAllPerson.Tasks.Clear();
            Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            Program.listPersons.Persons.Clear();
            Program.listPersons.SetPersonsFromList(Dals.ReadListFromProjectFile(Constants.PERSONS), Program.ListTasksAllPerson.Tasks);
            Program.ListPersonButton.PersonButtons.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListPersonButton.LoadListPersonButtons(
                Program.listPersons.Persons,
                Program.ListTasksAllPerson,
                Constants.ROW_HIGHT);
            int nextLocationY = 0;
            int nextRowsIndex;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    personButton.SetLocation(0, nextLocationY);
                    plPersonButton.Controls.Add(personButton.Button);
                    nextLocationY += (personButton.Button.Height + Constants.MIN_ROW_HIGHT+1);
                    const int TO_NEXT_ROWS = 1;
                    int maxCountSynchTask = personButton.Person.GetMaxCountSynchTask(Program.ListTasksAllPerson);
                    calendarTasks.Rows.Add(maxCountSynchTask + TO_NEXT_ROWS);
                }  
            }
            if (calendarTasks.Rows.Count > 1)
            {
                calendarTasks.Rows.RemoveAt(calendarTasks.Rows.Count - 2);
                calendarTasks.Rows.RemoveAt(calendarTasks.Rows.Count - 2);
            }
            int rowIndex = 0;
            while(rowIndex< calendarTasks.Rows.Count)
            {
                calendarTasks.Rows[rowIndex].Height = Constants.ROW_HIGHT;
                rowIndex++;
            }
            nextRowsIndex = -1;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    const int TO_NEXT_ROWS = 1; 
                    nextRowsIndex += (personButton.Person.GetMaxCountSynchTask(Program.ListTasksAllPerson) + TO_NEXT_ROWS);
                    if (nextRowsIndex < calendarTasks.Rows.Count - 1)
                    {
                        calendarTasks.Rows[nextRowsIndex].Height = Constants.MIN_ROW_HIGHT;
                        calendarTasks.Rows[nextRowsIndex].DefaultCellStyle.BackColor = Constants.MIN_COLUMN_COLOR;
                        calendarTasks.Rows[nextRowsIndex].HeaderCell.Style.BackColor = Color.Black;
                    } 
                }
            }
            LoadColumns(ref calendarTasks, ref dateTabl);
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListTaskButtons.LoadTaskButtons(
                Program.ListTasksAllPerson.Tasks,
                Program.ListPersonButton,
                CalendarTasks
                );
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
            {
                foreach (Button button in taskButton.Buttons)
                {
                    plMain.Controls.Add(button);
                    button.BringToFront();
                }
            }
            int height = 0;
            foreach (DataGridViewRow row in CalendarTasks.Rows)
            {
                height += row.Height;
            }
            height += CalendarTasks.ColumnHeadersHeight;

            int width = 0;
            foreach (DataGridViewColumn col in CalendarTasks.Columns)
            {
                width += col.Width;
            }
            width += CalendarTasks.RowHeadersWidth;

            CalendarTasks.ClientSize = new Size(width + 2, height + 2);
            DateTable.ClientSize = new Size(width + 2, DateTable.Height);
        }
        public void LoadColumns(ref DataGridView calendarTasks, ref DataGridView dateTabl)
        {
            DateTime dateToTables = Program.ListTasksAllPerson.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            calendarTasks.Columns[0].HeaderText = dateToTables.ToShortDateString();
            calendarTasks.Columns[0].Name = dateToTables.ToShortDateString();
            calendarTasks.Columns[0].Width = Constants.COLUMN_WITH;
            calendarTasks.RowHeadersWidth = 4;
            calendarTasks.Columns[0].Width = Constants.COLUMN_WITH;
            dateTabl.Columns[0].HeaderText = dateToTables.ToShortDateString() + "\n" + dateToTables.DayOfWeek;
            dateTabl.Columns[0].Name = dateToTables.ToShortDateString();
            dateTabl.Columns[0].Width = Constants.COLUMN_WITH;
            if (DateTime.Today.Date == dateToTables.Date)
                dateTabl.Columns[0].HeaderCell.Style.BackColor = Color.DarkBlue;
            dateTabl.RowHeadersWidth = 4;
            dateTabl.Columns[0].Width = Constants.COLUMN_WITH;
            int numColumn = 1;
            if (dateToTables != DateTime.MaxValue || dateMaxToTable != DateTime.MinValue)
            {
                calendarTasks.Visible = false;
                calendarTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dateTabl.Visible = false;
                dateTabl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                calendarTasks.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                dateTabl.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                dateTabl.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                calendarTasks.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                
                while (dateToTables.AddDays(1) <= dateMaxToTable)
                {
                    string strDateToTables = dateToTables.AddDays(1).ToShortDateString();
                    calendarTasks.Columns.Add(strDateToTables, strDateToTables);
                    //calendarTasks.Columns[numColumn].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    //calendarTasks.Columns[numColumn].Width = Constants.COLUMN_WITH;
                    dateTabl.Columns.Add(strDateToTables, strDateToTables + "\n" + dateToTables.AddDays(1).DayOfWeek);
                    //dateTabl.Columns[numColumn].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    //dateTabl.Columns[numColumn].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    //dateTabl.Columns[numColumn].Width = Constants.COLUMN_WITH;
                    dateToTables = dateToTables.AddDays(1);
                    if (DateTime.Today.Date == dateToTables.Date)
                    {
                        dateTabl.EnableHeadersVisualStyles = false;
                        dateTabl.Columns[numColumn].HeaderCell.Style.BackColor = Color.LightBlue;
                        if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday)
                            try { dateTabl.Columns[numColumn - 1].HeaderCell.Style.BackColor = Color.LightBlue; }
                            catch { }
                        if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                            try { dateTabl.Columns[numColumn + 1].HeaderCell.Style.BackColor = Color.LightBlue; }
                            catch { }
                    }
                    if (dateToTables.DayOfWeek == DayOfWeek.Saturday || dateToTables.DayOfWeek == DayOfWeek.Sunday)
                    {
                        dateTabl.EnableHeadersVisualStyles = false;
                        //dateTabl.Columns[numColumn].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dateTabl.Columns[numColumn].MinimumWidth = 2;
                        dateTabl.Columns[numColumn].Width = 2;
                        dateTabl.Columns[numColumn].DefaultCellStyle.BackColor = Color.LightBlue;
                        dateTabl.Columns[numColumn].HeaderCell.Style.BackColor = Color.LightBlue;
                        calendarTasks.EnableHeadersVisualStyles = false;
                        //calendarTasks.Columns[numColumn].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        calendarTasks.Columns[numColumn].MinimumWidth = 2;
                        calendarTasks.Columns[numColumn].Width = 2;
                        calendarTasks.Columns[numColumn].DefaultCellStyle.BackColor = Color.LightBlue;
                        calendarTasks.Columns[numColumn].HeaderCell.Style.BackColor = Color.LightBlue;
                    }
                    numColumn++;
                }
                
                calendarTasks.Visible = true;
                dateTabl.Visible = true;
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            myScrollBar.Height = plMain.Height;
            myScrollBar.Left = plMain.Width - myScrollBar.Width;
            myScrollBar.Top = 0;
            myScrollBar.Enabled = false;
            plMain.Controls.Add(myScrollBar);

            Dals.WriteProjectFolder(true);
            this.Activate();
            LoadRefreshForm(ref plPeraonButton,ref plMain,ref CalendarTasks,ref DateTable);
            NonWorkDaysWrite(DateTime.Now.AddYears(-1).Date, DateTime.Now.AddYears(1).Date);
            plMain.VerticalScroll.Visible = true;
            
        }

        private void CalendarTasks_MouseWheel(object sender, MouseEventArgs e)
        {
            plMain.Focus();
            panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
           
            int test = plMain.VerticalScroll.Value;
            plPeraonButton.VerticalScroll.Value = test;


        }

        public void ScrollToBottom(Panel p)
        {
            using (Control c = new Control() { Parent = p, Dock = DockStyle.Bottom })
            {
                p.ScrollControlIntoView(c);
                c.Parent = null;
            }
        }


        public void NonWorkDaysWrite(DateTime вeginningPeriod, DateTime endPeriod)
        {
            int i = 0;
            while(вeginningPeriod.AddDays(i) < endPeriod)
            {
                if (вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                    вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    Program.listNonWorkingDays.NonWorkingDays.Add(вeginningPeriod.AddDays(i));
                i++;
            }
        }

        private void ScrollChange(object sender, ScrollEventArgs e)
        {
            panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            plPeraonButton.VerticalScroll.Value = plMain.VerticalScroll.Value;
        }
       

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            fmWorkWithPersons fmWorkWithPersons = new fmWorkWithPersons();
            fmWorkWithPersons.ShowDialog();
            

        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            fmTasks fmTasks = new fmTasks();
            fmTasks.ShowDialog();
           
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dals.WriteProjectFolder();
            
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value = panel2.HorizontalScroll.Value;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int locationX = 0;
            int i = 0;
           
            while ((DateTable.Columns[i].Name!=DateTime.Today.Date.ToShortDateString())&&(i< DateTable.Columns.Count-1))
            {
                locationX += CalendarTasks.Columns[i].Width;
                i++;
            }
            locationX += CalendarTasks.RowHeadersWidth-5*Constants.COLUMN_WITH;
            panel2.Focus();
            plMain.Focus();
            plMain.HorizontalScroll.Value = locationX;
            plMain.HorizontalScroll.Value = locationX;
            panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            panel2.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value = 0;
            panel2.HorizontalScroll.Value = 0;
        }
        private void LoadDataGridPersonButton(ref DataGridView dataGridView, ListPersonButton listPersonButton)
        {
            Point startPositionPersonButton;

        }

        private void plMain_MouseEnter(object sender, EventArgs e)
        {

        }

        private void plMain_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            LoadRefreshForm(ref plPeraonButton, ref plMain, ref CalendarTasks, ref DateTable);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i=0; i< plMain.Controls.Count; i++)
            {
                if (plMain.Controls[i] is Button)
                {
                    plMain.Controls.Remove(plMain.Controls[i]);
                    i--;
                }
                    
            }
            while(CalendarTasks.RowCount>1)
            {
                CalendarTasks.Rows.RemoveAt(0);
            }
            while(CalendarTasks.ColumnCount>1)
            {
                CalendarTasks.Columns.RemoveAt(0);
            }
            while (DateTable.ColumnCount>1)
            {
                DateTable.Columns.RemoveAt(0);
            }
        }
    }
    public class ListTaskButton
    {
        public List<TaskButton> TaskButtons
        { get; private set; } = new List<TaskButton>();
        public void LoadTaskButtons(TaskButton taskButton)
        {
            TaskButtons.Add(taskButton);
        }
        public void LoadTaskButtons(List<Task> listTask,ListPersonButton listPersonButton, DataGridView dateTable)
        {
            foreach (Task task in listTask)
            {
                TaskButton taskButton = new TaskButton(task, listPersonButton, dateTable);
                LoadTaskButtons(taskButton);
            }
        }
    }

    public class TaskButton
    {
        public Task Task
        { get; private set; }
        public void SetTask(Task task)
        {
            Task = task;
        }
        public List<Button> Buttons
        { get; private set; } = new List<Button>();
        public void AddButton(int locationX, int locationY, int with, int height)
        {
            Button button = new Button();
            button.Location = new Point(locationX, locationY);
            button.Width = with;
            button.Height = height;
            button.Text = Task.Name;
            button.BackColor = Task.Color;
            button.FlatStyle = FlatStyle.Flat;
            button.Click += Button_Click;
            Buttons.Add(button);
        }
        private void Button_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();//прописать
        }
        public TaskButton(Task task, ListPersonButton listPersonButton,DataGridView dateTable)
        {
            Task = task;
            int locationY = 0;
            int locationX = 0;
            int width = 0;
            List<Task> listTaskThisPerson = new List<Task>();
            foreach (PersonButton personButton in listPersonButton.PersonButtons)
            {
                if (personButton.Person.PersonFamaly == task.PersonFamaly)
                {
                    listTaskThisPerson = personButton.Person.ListTask.Tasks;
                    locationY = personButton.Button.Location.Y;
                }
            }
            locationY += Task.PlaceInSynchTask * Constants.ROW_HIGHT;
            int i = 0;
            int dateTableLastNumCol = dateTable.Columns.Count;
            if (Task.Name == "jkjhg")
            {

            }
            while ((dateTable.Columns[i].Name != Task.DateStart.Date.ToShortDateString()) && (i < dateTable.Columns.Count - 1))
            {
                locationX += dateTable.Columns[i].Width;
                i++;
            }
            locationX += dateTable.RowHeadersWidth;
            do
            {
                if(
                    DateTime.Parse(dateTable.Columns[i].Name + " 00:00:00").DayOfWeek != DayOfWeek.Sunday &&
                    DateTime.Parse(dateTable.Columns[i].Name + " 00:00:00").DayOfWeek != DayOfWeek.Saturday
                    )
                {
                    width += dateTable.Columns[i].Width;
                    if (DateTime.Parse(dateTable.Columns[i].Name + " 00:00:00").DayOfWeek == DayOfWeek.Friday ||
                        dateTable.Columns[i].Name == Task.DateFinish.Date.ToShortDateString())
                    {
                        
                        AddButton(locationX, locationY, width, Constants.ROW_HIGHT);
                        locationX += width;
                        width = 0;
                    }
                }
                else
                {
                    locationX += dateTable.Columns[i].Width;
                }
                i++;
                
            }
            while ((dateTable.Columns[i-1].Name != Task.DateFinish.Date.ToShortDateString()) && (i < dateTableLastNumCol));
        }
    }
    public class ListPersonButton
    {
        public List<PersonButton> PersonButtons
        { get; private set; } = new List<PersonButton>();
        public void ListPersonButtonsAdd(PersonButton personButton)
        {
            PersonButtons.Add(personButton);
        }
        public void LoadListPersonButtons(List<Person> persons, ListTasks listTasksAllPerson, int hightRowForTasks)
        {
            foreach (Person person in persons)
            {
                PersonButton personButton = new PersonButton(person, listTasksAllPerson, hightRowForTasks);
                ListPersonButtonsAdd(personButton);
            }
        }
    }
    public class PersonButton
    {
        public PersonButton(Person person, ListTasks listTasksAllPerson, int hightRowForTasks)
        {
            Person = person;
            Person.setTasks(listTasksAllPerson);
            Button.Text = person.PersonFamaly;
            Button.Height = GetHightBooton(listTasksAllPerson, hightRowForTasks);
            Button.BringToFront();
            Button.Click += PersonButton_Click;
            Form1 form1 = new Form1();
            Button.Width = form1.GetPersonButtonWith();

        }
        public void SetLocation(int locationХ, int locationY)
        {
            Button.Location = new Point(locationХ, locationY);
        }
        private void PersonButton_Click(object sender, EventArgs e)
        {
            fmTasks fmTasks = new fmTasks();
            fmTasks.Load -= fmTasks.fmTasks_Load;
            fmTasks.Load += FmTasks_Load;
            void FmTasks_Load(object sender1, EventArgs e1)
            {
                fmTasks.Text = "Испольнитель:" + Person.PersonFamaly +"- задачи";
                foreach (Task task in Program.ListTasksAllPerson.Tasks)
                {
                    if (task.PersonFamaly== Person.PersonFamaly)
                    fmTasks.RetunlBxTasks().Items.Add(task.Number.ToString() + "\t" + task.Name);
                }
            }
            fmTasks.SetTextBox1().TextChanged -= fmTasks.textBox1_TextChanged;
            fmTasks.SetTextBox1().TextChanged += PersonButton_TextChanged;
            void PersonButton_TextChanged(object sender2, EventArgs e2)
            {
                fmTasks.LoadLBxTasksPerson(fmTasks.SetTextBox1().Text, Person.PersonFamaly);
            }
            fmTasks.ShowDialog();
        }
        public Button Button
        { get; private set; } = new Button();
        public Person Person
        { get; private set; }
        private int GetHightBooton(ListTasks listTasksAllPerson, int hightRowForTasks)
        {
            int maxCountSynchTask = Person.GetMaxCountSynchTask(listTasksAllPerson);
            return maxCountSynchTask * hightRowForTasks;
        }

    }
}
