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
        public fmHolidays(LoadRefreshForm loadRefreshForm)
        {
            InitializeComponent();
            thisloadRefreshForm = loadRefreshForm;
        }
        LoadRefreshForm thisloadRefreshForm;
        public ListHolidays CurrentListHolidays
        { get; private set; } = new ListHolidays();
        private void dTPOnceDay_ValueChanged(object sender, EventArgs e)
        {
            
        }
        private void AddHolidays(DateTime dateTime)
        {
            string daeForList = string.Empty;
            daeForList += dateTime;
            CurrentListHolidays.Holidays.Add(dateTime);
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

        private void button2_Click(object sender, EventArgs e)
        {
            tXBListDays.Text = string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string strForWork = tXBListDays.Text;
            strForWork = strForWork.Replace(" ", string.Empty);
            string[] arrStrForWork = strForWork.Split(';');
            string strForWrite = string.Empty;
            for(int i=0;i<arrStrForWork.Length-2;i++)
            {
                strForWrite += arrStrForWork[i] + "; ";
            }
            tXBListDays.Text = strForWrite;
        }

        private void dTPListDays_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string daeForList = string.Empty;
                daeForList += dTPListDays.Value.Date;
                tXBListDays.Text += daeForList.Split(' ')[0] + "; ";
            }
        }

        private void btnSetListDays_Click(object sender, EventArgs e)
        {
            string strForWork = tXBListDays.Text;
            strForWork = strForWork.Replace(" ", string.Empty);
            if (strForWork.Length != 0)
            {
                if (strForWork.Length == strForWork.LastIndexOf(';') + 1)
                    strForWork = strForWork.Remove(strForWork.Length - 1);
                string[] arrStrForWork = strForWork.Split(';');
                DateTime[] arrDateTime = new DateTime[arrStrForWork.Length];
                for (int i = 0; i < arrStrForWork.Length; i++)
                {
                    if (arrStrForWork[i] != "")
                        arrDateTime[i] = Convert.ToDateTime(arrStrForWork[i]);
                }
                foreach (DateTime dateTime in arrDateTime)
                {
                    CurrentListHolidays.Holidays.Add(dateTime);
                }
                foreach (string str in arrStrForWork)
                {
                    LBxHolidays.Items.Add(str);
                }
            }
            tXBListDays.Clear();
        }

        private void btnSetIntervalDays_Click(object sender, EventArgs e)
        {
            if (dTPFirstInTheInterval.Value.Date > dTPSecondInTheInterval.Value.Date)
                MessageBox.Show("Дата окончания интервала должна быть больше даты начала");
            if (dTPFirstInTheInterval.Value.Date <= dTPSecondInTheInterval.Value.Date)
            {
                DateTime CurentDateTime = dTPFirstInTheInterval.Value.Date;
                DateTime secondInTheInterval = dTPSecondInTheInterval.Value.Date;
                while (CurentDateTime <= secondInTheInterval)
                {
                    AddHolidays(CurentDateTime);
                    CurentDateTime = CurentDateTime.Date.AddDays(1);
                }
            }
        }

        private void RemoveDay_Click(object sender, EventArgs e)
        {
            if (LBxHolidays.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана дата.");
                return;
            }
            int selectedIndex = LBxHolidays.SelectedIndex;
            LBxHolidays.Items.RemoveAt(selectedIndex);
            CurrentListHolidays.Holidays.RemoveAt(selectedIndex);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            Program.ListHolidays.Holidays.Clear();
            Program.ListHolidays.Holidays.AddRange(CurrentListHolidays.Holidays);
            Dals.WriteObjectToFile(
                Constants.HOLYDAYS, 
                Program.ListHolidays.GetListForSave()
                );
            Dals.WriteObjectToFile(Constants.HOLYDAYS_BIN, Program.ListHolidays);
            Program.listNonWorkingDays.NonWorkingDays.AddRange(Program.ListHolidays.Holidays);
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                task.SetCountDays(Program.listNonWorkingDays);
                task.SetDateFinish();
            }
            Dals.WriteObjectToFile(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
            Dals.WriteObjectToFile(Constants.TASKS_BIN, Program.ListTasksAllPerson);
            thisloadRefreshForm?.Invoke();
            this.Close();
        }

        private void fmHolidays_Load(object sender, EventArgs e)
        {
            CurrentListHolidays.Holidays.AddRange(Program.ListHolidays.Holidays);
            LbxLoad();
            
        }
        
        private void LbxLoad()
        {
            foreach (DateTime dateTime in CurrentListHolidays.Holidays)
            {
                string strForLbx = string.Empty;
                strForLbx += dateTime;
                LBxHolidays.Items.Add(strForLbx.Split(' ')[0]);
            }
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            CurrentListHolidays.Holidays.Sort();
            LBxHolidays.Items.Clear();
            LbxLoad();
        
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }
    }
   
}
