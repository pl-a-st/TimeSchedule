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
    public partial class fmInpootText : Form
    {
        public fmInpootText()
        {
            InitializeComponent();
        }
        public ChoiceIsMade ChoiceIsMade
        { get; private set; } = ChoiceIsMade.no;
        private void fmInpootText_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
        public TextBox SetTextBox()
        {
            return tbxInpootText;
        }
        public Label SetLabel()
        {
            return label1;
        }
        public Button SetBtnYes()
        {
            return btnYes;
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            ChoiceIsMade = ChoiceIsMade.yes;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
