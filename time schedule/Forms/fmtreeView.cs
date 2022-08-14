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
        public BtnClick BtnClick {
            get;
            private set;
        } = BtnClick.noClick;
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
            BtnClick = BtnClick.ok;
            this.Close();
        }

        public BtnClick GetBtnClick() {
            return BtnClick;
        }
        public TreeView GetTreeView() {
            return treeView1;
        }

    }
}
