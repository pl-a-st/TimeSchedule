using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace time_schedule
{
    public enum TaskLoadFor
    {
        LoadForAll,
        LoadForPerson
    }
    public enum NeedCheckStatus
    {
        Need,
        Nedlessly
    }
    public partial class fmTasks : StyledWindowForm
    {
        private Form1 Form1Delegat;
        public fmTasks(Form1 form1Delegat)
        {
            Form1Delegat = form1Delegat;
            InitializeComponent();

        }
        /// <summary>Выравнивает поля и кнопки после создания области содержимого собственного заголовка.</summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ArrangeTaskControls();
        }

        /// <summary>
        /// Сохраняет одинаковые боковые отступы и промежутки, не меняя логику фильтров
        /// и число видимых строк списка задач.
        /// </summary>
        private void ArrangeTaskControls()
        {
            Control content = lBxTasks.Parent;
            if (content == null) return;
            int margin = lBxTasks.Left;
            int gap = Math.Max(6, margin * 2 / 3);
            int buttonWidth = btnNewTask.Width;
            int columnLeft = content.ClientSize.Width - margin - buttonWidth;
            int leftAreaRight = columnLeft - gap;
            if (leftAreaRight - lBxTasks.Left < 320) return;

            Button[] actions =
            {
                btnNewTask, btnChangeTask, btnGoToDateStartTask, btnDeleteTask,
                btnChangeToSelectTasks, butOpenTaskInExcell,
                butAddProjectToSelectTasks, butDeleteTaskInlBx
            };
            foreach (Button button in actions)
            {
                button.Left = columnLeft;
                button.Width = buttonWidth;
            }
            chkMultySelectTask.Left = columnLeft + 2;
            chkMultySelectTask.Width = buttonWidth - 2;

            int listWidth = leftAreaRight - lBxTasks.Left;
            lBxTasks.Width = listWidth;
            tBxTargetTask.Width = leftAreaRight - tBxTargetTask.Left;
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Width = leftAreaRight - groupBox3.Left;
            groupBox4.Width = leftAreaRight - groupBox4.Left;
            tBxStatus.Width = leftAreaRight - tBxStatus.Left;
            tBxDateFinish.Width = leftAreaRight - tBxDateFinish.Left;

            ArrangeTaskFilterGroups(margin, gap);
            ArrangeTaskSearch(margin, gap);

            butOpenTaskInExcell.Text = "Открыть в Excell\nвсе задачи списка";
            butAddProjectToSelectTasks.Text = "Добавить проекты\nвсем задачам списка";
            btnChangeToSelectTasks.Text = "Изменить все\nзадачи списка";
            butDeleteTaskInlBx.Text = "Удалить все\nзадачи списка";
        }

        /// <summary>Согласует ширины вложенных фильтров и интервалы между их полями.</summary>
        private void ArrangeTaskFilterGroups(int margin, int gap)
        {
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Width = groupBox4.ClientSize.Width - groupBox1.Left - margin;
            groupBox2.Width = groupBox4.ClientSize.Width - groupBox2.Left - margin;
            int innerMargin = Math.Max(8, margin - 2);
            dTPFilterDateStart.Left = innerMargin;
            dTPFilterDateFinish.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dTPFilterDateFinish.Left = groupBox1.ClientSize.Width - innerMargin - dTPFilterDateFinish.Width;
            btnFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFilter.Left = groupBox2.ClientSize.Width - innerMargin - btnFilter.Width;
            tBxStatusesFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tBxStatusesFilter.Left = innerMargin;
            tBxStatusesFilter.Width = btnFilter.Left - gap - tBxStatusesFilter.Left;
        }

        /// <summary>Разводит подпись поиска, её декоративную рамку и список задач.</summary>
        private void ArrangeTaskSearch(int margin, int gap)
        {
            tBxTargetTask.Top = label1.Bottom + margin / 2;
            lBxTasks.Top = tBxTargetTask.Bottom + gap;
        }

        public TaskLoadFor FmTasksStatusLoad
        {
            get;
            private set;
        } = TaskLoadFor.LoadForAll;
        public NeedCheckStatus NeedCheckStatus
        {
            get;
            private set;
        } = NeedCheckStatus.Nedlessly;
        public void SetFmTasksStatusLoad(TaskLoadFor fmTasksStatusLoad)
        {
            FmTasksStatusLoad = fmTasksStatusLoad;
        }
        /// <summary>Заполняет список задач и начальные значения фильтров с учётом прав пользователя.</summary>
        public void fmTasks_Load(object sender, EventArgs e)
        {
            LoadLBxTasks();

            dTPFilterDateStart.Value = DateTime.Now.Date;
            try
            {
                dTPFilterDateFinish.Value = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks().Date;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule tasks date filter initialization failed: " + ex);
            }
            if (Program.UserType != UserType.Admin)
            {
                btnDeleteTask.Enabled = false;
                btnNewTask.Enabled = false;
                btnChangeToSelectTasks.Enabled = false;
                btnChangeTask.Enabled = false;
                butDeleteTaskInlBx.Enabled = false;
                butAddProjectToSelectTasks.Enabled = false;
            }
        }
        public void SetFilterDateStart(DateTime dateTime)
        {
            dTPFilterDateStart.Value = dateTime;
        }
        public void SetFilterDateFinish(DateTime dateTime)
        {
            dTPFilterDateFinish.Value = dateTime;
        }
        public ListBox RetunlBxTasks()
        {
            return lBxTasks;
        }
        private Boolean IsMatchDateConditions(Task task)
        {
            if (!cBxFilterByDate.Checked)
                return true;
            if (task.DateFinish >= dTPFilterDateStart.Value.Date &&
                task.DateStart <= dTPFilterDateFinish.Value.Date)
            {
                return true;
            }
            return false;
        }
        private Boolean IsMatchPersonConditions(Task task)
        {
            if (FmTasksStatusLoad == TaskLoadFor.LoadForAll)
                return true;
            if (FmTasksStatusLoad == TaskLoadFor.LoadForPerson &&
                Program.Person.PersonFamaly == task.PersonFamaly)
            {
                return true;
            }
            return false;
        }

        private bool IsNotStarted(Task task)
        {
            if (cBxNotStarted.Checked)
            {
                return (task.DateStart.Date < DateTime.Now.Date) && (task.Status == TaskStatus.New);
            }
            return true;
        }
        private bool IsOverdue(Task task)
        {
            if (cBxOverdue.Checked)
            {
                return (task.DateFinish.Date < DateTime.Now.Date) && (task.Status != TaskStatus.Closed);
            }
            return true;
        }
        private Boolean IsAllCorresponds(Task task)
        {
            if (
                IsMatchPersonConditions(task) &&
                IsTaskContainsText(task) &&
                IsStatusFit(task) &&
                IsNotStarted(task) &&
                IsOverdue(task)
                )
            {
                if (!IsMatchDateConditions(task))
                    return false;
                return true;
            }
            return false;

        }
        private bool IsStatusFit(Task task)
        {
            if (NeedCheckStatus == NeedCheckStatus.Need)
            {
                return TaskStatuses.Contains(task.Status);
            }
            return true;
        }
        private bool IsTaskContainsText(Task task)
        {
            return (task.Name.ToUpper().Contains(tBxTargetTask.Text.ToUpper()) ||
                task.Number.ToString().Contains(tBxTargetTask.Text));
        }

        private void LoadLBxTasks()
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPersonToShow.Tasks)
            {
                if (IsAllCorresponds(task))
                    lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
            ClearTextBoxesIfListBoxOut();
        }
        private void ClearTextBoxesIfListBoxOut()
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                tBxPerson.Text = string.Empty;
                tBxStatus.Text = string.Empty;
                tBxDateStart.Text = string.Empty;
                tBxDateFinish.Text = string.Empty;
            }
        }
        public TextBox SetTextBox1()
        {
            return tBxTargetTask;
        }
        public void LoadLBxTasksPerson(string targetTaskName, string personFamaly)
        {
            lBxTasks.Items.Clear();
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                if (task.PersonFamaly == personFamaly && task.Name.ToUpper().Contains(targetTaskName.ToUpper()))
                    lBxTasks.Items.Add(task.Number.ToString() + "\t" + task.Name);
            }
        }
        private void btnNewTask_Click(object sender, EventArgs e)
        {
            fmAddChangeTask fmAddTask = new fmAddChangeTask(Program.delegatLoadRefreshForm);
            fmAddTask.SetCreateOrChange(CreateOrChange.Create);
            fmAddTask.ShowDialog();
            LoadLBxTasks();

        }
        public Button GetBtnChangeTask()
        {
            return btnChangeTask;
        }
        public Button GetBtnDeleteTask()
        {
            return btnDeleteTask;
        }
        public Button GetBtnNewTask()
        {
            return btnNewTask;
        }
        /// <summary>Проверяет, что выбранные номера присутствуют в показанном снимке.</summary>
        private Dictionary<long, string> CaptureTargets(string[] items)
        {
            return TaskStore.Capture(Program.ListTasksAllPersonToSave,
                items.Select(x => Convert.ToInt64(x.Split('\t')[0])));
        }

        /// <summary>Удаляет одну задачу по подтверждённому снимку, отсоединяя оставшихся непосредственных потомков.</summary>
        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            long number = Convert.ToInt64(lBxTasks.SelectedItem.ToString().Split('\t')[0]);
            long[] numbers = { number };
            Dictionary<long, string> expected;
            try { expected = TaskStore.CaptureDeletion(Program.ListTasksAllPersonToSave, numbers); }
            catch (TaskConflictException ex) { MessageBox.Show(this, ex.Message); return; }
            if (!ConfirmTaskDeletion(numbers, expected))
                return;

            this.Activate();
            if (Program.SaveProgramData(() => TaskStore.DeleteWithDirectDependents(
                Program.ListTasksAllPersonToSave, numbers, expected), expected) != Result.Sucess)
            {
                MessageBox.Show(this, Program.SaveFailureMessage);
                return;
            }
            Program.fmMain.ShowProgressbarAndMakeRefresh();
            Program.fmMain.EventRefreshFinished += ActionEventRefreshFinished;

            void ActionEventRefreshFinished()
            {
                LoadLBxTasks();
                Program.fmMain.EventRefreshFinished -= ActionEventRefreshFinished;
            }
        }

        /// <summary>Запрашивает выбор между отменой удаления и отсоединением непосредственных потомков.</summary>
        /// <param name="numbers">Уникальные номера задач, выбранных для удаления.</param>
        /// <param name="expected">Отпечатки удаляемых задач вместе с их остающимися потомками.</param>
        private bool ConfirmTaskDeletion(ICollection<long> numbers, IDictionary<long, string> expected)
        {
            int dependents = expected.Count - numbers.Count;
            string message;
            if (dependents == 0)
                message = numbers.Count == 1 ? "Удалить задачу №" + numbers.First() + "?"
                    : "Удалить выбранные задачи (" + numbers.Count + ")?";
            else
                message = "Количество непосредственных потомков, остающихся в проекте: " + dependents + "." +
                    "\n\nДа — удалить выбранные задачи и разорвать связи с этими потомками." +
                    "\nНет — отменить удаление.\nДаты потомков останутся прежними.";
            return MessageBox.Show(this, message, "Удаление задач", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void btnChangeTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            fmAddChangeTask fmAddTask = new fmAddChangeTask();
            fmAddTask.GhangeNamebtnCreateTask("Изменить");
            fmAddTask.SetCreateOrChange(CreateOrChange.Change);
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                if (task.Number == Convert.ToInt32(lBxTasks.SelectedItem.ToString().Split('\t')[0]))
                {
                    Program.Task = task;
                    break;
                }
            }
            fmAddTask.ShowDialog();
            //Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
            LoadLBxTasks();
        }


        public void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadLBxTasks();
        }

        private void fmTasks_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            Form1 form1 = this.Form1Delegat.SetForm1();
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }

            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                if (task.Number == Convert.ToInt32(lBxTasks.SelectedItem.ToString().Split('\t')[0]))
                {
                    Program.Task = task;
                    break;
                }
            }
            form1.ScrollToDate(Program.Task.DateStart.Date);
            foreach (TaskButton tButton in Program.ListTaskButtons.TaskButtons)
            {
                if (tButton.Task == Program.Task)
                {
                    tButton.SelectAllButtons();
                    break;
                }
            }
            this.Close();
        }

        private void lBxTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
                return;
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                if (task.Number == Convert.ToInt32(lBxTasks.SelectedItem.ToString().Split('\t')[0]))
                {
                    tBxPerson.Text = task.PersonFamaly;
                    tBxStatus.Text = GetStatusInRus(task.Status.ToString()).Replace('_', ' ');
                    tBxDateStart.Text = task.DateStart.ToString("dd  MMMM yyyy");
                    tBxDateFinish.Text = task.DateFinish.ToString("dd  MMMM yyyy");
                    return;
                }
            }
        }
        private string GetStatusInRus(string statusInEng)
        {
            return ((TaskStatusRus)Enum.Parse(typeof(TaskStatus), statusInEng, true)).ToString();
        }

        private void cBxFilterByDate_CheckedChanged(object sender, EventArgs e)
        {
            if (cBxFilterByDate.Checked)
            {
                dTPFilterDateStart.Enabled = true;
                dTPFilterDateFinish.Enabled = true;
                cBxNotStarted.Checked = false;
                cBxOverdue.Checked = false;
            }
            if (!cBxFilterByDate.Checked)
            {
                dTPFilterDateStart.Enabled = false;
                dTPFilterDateFinish.Enabled = false;
            }
            LoadLBxTasks();

        }
        private void dTPFilterDateStart_ValueChanged(object sender, EventArgs e)
        {
            LoadLBxTasks();
        }
        private void dTPFilterDateFinish_ValueChanged(object sender, EventArgs e)
        {
            LoadLBxTasks();
        }
        /// <summary>Выполняет экспорт в Excel в отдельном STA-потоке, оставляя окно прогресса в UI-потоке.</summary>
        private void button1_Click_1(object sender, EventArgs e)
        {
            string[] targetItems = GetTargetItems();
            Exception exportError = null;
            bool workerStarted = false;
            using (fmProgressBar progressForm = new fmProgressBar())
            {
                progressForm.SetTextMessege("Идет формирование файла Excell");
                Thread worker = new Thread(() =>
                {
                    try
                    {
                        WriteItemsToExcell(targetItems);
                    }
                    catch (Exception ex)
                    {
                        exportError = ex;
                        Trace.WriteLine("TimeSchedule Excel export failed: " + ex);
                    }
                    finally
                    {
                        try
                        {
                            progressForm.BeginInvoke(new Action(progressForm.Close));
                        }
                        catch (InvalidOperationException ex)
                        {
                            Trace.WriteLine("TimeSchedule Excel progress window already closed: " + ex);
                        }
                    }
                });
                worker.IsBackground = true;
                worker.SetApartmentState(ApartmentState.STA);
                progressForm.Shown += (s, args) =>
                {
                    try
                    {
                        worker.Start();
                        workerStarted = true;
                    }
                    catch (Exception ex)
                    {
                        exportError = ex;
                        Trace.WriteLine("TimeSchedule Excel export worker failed to start: " + ex);
                        progressForm.Close();
                    }
                };
                progressForm.ShowDialog(this);
                if (workerStarted)
                    worker.Join();
            }
            if (exportError != null)
                MessageBox.Show(this, "Не удалось сформировать файл Excel.\n\n" + exportError.Message,
                    "Ошибка экспорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string[] GetTargetItems()
        {
            string[] targetItems;
            if (chkMultySelectTask.Checked)
            {
                targetItems = new string[lBxTasks.SelectedItems.Count];
                lBxTasks.SelectedItems.CopyTo(targetItems, 0);
            }
            else
            {
                targetItems = new string[lBxTasks.Items.Count];
                lBxTasks.Items.CopyTo(targetItems, 0);
            }

            return targetItems;
        }
        private void WriteItemsToExcell(string[] targetItems)
        {

            var listTaskNumber = new List<long>();
            foreach (string item in targetItems)
            {
                listTaskNumber.Add(Convert.ToInt32(item.ToString().Split('\t')[0]));
            }
            var listTaskForWrite = new ListTasks();
            foreach (long taskNumber in listTaskNumber)
            {
                foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
                {
                    if (task.Number == taskNumber)
                    {
                        listTaskForWrite.AddTask(task);
                        break;
                    }
                }
            }
            string excelFileNameToSave = DateTime.Now.Date.ToString("dd MMMM yyyy") + ".xlsm";
            Dals.ExelWriteListTasks(Dals.ProjectFolderPath + "\\" + excelFileNameToSave, listTaskForWrite);
        }

        public List<TaskStatus> TaskStatuses
        {
            get;
            private set;
        } = new List<TaskStatus>();
        private void cBxStatus_Click(object sender, EventArgs e)
        {
            LoadListStatuses();
            LoadLBxTasks();
        }
        /// <summary>Показывает выбор статусов, обновляет фильтр и освобождает диалог вместе с его подписками.</summary>
        private void LoadListStatuses()
        {
            using (fmtreeView fmtreeView = new fmtreeView())
            {
                fmtreeView.Text = "Выберите статусы";
                TreeView treeView = fmtreeView.GetTreeView();
                treeView.CheckBoxes = true;
                bool isChecked = fmtreeView.GetIsChecked();
                treeView.Nodes.Add(new TreeNode("Все"));

                treeView.NodeMouseClick += TreeView_NodeMouseClick;
                void TreeView_NodeMouseClick(object senderTree, TreeNodeMouseClickEventArgs tree)
                {
                    if (isChecked)
                    {
                        foreach (TreeNode node in tree.Node.Nodes)
                        {
                            node.Checked = tree.Node.Checked;
                        }
                        if (tree.Node.Parent != null)
                        {
                            bool isNodeChecked = true;
                            foreach (TreeNode node in tree.Node.Parent.Nodes)
                            {
                                isNodeChecked &= node.Checked;
                            }
                            tree.Node.Parent.Checked = isNodeChecked;
                        }
                        isChecked = false;
                    }
                }
                treeView.BeforeCollapse += TreeView_BeforeCollapse; ;
                treeView.BeforeCheck += TreeView_BeforeCheck;
                foreach (int element in Enum.GetValues(typeof(TaskStatusRus)))
                {
                    treeView.Nodes[0].Nodes.Add(new TreeNode(
                        ((TaskStatusRus)element).ToString().Replace('_', ' ')));
                }
                foreach (TreeNode treeNode in treeView.Nodes[0].Nodes)
                {
                    if (TaskStatuses.Contains((TaskStatus)Enum.Parse(
                                        typeof(TaskStatusRus),
                                        treeNode.Text.Replace(' ', '_'),
                                        true)))
                    {
                        treeNode.Checked = true;
                    }
                }
                void TreeView_BeforeCollapse(object senderTree, TreeViewCancelEventArgs tree)
                {
                    tree.Cancel = true;
                }
                void TreeView_BeforeCheck(object sensenderTreeder, TreeViewCancelEventArgs tree)
                {
                    isChecked = true;
                }
                treeView.ExpandAll();
                var listStatuses = new List<TaskStatus>();
                fmtreeView.ShowDialog();
                if (fmtreeView.BtnClick == BtnClick.ok)
                {
                    TaskStatuses.Clear();
                    string allCheckedStatuses = string.Empty;
                    foreach (TreeNode node in treeView.Nodes[0].Nodes)
                    {
                        if (node.Checked)
                        {
                            if (allCheckedStatuses != string.Empty)
                                allCheckedStatuses += ", ";
                            allCheckedStatuses += node.Text;
                            TaskStatuses.Add
                                (
                                    (TaskStatus)Enum.Parse(
                                        typeof(TaskStatusRus),
                                        node.Text.Replace(' ', '_'),
                                        true)
                                );
                        }
                    }
                    tBxStatusesFilter.Text = allCheckedStatuses;
                }
            }
        }
        private void cBxFilterByStatus_CheckedChanged(object sender, EventArgs e)
        {
            if (cBxFilterByStatus.Checked)
            {
                tBxStatusesFilter.Enabled = true;
                btnFilter.Enabled = true;
                NeedCheckStatus = NeedCheckStatus.Need;
                cBxNotStarted.Checked = false;
                cBxOverdue.Checked = false;
                LoadListStatuses();
            }
            if (!cBxFilterByStatus.Checked)
            {
                tBxStatusesFilter.Enabled = false;
                btnFilter.Enabled = false;
                NeedCheckStatus = NeedCheckStatus.Nedlessly;
                TaskStatuses.Clear();
                tBxStatusesFilter.Text = "";
            }
            LoadLBxTasks();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            LoadListStatuses();
            LoadLBxTasks();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void cBxOverdue_CheckedChanged(object sender, EventArgs e)
        {
            if (cBxOverdue.Checked)
            {
                cBxFilterByStatus.Checked = false;
                cBxFilterByDate.Checked = false;
                cBxNotStarted.Checked = false;
            }
            LoadLBxTasks();
        }

        private void cBxNotStarted_CheckedChanged(object sender, EventArgs e)
        {
            if (cBxNotStarted.Checked)
            {
                cBxFilterByStatus.Checked = false;
                cBxFilterByDate.Checked = false;
                cBxOverdue.Checked = false;
            }
            LoadLBxTasks();
        }

        /// <summary>Применяет массовые изменения к проверенному набору задач одной транзакцией.</summary>
        private void ChangeSelectTasks_Click(object sender, EventArgs e)
        {

            string[] selected = GetTargetItems();
            Dictionary<long, string> expected;
            try { expected = CaptureTargets(selected); }
            catch (TaskConflictException ex) { MessageBox.Show(ex.Message); return; }
            fmAddChangeTask fmChangeTask = new fmAddChangeTask(Program.delegatLoadRefreshForm);
            fmChangeTask.GhangeNamebtnCreateTask("Изменить для \n выбранных задач");
            fmChangeTask.SetCreateOrChange(CreateOrChange.ChangeToSelectTasks);
            fmChangeTask.ShowDialog();
            if (fmChangeTask.ClickButton == ClickButton.Aplly)
            {
                if (Program.SaveProgramData(ChangeTasks, expected) != Result.Sucess)
                {
                    MessageBox.Show(Program.SaveFailureMessage);
                    fmChangeTask.ShowDialog();
                    return;
                }
                LoadLBxTasks();
                Program.fmMain.ShowProgressbarAndMakeRefresh();
            }

            void ChangeTasks()
            {
                foreach (string itemLbx in selected)
                {
                    foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
                    {
                        if (task.Number == Convert.ToInt32(itemLbx.ToString().Split('\t')[0]))
                        {
                            if (fmChangeTask.GetPriority().Enabled)
                                task.SetPriority((int)fmChangeTask.GetPriority().Value);
                            if (fmChangeTask.GetStatus().Enabled)
                            {
                                task.SetTaskStatus((TaskStatus)Enum.Parse(
                                            typeof(TaskStatusRus),
                                            fmChangeTask.GetStatus().Text.Replace(' ', '_'),
                                            true
                                            ));
                            }
                            if (fmChangeTask.GetColor().Enabled)
                            {
                                task.SetTaskColor(fmChangeTask.GetColor().BackColor);
                            }
                            if (fmChangeTask.GetProject().Enabled)
                            {
                                task.SetTreeProject(fmChangeTask.TreeProjects);
                            }
                            if (fmChangeTask.GetPerson().Enabled)
                            {
                                task.SetPersonFamaly(fmChangeTask.GetPerson().Text);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void chkMultySelectTask_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMultySelectTask.Checked)
            {
                butDeleteTaskInlBx.Text = "Удалить выделенные\nзадачи в списке";
                btnChangeToSelectTasks.Text = "Изменить выделенные\nзадачи в списке";
                butOpenTaskInExcell.Text = "Открыть выделенные\nзадачи в Excell";
                butAddProjectToSelectTasks.Text = "Добавить проекты\nвыделенным задачам\nсписка";
                btnChangeTask.Enabled = false;
                btnDeleteTask.Enabled = false;
                lBxTasks.SelectionMode = SelectionMode.MultiExtended;
            }
            if (!chkMultySelectTask.Checked)
            {
                butDeleteTaskInlBx.Text = "Удалить все\nзадачи из списка";
                btnChangeToSelectTasks.Text = "Изменить все\nзадачи в списке";
                butOpenTaskInExcell.Text = "Открыть в Excell\nвсе задачи списка";
                butAddProjectToSelectTasks.Text = "Добавить проекты\nвсем задачам списка";
                btnChangeTask.Enabled = true;
                btnDeleteTask.Enabled = true;
                lBxTasks.SelectionMode = SelectionMode.One;
            }
        }

        /// <summary>Удаляет выбранные задачи одной операцией, отсоединяя не удаляемых вместе с ними потомков.</summary>
        private void butDeleteTaskInlBx_Click(object sender, EventArgs e)
        {
            long[] numbers = GetTargetItems().Select(item =>
                Convert.ToInt64(item.Split('\t')[0])).Distinct().ToArray();
            if (numbers.Length == 0)
                return;

            Dictionary<long, string> expected;
            try { expected = TaskStore.CaptureDeletion(Program.ListTasksAllPersonToSave, numbers); }
            catch (TaskConflictException ex) { MessageBox.Show(this, ex.Message); return; }
            if (!ConfirmTaskDeletion(numbers, expected))
                return;

            this.Activate();
            if (Program.SaveProgramData(() => TaskStore.DeleteWithDirectDependents(
                Program.ListTasksAllPersonToSave, numbers, expected), expected) != Result.Sucess)
            {
                MessageBox.Show(this, Program.SaveFailureMessage);
                return;
            }
            Form1 form1 = this.Form1Delegat.SetForm1();
            form1.LoadRefreshForm(Statuses.ProgressBar.Use);
            LoadLBxTasks();
        }

        /// <summary>Добавляет проекты выбранным задачам без перезаписи чужих изменений.</summary>
        private void butAddProjectToSelectTasks_Click(object sender, EventArgs e)
        {
            string[] targetItems = GetTargetItems();
            Dictionary<long, string> expected;
            try { expected = CaptureTargets(targetItems); }
            catch (TaskConflictException ex) { MessageBox.Show(ex.Message); return; }
            fmProjectTree fmProjectTree = new fmProjectTree();
            fmProjectTree.StartPosition = FormStartPosition.CenterParent;
            TreeProjects patternTtreeProjects = TreeProjects.GetMainTree();
            fmProjectTree.SetTreeView(patternTtreeProjects);
            fmProjectTree.GetTreeView().Nodes[0].Expand();
            fmProjectTree.SetHasLoad(HasLoad.Yes);
            fmProjectTree.ShowDialog();
            if (fmProjectTree.ClickButton != ClickButton.Aplly)
            {
                return; 
            }
            patternTtreeProjects.SetTreeViewProjects(fmProjectTree.projectTreeView);
            if (Program.SaveProgramData(() =>
            {
                foreach (string itemLbx in targetItems)
                {
                    foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
                {
                    if (task.Number == Convert.ToInt32(itemLbx.ToString().Split('\t')[0]))
                    {
                        task.GetTreeProjects().SetTreeViewProjects(
                        TreeProjects.AddCheckedFromListNodeToResultListNode(task.GetTreeProjects().ListTreeNode, patternTtreeProjects.ListTreeNode));
                        break;
                    }
                }
                }
            }, expected) != Result.Sucess)
            {
                MessageBox.Show(Program.SaveFailureMessage);
                return;
            }
            LoadLBxTasks();
            Program.fmMain.SetForm1().LoadRefreshForm(Statuses.ProgressBar.Use);
        }
    }
}
