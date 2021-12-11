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
    

    public partial class Form1 : Form
    {
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
        public void LoadRefreshForm(ref Panel plPersonButton,ref Panel plMain, Bitmap bitmap)
        {
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
            ResizeImage(new Size(Bmp.Width, maxButtonLocationY));
            LoadColumns();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListTaskButtons.LoadTaskButtons(
                Program.ListTasksAllPerson,
                Program.ListPersonButton
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
            //foreach (DataGridViewRow row in CalendarTasks.Rows)
            //{
            //    height += row.Height;
            //}
            //height += CalendarTasks.ColumnHeadersHeight;

            //int width = 0;
            //foreach (DataGridViewColumn col in CalendarTasks.Columns)
            //{
            //    width += col.Width;
            //}
            //width += CalendarTasks.RowHeadersWidth;

            //CalendarTasks.ClientSize = new Size(width + 2, height + 2);
            //DateTable.ClientSize = new Size(width + 2, DateTable.Height);
        }
        public void LoadColumns()
        {
            DateTime dateToTables = Program.ListTasksAllPerson.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            int height = plMain.Location.Y - plForDate.Location.Y;
            int locationX = 0;
            while (dateToTables< dateMaxToTable)
            {
                if (dateToTables.DayOfWeek!=DayOfWeek.Saturday&& dateToTables.DayOfWeek != DayOfWeek.Sunday)
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
                dateToTables= dateToTables.AddDays(1);
            }
            ResizeImage(new Size(locationX, Bmp.Height));
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
            plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int locationX = 0;
            int i = 0;
           
            //while ((DateTable.Columns[i].Name!=DateTime.Today.Date.ToShortDateString())&&(i< DateTable.Columns.Count-1))
            //{
            //    locationX += CalendarTasks.Columns[i].Width;
            //    i++;
            //}
            //locationX += CalendarTasks.RowHeadersWidth-5*Constants.COLUMN_WITH;
            plForDate.Focus();
            plMain.Focus();
            plMain.HorizontalScroll.Value = locationX;
            plMain.HorizontalScroll.Value = locationX;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value = 0;
            plForDate.HorizontalScroll.Value = 0;
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
            for (int i=0; i< plMain.Controls.Count; i++)
            {
                if (plMain.Controls[i] is Button)
                {
                    plMain.Controls.Remove(plMain.Controls[i]);
                    i--;
                }
                    
            }
            
        }
        public Bitmap Bmp
        { get; private set; } = new Bitmap(1, 1);

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
                MessageBox.Show("123");

            }
        }
    }
   
}
