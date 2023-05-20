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
        PoolTasksPanel PoolTasksPanel = new PoolTasksPanel();
        public FrmAddProject()
        {
            InitializeComponent();
        }

        private void butAddTask_Click(object sender, EventArgs e)
        {
            PoolTasksPanel.AddTaskPanel();
            TaskPanel taskPanel = PoolTasksPanel.TaskPanels[PoolTasksPanel.TaskPanels.Count - 1];
            taskPanel.PnlForTask.Location = new Point(
                x: taskPanel.PnlForTask.Location.X,
                y: taskPanel.PnlForTask.Location.Y - pnlMain.VerticalScroll.Value);
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

        private void butDelete_Click(object sender, EventArgs e)
        {
            int startposition = 0;
            foreach(var panel in PoolTasksPanel.TaskPanels)
            {
                if(panel.TaskAfter == null)
                {
                    continue;
                }
                if (panel.TaskAfter.CheckStatus == CheckStatus.Checked)
                {
                    panel.TaskAfter = null;
                    panel.NudTaskAfter.Value = 0;
                }
                
            }
            DeleteTaskAndReplaceOther(startposition);
            //pnlMain.Controls.Clear();
            //foreach (TaskPanel taskPanel in PoolTasksPanel.TaskPanels)
            //{
            //    pnlMain.Controls.Add(taskPanel.PnlForTask);
            //}
            PoolTasksPanel.SetNumForPanels();
        }
        private void DeleteTaskAndReplaceOther(int startPosition)
        {
            int verticalScrollValue = pnlMain.VerticalScroll.Value;
            for (int i = startPosition; i < PoolTasksPanel.TaskPanels.Count; i++)
            {
                if (PoolTasksPanel.TaskPanels[i].CheckStatus == CheckStatus.Checked)
                {
                    PoolTasksPanel.TaskPanels.RemoveAt(i);
                    pnlMain.Controls.RemoveAt(i);

                    for (int j = i; j < PoolTasksPanel.TaskPanels.Count; j++)
                    {
                        if (PoolTasksPanel.TaskPanels[i].CheckStatus == CheckStatus.Checked)
                        {
                            DeleteTaskAndReplaceOther(j);
                        }
                        if(j< PoolTasksPanel.TaskPanels.Count)
                        {
                            PoolTasksPanel.TaskPanels[j].PnlForTask.Location = new Point(
                            x: 3,
                            y: PoolTasksPanel.TaskPanels[j].PnlForTask.Location.Y - TaskPanel.TaskPaneleLastHeight + 1 );
                        }
                    }
                    i--;
                }

            }
        }

        private void FrmAddProject_Load(object sender, EventArgs e)
        {

        }

        private void butUplift_Click(object sender, EventArgs e)
        {
            int startPosition = 0;
            for (int i = startPosition; i < PoolTasksPanel.TaskPanels.Count; i++)
            {
                if (PoolTasksPanel.TaskPanels[i].CheckStatus== CheckStatus.Checked)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    Point tempLocation = PoolTasksPanel.TaskPanels[i].PnlForTask.Location;
                    PoolTasksPanel.TaskPanels[i].PnlForTask.Location = PoolTasksPanel.TaskPanels[i - 1].PnlForTask.Location;
                    PoolTasksPanel.TaskPanels[i - 1].PnlForTask.Location = tempLocation;
                    var selectedControl = pnlMain.Controls[i];
                    var prewSelectControl = pnlMain.Controls[i - 1];
                    pnlMain.Controls.SetChildIndex(selectedControl, i - 1);
                    pnlMain.Controls.SetChildIndex(prewSelectControl, i);
                    PoolTasksPanel.TaskPanels.Reverse(i - 1, 2);
                }
            }
            PoolTasksPanel.SetNumForPanels();
        }

        private void butDown_Click(object sender, EventArgs e)
        {
            
            for (int i = PoolTasksPanel.TaskPanels.Count-1; i>=0; i--)
            {
                if (PoolTasksPanel.TaskPanels[i].CheckStatus == CheckStatus.Checked)
                {
                    if (i == PoolTasksPanel.TaskPanels.Count - 1)
                    {
                        continue;
                    }
                    Point tempLocation = PoolTasksPanel.TaskPanels[i].PnlForTask.Location;
                    PoolTasksPanel.TaskPanels[i].PnlForTask.Location = PoolTasksPanel.TaskPanels[i + 1].PnlForTask.Location;
                    PoolTasksPanel.TaskPanels[i + 1].PnlForTask.Location = tempLocation;
                    var selectedControl = pnlMain.Controls[i];
                    var prewSelectControl = pnlMain.Controls[i + 1];
                    pnlMain.Controls.SetChildIndex(selectedControl, i + 1);
                    pnlMain.Controls.SetChildIndex(prewSelectControl, i);
                    PoolTasksPanel.TaskPanels.Reverse(i, 2);
                }
            }
            PoolTasksPanel.SetNumForPanels();
        }
    }
}
