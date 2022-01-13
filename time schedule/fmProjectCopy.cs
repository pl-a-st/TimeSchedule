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
        public class Project
        {
            public string Name
            { get; private set;}
            public void SetName (string name)
            {
                Name = name;
            }
            public string Address
            { get; private set; }
            public void SetAddress(string address)
            {
                Address = address;
            }
            public Project (string name, string address)
            {
                Name = name;
                Address = Address;
            }
        }
        public class ListProjects
        {
            public List<Project> projects
            { get; private set; } = new List<Project>();

        }
        private void fmProjectCopy_Load(object sender, EventArgs e)
        {
            //Project project = new Project();
            //project.SetName("sd");
        }
    }
}
