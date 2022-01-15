namespace time_schedule
{
    partial class fmTasks
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
            this.btnChangeTask = new System.Windows.Forms.Button();
            this.btnDeleteTask = new System.Windows.Forms.Button();
            this.btnNewTask = new System.Windows.Forms.Button();
            this.lBxTasks = new System.Windows.Forms.ListBox();
            this.tBxTargetTask = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnGoToDateStartTask = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnChangeTask
            // 
            this.btnChangeTask.Location = new System.Drawing.Point(463, 96);
            this.btnChangeTask.Name = "btnChangeTask";
            this.btnChangeTask.Size = new System.Drawing.Size(142, 29);
            this.btnChangeTask.TabIndex = 7;
            this.btnChangeTask.Text = "Изменить задачу";
            this.btnChangeTask.UseVisualStyleBackColor = true;
            this.btnChangeTask.Click += new System.EventHandler(this.btnChangeTask_Click);
            // 
            // btnDeleteTask
            // 
            this.btnDeleteTask.Location = new System.Drawing.Point(463, 61);
            this.btnDeleteTask.Name = "btnDeleteTask";
            this.btnDeleteTask.Size = new System.Drawing.Size(142, 29);
            this.btnDeleteTask.TabIndex = 6;
            this.btnDeleteTask.Text = "Удалить задачу";
            this.btnDeleteTask.UseVisualStyleBackColor = true;
            this.btnDeleteTask.Click += new System.EventHandler(this.btnDeleteTask_Click);
            // 
            // btnNewTask
            // 
            this.btnNewTask.Location = new System.Drawing.Point(463, 26);
            this.btnNewTask.Name = "btnNewTask";
            this.btnNewTask.Size = new System.Drawing.Size(142, 29);
            this.btnNewTask.TabIndex = 5;
            this.btnNewTask.Text = "Создать задачу";
            this.btnNewTask.UseVisualStyleBackColor = true;
            this.btnNewTask.Click += new System.EventHandler(this.btnNewTask_Click);
            // 
            // lBxTasks
            // 
            this.lBxTasks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lBxTasks.FormattingEnabled = true;
            this.lBxTasks.Location = new System.Drawing.Point(12, 51);
            this.lBxTasks.Name = "lBxTasks";
            this.lBxTasks.Size = new System.Drawing.Size(445, 199);
            this.lBxTasks.TabIndex = 4;
            // 
            // tBxTargetTask
            // 
            this.tBxTargetTask.Location = new System.Drawing.Point(12, 26);
            this.tBxTargetTask.Name = "tBxTargetTask";
            this.tBxTargetTask.Size = new System.Drawing.Size(445, 20);
            this.tBxTargetTask.TabIndex = 8;
            this.tBxTargetTask.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Найти задачу";
            // 
            // btnGoToDateStartTask
            // 
            this.btnGoToDateStartTask.Location = new System.Drawing.Point(463, 131);
            this.btnGoToDateStartTask.Name = "btnGoToDateStartTask";
            this.btnGoToDateStartTask.Size = new System.Drawing.Size(142, 23);
            this.btnGoToDateStartTask.TabIndex = 10;
            this.btnGoToDateStartTask.Text = "Найти в календаре";
            this.btnGoToDateStartTask.UseVisualStyleBackColor = true;
            this.btnGoToDateStartTask.Click += new System.EventHandler(this.button1_Click);
            // 
            // fmTasks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 274);
            this.Controls.Add(this.btnGoToDateStartTask);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tBxTargetTask);
            this.Controls.Add(this.btnChangeTask);
            this.Controls.Add(this.btnDeleteTask);
            this.Controls.Add(this.btnNewTask);
            this.Controls.Add(this.lBxTasks);
            this.Name = "fmTasks";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "fmTasks";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fmTasks_FormClosed);
            this.Load += new System.EventHandler(this.fmTasks_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnChangeTask;
        private System.Windows.Forms.Button btnDeleteTask;
        private System.Windows.Forms.Button btnNewTask;
        private System.Windows.Forms.ListBox lBxTasks;
        private System.Windows.Forms.TextBox tBxTargetTask;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnGoToDateStartTask;
    }
}