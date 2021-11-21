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
    public partial class fmWorkWithPersons : Form
    {
        public fmWorkWithPersons()
        {
            InitializeComponent();
        }

        private void btnNewPerson_Click(object sender, EventArgs e)
        {
            fmAddPerson fmAddPerson = new fmAddPerson();
            fmAddPerson.ShowDialog();
            LisBoxRefresh();

        }
        private void LisBoxRefresh()
        {
            lBxPersons.Items.Clear();
            foreach (Person person in Program.listPersons.Persons)
            {
                lBxPersons.Items.Add(person.PersonFamaly);
            }
        }

        private void fmWorkWithPersons_Load(object sender, EventArgs e)
        {
            LisBoxRefresh();
        }

        private void btnChangePerson_Click(object sender, EventArgs e)
        {

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lBxPersons.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            foreach (Person person in Program.listPersons.Persons)
            {
                if (person.PersonFamaly == lBxPersons.SelectedItem.ToString().Split('\t')[0])
                {
                    MessageBox.Show(Convert.ToString(person.GetMaxCountSynchTask(Program.ListTasksAllPerson)));
                    break;
                }
            }
        }
    }
}
