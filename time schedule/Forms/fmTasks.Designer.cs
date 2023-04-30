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
            this.dTPFilterDateStart = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dTPFilterDateFinish = new System.Windows.Forms.DateTimePicker();
            this.cBxFilterByDate = new System.Windows.Forms.CheckBox();
            this.tBxPerson = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tBxStatus = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tBxDateStart = new System.Windows.Forms.TextBox();
            this.tBxDateFinish = new System.Windows.Forms.TextBox();
            this.butOpenTaskInExcell = new System.Windows.Forms.Button();
            this.cBxFilterByStatus = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tBxStatusesFilter = new System.Windows.Forms.TextBox();
            this.btnFilter = new System.Windows.Forms.Button();
            this.cBxOverdue = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cBxNotStarted = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnChangeToSelectTasks = new System.Windows.Forms.Button();
            this.chkMultySelectTask = new System.Windows.Forms.CheckBox();
            this.butDeleteTaskInlBx = new System.Windows.Forms.Button();
            this.butAddProjectToSelectTasks = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnChangeTask
            // 
            this.btnChangeTask.Location = new System.Drawing.Point(481, 54);
            this.btnChangeTask.Name = "btnChangeTask";
            this.btnChangeTask.Size = new System.Drawing.Size(122, 29);
            this.btnChangeTask.TabIndex = 7;
            this.btnChangeTask.Text = "Изменить задачу";
            this.btnChangeTask.UseVisualStyleBackColor = true;
            this.btnChangeTask.Click += new System.EventHandler(this.btnChangeTask_Click);
            // 
            // btnDeleteTask
            // 
            this.btnDeleteTask.Location = new System.Drawing.Point(480, 169);
            this.btnDeleteTask.Name = "btnDeleteTask";
            this.btnDeleteTask.Size = new System.Drawing.Size(121, 29);
            this.btnDeleteTask.TabIndex = 6;
            this.btnDeleteTask.Text = "Удалить задачу";
            this.btnDeleteTask.UseVisualStyleBackColor = true;
            this.btnDeleteTask.Click += new System.EventHandler(this.btnDeleteTask_Click);
            // 
            // btnNewTask
            // 
            this.btnNewTask.Location = new System.Drawing.Point(481, 19);
            this.btnNewTask.Name = "btnNewTask";
            this.btnNewTask.Size = new System.Drawing.Size(122, 29);
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
            this.lBxTasks.Location = new System.Drawing.Point(12, 246);
            this.lBxTasks.Name = "lBxTasks";
            this.lBxTasks.Size = new System.Drawing.Size(462, 342);
            this.lBxTasks.TabIndex = 4;
            this.lBxTasks.SelectedIndexChanged += new System.EventHandler(this.lBxTasks_SelectedIndexChanged);
            // 
            // tBxTargetTask
            // 
            this.tBxTargetTask.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.tBxTargetTask.Location = new System.Drawing.Point(12, 217);
            this.tBxTargetTask.Name = "tBxTargetTask";
            this.tBxTargetTask.Size = new System.Drawing.Size(462, 20);
            this.tBxTargetTask.TabIndex = 8;
            this.tBxTargetTask.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 204);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Найти задачу";
            // 
            // btnGoToDateStartTask
            // 
            this.btnGoToDateStartTask.Location = new System.Drawing.Point(482, 89);
            this.btnGoToDateStartTask.Name = "btnGoToDateStartTask";
            this.btnGoToDateStartTask.Size = new System.Drawing.Size(122, 28);
            this.btnGoToDateStartTask.TabIndex = 10;
            this.btnGoToDateStartTask.Text = "Найти в календаре";
            this.btnGoToDateStartTask.UseVisualStyleBackColor = true;
            this.btnGoToDateStartTask.Click += new System.EventHandler(this.button1_Click);
            // 
            // dTPFilterDateStart
            // 
            this.dTPFilterDateStart.Enabled = false;
            this.dTPFilterDateStart.Location = new System.Drawing.Point(6, 19);
            this.dTPFilterDateStart.Name = "dTPFilterDateStart";
            this.dTPFilterDateStart.Size = new System.Drawing.Size(171, 20);
            this.dTPFilterDateStart.TabIndex = 11;
            this.dTPFilterDateStart.ValueChanged += new System.EventHandler(this.dTPFilterDateStart_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dTPFilterDateFinish);
            this.groupBox1.Controls.Add(this.dTPFilterDateStart);
            this.groupBox1.Location = new System.Drawing.Point(32, 19);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 53);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "По датам";
            // 
            // dTPFilterDateFinish
            // 
            this.dTPFilterDateFinish.Enabled = false;
            this.dTPFilterDateFinish.Location = new System.Drawing.Point(221, 18);
            this.dTPFilterDateFinish.Name = "dTPFilterDateFinish";
            this.dTPFilterDateFinish.Size = new System.Drawing.Size(182, 20);
            this.dTPFilterDateFinish.TabIndex = 11;
            this.dTPFilterDateFinish.ValueChanged += new System.EventHandler(this.dTPFilterDateFinish_ValueChanged);
            // 
            // cBxFilterByDate
            // 
            this.cBxFilterByDate.AutoSize = true;
            this.cBxFilterByDate.Location = new System.Drawing.Point(11, 43);
            this.cBxFilterByDate.Name = "cBxFilterByDate";
            this.cBxFilterByDate.Size = new System.Drawing.Size(15, 14);
            this.cBxFilterByDate.TabIndex = 12;
            this.cBxFilterByDate.UseVisualStyleBackColor = true;
            this.cBxFilterByDate.CheckedChanged += new System.EventHandler(this.cBxFilterByDate_CheckedChanged);
            // 
            // tBxPerson
            // 
            this.tBxPerson.Location = new System.Drawing.Point(13, 617);
            this.tBxPerson.Name = "tBxPerson";
            this.tBxPerson.ReadOnly = true;
            this.tBxPerson.Size = new System.Drawing.Size(186, 20);
            this.tBxPerson.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 601);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Исполнитель";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 646);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Дата начала";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(265, 646);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Дата окончания";
            // 
            // tBxStatus
            // 
            this.tBxStatus.Location = new System.Drawing.Point(268, 617);
            this.tBxStatus.Name = "tBxStatus";
            this.tBxStatus.ReadOnly = true;
            this.tBxStatus.Size = new System.Drawing.Size(205, 20);
            this.tBxStatus.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(268, 601);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Статус";
            // 
            // tBxDateStart
            // 
            this.tBxDateStart.Location = new System.Drawing.Point(13, 662);
            this.tBxDateStart.Name = "tBxDateStart";
            this.tBxDateStart.ReadOnly = true;
            this.tBxDateStart.Size = new System.Drawing.Size(186, 20);
            this.tBxDateStart.TabIndex = 18;
            // 
            // tBxDateFinish
            // 
            this.tBxDateFinish.Location = new System.Drawing.Point(268, 662);
            this.tBxDateFinish.Name = "tBxDateFinish";
            this.tBxDateFinish.ReadOnly = true;
            this.tBxDateFinish.Size = new System.Drawing.Size(204, 20);
            this.tBxDateFinish.TabIndex = 18;
            // 
            // butOpenTaskInExcell
            // 
            this.butOpenTaskInExcell.Location = new System.Drawing.Point(480, 343);
            this.butOpenTaskInExcell.Name = "butOpenTaskInExcell";
            this.butOpenTaskInExcell.Size = new System.Drawing.Size(121, 48);
            this.butOpenTaskInExcell.TabIndex = 10;
            this.butOpenTaskInExcell.Text = "Открыть в Excell все задачи списка";
            this.butOpenTaskInExcell.UseVisualStyleBackColor = true;
            this.butOpenTaskInExcell.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // cBxFilterByStatus
            // 
            this.cBxFilterByStatus.AutoSize = true;
            this.cBxFilterByStatus.Location = new System.Drawing.Point(11, 98);
            this.cBxFilterByStatus.Name = "cBxFilterByStatus";
            this.cBxFilterByStatus.Size = new System.Drawing.Size(15, 14);
            this.cBxFilterByStatus.TabIndex = 12;
            this.cBxFilterByStatus.UseVisualStyleBackColor = true;
            this.cBxFilterByStatus.CheckedChanged += new System.EventHandler(this.cBxFilterByStatus_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tBxStatusesFilter);
            this.groupBox2.Controls.Add(this.btnFilter);
            this.groupBox2.Location = new System.Drawing.Point(32, 78);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(410, 48);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "По статусу";
            // 
            // tBxStatusesFilter
            // 
            this.tBxStatusesFilter.Enabled = false;
            this.tBxStatusesFilter.Location = new System.Drawing.Point(6, 20);
            this.tBxStatusesFilter.Name = "tBxStatusesFilter";
            this.tBxStatusesFilter.ReadOnly = true;
            this.tBxStatusesFilter.Size = new System.Drawing.Size(365, 20);
            this.tBxStatusesFilter.TabIndex = 21;
            // 
            // btnFilter
            // 
            this.btnFilter.BackgroundImage = global::time_schedule.Properties.Resources.img_369387__2_;
            this.btnFilter.Enabled = false;
            this.btnFilter.Image = global::time_schedule.Properties.Resources.kisspng_computer_icons_electronic_filter_low_pass_filter_5af607c33d0303_5914893915260732832499;
            this.btnFilter.Location = new System.Drawing.Point(377, 9);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(27, 33);
            this.btnFilter.TabIndex = 20;
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // cBxOverdue
            // 
            this.cBxOverdue.AutoSize = true;
            this.cBxOverdue.Location = new System.Drawing.Point(11, 22);
            this.cBxOverdue.Name = "cBxOverdue";
            this.cBxOverdue.Size = new System.Drawing.Size(15, 14);
            this.cBxOverdue.TabIndex = 12;
            this.cBxOverdue.UseVisualStyleBackColor = true;
            this.cBxOverdue.CheckedChanged += new System.EventHandler(this.cBxOverdue_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(36, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "просроченные";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // cBxNotStarted
            // 
            this.cBxNotStarted.AutoSize = true;
            this.cBxNotStarted.Location = new System.Drawing.Point(153, 22);
            this.cBxNotStarted.Name = "cBxNotStarted";
            this.cBxNotStarted.Size = new System.Drawing.Size(15, 14);
            this.cBxNotStarted.TabIndex = 12;
            this.cBxNotStarted.UseVisualStyleBackColor = true;
            this.cBxNotStarted.CheckedChanged += new System.EventHandler(this.cBxNotStarted_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(174, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(123, 13);
            this.label7.TabIndex = 21;
            this.label7.Text = "не начатые на сегодня";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.cBxOverdue);
            this.groupBox3.Controls.Add(this.cBxNotStarted);
            this.groupBox3.Location = new System.Drawing.Point(13, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(461, 43);
            this.groupBox3.TabIndex = 22;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Показать только";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.groupBox1);
            this.groupBox4.Controls.Add(this.cBxFilterByDate);
            this.groupBox4.Controls.Add(this.groupBox2);
            this.groupBox4.Controls.Add(this.cBxFilterByStatus);
            this.groupBox4.Location = new System.Drawing.Point(13, 61);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(461, 137);
            this.groupBox4.TabIndex = 23;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Отфильтровать";
            // 
            // btnChangeToSelectTasks
            // 
            this.btnChangeToSelectTasks.Location = new System.Drawing.Point(480, 284);
            this.btnChangeToSelectTasks.Name = "btnChangeToSelectTasks";
            this.btnChangeToSelectTasks.Size = new System.Drawing.Size(121, 48);
            this.btnChangeToSelectTasks.TabIndex = 10;
            this.btnChangeToSelectTasks.Text = "Изменить все задачи списка";
            this.btnChangeToSelectTasks.UseVisualStyleBackColor = true;
            this.btnChangeToSelectTasks.Click += new System.EventHandler(this.ChangeSelectTasks_Click);
            // 
            // chkMultySelectTask
            // 
            this.chkMultySelectTask.Location = new System.Drawing.Point(482, 246);
            this.chkMultySelectTask.Name = "chkMultySelectTask";
            this.chkMultySelectTask.Size = new System.Drawing.Size(121, 32);
            this.chkMultySelectTask.TabIndex = 24;
            this.chkMultySelectTask.Text = "Выделение  нескольких задач";
            this.chkMultySelectTask.UseVisualStyleBackColor = true;
            this.chkMultySelectTask.CheckedChanged += new System.EventHandler(this.chkMultySelectTask_CheckedChanged);
            // 
            // butDeleteTaskInlBx
            // 
            this.butDeleteTaskInlBx.Location = new System.Drawing.Point(481, 542);
            this.butDeleteTaskInlBx.Name = "butDeleteTaskInlBx";
            this.butDeleteTaskInlBx.Size = new System.Drawing.Size(121, 46);
            this.butDeleteTaskInlBx.TabIndex = 25;
            this.butDeleteTaskInlBx.Text = "Удалить все задачи списка";
            this.butDeleteTaskInlBx.UseVisualStyleBackColor = true;
            this.butDeleteTaskInlBx.Click += new System.EventHandler(this.butDeleteTaskInlBx_Click);
            // 
            // butAddProjectToSelectTasks
            // 
            this.butAddProjectToSelectTasks.Location = new System.Drawing.Point(480, 402);
            this.butAddProjectToSelectTasks.Name = "butAddProjectToSelectTasks";
            this.butAddProjectToSelectTasks.Size = new System.Drawing.Size(122, 48);
            this.butAddProjectToSelectTasks.TabIndex = 26;
            this.butAddProjectToSelectTasks.Text = "Добавить проекты всем задачам списка";
            this.butAddProjectToSelectTasks.UseVisualStyleBackColor = true;
            this.butAddProjectToSelectTasks.Click += new System.EventHandler(this.butAddProjectToSelectTasks_Click);
            // 
            // fmTasks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 687);
            this.Controls.Add(this.butAddProjectToSelectTasks);
            this.Controls.Add(this.butDeleteTaskInlBx);
            this.Controls.Add(this.chkMultySelectTask);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.tBxDateFinish);
            this.Controls.Add(this.tBxDateStart);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tBxStatus);
            this.Controls.Add(this.tBxPerson);
            this.Controls.Add(this.btnChangeToSelectTasks);
            this.Controls.Add(this.butOpenTaskInExcell);
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
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
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
        private System.Windows.Forms.DateTimePicker dTPFilterDateStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cBxFilterByDate;
        private System.Windows.Forms.DateTimePicker dTPFilterDateFinish;
        private System.Windows.Forms.TextBox tBxPerson;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tBxStatus;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tBxDateStart;
        private System.Windows.Forms.TextBox tBxDateFinish;
        private System.Windows.Forms.Button butOpenTaskInExcell;
        private System.Windows.Forms.CheckBox cBxFilterByStatus;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.TextBox tBxStatusesFilter;
        private System.Windows.Forms.CheckBox cBxOverdue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cBxNotStarted;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnChangeToSelectTasks;
        private System.Windows.Forms.CheckBox chkMultySelectTask;
        private System.Windows.Forms.Button butDeleteTaskInlBx;
        private System.Windows.Forms.Button butAddProjectToSelectTasks;
    }
}