using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    internal class ButtonForShedule : Button
    {
        
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            //this.BackColor = Color.FromArgb(90, 90, 90);
            //this.ForeColor = Color.FromArgb(240, 240, 240);
            if (IsParentsEnabled(this))
            {
                this.BackColor = Color.FromArgb(80, 80, 85);
                this.ForeColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                this.BackColor = Color.FromArgb(180, 180, 180);
                this.ForeColor = Color.FromArgb(250, 250, 250);   
            }
        }
        private bool IsParentsEnabled(Control control)
        {
            if (control.Enabled)
            {
                if (control.Parent != null)
                {
                    return IsParentsEnabled(control.Parent);
                }
                return true;
            }
            return false;
        }
        protected override void OnParentEnabledChanged(EventArgs e)
        {
            if (this.Parent.Enabled)
            {
                this.BackColor = Color.FromArgb(90, 90, 90);
                this.ForeColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                this.BackColor = Color.FromArgb(180, 180, 180);
                this.ForeColor = Color.FromArgb(250, 250, 250);
            }
            base.OnParentEnabledChanged(e);
        }
        protected override void OnEnabledChanged(EventArgs e)
        {
            if (this.Enabled)
            {
                this.BackColor = Color.FromArgb(90, 90, 90);
                this.ForeColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                this.BackColor = Color.FromArgb(180, 180, 180);
                this.ForeColor = Color.FromArgb(250, 250, 250);
            }
            base.OnEnabledChanged(e);
        }

    }
}
