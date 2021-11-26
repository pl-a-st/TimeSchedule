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
        public void LoadRefreshForm(ref Panel plPersonButton,ref Panel plMain, ref DataGridView calendarTasks)
        {
            Program.ListTasksAllPerson.Tasks.Clear();
            Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            Program.listPersons.Persons.Clear();
            Program.listPersons.SetPersonsFromList(Dals.ReadListFromProjectFile(Constants.PERSONS), Program.ListTasksAllPerson.Tasks);
            Program.ListPersonButton.LoadListPersonButtons(
                Program.listPersons.Persons,
                Program.ListTasksAllPerson,
                Constants.ROW_HIGHT);
           
            int nextLocationY = 0;
            int nextRowsIndex;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.Tasks.Count > 0)
                {
                    personButton.SetLocation(0, nextLocationY);

                    plPersonButton.Controls.Add(personButton.Button);
                    nextLocationY += (personButton.Button.Height + Constants.MIN_ROW_HIGHT+1);
                    const int TO_NEXT_ROWS = 1;
                    calendarTasks.Rows.Add(personButton.Person.GetMaxCountSynchTask(Program.ListTasksAllPerson)+ TO_NEXT_ROWS);
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
                if (personButton.Person.Tasks.Count > 0)
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
            
            //PersonButton personButton;
            //if (Program.listPersons.Persons.Count > 0)
            //{
            //    personButton = new PersonButton(Program.listPersons.Persons[1], Program.ListTasksAllPerson, 100);
            //    plPeraonButton.Controls.Add(personButton.Button);
            //}

                //for (int i = 0; i < 50; i++)
                //{
                //    string nameAndText = Convert.ToString(i);
                //    dataGridView3.Columns.Add(nameAndText, nameAndText);
                //}
                //for (int i = 0; i < 50; i++)
                //{
                //    string nameAndText = Convert.ToString(i);
                //    dataGridView3.Rows.Add(nameAndText, nameAndText);
                //}
                //dataGridView3.Controls.Add(personButton.Button);

                //personButton.Button.BringToFront();
                //personButton.Button.Region = dataGridView3.Region;
                //DataGridViewButtonCell dataGridViewButtonCell = new DataGridViewButtonCell();
                //dataGridView3.dataGridViewButtonCell;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Dals.WriteProjectFolder(true);
            this.Activate();
            LoadRefreshForm(ref plPeraonButton,ref plMain,ref CalendarTasks);
            NonWorkDaysWrite(DateTime.Now.AddYears(-1).Date, DateTime.Now.AddYears(1).Date);
            CalendarTasks.Columns[0].HeaderText = "01.01.2021";
            CalendarTasks.RowHeadersWidth = 100;
            for (int i = 0; i < 50; i++)
            {
                string nameAndText = Convert.ToString(i);
                CalendarTasks.Columns.Add(nameAndText, nameAndText);
            }
            dataGridView2.Columns[0].HeaderText = "01.01.2021";
            dataGridView2.RowHeadersWidth = 100;
            for (int i = 0; i < 50; i++)
            {
                string nameAndText = Convert.ToString(i);
                dataGridView2.Columns.Add(nameAndText, nameAndText);
            }
            

            //CalendarTasks.Rows.Add(50);
            //CalendarTasks.Rows[1].HeaderCell.Value = "Верин";
            
            //CalendarTasks.Rows[5].DefaultCellStyle.BackColor = Color.Black;
            //CalendarTasks.Rows[5].HeaderCell.Style.BackColor = Color.Black;
            //CalendarTasks.Rows[5].Height = 1;
            
            

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
            dataGridView2.ClientSize = new Size(width + 2, dataGridView2.Height);

            width = 0;
            for (int i=0;i<6;i++)
            {
                width += CalendarTasks.Columns[i].Width;
            }
            width += CalendarTasks.RowHeadersWidth;

            height = 0;
            for (int i = 0; i < 6; i++)
            {
                height += CalendarTasks.Rows[i].Height;
            }
            
            height += CalendarTasks.ColumnHeadersHeight;

            button1.Location = new Point(width+ CalendarTasks.Left, height + CalendarTasks.Top);
            button1.BackColor = Color.Azure;
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.CheckedBackColor = Color.Azure;
            button1.FlatAppearance.BorderColor= Color.Black;
            button1.FlatAppearance.BorderSize = 1;
            //panel2.AutoScroll = false;
            
            //plMain.AutoScrollPosition = new Point (0,50) ;
            //ScrollToBottom(plMain);
            //plMain.
            dataGridView2.Select();
            CalendarTasks.Select();
            
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
            // panel2.Select();
            plMain.HorizontalScroll.Value = panel2.HorizontalScroll.Value;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            plMain.HorizontalScroll.Value = 200;
            plMain.HorizontalScroll.Value = 200;
            panel2.HorizontalScroll.Value = 200;
            panel2.HorizontalScroll.Value = 200;
            //MessageBox.Show(Convert.ToString(plMain.HorizontalScroll.Value));
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
    }
    public class TaskButton
    {

    }
    public class ListPersonButton
    {
        public List<PersonButton> PersonButtons
        { get; private set; } = new List<PersonButton>();
        public void ListPersonButtonsAdd(PersonButton personButton)
        {
            PersonButtons.Add(personButton);
        }
        public void LoadListPersonButtons(List<Person> persons, ListTasksAllPerson listTasksAllPerson, int hightRowForTasks)
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
        public PersonButton(Person person, ListTasksAllPerson listTasksAllPerson, int hightRowForTasks)
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
        private int GetHightBooton(ListTasksAllPerson listTasksAllPerson, int hightRowForTasks)
        {
            return Person.GetMaxCountSynchTask(listTasksAllPerson) * hightRowForTasks;
        }

    }
}
