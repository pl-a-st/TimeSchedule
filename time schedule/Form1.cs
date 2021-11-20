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
    public partial class Form1 : Form
    {
        public Form1()
        {
            Program.fmMain = this;
            InitializeComponent();
           
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Dals.WriteProjectFolder(true);
            this.Activate();
            Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            Program.listPersons.SetPersonsFromList(Dals.ReadListFromProjectFile(Constants.PERSONS), Program.ListTasksAllPerson.Tasks);
            
            dataGridView1.Columns[0].HeaderText = "01.01.2021";
            dataGridView1.RowHeadersWidth = 100;
            for (int i = 0; i < 50; i++)
            {
                string nameAndText = Convert.ToString(i);
                dataGridView1.Columns.Add(nameAndText, nameAndText);
            }
            dataGridView2.Columns[0].HeaderText = "01.01.2021";
            dataGridView2.RowHeadersWidth = 100;
            for (int i = 0; i < 50; i++)
            {
                string nameAndText = Convert.ToString(i);
                dataGridView2.Columns.Add(nameAndText, nameAndText);
            }
            

            dataGridView1.Rows.Add(50);
            dataGridView1.Rows[1].HeaderCell.Value = "Верин";
            
            dataGridView1.Rows[5].DefaultCellStyle.BackColor = Color.Black;
            dataGridView1.Rows[5].HeaderCell.Style.BackColor = Color.Black;
            dataGridView1.Rows[5].Height = 1;
            
            

            int height = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                height += row.Height;
            }
            height += dataGridView1.ColumnHeadersHeight;

            int width = 0;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                width += col.Width;
            }
            width += dataGridView1.RowHeadersWidth;

            dataGridView1.ClientSize = new Size(width + 2, height + 2);
            dataGridView2.ClientSize = new Size(width + 2, dataGridView2.Height);

            width = 0;
            for (int i=0;i<6;i++)
            {
                width += dataGridView1.Columns[i].Width;
            }
            width += dataGridView1.RowHeadersWidth;

            height = 0;
            for (int i = 0; i < 6; i++)
            {
                height += dataGridView1.Rows[i].Height;
            }
            
            height += dataGridView1.ColumnHeadersHeight;

            button1.Location = new Point(width+ dataGridView1.Left, height + dataGridView1.Top);
            button1.BackColor = Color.Azure;
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.CheckedBackColor = Color.Azure;
            button1.FlatAppearance.BorderColor= Color.Black;
            button1.FlatAppearance.BorderSize = 1;
            //panel2.AutoScroll = false;

            //panel2.HorizontalScroll.Value = panel2.HorizontalScroll.Value+100;
            NonWorkDaysWrite(DateTime.Now.AddYears(-1).Date, DateTime.Now.AddYears(1).Date);
            dataGridView2.Select();
            dataGridView1.Select();
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
            panel2.HorizontalScroll.Value = panel1.HorizontalScroll.Value;
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
            panel1.HorizontalScroll.Value = panel2.HorizontalScroll.Value;
        }
    }
}
