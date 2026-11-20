using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule {
    enum PersonPlaceUpDowm {
        up,
        down
    }
    public partial class fmWorkWithPersons : StyledWindowForm {
        public fmWorkWithPersons(LoadRefreshForm loadRefreshForm) {
            InitializeComponent();
            thisloadRefreshForm = loadRefreshForm;
        }
        LoadRefreshForm thisloadRefreshForm;
        /// <summary>Версия списка исполнителей, показанного в данном окне.</summary>
        private string personRevision;
        public Form setForm() {
            return this;
        }
        private void btnNewPerson_Click(object sender, EventArgs e) {
            fmAddPerson fmAddPerson = new fmAddPerson();
            fmAddPerson.ShowDialog();
            personRevision = TaskStore.Revision(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
            LisBoxRefresh();

        }
        private void LisBoxRefresh() {
            lBxPersons.Items.Clear();
            foreach (Person person in Program.listPersons.Persons) {
                lBxPersons.Items.Add(person.PersonFamaly);
            }
        }

        /// <summary>Загружает актуальный список исполнителей и его версию.</summary>
        private void fmWorkWithPersons_Load(object sender, EventArgs e) {
            try
            {
                Program.listPersons = TaskStore.ReadPersonsWithRevision(
                    Dals.TakeMainPathFile(Constants.PERSONS_BIN), out personRevision);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException)
            {
                MessageBox.Show("Не удалось прочитать список исполнителей: " + ex.Message);
                this.Close();
                return;
            }
            LisBoxRefresh();
            lblPersonPlace.Text = "Порядок " + "\n" + "исполнителя";
        }

        /// <summary>Переименовывает исполнителя и связанные задачи в двухфайловой транзакции.</summary>
        private void btnChangePerson_Click(object sender, EventArgs e) {
            fmInpootText fmInpootText = new fmInpootText();
            if (lBxPersons.SelectedIndex == -1) {
                MessageBox.Show("Не выбран исполнитель.");
                return;
            }
            fmInpootText.GetTextBox().Text = lBxPersons.Items[lBxPersons.SelectedIndex].ToString();
            fmInpootText.Text = "Введите новое имя исполнителя";
            fmInpootText.GetBtnYes().Text = "Изменить";
            fmInpootText.GetLabel().Text = "Введите новое имя исполнителя";
            fmInpootText.ShowDialog();
            if (fmInpootText.ChoiceIsMade == ChoiceIsMade.no)
                return;
            string originalName = lBxPersons.Items[lBxPersons.SelectedIndex].ToString();
            string renamed = fmInpootText.GetTextBox().Text;
            var expected = TaskStore.Capture(Program.ListTasksAllPersonToSave,
                Program.ListTasksAllPersonToSave.Tasks.Where(t => t.PersonFamaly == originalName).Select(t => t.Number));
            try
            {
                Program.ListTasksAllPersonToSave = TaskStore.UpdatePersonsAndTasks(
                    Dals.TakeMainPathFile(Constants.TASKS_BIN), (people, tasks) =>
                    {
                        Person old = people.Persons.FirstOrDefault(p => p.PersonFamaly == originalName);
                        if (old == null) throw new TaskConflictException("Исполнитель уже изменён или удалён другим пользователем.");
                        if (people.Persons.Any(p => p.PersonFamaly == renamed && p != old))
                            throw new TaskConflictException("Исполнитель с таким именем уже существует.");
                        old.SetPersonFamaly(renamed);
                        foreach (Task task in tasks.Tasks)
                            if (task.PersonFamaly == originalName) task.SetPersonFamaly(renamed);
                    }, expected, personRevision);
                Program.listPersons = TaskStore.ReadPersons(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
                personRevision = TaskStore.Revision(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException)
            {
                MessageBox.Show("Переименование не завершено: " + ex.Message);
                return;
            }
            LisBoxRefresh();
            thisloadRefreshForm?.Invoke();
        }

        /// <summary>Удаляет исполнителя после проверки версии списка.</summary>
        private void btnDelete_Click(object sender, EventArgs e) {
            if (lBxPersons.SelectedIndex == -1) {
                MessageBox.Show("Не выбран исполнитель.");
                return;
            }
            DialogResult result = MessageBox.Show(
                "Вы уверены что хотите удалить исполнителя?",
                "Сообщение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No) {
                Program.fmMain.TopMost = true;
                this.TopMost = true;
                return;
            }
            this.Activate();
            string selected = lBxPersons.SelectedItem.ToString();
            try
            {
                Program.listPersons = TaskStore.UpdatePersons(Dals.TakeMainPathFile(Constants.PERSONS_BIN), people =>
                {
                    Person person = people.Persons.FirstOrDefault(p => p.PersonFamaly == selected);
                    if (person == null) throw new TaskConflictException("Исполнитель уже удалён или переименован.");
                    people.Persons.Remove(person);
                }, personRevision);
                personRevision = TaskStore.Revision(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
                LisBoxRefresh();
                thisloadRefreshForm?.Invoke();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Не удалось удалить исполнителя: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            if (lBxPersons.SelectedIndex == -1) {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            foreach (Person person in Program.listPersons.Persons) {
                if (person.PersonFamaly == lBxPersons.SelectedItem.ToString().Split('\t')[0]) {
                    MessageBox.Show(Convert.ToString(person.GetMaxCountSynchTask(Program.ListTasksAllPersonToSave)));
                    break;
                }
            }
        }
        /// <summary>Сохраняет порядок без перезаписи списка, изменённого другим пользователем.</summary>
        private void ReloadListPerson() {
            string[] order = lBxPersons.Items.Cast<object>().Select(x => x.ToString()).ToArray();
            try
            {
                Program.listPersons = TaskStore.UpdatePersons(Dals.TakeMainPathFile(Constants.PERSONS_BIN), people =>
                {
                    if (people.Persons.Count != order.Length)
                        throw new TaskConflictException("Состав исполнителей изменён другим пользователем.");
                    var ordered = new List<Person>();
                    foreach (string name in order)
                    {
                        Person person = people.Persons.FirstOrDefault(p => p.PersonFamaly == name);
                        if (person == null || ordered.Contains(person))
                            throw new TaskConflictException("Состав исполнителей изменён другим пользователем.");
                        ordered.Add(person);
                    }
                    people.SetPersonList(ordered);
                }, personRevision);
                personRevision = TaskStore.Revision(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                MessageBox.Show("Не удалось сохранить порядок исполнителей: " + ex.Message);
                Program.listPersons = TaskStore.ReadPersons(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
                personRevision = TaskStore.Revision(Dals.TakeMainPathFile(Constants.PERSONS_BIN));
                LisBoxRefresh();
            }
        }
        private void RemovePersonPlace(PersonPlaceUpDowm personPlaceUpDowm) {
            if (lBxPersons.SelectedIndex == -1) {
                MessageBox.Show("Не выбран исполнитель.");
                return;
            }
            object selectedItem = lBxPersons.SelectedItem;
            int index = lBxPersons.SelectedIndex;
            if (personPlaceUpDowm == PersonPlaceUpDowm.up && index > 0) {
                lBxPersons.Items.Insert(index - 1, selectedItem);
                lBxPersons.SelectedIndex = index - 1;
                lBxPersons.Items.RemoveAt(index + 1);
                ReloadListPerson();
            }
            if (personPlaceUpDowm == PersonPlaceUpDowm.down && index < lBxPersons.Items.Count - 1) {
                lBxPersons.Items.Insert(index + 2, selectedItem);
                lBxPersons.Items.RemoveAt(index);
                lBxPersons.SelectedIndex = index + 1;
                ReloadListPerson();
            }
        }
        private void button1_Click_1(object sender, EventArgs e) {
            RemovePersonPlace(PersonPlaceUpDowm.up);
        }

        private void fmWorkWithPersons_FormClosed(object sender, FormClosedEventArgs e) {
            thisloadRefreshForm?.Invoke();
        }

        private void button2_Click(object sender, EventArgs e) {
            RemovePersonPlace(PersonPlaceUpDowm.down);
        }
    }
}
