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
    public partial class fmAddPerson : Form
    {
        public fmAddPerson()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Person person = new Person(textBox1.Text);
            Program.listPersons.AddPerson(person);
            Dals.WriteListProjectFileAppend(Constants.PERSONS, Program.listPersons.GetListForSave());
            this.Close();
        }
    }
}
