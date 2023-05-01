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
    public enum WasClickInfrmWithRadioButton
    {
        Ok,
        Cancel
    }
    public partial class frmWithRadioButton : Form
    {
        public WasClickInfrmWithRadioButton WasClickInfrmWithRadioButton
        {
            get;
            private set;
        } = WasClickInfrmWithRadioButton.Cancel;
        public int CoutnRaioButton
        {
            get;
            private set;
        } = 0;
        public frmWithRadioButton()
        {
            InitializeComponent();
        }

        private void frmWithRadioButton_Load(object sender, EventArgs e)
        {
            foreach(Control control in this.Controls)
            {
                if(!(control is RadioButton))
                {
                    continue;
                }
                (control as RadioButton).Checked = true;
                break;
            }
        }
        public void SetLable(string text)
        {
            label1.Text = text;
        }
        public void AddRaioButton(string TextRadioButton)
        {
            RadioButton radioButton = new RadioButton();
            radioButton.AutoSize = true;
            radioButton.Text = TextRadioButton;
            
            radioButton.Location = new Point(x: 5, y: 20 + label1.Height + CoutnRaioButton * radioButton.Height);
            this.Controls.Add(radioButton);
            butOk.Location = new Point(butOk.Location.X, butOk.Location.Y + radioButton.Height+6);
            butCancel.Location = new Point(butCancel.Location.X, butCancel.Location.Y +   radioButton.Height+6);
            this.Height += radioButton.Height+6;
            CoutnRaioButton++;
        }
        public string GetTextSelectedRadio()
        {
            string textSelectedRadio = string.Empty;
            foreach (Control control in this.Controls)
            {
                if (control is RadioButton)
                {
                    if ((control as RadioButton).Checked)
                    {
                        textSelectedRadio = (control as RadioButton).Text;
                    }
                }
            }
            return textSelectedRadio;
        }
        private void butOk_Click(object sender, EventArgs e)
        {
            WasClickInfrmWithRadioButton = WasClickInfrmWithRadioButton.Ok;
            this.Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            WasClickInfrmWithRadioButton = WasClickInfrmWithRadioButton.Cancel;
            this.Close();
        }
    }
}
