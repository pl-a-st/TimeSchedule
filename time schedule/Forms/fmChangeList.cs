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

    public partial class fmChangeList : StyledWindowForm {

        /// <summary>Не позволяет уменьшить окно до пересечения списка и управляющих кнопок.</summary>
        public fmChangeList() {
            InitializeComponent();
            MinimumSize = new Size(ClientSize.Width, ClientSize.Height + WindowChromeHeight);
        }

        /// <summary>Панель формы, отслеживаемая только при выборе резервной копии.</summary>
        private Control restoreContent;

        /// <summary>В режиме восстановления разворачивает список на свободную ширину и выравнивает нижние команды.</summary>
        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            if (btnNew.Visible || btnChange.Visible || btnDelete.Visible || groupBox1.Visible) return;
            restoreContent = lBx.Parent;
            if (restoreContent != null) restoreContent.SizeChanged += RestoreContentSizeChanged;
            ArrangeRestoreControls();
        }

        /// <summary>Снимает подписку перед уничтожением диалога.</summary>
        protected override void OnFormClosed(FormClosedEventArgs e) {
            if (restoreContent != null) restoreContent.SizeChanged -= RestoreContentSizeChanged;
            restoreContent = null;
            base.OnFormClosed(e);
        }

        /// <summary>Сохраняет отступы режима восстановления при изменении размера окна.</summary>
        private void RestoreContentSizeChanged(object sender, EventArgs e) {
            if (!IsDisposed && !Disposing) ArrangeRestoreControls();
        }

        /// <summary>Убирает пустую колонку скрытых действий и центрирует кнопки подтверждения и отмены.</summary>
        private void ArrangeRestoreControls() {
            Control content = lBx.Parent;
            if (content == null || content.ClientSize.Width <= 0) return;
            const int margin = 12;
            const int gap = 12;
            int actionWidth = Math.Min(btnApply.Width, Math.Max(80, (content.ClientSize.Width - 3 * margin) / 2));
            int left = (content.ClientSize.Width - actionWidth * 2 - gap) / 2;
            int top = content.ClientSize.Height - margin - Math.Max(btnApply.Height, btnCancel.Height);
            btnApply.SetBounds(left, top, actionWidth, btnApply.Height);
            btnCancel.SetBounds(left + actionWidth + gap, top, actionWidth, btnCancel.Height);
            lBx.SetBounds(margin, margin, Math.Max(1, content.ClientSize.Width - 2 * margin),
                Math.Max(1, top - gap - margin));
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
            if (lBx.SelectedIndex != -1) {
                lBx.Items.RemoveAt(lBx.SelectedIndex);
            }
        }
        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }
        private void btnChange_Click(object sender, EventArgs e) {
            if (LBx.SelectedIndex != -1) {
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
}
