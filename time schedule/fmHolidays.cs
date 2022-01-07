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
    public partial class fmHolidays : Form
    {
        public fmHolidays()
        {
            InitializeComponent();
        }
        public ListHolidays CurrentListHolidays
        { get; private set; } = Program.ListHolidays;
        private void dTPOnceDay_ValueChanged(object sender, EventArgs e)
        {
            
        }
        private void AddHolidays(DateTime dateTime)
        {
            string daeForList = string.Empty;
            daeForList += dateTime;
            CurrentListHolidays.Holidays.Add(dTPOnceDay.Value.Date);
            LBxHolidays.Items.Add(daeForList.Split(' ')[0]);
        }
        private void dTPOnceDay_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddHolidays(dTPOnceDay.Value.Date);
            }
        }

        private void dTPOnceDay_CloseUp(object sender, EventArgs e)
        {
            AddHolidays(dTPOnceDay.Value.Date);
        }

        private void dTPListDays_CloseUp(object sender, EventArgs e)
        {
            string daeForList = string.Empty;
            daeForList += dTPListDays.Value.Date;
            tXBListDays.Text += daeForList.Split(' ')[0] + "; ";
        }
    }
   
}
