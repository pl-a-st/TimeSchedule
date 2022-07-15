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
    public partial class fmtreeView : Form {
        public FormBtnClick BtnClick {
            get;
            private set;
        } = FormBtnClick.NoClick;
        public bool IsChecked {
            get;
            private set;
        } = false;
        public bool GetIsChecked() {
            return IsChecked;
        }
        public fmtreeView() {
            InitializeComponent();
        }

        private void fmtreeView_Load(object sender, EventArgs e) {
            btnOk.Click += BtnOk_Click;
        }

        private void BtnOk_Click(object sender, EventArgs e) {
            BtnClick = FormBtnClick.Ok;
            this.Close();
        }

        public FormBtnClick GetBtnClick() {
            return BtnClick;
        }
        public TreeView GetTreeView() {
            return treeView1;
        }

    }
}
