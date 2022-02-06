﻿namespace time_schedule
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
            this.cBxFilterByDate = new System.Windows.Forms.CheckBox();
            this.dTPFilterDateFinish = new System.Windows.Forms.DateTimePicker();
            this.tBxPerson = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tBxStatus = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tBxDateStart = new System.Windows.Forms.TextBox();
            this.tBxDateFinish = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnChangeTask
            // 
            this.btnChangeTask.Location = new System.Drawing.Point(471, 96);
            this.btnChangeTask.Name = "btnChangeTask";
            this.btnChangeTask.Size = new System.Drawing.Size(142, 29);
            this.btnChangeTask.TabIndex = 7;
            this.btnChangeTask.Text = "Изменить задачу";
            this.btnChangeTask.UseVisualStyleBackColor = true;
            this.btnChangeTask.Click += new System.EventHandler(this.btnChangeTask_Click);
            // 
            // btnDeleteTask
            // 
            this.btnDeleteTask.Location = new System.Drawing.Point(471, 61);
            this.btnDeleteTask.Name = "btnDeleteTask";
            this.btnDeleteTask.Size = new System.Drawing.Size(142, 29);
            this.btnDeleteTask.TabIndex = 6;
            this.btnDeleteTask.Text = "Удалить задачу";
            this.btnDeleteTask.UseVisualStyleBackColor = true;
            this.btnDeleteTask.Click += new System.EventHandler(this.btnDeleteTask_Click);
            // 
            // btnNewTask
            // 
            this.btnNewTask.Location = new System.Drawing.Point(471, 26);
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
            this.lBxTasks.Location = new System.Drawing.Point(12, 116);
            this.lBxTasks.Name = "lBxTasks";
            this.lBxTasks.Size = new System.Drawing.Size(445, 329);
            this.lBxTasks.TabIndex = 4;
            this.lBxTasks.SelectedIndexChanged += new System.EventHandler(this.lBxTasks_SelectedIndexChanged);
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
            this.btnGoToDateStartTask.Location = new System.Drawing.Point(471, 131);
            this.btnGoToDateStartTask.Name = "btnGoToDateStartTask";
            this.btnGoToDateStartTask.Size = new System.Drawing.Size(142, 28);
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
            this.groupBox1.Location = new System.Drawing.Point(47, 52);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 53);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Отфильтровать по датам";
            // 
            // cBxFilterByDate
            // 
            this.cBxFilterByDate.AutoSize = true;
            this.cBxFilterByDate.Location = new System.Drawing.Point(26, 76);
            this.cBxFilterByDate.Name = "cBxFilterByDate";
            this.cBxFilterByDate.Size = new System.Drawing.Size(15, 14);
            this.cBxFilterByDate.TabIndex = 12;
            this.cBxFilterByDate.UseVisualStyleBackColor = true;
            this.cBxFilterByDate.CheckedChanged += new System.EventHandler(this.cBxFilterByDate_CheckedChanged);
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
            // tBxPerson
            // 
            this.tBxPerson.Location = new System.Drawing.Point(12, 467);
            this.tBxPerson.Name = "tBxPerson";
            this.tBxPerson.ReadOnly = true;
            this.tBxPerson.Size = new System.Drawing.Size(186, 20);
            this.tBxPerson.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 451);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Исполнитель";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 496);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Дата начала";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(249, 496);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Дата окончания";
            // 
            // tBxStatus
            // 
            this.tBxStatus.Location = new System.Drawing.Point(252, 467);
            this.tBxStatus.Name = "tBxStatus";
            this.tBxStatus.ReadOnly = true;
            this.tBxStatus.Size = new System.Drawing.Size(205, 20);
            this.tBxStatus.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(252, 451);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Статус";
            // 
            // tBxDateStart
            // 
            this.tBxDateStart.Location = new System.Drawing.Point(12, 512);
            this.tBxDateStart.Name = "tBxDateStart";
            this.tBxDateStart.ReadOnly = true;
            this.tBxDateStart.Size = new System.Drawing.Size(186, 20);
            this.tBxDateStart.TabIndex = 18;
            // 
            // tBxDateFinish
            // 
            this.tBxDateFinish.Location = new System.Drawing.Point(252, 512);
            this.tBxDateFinish.Name = "tBxDateFinish";
            this.tBxDateFinish.ReadOnly = true;
            this.tBxDateFinish.Size = new System.Drawing.Size(204, 20);
            this.tBxDateFinish.TabIndex = 18;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(471, 275);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(142, 28);
            this.button1.TabIndex = 10;
            this.button1.Text = "Открыть в экселе";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // fmTasks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 554);
            this.Controls.Add(this.cBxFilterByDate);
            this.Controls.Add(this.tBxDateFinish);
            this.Controls.Add(this.tBxDateStart);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tBxStatus);
            this.Controls.Add(this.tBxPerson);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
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
        private System.Windows.Forms.Button button1;
    }
}