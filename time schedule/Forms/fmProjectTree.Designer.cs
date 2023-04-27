
namespace time_schedule {
    
    partial class fmProjectTree {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.projectTreeView = new System.Windows.Forms.TreeView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // projectTreeView
            // 
            this.projectTreeView.Location = new System.Drawing.Point(12, 12);
            this.projectTreeView.Name = "projectTreeView";
            this.projectTreeView.Size = new System.Drawing.Size(262, 447);
            this.projectTreeView.TabIndex = 0;
            this.projectTreeView.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.projectTreeView_BeforeCheck);
            this.projectTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.projectTreeView_AfterCheck);
            this.projectTreeView.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.projectTreeView_BeforeCollapse);
            this.projectTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.projectTreeView_BeforeExpand);
            this.projectTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.projectTreeView_AfterSelect);
            this.projectTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.projectTreeView_NodeMouseClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(280, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Добавить";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.addNode_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(280, 41);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(77, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Удалить";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.RemoveNode_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(280, 407);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(77, 23);
            this.button3.TabIndex = 1;
            this.button3.Text = "Применить";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Apply_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(280, 436);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(77, 23);
            this.button4.TabIndex = 1;
            this.button4.Text = "Отмена";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.cancel_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(302, 226);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(30, 23);
            this.button5.TabIndex = 5;
            this.button5.Text = "▼";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.DownNode_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(302, 188);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(30, 23);
            this.button6.TabIndex = 6;
            this.button6.Text = "▲";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.UpNode_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(280, 264);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(77, 36);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Сохранить  настройки";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // fmProjectTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(362, 481);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.projectTreeView);
            this.Name = "fmProjectTree";
            this.Text = "Дерево проектов";
            this.Load += new System.EventHandler(this.fmProjectTree_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public  System.Windows.Forms.TreeView projectTreeView;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button btnSave;
    }
}