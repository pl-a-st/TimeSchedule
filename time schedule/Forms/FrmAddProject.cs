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
    public partial class FrmAddProject : Form
    {
        public FrmAddProject()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TaskPanel taskPanel = new TaskPanel(
                "Пример",
                TaskPanel.TaskPanelesCount,
                new Point(x: 3, y: 3+TaskPanel.TaskPaneleLastHeight* TaskPanel.TaskPanelesCount- TaskPanel.TaskPanelesCount)
                );
            pnlMain.Controls.Add(taskPanel.PnlForTask);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
           
            
        }

        private void groupBox1_Leave(object sender, EventArgs e)
        {
           
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Enter(object sender, EventArgs e)
        {
            
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void panel1_Leave(object sender, EventArgs e)
        {
            
        }
    }
}
