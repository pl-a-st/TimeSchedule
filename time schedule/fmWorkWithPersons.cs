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
            listBox1.Items.Clear();
            foreach (Person person in Program.listPersons.Persons)
            {
                listBox1.Items.Add(person.PersonFamaly);
            }
        }

        private void fmWorkWithPersons_Load(object sender, EventArgs e)
        {
            LisBoxRefresh();
        }
    }
}
