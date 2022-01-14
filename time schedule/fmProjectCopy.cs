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
    public partial class fmProjectCopy : Form
    {
        public fmProjectCopy()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана задача.");
                return;
            }
        }
        public ListProjects ListProjects
        { get; private set; } = new ListProjects();
        private void fmProjectCopy_Load(object sender, EventArgs e)
        {
            ListProjects = new ListProjects(
                Dals.ReadListFromProjectFile(Constants.PROFECT_TO_COPY));
            foreach(Project project in ListProjects.Projects)
            {
                lBxProject.Items.Add(project.Name);
            }
        }

        private void lBxProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lBxProject.SelectedIndex != -1)
                tBxAddress.Text = ListProjects.Projects[lBxProject.SelectedIndex].Address;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            fmAddProject fmAddProject = new fmAddProject();
            fmAddProject.ShowDialog();
            ListProjects.Add(fmAddProject.Project);
            lBxProject.Items.Clear();
            foreach (Project project in ListProjects.Projects)
            {
                lBxProject.Items.Add(project.Name);
            }
            if (lBxProject.Items.Count>0)
                lBxProject.SelectedIndex = lBxProject.Items.Count-1;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {

        }
    }
}
