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
    public partial class fmAddPerson : StyledWindowForm {
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
        /// <summary>Добавляет исполнителя в актуальный согласованный список без потери чужих записей.</summary>
        private void btnAddPerson_Click(object sender, EventArgs e) {
            if (personName.Text == "") {
                MessageBox.Show("Не указанно имя испонителя!");
                return;
            }
            try {
                Program.listPersons = TaskStore.UpdatePersons(Dals.TakeMainPathFile(Constants.PERSONS_BIN), people => {
                    if (people.Persons.Any(p => p.PersonFamaly == personName.Text))
                        throw new TaskConflictException("Исполнитель с таким именем уже существует.");
                    people.AddPerson(new Person(personName.Text));
                });
                CreatePerson = CreatePerson.yes;
                this.Close();
            }
            catch (System.IO.IOException ex) {
                MessageBox.Show("Не удалось записать нового исполнителя: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex) {
                MessageBox.Show("Нет доступа к файлу исполнителей: " + ex.Message);
            }
        }

        private void fmAddPerson_Load(object sender, EventArgs e) {
            this.TopMost = true;
        }
    }
}
