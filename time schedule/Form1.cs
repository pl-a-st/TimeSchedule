﻿using System;
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
    public delegate void LoadRefreshForm();
    public enum RefreshType
    {
        All,
        Minimum
    }
    public partial class Form1 : Form
    {
        public RefreshType RefreshType
        { get; private set; } = RefreshType.Minimum;
        public int PlMainScrollYSaved
        { get; private set; } = 0;
        public int PlMainScrollXSaved
        { get; private set; } = 0;
        public int PlForDateScrollXSaved
        { get; private set; } = 0;
        public int PlPersonButtonYSaved
        { get; private set; } = 0;
        public DateTime MinDateStart
        { get; private set; } = DateTime.MaxValue;
        public DateTime MaxDateFinish
        { get; private set; } = DateTime.MinValue;
        public Form1()
        {
            Program.fmMain = this;
            InitializeComponent();
            
            plMain.MouseWheel += CalendarTasks_MouseWheel;
            plMain.ClientSizeChanged += PlMain_ClientSizeChanged;
            plPersonButton.MouseWheel += PlPersonButton_MouseWheel;
        }

        private void PlPersonButton_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                plMain.VerticalScroll.Value = plPersonButton.VerticalScroll.Value;
                plMain.VerticalScroll.Value = plPersonButton.VerticalScroll.Value;
            }
            catch
            {

            }


        }

        VScrollBar myScrollBar = new VScrollBar();
        private void PlMain_ClientSizeChanged(object sender, EventArgs e)
        {
            //plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            //plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }

        private void CalendarTasks_MouseWheel1(object sender, MouseEventArgs e)
        {
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                int test = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = test;
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
            return plMain.Location.X - plPersonButton.Location.X;
        }
        private void CleanOldExemplar(ref Panel plPersonButton, ref Panel plMain)
        {
            for (int i = 0; i < plMain.Controls.Count; i++)
            {
                if (plMain.Controls[i] is Button)
                {
                    plMain.Controls.Remove(plMain.Controls[i]);
                    i--;
                }

            }
            for (int i = 0; i < plPersonButton.Controls.Count; i++)
            {
                if (plPersonButton.Controls[i] is Button)
                {
                    plPersonButton.Controls.Remove(this.plPersonButton.Controls[i]);
                    i--;
                }

            }
            if (MaxDateFinish != Program.ListTasksAllPerson.GetMaxDateFinishTasks() ||
                MinDateStart != Program.ListTasksAllPerson.GetMinDateStartTasks()) 
                plForDate.Controls.Clear();
            pBForLine.CreateGraphics().Clear(Color.White);
            Bmp = new Bitmap(1,1);

        }
        private int ButtonsPersonLocationAndAdd(ref ListPersonButton listPersonButton, ref Panel plPersonButton)
        {
            int maxButtonLocationY = 0;

            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    personButton.SetLocation(0, maxButtonLocationY);
                    plPersonButton.Controls.Add(personButton.Button);
                    maxButtonLocationY += (personButton.Button.Height + Constants.MIN_ROW_HIGHT +1);
                }
            }
            return maxButtonLocationY;
        }
        public void Drow(Pen pen, int pX0, int pY0, int pX1, int pY1)
        {
            
            Graphics graphics = Graphics.FromImage(Bmp);
            graphics.DrawLine(pen, pX0, pY0, pX1, pY1);
            pBForLine.Image = Bmp;
            graphics.Dispose();
        }
        public void LoadHorizontLine()
        {
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if(personButton.Person.ListTask.Tasks.Count>0)
                {
                    int locationY = personButton.Button.Location.Y;
                    Pen pen = new Pen(Color.LightGray, 1);
                    do
                    {
                        Drow(pen, 0, locationY, pBForLine.Width, locationY);
                        locationY += Constants.ROW_HIGHT;
                    }
                    while (locationY < (personButton.Button.Location.Y + personButton.Button.Height));
                    Pen pen1 = new Pen(Color.Black, 5);
                    Drow(pen1, 0, locationY + 1, pBForLine.Width, locationY);
                }  
            }
        }
        public void LoadVerticalLine()
        {
            Pen penGrey = new Pen(Color.LightGray, 1);
            Pen penForMonday = new Pen(Color.FromArgb(32,55,100), 2);
            foreach (Control textbox in plForDate.Controls)
            {
                if (textbox is TextBox)
                {                    
                    int locationX = textbox.Location.X;
                    if (textbox.Text.Contains(""+DayOfWeek.Monday))
                    {
                        Drow(penForMonday, locationX, 0, locationX, pBForLine.Height);
                    }
                    else
                    {
                        Drow(penGrey, locationX, 0, locationX, pBForLine.Height);
                    }
                   
                }
            }
        }
        public void SaveScrolls()
        {
            PlForDateScrollXSaved = plForDate.HorizontalScroll.Value;
            PlMainScrollXSaved = plMain.HorizontalScroll.Value;
            PlMainScrollYSaved = plMain.VerticalScroll.Value;
            PlPersonButtonYSaved = plPersonButton.VerticalScroll.Value;
        }
        public void LoadScrolls()
        {
            const int MAGIC_ZEROING_FOR_CORRECT_OPERATION = 0;
            
            try
            {
                plForDate.HorizontalScroll.Value = MAGIC_ZEROING_FOR_CORRECT_OPERATION;
                plForDate.HorizontalScroll.Value = MAGIC_ZEROING_FOR_CORRECT_OPERATION;
                plForDate.HorizontalScroll.Value = PlMainScrollXSaved;
                plForDate.HorizontalScroll.Value = PlMainScrollXSaved;

                plMain.HorizontalScroll.Value = PlMainScrollXSaved;
                plMain.HorizontalScroll.Value = PlMainScrollXSaved;
                plMain.VerticalScroll.Value = PlMainScrollYSaved;
                plPersonButton.VerticalScroll.Value = PlPersonButtonYSaved;
                plMain.VerticalScroll.Value = PlMainScrollYSaved;
                plPersonButton.VerticalScroll.Value = PlPersonButtonYSaved;
            }
            catch { }            
        }
        public void ScrollToZero()
        {
            plForDate.HorizontalScroll.Value = 0;
            plMain.HorizontalScroll.Value = 0;
            plMain.VerticalScroll.Value = 0;
            plPersonButton.VerticalScroll.Value = 0;
        }
        public void SaveMinMaxDate()
        {
            MaxDateFinish = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            MinDateStart = Program.ListTasksAllPerson.GetMinDateStartTasks();
        }
        public void LoadRefreshForm(RefreshType refreshType)
        {
            if (refreshType == RefreshType.All)
            {
                RefreshType refreshTypeToSave = RefreshType;
                RefreshType = RefreshType.All;
                LoadRefreshForm();
                RefreshType = refreshTypeToSave;
            }
        }
        public void LoadRefreshForm()
        {
            LoadRefreshForm( plPersonButton, plMain, Bmp);
        }
        public void LoadRefreshForm(Panel plPersonButton,Panel plMain, Bitmap bitmap)
        {
            plForDate.Enabled = false;
            SaveScrolls();
            ScrollToZero();
            
            CleanOldExemplar(ref plPersonButton, ref plMain); // 346 мс
            Program.ListTasksAllPerson.Tasks.Clear();
            Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            Program.listPersons.Persons.Clear();
            Program.listPersons.SetPersonsFromList(Dals.ReadListFromProjectFile(Constants.PERSONS), Program.ListTasksAllPerson.Tasks);
            Program.ListPersonButton.PersonButtons.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListPersonButton.LoadListPersonButtons(
                Program.listPersons.Persons,
                Program.ListTasksAllPerson,
                Constants.ROW_HIGHT,
                this);
            int maxButtonLocationY = ButtonsPersonLocationAndAdd(ref Program.ListPersonButton, ref plPersonButton);
            pBForLine.Height = maxButtonLocationY;
            Program.ListHolidays.Holidays.Clear();
            Program.ListHolidays.SetHolidaysFromList(Dals.ReadListFromProjectFile(Constants.HOLYDAYS));
            Program.listNonWorkingDays.NonWorkingDays.Clear();
            NonWorkDaysWrite(Program.ListTasksAllPerson.GetMinDateStartTasks(), Program.ListTasksAllPerson.GetMaxDateFinishTasks());
            Program.listNonWorkingDays.NonWorkingDays.AddRange(Program.ListHolidays.Holidays);
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListTaskButtons.LoadTaskButtons(
                Program.ListTasksAllPerson,
                Program.ListPersonButton
                );
            
            LoadColumns(); //735 мс
            LoadHorizontLine();
            LoadVerticalLine();
            plMain.Visible = false;
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons) // 351 мс, 645 мс
            {
                foreach (Button button in taskButton.Buttons)
                {
                    plMain.Controls.Add(button);
                    button.BringToFront();
                }
            }
            plMain.Visible = true;
            LoadScrolls();
            SaveMinMaxDate();
            
        }
        public void LoadColumns()
        {
            DateTime dateToTables = Program.ListTasksAllPerson.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            
            if (MaxDateFinish != dateMaxToTable ||
                MinDateStart!= dateToTables ||
                RefreshType == RefreshType.All)
            {

                int height = plMain.Location.Y - plForDate.Location.Y;
                int locationX = 0;
                plForDate.Visible = false;
                plForDate.Controls.Clear();
                while (dateToTables <= dateMaxToTable)
                {
                    if (dateToTables.DayOfWeek != DayOfWeek.Saturday &&
                        dateToTables.DayOfWeek != DayOfWeek.Sunday &&
                        !Program.listNonWorkingDays.NonWorkingDays.Contains(dateToTables))
                    {
                        TextBox textBox = new TextBox();
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        textBox.AutoSize = false;
                        textBox.Size = new Size(Constants.COLUMN_WITH, height);
                        textBox.Multiline = true;
                        textBox.Text = dateToTables.ToShortDateString() + "\n" + dateToTables.DayOfWeek;
                        textBox.BackColor = Color.AliceBlue;
                        if (dateToTables.Date == DateTime.Now.Date)
                            textBox.BackColor = Color.CadetBlue;
                        textBox.ForeColor = Color.Black;
                        textBox.TextAlign = HorizontalAlignment.Center;
                        textBox.ReadOnly = true;
                        if (dateToTables == DateTime.Now)
                            textBox.BackColor = Color.LightBlue;
                        textBox.Location = new Point(locationX, 0);
                        plForDate.Controls.Add(textBox);
                        locationX += Constants.COLUMN_WITH;
                    }
                    dateToTables = dateToTables.AddDays(1);
                }
                plForDate.Visible = true;
                pBForLine.Width = locationX;
                ResizeImage(new Size(locationX, pBForLine.Height));
            }
            else
            {
                ResizeImage(new Size(pBForLine.Width, pBForLine.Height));
            }
        }
        public static Form1 SelfRef
        {
            get; set;
        }
        public Form1(Form form)
        {
            SelfRef = this;
        }
        public Panel GetPlPeraonButton()
        {
            return plPersonButton;
        }
        public Panel GetPlMain()
        {
            return plMain;
        }
        public Bitmap GetBmp()
        {
            return Bmp;
        }
        public Form1 SetForm1()
        {
            return this;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            Program.delegatLoadRefreshForm = LoadRefreshForm;
            myScrollBar.Height = plMain.Height;
            myScrollBar.Left = plMain.Width - myScrollBar.Width;
            myScrollBar.Top = 0;
            myScrollBar.Enabled = false;
            //plMain.Controls.Add(myScrollBar);
            Dals.WriteProjectFolder(true);
            this.Text = Dals.ProjectFolderPath;
            this.Text = this.Text.Replace("\\Проект",string.Empty);
            this.Activate();
            LoadRefreshForm( plPersonButton, plMain, Bmp);
            
            plMain.VerticalScroll.Visible = true;
            plForDate.Enabled = true;
            btnNewTask.Visible = false;
        }

        private void CalendarTasks_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                plMain.Focus();
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                int test = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = test;
                plPersonButton.VerticalScroll.Value = test;
            }
            catch
            { }
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
            while(вeginningPeriod.AddDays(i) < endPeriod ||
                вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                Program.ListHolidays.Holidays.Contains(вeginningPeriod.AddDays(i))
                )
            {
                if (вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                    вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    Program.listNonWorkingDays.NonWorkingDays.Add(вeginningPeriod.AddDays(i));
                i++;
            }
        }

        private void ScrollChange(object sender, ScrollEventArgs e)
        {
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plPersonButton.VerticalScroll.Value = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = plMain.VerticalScroll.Value;
            }
            catch
            {

            }
        }
       

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            fmWorkWithPersons fmWorkWithPersons = new fmWorkWithPersons(Program.delegatLoadRefreshForm);
            fmWorkWithPersons.ShowDialog();
            

        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            fmAddTask fmAddTask = new fmAddTask(Program.delegatLoadRefreshForm);
            fmAddTask.StartPosition = FormStartPosition.CenterScreen;
            fmAddTask.SetCreateOrChange(CreateOrChange.Create);
            fmAddTask.Show();
            Dals.WriteListProjectFileAppend(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dals.WriteProjectFolder();
            this.Text = Dals.ProjectFolderPath;
            this.Text = this.Text.Replace("\\Проект", string.Empty);
            this.Activate();
            LoadRefreshForm(plPersonButton, plMain, Bmp);

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            plForDate.Enabled = true;
            
        }
        public void ScrollToDate(DateTime targetDateTime)
        {
            int locationX = 0;
            plMain.HorizontalScroll.Value = 0;
            plForDate.HorizontalScroll.Value = 0;
            plMain.HorizontalScroll.Value = 0;
            plForDate.HorizontalScroll.Value = 0;
            foreach (Control textBox in plForDate.Controls)
            {
                if (textBox is TextBox)
                {
                    DateTime dateTime = DateTime.Parse(textBox.Text.Split('\n')[0]);
                    if (dateTime <= targetDateTime)
                    {
                        locationX = textBox.Location.X;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            plForDate.Focus();
            plMain.Focus();
            int NewLocationX = locationX - 4 * Constants.COLUMN_WITH;
            try
            {
                if (NewLocationX > 0)
                {
                    plMain.HorizontalScroll.Value = NewLocationX;
                    plMain.HorizontalScroll.Value = NewLocationX;
                    plForDate.HorizontalScroll.Value = NewLocationX;
                    plForDate.HorizontalScroll.Value = NewLocationX;

                }
                else
                {
                    plMain.HorizontalScroll.Value = locationX;
                    plMain.HorizontalScroll.Value = locationX;
                    plForDate.HorizontalScroll.Value = locationX;
                    plForDate.HorizontalScroll.Value = locationX;
                }
            }
            catch { }
            
        }
        private void button3_Click(object sender, EventArgs e)
        {
            ScrollToDate(DateTime.Now.Date);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value = 0;
            plForDate.HorizontalScroll.Value = 0;
            ScrollToDate(DateTime.Now.Date);
        }
        private void LoadDataGridPersonButton(ref DataGridView dataGridView, ListPersonButton listPersonButton)
        {
            //Point startPositionPersonButton;

        }

        private void plMain_MouseEnter(object sender, EventArgs e)
        {

        }

        private void plMain_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            LoadRefreshForm();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                plMain.HorizontalScroll.Value += 5 * Constants.COLUMN_WITH;
                plMain.HorizontalScroll.Value += 5 * Constants.COLUMN_WITH;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            }
            catch
            {

            }
        }
        public Bitmap Bmp
        { get; set; } = new Bitmap(1, 1);

        public void ResizeImage(Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(Bmp, 0, 0, size.Width, size.Height);
                }
                Bmp = b;
            }
            catch
            {
                

            }
        }

        private void minusFiveDay_Click(object sender, EventArgs e)
        {
            try
            {
                plMain.HorizontalScroll.Value -= 5 * Constants.COLUMN_WITH;
                plMain.HorizontalScroll.Value -= 5 * Constants.COLUMN_WITH;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            }
            catch
            {

            }
            
        }

        private void dateTimePicker1_CloseUp(object sender, EventArgs e)
        {
            ScrollToDate(dateTimePicker1.Value.Date);
        }

        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
            
        }

        private void dateTimePicker1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ScrollToDate(dateTimePicker1.Value.Date);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(""+plForDate.HorizontalScroll.Value);
        }
        //LoadRefreshForm ThisloadRefreshForm;
        private void задачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            fmTasks fmTasks = new fmTasks(this);
            
            fmTasks.ShowDialog();
        }

        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            Program.UserType = UserType.Reader;
            btnNewTask.Visible = false;
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.UserType = UserType.Admin;
            btnNewTask.Visible = true;
        }

        private void plPersonButton_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;
                plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;

            }
            catch
            {

            }
        }

        private void нерабочиеДниToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fmHolidays holidays = new fmHolidays(LoadRefreshForm);
            holidays.ShowDialog();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadRefreshForm(RefreshType.All);
        }
    }
   
}
