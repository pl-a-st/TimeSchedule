using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule.Forms {

    public partial class fmChangeList : Form {

        public fmChangeList() {
            InitializeComponent();
        }
        public BtnUpDowm BtnUpDowm {
            get;
            private set;
        }
        public BtnClick BtnClick {
            get;
            private set;
        } = BtnClick.cancel;
        public void SetBtnClick(BtnClick btnClick) {
            BtnClick = btnClick;
        }
        public List<StringNumChange> ListStringNumChange {
            get;
            private set;
        } = new List<StringNumChange>();
        private void fmChangeList_Load(object sender, EventArgs e) {
            int i = 0;
            foreach (string str in LBx.Items) {
                ListStringNumChange.Add(new StringNumChange(str, i));
                i++;
            }
        }
        public Form GetForm() {
            return this;
        }
        private void ReplaceItem(BtnUpDowm btnUpDowm) {
            if (lBx.SelectedIndex == -1) {
                return;
            }
            object selectedItem = lBx.SelectedItem;
            int index = lBx.SelectedIndex;
            if (btnUpDowm == BtnUpDowm.up && index > 0) {
                ListStringNumChange.Insert(index - 1, ListStringNumChange[index]);
                ListStringNumChange.RemoveAt(index + 1);
                lBx.Items.Insert(index - 1, selectedItem);
                lBx.SelectedIndex = index - 1;
                lBx.Items.RemoveAt(index + 1);
            }
            if (btnUpDowm == BtnUpDowm.down && index < lBx.Items.Count - 1) {
                ListStringNumChange.Insert(index + 2, ListStringNumChange[index]);
                ListStringNumChange.RemoveAt(index);
                lBx.Items.Insert(index + 2, selectedItem);
                lBx.Items.RemoveAt(index);
                lBx.SelectedIndex = index + 1;
            }
        }
        private void btnUp_Click(object sender, EventArgs e) {
            ReplaceItem(BtnUpDowm.up);
        }
        private void btnDown_Click(object sender, EventArgs e) {
            ReplaceItem(BtnUpDowm.down);
        }

        private void btnApply_Click(object sender, EventArgs e) {
            BtnClick = BtnClick.aplly;
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            lBx.Items.RemoveAt(lBx.SelectedIndex);
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void btnChange_Click(object sender, EventArgs e) {
            fmInpootText fm = new fmInpootText();
            fm.Text = "Ввод имени настроек";
            fm.GetLabel().Text = "Измените имя";
            fm.GetTextBox().Text = LBx.SelectedItem.ToString();
            fm.GetBtnYes().Text = "Ок";
            fm.ShowDialog();
            if (fm.ChoiceIsMade == ChoiceIsMade.yes) {
                lBx.Items[lBx.SelectedIndex] = fm.GetTextBox().Text;
                ListStringNumChange[LBx.SelectedIndex].SetStr(fm.GetTextBox().Text);
            }
            
        }
    }
}
