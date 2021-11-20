namespace time_schedule
{
    partial class fmSelectTask
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lBxTasks = new System.Windows.Forms.ListBox();
            this.btnSelectTask = new System.Windows.Forms.Button();
            this.tBxTargetTask = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bTnCacel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lBxTasks
            // 
            this.lBxTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lBxTasks.FormattingEnabled = true;
            this.lBxTasks.Location = new System.Drawing.Point(16, 60);
            this.lBxTasks.Name = "lBxTasks";
            this.lBxTasks.Size = new System.Drawing.Size(250, 446);
            this.lBxTasks.TabIndex = 0;
            // 
            // btnSelectTask
            // 
            this.btnSelectTask.Location = new System.Drawing.Point(272, 60);
            this.btnSelectTask.Name = "btnSelectTask";
            this.btnSelectTask.Size = new System.Drawing.Size(75, 23);
            this.btnSelectTask.TabIndex = 1;
            this.btnSelectTask.Text = "Выбрать";
            this.btnSelectTask.UseVisualStyleBackColor = true;
            this.btnSelectTask.Click += new System.EventHandler(this.btnSelectTask_Click);
            // 
            // tBxTargetTask
            // 
            this.tBxTargetTask.Location = new System.Drawing.Point(16, 34);
            this.tBxTargetTask.Name = "tBxTargetTask";
            this.tBxTargetTask.Size = new System.Drawing.Size(250, 20);
            this.tBxTargetTask.TabIndex = 2;
            this.tBxTargetTask.TextChanged += new System.EventHandler(this.tBxTargetTask_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Найти задачу";
            // 
            // bTnCacel
            // 
            this.bTnCacel.Location = new System.Drawing.Point(272, 89);
            this.bTnCacel.Name = "bTnCacel";
            this.bTnCacel.Size = new System.Drawing.Size(75, 23);
            this.bTnCacel.TabIndex = 1;
            this.bTnCacel.Text = "Отмена";
            this.bTnCacel.UseVisualStyleBackColor = true;
            this.bTnCacel.Click += new System.EventHandler(this.bTnCacel_Click);
            // 
            // fmSelectTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 519);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tBxTargetTask);
            this.Controls.Add(this.bTnCacel);
            this.Controls.Add(this.btnSelectTask);
            this.Controls.Add(this.lBxTasks);
            this.Name = "fmSelectTask";
            this.Text = "Выбор задачи";
            this.Load += new System.EventHandler(this.fmSelectTask_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lBxTasks;
        private System.Windows.Forms.Button btnSelectTask;
        private System.Windows.Forms.TextBox tBxTargetTask;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bTnCacel;
    }
}