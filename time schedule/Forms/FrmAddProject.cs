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
    public partial class FrmAddProject : StyledWindowForm
    {
        PoolTasksPanel PoolTasksPanel = new PoolTasksPanel();
        /// <summary>Ширина таблицы при исходной компоновке, не допускающая наложения столбцов.</summary>
        private int originalWorkspaceWidth;
        /// <summary>Исходная ширина рабочей панели с задачами.</summary>
        private int originalTasksPanelWidth;
        /// <summary>Правые управляющие кнопки, сохраняющие расположение при прокрутке и расширении.</summary>
        private Control[] projectActions;
        /// <summary>Горизонтальные координаты правых кнопок при исходной ширине формы.</summary>
        private int[] originalActionLeft;
        /// <summary>Исключает служебные заголовки колонок из переходов Tab и задаёт порядок рабочих элементов.</summary>
        public FrmAddProject()
        {
            InitializeComponent();
            comboBox1.TabIndex = 0;
            txtProjectName.TabIndex = 1;
            dateTimePicker1.TabIndex = 2;
            pnlMain.TabIndex = 3;
            butAddTask.TabIndex = 4;
            butDelete.TabIndex = 5;
            butUplift.TabIndex = 6;
            butDown.TabIndex = 7;

            foreach (Button header in new Button[]
            {
                butNum, butTaskName, butDateStart, butDateFinish,
                butWorkDaysCount, button1, button2
            })
                header.TabStop = false;
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

        /// <summary>Обеспечивает доступ к широкой таблице на узком экране без смещения её заголовков относительно строк.</summary>
        private void FrmAddProject_Load(object sender, EventArgs e)
        {
            ConfigureProjectViewport();
        }

        /// <summary>
        /// Сохраняет исходную ширину колонок, включая кнопку управления задачами,
        /// а на небольших экранах включает горизонтальную прокрутку всей таблицы.
        /// </summary>
        private void ConfigureProjectViewport()
        {
            Panel content = pnlMain.Parent as Panel;
            if (content == null) return;

            originalWorkspaceWidth = content.ClientSize.Width;
            originalTasksPanelWidth = pnlMain.Width;
            projectActions = new Control[] { butAddTask, butDelete, butUplift, butDown };
            originalActionLeft = projectActions.Select(action => action.Left).ToArray();
            pnlMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            foreach (Control action in projectActions)
                action.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            content.AutoScrollMinSize = new Size(originalWorkspaceWidth, 0);
            content.AutoScroll = true;

            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            if (Width > workArea.Width || Height > workArea.Height)
            {
                Size = new Size(Math.Min(Width, workArea.Width), Math.Min(Height, workArea.Height));
                Location = new Point(workArea.Left + (workArea.Width - Width) / 2,
                    workArea.Top + (workArea.Height - Height) / 2);
            }
            ArrangeProjectViewport();
        }

        /// <summary>Сохраняет расширение таблицы на широком окне и фиксированную геометрию на узком.</summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ArrangeProjectViewport();
        }

        /// <summary>Размещает рабочую панель и кнопки относительно исходной ширины таблицы.</summary>
        private void ArrangeProjectViewport()
        {
            if (projectActions == null || pnlMain == null || pnlMain.IsDisposed ||
                pnlMain.Parent == null) return;

            Panel content = pnlMain.Parent as Panel;
            if (content == null) return;
            int growth = Math.Max(0, content.ClientSize.Width - originalWorkspaceWidth);
            pnlMain.Width = originalTasksPanelWidth + growth;
            int scrollOffset = content.AutoScrollPosition.X;
            for (int i = 0; i < projectActions.Length; i++)
                projectActions[i].Left = originalActionLeft[i] + growth + scrollOffset;
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
