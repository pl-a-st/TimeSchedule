using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule {
    public enum CreatePerson {
        yes,
        no
    }
    public partial class fmAddPerson : Form {
        public fmAddPerson() {
            InitializeComponent();
        }
        public CreatePerson CreatePerson { get; private set; } = CreatePerson.no;
        public void SetCreatePerson(CreatePerson createPerson) {
            CreatePerson = createPerson;
        }
        public string GetPersonName() {
            return personName.Text;
        }
        private void btnAddPerson_Click(object sender, EventArgs e) {
            if (personName.Text == "") {
                MessageBox.Show("Не указанно имя испонителя!");
                return;
            }
            Person person = new Person(personName.Text);
            for(int i = 0; i < Constants.COUNT_OF_TRIES; i++) {
                Program.fmMain.ReReadListPerson();
                Program.listPersons.AddPerson(person);
                if(Dals.WriteObjectToMainPathFile(Constants.PERSONS_BIN, Program.listPersons) == MethodResultStatus.Ok) {
                    CreatePerson = CreatePerson.yes;
                    this.Close();
                    return;
                }
            }
            MessageBox.Show($"Не удалось записать нового исполнителя в файл{Constants.PERSONS_BIN}!");
        }

        private void fmAddPerson_Load(object sender, EventArgs e) {
            this.TopMost = true;
        }
    }
}
