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
    public enum CreatePerson
    {
        yes,
        no
    }
    public partial class fmAddPerson : Form
    {
        public fmAddPerson()
        {
            InitializeComponent();
        }
        public CreatePerson CreatePerson
        { get; private set; } = CreatePerson.no;
        public void SetCreatePerson(CreatePerson createPerson)
        {
            CreatePerson = createPerson;
        }
        public string GetPersonName()
        {
            return personName.Text;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Person person = new Person(personName.Text);
            Program.listPersons.AddPerson(person);
            Dals.WriteListProjectFileAppend(Constants.PERSONS, Program.listPersons.GetListForSave());
            CreatePerson = CreatePerson.yes;
            this.Close();
        }

        private void fmAddPerson_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
    }
}
