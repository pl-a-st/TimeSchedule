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
        public int PlMainScrollYSaved
        { get; private set; } = 0;
        public int PlMainScrollXSaved
        { get; private set; } = 0;
        public int PlForDateScrollXSaved
        { get; private set; } = 0;
        public int PlPeraonButtonYSaved
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
        }
        VScrollBar myScrollBar = new VScrollBar();
        private void PlMain_ClientSizeChanged(object sender, EventArgs e)
        {
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }

        private void CalendarTasks_MouseWheel1(object sender, MouseEventArgs e)
        {
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
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
                    plPersonButton.Controls.Remove(plPeraonButton.Controls[i]);
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
                    maxButtonLocationY += (personButton.Button.Height + Constants.MIN_ROW_HIGHT + 1);
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
                int locationY = personButton.Button.Location.Y;
                Pen pen = new Pen(Color.LightGray, 1);
                do
                {
                    Drow(pen, 0, locationY, pBForLine.Width, locationY);
                    locationY += Constants.ROW_HIGHT;
                }
                while (locationY < (personButton.Button.Location.Y + personButton.Button.Height));
                Pen pen1 = new Pen(Color.Black, 5);
                Drow(pen1, 0, locationY+1, pBForLine.Width, locationY);
            }
        }
        public void LoadVerticalLine()
        {
            Pen penGrey = new Pen(Color.LightGray, 1);
            Pen penForMonday = new Pen(Color.DodgerBlue, 2);
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
            PlPeraonButtonYSaved = plPeraonButton.VerticalScroll.Value;
        }
        public void LoadScrolls()
        {
            plForDate.HorizontalScroll.Value = PlForDateScrollXSaved;
            plMain.HorizontalScroll.Value = PlMainScrollXSaved;
            plMain.VerticalScroll.Value = PlMainScrollYSaved;
            plPeraonButton.VerticalScroll.Value = PlPeraonButtonYSaved;
        }
        public void ScrollToZero()
        {
            plForDate.HorizontalScroll.Value = 0;
            plMain.HorizontalScroll.Value = 0;
            plMain.VerticalScroll.Value = 0;
            plPeraonButton.VerticalScroll.Value = 0;
        }
        public void SaveMinMaxDate()
        {
            MaxDateFinish = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            MinDateStart = Program.ListTasksAllPerson.GetMinDateStartTasks();
        }
        public void LoadRefreshForm(ref Panel plPersonButton,ref Panel plMain, Bitmap bitmap)
        {
            SaveScrolls();
            ScrollToZero();
            SaveMinMaxDate();
            CleanOldExemplar(ref plPersonButton, ref plMain);
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
            int maxButtonLocationY = ButtonsPersonLocationAndAdd(ref Program.ListPersonButton, ref plPersonButton);
            pBForLine.Height = maxButtonLocationY; 
            LoadColumns();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListTaskButtons.LoadTaskButtons(
                Program.ListTasksAllPerson,
                Program.ListPersonButton
                );
            LoadHorizontLine();
            LoadVerticalLine();
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
            {
                foreach (Button button in taskButton.Buttons)
                {
                    plMain.Controls.Add(button);
                    button.BringToFront();
                }
            }
            LoadScrolls();
        }
        public void LoadColumns()
        {
            DateTime dateToTables = Program.ListTasksAllPerson.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            
            if (MaxDateFinish != dateMaxToTable||MinDateStart!= dateToTables)
            {
                int height = plMain.Location.Y - plForDate.Location.Y;
                int locationX = 0;
                plForDate.Visible = false;
                while (dateToTables < dateMaxToTable)
                {

                    if (dateToTables.DayOfWeek != DayOfWeek.Saturday && dateToTables.DayOfWeek != DayOfWeek.Sunday)
                    {
                        TextBox textBox = new TextBox();
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        textBox.AutoSize = false;
                        textBox.Size = new Size(Constants.COLUMN_WITH, height);
                        textBox.Multiline = true;
                        textBox.Text = dateToTables.ToShortDateString() + "\n" + dateToTables.DayOfWeek;
                        textBox.BackColor = Color.AliceBlue;
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
        private void Form1_Load(object sender, EventArgs e)
        {
            myScrollBar.Height = plMain.Height;
            myScrollBar.Left = plMain.Width - myScrollBar.Width;
            myScrollBar.Top = 0;
            myScrollBar.Enabled = false;
            plMain.Controls.Add(myScrollBar);

            Dals.WriteProjectFolder(true);
            this.Activate();
            LoadRefreshForm(ref plPeraonButton,ref plMain, Bmp);
            NonWorkDaysWrite(DateTime.Now.AddYears(-1).Date, DateTime.Now.AddYears(1).Date);
            plMain.VerticalScroll.Visible = true;
            
        }

        private void CalendarTasks_MouseWheel(object sender, MouseEventArgs e)
        {
            plMain.Focus();
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            int test = plMain.VerticalScroll.Value;
            plPeraonButton.VerticalScroll.Value = test;
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
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plPeraonButton.VerticalScroll.Value = plMain.VerticalScroll.Value;
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
            //plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;
            
        }
        private void ScrollToDate(DateTime targetDateTime)
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
            LoadRefreshForm(ref plPeraonButton, ref plMain, Bmp);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value += 5 * Constants.COLUMN_WITH;
            plMain.HorizontalScroll.Value += 5 * Constants.COLUMN_WITH;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
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
                MessageBox.Show("Что-то с рисованием");

            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            
            
        }

        private void minusFiveDay_Click(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value -= 5 * Constants.COLUMN_WITH;
            plMain.HorizontalScroll.Value -= 5 * Constants.COLUMN_WITH;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
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
    }
   
}
