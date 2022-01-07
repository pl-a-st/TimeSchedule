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
    public partial class Holidays : Form
    {
        public Holidays()
        {
            InitializeComponent();
        }
        public ListHolidays ListHolidays
        { get; private set; } = new ListHolidays();
        private void dTPOnceDay_ValueChanged(object sender, EventArgs e)
        {

            string daeForList = string.Empty;
            daeForList += dTPOnceDay.Value.Date;
            LBxHolidays.Items.Add(daeForList.Split(' ')[0]);
            //if (e.KeyCode == Keys.Enter)
            //{
            //    ScrollToDate(dateTimePicker1.Value.Date);
            //}
        }
       
    }
    public class ListHolidays
    {
        public List<DateTime> Holidays
        { get; private set; } = new List<DateTime>();
        public List<DateTime> GetHolidays()
        {
            return Holidays;
        }
    }
}
