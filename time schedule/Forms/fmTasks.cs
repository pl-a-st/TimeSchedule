using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
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
    public partial class fmTasks : Form
    {
        private Form1 Form1Delegat;
        public fmTasks(Form1 form1Delegat)
        {
            Form1Delegat = form1Delegat;
            InitializeComponent();

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
        public void fmTasks_Load(object sender, EventArgs e)
        {
            LoadLBxTasks();

            dTPFilterDateStart.Value = DateTime.Now.Date;
            try
            {
                dTPFilterDateFinish.Value = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks().Date;
            }
            catch { }
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
        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            DialogResult result = MessageBox.Show(
                "Вы уверены что хотите удалить задачу?",
                "Сообщение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                Program.fmMain.TopMost = true;
                this.TopMost = true;

                return;
            }
            this.Activate();
            string targetItem = lBxTasks.SelectedItem.ToString();
            DeleteTaskFromListToSave(targetItem);
            Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
            Form1 form1 = this.Form1Delegat.SetForm1();
            form1.LoadRefreshForm(Statuses.ProgressBar.Use);
            LoadLBxTasks();
        }

        private static void DeleteTaskFromListToSave(string targetItem)
        {
            foreach (Task task in Program.ListTasksAllPersonToSave.Tasks)
            {
                if (task.Number == Convert.ToInt32(targetItem.Split('\t')[0]))
                {
                    Program.ListTasksAllPersonToSave.Tasks.Remove(task);
                    break;
                }

            }
        }

        private void btnChangeTask_Click(object sender, EventArgs e)
        {
            if (lBxTasks.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
            fmAddChangeTask fmAddTask = new fmAddChangeTask(Program.delegatLoadRefreshForm);
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
            Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
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
        private void button1_Click_1(object sender, EventArgs e)
        {
            string[] targetItems = GetTargetItems();
            fmProgressBar fmProgressBar = new fmProgressBar();
            fmProgressBar.SetTextMessege("Идет формирование файла Excell");
            System.Threading.Tasks.Task task = new System.Threading.Tasks.Task
                (() => DoActionsCloseForm(() => WriteItemsToExcell(targetItems), fmProgressBar));
            task.Start();
            fmProgressBar.ShowDialog();
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
        private void DoActionsCloseForm(Action action, Form form)
        {
            action.Invoke();
            if (form.InvokeRequired)
            {
                BeginInvoke(new Action(() => { form.Close(); }));
                return;
            }
            form.Close();
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
        private void LoadListStatuses()
        {
            fmtreeView fmtreeView = new fmtreeView();
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

        private void ChangeSelectTasks_Click(object sender, EventArgs e)
        {

            fmAddChangeTask fmChangeTask = new fmAddChangeTask(Program.delegatLoadRefreshForm);
            fmChangeTask.GhangeNamebtnCreateTask("Изменить для \n выбранных задач");
            fmChangeTask.SetCreateOrChange(CreateOrChange.ChangeToSelectTasks);
            fmChangeTask.ShowDialog();
            if (fmChangeTask.ClickButton == ClickButton.Aplly)
            {
                string[] targetItems = GetTargetItems();
                foreach (string itemLbx in targetItems)
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
                Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
                LoadLBxTasks();
                Program.fmMain.SetForm1().LoadRefreshForm(Statuses.ProgressBar.Use);
            }

        }

        private void chkMultySelectTask_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMultySelectTask.Checked)
            {
                butDeleteTaskInlBx.Text = "Удалить выделенные задачи в списке";
                btnChangeToSelectTasks.Text = "Изменить выделенные задачи в списке";
                butOpenTaskInExcell.Text = "Открыть выделенные задачи в Excell";
                butAddProjectToSelectTasks.Text = "Добавить проекты выделенным задачам списка";
                btnChangeTask.Enabled = false;
                btnDeleteTask.Enabled = false;
                lBxTasks.SelectionMode = SelectionMode.MultiExtended;
            }
            if (!chkMultySelectTask.Checked)
            {
                butDeleteTaskInlBx.Text = "Удалить все задачи из списка";
                btnChangeToSelectTasks.Text = "Изменить все задачи в списке";
                butOpenTaskInExcell.Text = "Открыть в Excell все задачи списка";
                butAddProjectToSelectTasks.Text = "Добавить проекты всем задачам списка";
                btnChangeTask.Enabled = true;
                btnDeleteTask.Enabled = true;
                lBxTasks.SelectionMode = SelectionMode.One;
            }
        }

        private void butDeleteTaskInlBx_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show(
               "Вы уверены что хотите УДАЛИТЬ выбранные задачи?",
               "ВНИМАНИЕ",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning,
               MessageBoxDefaultButton.Button1,
               MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                Program.fmMain.TopMost = true;
                this.TopMost = true;
                return;
            }
            this.Activate();
            string[] targetItems = GetTargetItems();
            foreach (string targetItem in targetItems)
            {
                DeleteTaskFromListToSave(targetItem);
            }
            Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
            Form1 form1 = this.Form1Delegat.SetForm1();
            form1.LoadRefreshForm(Statuses.ProgressBar.Use);
            LoadLBxTasks();
        }

        private void butAddProjectToSelectTasks_Click(object sender, EventArgs e)
        {
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
            string[] targetItems = GetTargetItems();
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
            Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
            LoadLBxTasks();
            Program.fmMain.SetForm1().LoadRefreshForm(Statuses.ProgressBar.Use);
        }
    }
}
