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
    public enum ChoiceIsMade
    {
        yes,
        no
    }
    public partial class fmAddProject : Form
    {
        public fmAddProject()
        {
            InitializeComponent();
        }
        public Project Project = new Project();
        private void fmAddProject_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }
        public ChoiceIsMade ChoiceIsMade
        { get; private set; } = ChoiceIsMade.no;
        private void button1_Click(object sender, EventArgs e)
        {
            if (tBxNameProject.Text=="")
            {
                
                MessageBox.Show("Не назанно условное обозначение проекта");
                return;
            }
            string folderName = string.Empty;
            
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Выбирете папку \"Проект\" !!! ";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                ChoiceIsMade = ChoiceIsMade.yes;
                folderName = folderBrowserDialog.SelectedPath;
                Project = new Project(tBxNameProject.Text, folderName);
                this.Close();
            }
            
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
