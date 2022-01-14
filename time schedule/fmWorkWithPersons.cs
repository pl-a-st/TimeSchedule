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
    enum PersonPlaceUpDowm
    {
        up,
        down
    }
    public partial class fmWorkWithPersons : Form
    {
        public fmWorkWithPersons(LoadRefreshForm loadRefreshForm)
        {
            InitializeComponent();
            thisloadRefreshForm = loadRefreshForm;
        }
        LoadRefreshForm thisloadRefreshForm;
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
            lblPersonPlace.Text = "Порядок " + "\n" + "исполнителя";
        }

        private void btnChangePerson_Click(object sender, EventArgs e)
        {
            fmInpootText fmInpootText = new fmInpootText();
            if (lBxPersons.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбран исполнитель.");
                return;
            }
            fmInpootText.SetTextBox().Text = lBxPersons.Items[lBxPersons.SelectedIndex].ToString();
            fmInpootText.Text = "Введите новое имя исполнителя";
            fmInpootText.SetBtnYes().Text = "Изменить";
            fmInpootText.SetLabel().Text = "Введите новое имя исполнителя";
            fmInpootText.ShowDialog();
            if (fmInpootText.ChoiceIsMade == ChoiceIsMade.no)
                return;
            foreach (Person person in Program.listPersons.Persons)
            {
                if (person.PersonFamaly == lBxPersons.Items[lBxPersons.SelectedIndex].ToString())
                    person.SetPersonFamaly(fmInpootText.SetTextBox().Text);
            }
            foreach (Task task in Program.ListTasksAllPerson.Tasks)
            {
                if (task.PersonFamaly == lBxPersons.Items[lBxPersons.SelectedIndex].ToString())
                    task.SetPersonFamaly(fmInpootText.SetTextBox().Text);
            }
            
            Dals.WriteListProjectFileAppend(
                Constants.PERSONS, 
                Program.listPersons.GetListForSave());
            Dals.WriteListProjectFileAppend(
                Constants.TASKS,
                Program.ListTasksAllPerson.GetListForSave());
            LisBoxRefresh();
            thisloadRefreshForm?.Invoke();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lBxPersons.SelectedIndex == -1)
            {
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
            if (result == DialogResult.No)
            {
                Program.fmMain.TopMost = true;
                this.TopMost = true;
                return;
            }
            this.Activate();
            
            Program.listPersons.Persons.RemoveAt(lBxPersons.SelectedIndex);
             
            Dals.WriteListProjectFileAppend(Constants.PERSONS, Program.listPersons.GetListForSave());
            LisBoxRefresh();
            thisloadRefreshForm?.Invoke();
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
        private void ReloadListPerson()
        {
            List<Person> listPerson = new List<Person>();
            foreach (object item in lBxPersons.Items)
            {
                foreach (Person person in Program.listPersons.Persons)
                {
                    if (item.ToString() == person.PersonFamaly)
                    {
                        listPerson.Add(person);
                    }
                }
            }
            Program.listPersons.SetPersonList(listPerson);
            Dals.WriteListProjectFileAppend(Constants.PERSONS, Program.listPersons.GetListForSave());
        }
        private void RemovePersonPlace(PersonPlaceUpDowm personPlaceUpDowm)
        {
            if (lBxPersons.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбран исполнитель.");
                return;
            }
            object selectedItem = lBxPersons.SelectedItem;
            int index = lBxPersons.SelectedIndex;
            if (personPlaceUpDowm == PersonPlaceUpDowm.up && index > 0)
            {
                lBxPersons.Items.Insert(index - 1, selectedItem);
                lBxPersons.SelectedIndex = index - 1;
                lBxPersons.Items.RemoveAt(index + 1);
                ReloadListPerson();
            }
            if (personPlaceUpDowm == PersonPlaceUpDowm.down && index < lBxPersons.Items.Count-1)
            {
                lBxPersons.Items.Insert(index + 2, selectedItem);
                lBxPersons.Items.RemoveAt(index);
                lBxPersons.SelectedIndex = index+1;
                ReloadListPerson();
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            RemovePersonPlace(PersonPlaceUpDowm.up);
        }

        private void fmWorkWithPersons_FormClosed(object sender, FormClosedEventArgs e)
        {
            thisloadRefreshForm?.Invoke();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RemovePersonPlace(PersonPlaceUpDowm.down);
        }
    }
}
