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
        /// <summary>Устанавливает Enter как подтверждение ввода имени исполнителя.</summary>
        public fmAddPerson() {
            InitializeComponent();
            AcceptButton = btnAddPerson;
        }
        /// <summary>Согласует вертикальные центры поля и кнопки и убирает пустое пространство снизу.</summary>
        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            Control visualInput = ThemeControlStyler.GetVisualControl(personName);
            if (visualInput == null || !ReferenceEquals(visualInput.Parent, btnAddPerson.Parent)) return;
            const int spacing = 6;
            const int bottomMargin = 12;
            label1.Left = visualInput.Left;
            label1.Top = visualInput.Top - label1.Height - spacing;
            btnAddPerson.Top = visualInput.Top + (visualInput.Height - btnAddPerson.Height) / 2;
            int contentHeight = Math.Max(visualInput.Bottom, btnAddPerson.Bottom) + bottomMargin;
            int chrome = ReferenceEquals(btnAddPerson.Parent, this) ? 0 : WindowChromeHeight;
            if (ClientSize.Height != chrome + contentHeight)
                ClientSize = new Size(ClientSize.Width, chrome + contentHeight);
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
                ThemedMessageBox.Show("Не указано имя исполнителя.");
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
                ThemedMessageBox.Show("Не удалось записать нового исполнителя: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex) {
                ThemedMessageBox.Show("Нет доступа к файлу исполнителей: " + ex.Message);
            }
        }

        private void fmAddPerson_Load(object sender, EventArgs e) {
            this.TopMost = true;
        }
    }
}
