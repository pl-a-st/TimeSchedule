using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace time_schedule {
    public enum HasLoad {
        Yes,
        No
    }
    public enum ApllyingChaced {
        Yes,
        No
    }
    public enum StatusFmProjectTree {
        ForAll,
        ForTaskCreate,
        ForTaskChange
    }
    public enum ClickButton {
        Aplly,
        Cancel
    }
    [Serializable]
    public partial class fmProjectTree : Form {
        public HasLoad HasLoad {
            get; private set; 
        } = HasLoad.No;
        public ApllyingChaced ApllyingChaced { 
            get; private set; 
        } = ApllyingChaced.No;
        public StatusFmProjectTree StatusFmProjectTree {
            get; private set;
        }
        public ClickButton ClickButton {
            get; private set;
        } = ClickButton.Cancel;
        public fmProjectTree() {
            InitializeComponent();
        }

        public TreeView GetTreeView() {
            return projectTreeView;
        }
        public  void SetHasLoad(HasLoad hasLoad) {
             HasLoad = hasLoad;
        }
        public void SetStatusFmProjectTree(StatusFmProjectTree statusFmProjectTree) {
            StatusFmProjectTree = statusFmProjectTree;
        }
        public void SetTreeView(TreeProjects treeProjects) {
            projectTreeView.Nodes.Clear();
            projectTreeView.CheckBoxes = true;

            foreach (TreeNode node in treeProjects.ListTreeNode) {
                projectTreeView.Nodes.Add(new TreeNode(node.Text));
                projectTreeView.Nodes[projectTreeView.Nodes.Count - 1].Checked =
                    node.Checked;
                if (node.Tag != null) {
                    projectTreeView.Nodes[projectTreeView.Nodes.Count - 1].Expand();
                }
                if (node.Nodes.Count > 0) {
                    foreach (TreeNode copyNode in node.Nodes) {
                        CopyNodeToProjectTreeView(
                            projectTreeView.Nodes[projectTreeView.Nodes.Count - 1],
                            copyNode);
                    }
                }
            }
            //foreach (TreeNode node in treeProjects.ListTreeNode) {
            //    projectTreeView.Nodes.Add(node.Clone() as TreeNode);
            //}
        }
        private void CopyNodeToProjectTreeView(TreeNode changedNode, TreeNode copyNode) {
            changedNode.Nodes.Add(new TreeNode(copyNode.Text));
            changedNode.LastNode.Checked = copyNode.Checked;
            if (copyNode.Tag != null) {
                changedNode.LastNode.Expand();
            }
            if (copyNode.Nodes.Count > 0) {
                foreach (TreeNode ReqCopyNode in copyNode.Nodes) {
                    CopyNodeToProjectTreeView(changedNode.LastNode, ReqCopyNode);
                }
            }
           
        }
        private void MarkExpandNodes(TreeNode treeNode) {
            if (treeNode.IsExpanded) {
                treeNode.Tag = true;
            }
            else {
                treeNode.Tag = null;
            }
            foreach (TreeNode node in treeNode.Nodes) {
                MarkExpandNodes(node);
            }
        }
        private void Apply_Click(object sender, EventArgs e) {

            foreach (TreeNode node in projectTreeView.Nodes) {
                if (node.IsExpanded) {
                    node.Tag = true;
                }
                else {
                    node.Tag = null;
                }
                MarkExpandNodes(node);
            }
            //Program.ProjetTree = projectTreeView;
            TreeProjects treeProjects = new TreeProjects();
            treeProjects.SetTreeViewProjects(projectTreeView);
            ClickButton = ClickButton.Aplly;
            this.Close();
            
        }

        private void fmProjectTree_Load(object sender, EventArgs e) {
            projectTreeView.CheckBoxes = true;
            if (projectTreeView.Nodes.Count == 0) {
                projectTreeView.Nodes.Add(new TreeNode("Все"));
            }
        }
        private bool CheckNameAndAddNode(fmInpootText inpootText, bool isNameCorrecrt) {
            foreach (TreeNode treeNode in projectTreeView.SelectedNode.Nodes) {
                if (treeNode.Text == inpootText.SetTextBox().Text) {
                    MessageBox.Show("Такое название уже существует.");
                    isNameCorrecrt = false;
                    return isNameCorrecrt;
                }
                isNameCorrecrt = true;
            }
            projectTreeView.SelectedNode.Nodes.Add(inpootText.SetTextBox().Text);
            projectTreeView.SelectedNode.Expand();
            this.ApllyCheckedToParentIfAllChieldChecked(projectTreeView.SelectedNode);
            return isNameCorrecrt;
        }
        private void addNode_Click(object sender, EventArgs e) {
            projectTreeView.Focus();
            fmInpootText inpootText = new fmInpootText();
            inpootText.SetLabel().Text = "Введите имя узла";
            inpootText.SetBtnYes().Text = "Ок";
            inpootText.Text = "Ввод названия";
            bool isNameCorrecrt = false;
            while (!isNameCorrecrt || inpootText.ChoiceIsMade != ChoiceIsMade.yes) {
                inpootText.ShowDialog();
                if (inpootText.ChoiceIsMade == ChoiceIsMade.yes) {
                    isNameCorrecrt = CheckNameAndAddNode(inpootText, isNameCorrecrt);
                }
                else {
                    break;
                }
            } 
        }
       
        private void RemoveNode_Click(object sender, EventArgs e) {
            projectTreeView.Focus();
            if (projectTreeView.SelectedNode.Level == 0) {
                MessageBox.Show("Нельзя удалять корневой элемент!");
                return;
            }
            DialogResult result = MessageBox.Show(
                "Вы уверены что хотите удалить ветку?",
                "Сообщение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
            this.Activate();
            if (result == DialogResult.Yes) {
                projectTreeView.SelectedNode.Remove();
            }
            projectTreeView.Focus();

        }

        private void cancel_Click(object sender, EventArgs e) {
            this.Close();
        }
        private void RestoreExpand(TreeNode treeNode) {
            if (treeNode.Tag != null)
                treeNode.Expand();
            foreach(TreeNode chNode in treeNode.Nodes) {
                RestoreExpand(chNode);
            }
        }
        private void MoveNode(int index) {
           
            TreeNode selectTreeNode = projectTreeView.SelectedNode;
            MarkExpandNodes(selectTreeNode);
            TreeNode ParentTreeNode = projectTreeView.SelectedNode.Parent;

            ParentTreeNode.Nodes.Insert(
                index,
                (projectTreeView.SelectedNode.Clone() as TreeNode)
                );
            projectTreeView.SelectedNode = ParentTreeNode.Nodes[index];
            
            selectTreeNode.Remove();
            RestoreExpand(projectTreeView.SelectedNode);


        }
        private void UpNode_Click(object sender, EventArgs e) {
            projectTreeView.Focus();
            if (projectTreeView.SelectedNode.Level == 0) {
                MessageBox.Show("Нельзя перемещать корневой элемент!");
                return;
            }
            if (projectTreeView.SelectedNode.Index == 0)
                return;
            int index = projectTreeView.SelectedNode.PrevNode.Index;
            MoveNode(index);
        }


        private void DownNode_Click(object sender, EventArgs e) {
            projectTreeView.Focus();
            if (projectTreeView.SelectedNode.Level == 0) {
                MessageBox.Show("Нельзя перемещать корневой элемент!");
                return;
            }
            if (
                projectTreeView.SelectedNode.Index == 
                projectTreeView.SelectedNode.Parent.Nodes.Count-1
                )
                return;
            int index = projectTreeView.SelectedNode.NextNode.Index+1;
            MoveNode(index);
        }

        private void ApllyCheckedToChilde(TreeNode node) {
            foreach (TreeNode chNode in node.Nodes) {
                chNode.Checked = node.Checked;
                ApllyCheckedToChilde(chNode);
            }
        }
        private void ApllyCheckedToParentIfAllChieldChecked(TreeNode node) {
            if (node.Parent == null)
                return;
            foreach (TreeNode chNode in node.Parent.Nodes) {
                if (!chNode.Checked)
                    return;
            }
            node.Parent.Checked = true;
            ApllyCheckedToParentIfAllChieldChecked(node.Parent);
        }
        private void ApllyCheckedToParent(TreeNode node) {
            if (node.Parent == null)
                return;
            if (!node.Checked)
                node.Parent.Checked = false;
            ApllyCheckedToParent(node.Parent);
        }

        private void projectTreeView_AfterCheck(object sender, TreeViewEventArgs node) {
            if (HasLoad == HasLoad.No)
                return;
            if (ApllyingChaced == ApllyingChaced.Yes)
                return;
            ApllyingChaced = ApllyingChaced.Yes;
            Thread.Sleep(300);
            ApllyCheckedToChilde(node.Node);
            ApllyCheckedToParent(node.Node);
            ApllyCheckedToParentIfAllChieldChecked(node.Node);
            ApllyingChaced = ApllyingChaced.No;
            this.Enabled = true;
        }

        private void projectTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            //NodeMouseClick = NodeMouseClick.yes;
        }

        private void projectTreeView_BeforeCheck(object sender, TreeViewCancelEventArgs node) {
            
        }
    }
}
