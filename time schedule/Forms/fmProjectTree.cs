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
    [Serializable]
    public partial class fmProjectTree : Form {
        public fmProjectTree() {
            InitializeComponent();
        }

        public TreeView GetTreeView() {
            return projectTreeView;
        }
        public void SetTreeView(TreeProjects treeProjects) {
            projectTreeView.Nodes.Clear();
            projectTreeView.CheckBoxes = true;

            foreach (TreeNode node in treeProjects.TreeViewProjects) {
                projectTreeView.Nodes.Add(new TreeNode (node.Text));
                projectTreeView.Nodes[projectTreeView.Nodes.Count - 1].Checked =
                    node.Checked;
                if (node.Tag!=null) {
                    projectTreeView.Nodes[projectTreeView.Nodes.Count - 1].Expand();
                }
                if (node.Nodes.Count > 0) {
                    foreach (TreeNode copyNode in node.Nodes) {
                        CopyNodeToProjectTreeView(
                            projectTreeView.Nodes[projectTreeView.Nodes.Count -1],
                            copyNode);
                    }
                } 
            }
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
            Dals.WriteObjectToFile(Constants.PROJECTS_LIST, treeProjects);
            this.Close();
        }

        private void fmProjectTree_Load(object sender, EventArgs e) {
            projectTreeView.CheckBoxes = true;
            if (projectTreeView.Nodes.Count == 0) {
                projectTreeView.Nodes.Add(new TreeNode("Все"));
            }
        }

        private void addNode_Click(object sender, EventArgs e) {
            projectTreeView.Focus();
            fmInpootText inpootText = new fmInpootText();
            inpootText.SetLabel().Text = "Введите имя узла";
            inpootText.SetBtnYes().Text = "Ок";
            inpootText.Text = "Ввод названия";
            inpootText.ShowDialog();
            if (inpootText.ChoiceIsMade== ChoiceIsMade.yes) {
                projectTreeView.SelectedNode.Nodes.Add(inpootText.SetTextBox().Text);
                projectTreeView.SelectedNode.Expand();
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
    }
}
