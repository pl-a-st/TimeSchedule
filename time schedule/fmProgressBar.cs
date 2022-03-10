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
    public delegate void ForControl ();
    public partial class fmProgressBar : Form {
        public fmProgressBar() {
            InitializeComponent();
        }
        public void SetTextMessege(string textToMessege) {
            label1.Text = textToMessege;
        }
        public Form GetForm () {
            return this;
        }
        public Statuses.FormLoad StatusLoad {get;private set;} = Statuses.FormLoad.UnLoad;
        //ForControl ForControl = new ForControl(GetForm);
        private void fmProgressBar_Load(object sender, EventArgs e) {
            StatusLoad = Statuses.FormLoad.Load;
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Style = ProgressBarStyle.Marquee;
            //Program.delegatForControl = GetForm;
        }
    }
}
