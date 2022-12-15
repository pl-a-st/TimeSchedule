namespace time_schedule
{
    partial class fmAddChangeTask
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmBxPerson = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tBxTaskName = new System.Windows.Forms.TextBox();
            this.dTmTaskDateStart = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dTmTaskDateFinish = new System.Windows.Forms.DateTimePicker();
            this.nUpDnCounWorkDay = new System.Windows.Forms.NumericUpDown();
            this.rBnWorksDay = new System.Windows.Forms.RadioButton();
            this.rBnDayFinish = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rBnDayStart = new System.Windows.Forms.RadioButton();
            this.rBnPreviousTask = new System.Windows.Forms.RadioButton();
            this.btnPreviousTask = new System.Windows.Forms.Button();
            this.tBxPreviousTask = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.nUpDnPreviousTask = new System.Windows.Forms.NumericUpDown();
            this.btnCreateTask = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.bTnColor = new System.Windows.Forms.Button();
            this.cmBxTaskStatus = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.nUpDnTaskNumber = new System.Windows.Forms.NumericUpDown();
            this.lBlTaskNum = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.nUpDnPrioirity = new System.Windows.Forms.NumericUpDown();
            this.addPerson = new System.Windows.Forms.Button();
            this.btnCopyAnotherProject = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.tBxProjects = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnCounWorkDay)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnPreviousTask)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnTaskNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnPrioirity)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Исполнитель";
            // 
            // cmBxPerson
            // 
            this.cmBxPerson.FormattingEnabled = true;
            this.cmBxPerson.Location = new System.Drawing.Point(12, 108);
            this.cmBxPerson.Name = "cmBxPerson";
            this.cmBxPerson.Size = new System.Drawing.Size(239, 21);
            this.cmBxPerson.TabIndex = 1;
            this.cmBxPerson.SelectedIndexChanged += new System.EventHandler(this.cmBxPerson_SelectedIndexChanged);
            this.cmBxPerson.Click += new System.EventHandler(this.cmBxPerson_Click);
            this.cmBxPerson.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cmBxPerson_KeyUp);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Наименование работ";
            // 
            // tBxTaskName
            // 
            this.tBxTaskName.Location = new System.Drawing.Point(12, 67);
            this.tBxTaskName.Name = "tBxTaskName";
            this.tBxTaskName.Size = new System.Drawing.Size(269, 20);
            this.tBxTaskName.TabIndex = 3;
            // 
            // dTmTaskDateStart
            // 
            this.dTmTaskDateStart.Location = new System.Drawing.Point(6, 123);
            this.dTmTaskDateStart.Name = "dTmTaskDateStart";
            this.dTmTaskDateStart.Size = new System.Drawing.Size(143, 20);
            this.dTmTaskDateStart.TabIndex = 4;
            this.dTmTaskDateStart.ValueChanged += new System.EventHandler(this.dTTaskDateStart_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Дата начала";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Дата завершения";
            // 
            // dTmTaskDateFinish
            // 
            this.dTmTaskDateFinish.Location = new System.Drawing.Point(6, 123);
            this.dTmTaskDateFinish.Name = "dTmTaskDateFinish";
            this.dTmTaskDateFinish.Size = new System.Drawing.Size(140, 20);
            this.dTmTaskDateFinish.TabIndex = 6;
            this.dTmTaskDateFinish.ValueChanged += new System.EventHandler(this.dTmTaskDateFinish_ValueChanged);
            // 
            // nUpDnCounWorkDay
            // 
            this.nUpDnCounWorkDay.Location = new System.Drawing.Point(6, 63);
            this.nUpDnCounWorkDay.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nUpDnCounWorkDay.Minimum = new decimal(new int[] {
            10000000,
            0,
            0,
            -2147483648});
            this.nUpDnCounWorkDay.Name = "nUpDnCounWorkDay";
            this.nUpDnCounWorkDay.Size = new System.Drawing.Size(140, 20);
            this.nUpDnCounWorkDay.TabIndex = 9;
            this.nUpDnCounWorkDay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nUpDnCounWorkDay.ValueChanged += new System.EventHandler(this.nUpDnCounWorkDay_ValueChanged);
            this.nUpDnCounWorkDay.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nUpDnCounWorkDay_KeyUp);
            // 
            // rBnWorksDay
            // 
            this.rBnWorksDay.AutoSize = true;
            this.rBnWorksDay.Location = new System.Drawing.Point(152, 64);
            this.rBnWorksDay.Name = "rBnWorksDay";
            this.rBnWorksDay.Size = new System.Drawing.Size(114, 17);
            this.rBnWorksDay.TabIndex = 10;
            this.rBnWorksDay.TabStop = true;
            this.rBnWorksDay.Text = "По рабочим дням";
            this.rBnWorksDay.UseVisualStyleBackColor = true;
            this.rBnWorksDay.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // rBnDayFinish
            // 
            this.rBnDayFinish.AutoSize = true;
            this.rBnDayFinish.Location = new System.Drawing.Point(152, 123);
            this.rBnDayFinish.Name = "rBnDayFinish";
            this.rBnDayFinish.Size = new System.Drawing.Size(136, 17);
            this.rBnDayFinish.TabIndex = 11;
            this.rBnDayFinish.TabStop = true;
            this.rBnDayFinish.Text = "По дате заверешения";
            this.rBnDayFinish.UseVisualStyleBackColor = true;
            this.rBnDayFinish.CheckedChanged += new System.EventHandler(this.rBnDayFinish_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Рабочие дни";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rBnDayStart);
            this.groupBox1.Controls.Add(this.rBnPreviousTask);
            this.groupBox1.Controls.Add(this.btnPreviousTask);
            this.groupBox1.Controls.Add(this.tBxPreviousTask);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dTmTaskDateStart);
            this.groupBox1.Controls.Add(this.nUpDnPreviousTask);
            this.groupBox1.Location = new System.Drawing.Point(12, 143);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(269, 148);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Начало работы";
            // 
            // rBnDayStart
            // 
            this.rBnDayStart.AutoSize = true;
            this.rBnDayStart.Location = new System.Drawing.Point(155, 123);
            this.rBnDayStart.Name = "rBnDayStart";
            this.rBnDayStart.Size = new System.Drawing.Size(103, 17);
            this.rBnDayStart.TabIndex = 17;
            this.rBnDayStart.TabStop = true;
            this.rBnDayStart.Text = "По дате начала";
            this.rBnDayStart.UseVisualStyleBackColor = true;
            this.rBnDayStart.CheckedChanged += new System.EventHandler(this.rBnDayStart_CheckedChanged);
            // 
            // rBnPreviousTask
            // 
            this.rBnPreviousTask.AutoSize = true;
            this.rBnPreviousTask.Location = new System.Drawing.Point(155, 46);
            this.rBnPreviousTask.Name = "rBnPreviousTask";
            this.rBnPreviousTask.Size = new System.Drawing.Size(77, 17);
            this.rBnPreviousTask.TabIndex = 16;
            this.rBnPreviousTask.TabStop = true;
            this.rBnPreviousTask.Text = "По задаче";
            this.rBnPreviousTask.UseVisualStyleBackColor = true;
            this.rBnPreviousTask.CheckedChanged += new System.EventHandler(this.rBtPreviousTask_CheckedChanged);
            // 
            // btnPreviousTask
            // 
            this.btnPreviousTask.Enabled = false;
            this.btnPreviousTask.Location = new System.Drawing.Point(235, 74);
            this.btnPreviousTask.Name = "btnPreviousTask";
            this.btnPreviousTask.Size = new System.Drawing.Size(28, 20);
            this.btnPreviousTask.TabIndex = 15;
            this.btnPreviousTask.Text = "...";
            this.btnPreviousTask.UseVisualStyleBackColor = true;
            this.btnPreviousTask.Click += new System.EventHandler(this.btnPreviousTask_Click);
            // 
            // tBxPreviousTask
            // 
            this.tBxPreviousTask.Enabled = false;
            this.tBxPreviousTask.Location = new System.Drawing.Point(6, 74);
            this.tBxPreviousTask.Name = "tBxPreviousTask";
            this.tBxPreviousTask.ReadOnly = true;
            this.tBxPreviousTask.Size = new System.Drawing.Size(217, 20);
            this.tBxPreviousTask.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "После задачи:";
            // 
            // nUpDnPreviousTask
            // 
            this.nUpDnPreviousTask.Enabled = false;
            this.nUpDnPreviousTask.Location = new System.Drawing.Point(6, 39);
            this.nUpDnPreviousTask.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nUpDnPreviousTask.Name = "nUpDnPreviousTask";
            this.nUpDnPreviousTask.ReadOnly = true;
            this.nUpDnPreviousTask.Size = new System.Drawing.Size(143, 20);
            this.nUpDnPreviousTask.TabIndex = 9;
            this.nUpDnPreviousTask.ValueChanged += new System.EventHandler(this.nUpDnCounWorkDay_ValueChanged);
            // 
            // btnCreateTask
            // 
            this.btnCreateTask.Location = new System.Drawing.Point(478, 60);
            this.btnCreateTask.Name = "btnCreateTask";
            this.btnCreateTask.Size = new System.Drawing.Size(95, 35);
            this.btnCreateTask.TabIndex = 14;
            this.btnCreateTask.Text = "Создать";
            this.btnCreateTask.UseVisualStyleBackColor = true;
            this.btnCreateTask.Click += new System.EventHandler(this.btnCreateChangeTask_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nUpDnCounWorkDay);
            this.groupBox2.Controls.Add(this.dTmTaskDateFinish);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.rBnWorksDay);
            this.groupBox2.Controls.Add(this.rBnDayFinish);
            this.groupBox2.Location = new System.Drawing.Point(287, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(286, 148);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Окончание работы";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(478, 101);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(95, 36);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.button1_Click);
            // 
            // bTnColor
            // 
            this.bTnColor.Location = new System.Drawing.Point(418, 60);
            this.bTnColor.Name = "bTnColor";
            this.bTnColor.Size = new System.Drawing.Size(54, 77);
            this.bTnColor.TabIndex = 16;
            this.bTnColor.Text = "Цвет";
            this.bTnColor.UseVisualStyleBackColor = true;
            this.bTnColor.Click += new System.EventHandler(this.bTnColor_Click);
            // 
            // cmBxTaskStatus
            // 
            this.cmBxTaskStatus.FormattingEnabled = true;
            this.cmBxTaskStatus.Location = new System.Drawing.Point(287, 108);
            this.cmBxTaskStatus.Name = "cmBxTaskStatus";
            this.cmBxTaskStatus.Size = new System.Drawing.Size(125, 21);
            this.cmBxTaskStatus.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(284, 92);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Статус";
            // 
            // nUpDnTaskNumber
            // 
            this.nUpDnTaskNumber.Enabled = false;
            this.nUpDnTaskNumber.Location = new System.Drawing.Point(12, 25);
            this.nUpDnTaskNumber.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nUpDnTaskNumber.Name = "nUpDnTaskNumber";
            this.nUpDnTaskNumber.ReadOnly = true;
            this.nUpDnTaskNumber.Size = new System.Drawing.Size(89, 20);
            this.nUpDnTaskNumber.TabIndex = 19;
            this.nUpDnTaskNumber.Visible = false;
            // 
            // lBlTaskNum
            // 
            this.lBlTaskNum.AutoSize = true;
            this.lBlTaskNum.Location = new System.Drawing.Point(9, 9);
            this.lBlTaskNum.Name = "lBlTaskNum";
            this.lBlTaskNum.Size = new System.Drawing.Size(57, 13);
            this.lBlTaskNum.TabIndex = 20;
            this.lBlTaskNum.Text = "Задача №";
            this.lBlTaskNum.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(284, 52);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Приоритет";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // nUpDnPrioirity
            // 
            this.nUpDnPrioirity.Location = new System.Drawing.Point(287, 68);
            this.nUpDnPrioirity.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nUpDnPrioirity.Name = "nUpDnPrioirity";
            this.nUpDnPrioirity.Size = new System.Drawing.Size(125, 20);
            this.nUpDnPrioirity.TabIndex = 23;
            this.nUpDnPrioirity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // addPerson
            // 
            this.addPerson.Location = new System.Drawing.Point(257, 106);
            this.addPerson.Name = "addPerson";
            this.addPerson.Size = new System.Drawing.Size(21, 23);
            this.addPerson.TabIndex = 24;
            this.addPerson.Text = "+";
            this.addPerson.UseVisualStyleBackColor = true;
            this.addPerson.Click += new System.EventHandler(this.addPerson_Click);
            // 
            // btnCopyAnotherProject
            // 
            this.btnCopyAnotherProject.Location = new System.Drawing.Point(203, 297);
            this.btnCopyAnotherProject.Name = "btnCopyAnotherProject";
            this.btnCopyAnotherProject.Size = new System.Drawing.Size(171, 23);
            this.btnCopyAnotherProject.TabIndex = 25;
            this.btnCopyAnotherProject.Text = " Копировать в другой файл";
            this.btnCopyAnotherProject.UseVisualStyleBackColor = true;
            this.btnCopyAnotherProject.Click += new System.EventHandler(this.btnCopyAnotherMainPath_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(126, 9);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(118, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Включение в проекты";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(543, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 20);
            this.button2.TabIndex = 19;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tBxProjects
            // 
            this.tBxProjects.Location = new System.Drawing.Point(129, 27);
            this.tBxProjects.Name = "tBxProjects";
            this.tBxProjects.ReadOnly = true;
            this.tBxProjects.Size = new System.Drawing.Size(408, 20);
            this.tBxProjects.TabIndex = 18;
            // 
            // fmAddChangeTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 331);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tBxProjects);
            this.Controls.Add(this.btnCopyAnotherProject);
            this.Controls.Add(this.addPerson);
            this.Controls.Add(this.nUpDnPrioirity);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lBlTaskNum);
            this.Controls.Add(this.nUpDnTaskNumber);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmBxTaskStatus);
            this.Controls.Add(this.bTnColor);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreateTask);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tBxTaskName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmBxPerson);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(601, 400);
            this.MinimumSize = new System.Drawing.Size(601, 349);
            this.Name = "fmAddChangeTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавление задачи";
            this.Activated += new System.EventHandler(this.fmAddChangeTask_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.fmAddTask_FormClosed);
            this.Load += new System.EventHandler(this.fmAddTask_Load);
            this.Shown += new System.EventHandler(this.fmAddChangeTask_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnCounWorkDay)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnPreviousTask)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnTaskNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnPrioirity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmBxPerson;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tBxTaskName;
        private System.Windows.Forms.DateTimePicker dTmTaskDateStart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dTmTaskDateFinish;
        private System.Windows.Forms.NumericUpDown nUpDnCounWorkDay;
        private System.Windows.Forms.RadioButton rBnWorksDay;
        private System.Windows.Forms.RadioButton rBnDayFinish;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rBnDayStart;
        private System.Windows.Forms.RadioButton rBnPreviousTask;
        private System.Windows.Forms.Button btnPreviousTask;
        private System.Windows.Forms.TextBox tBxPreviousTask;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nUpDnPreviousTask;
        private System.Windows.Forms.Button btnCreateTask;
        public void GhangeNamebtnCreateTask(string name)
        {
            btnCreateTask.Text = name;
        }
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button bTnColor;
        private System.Windows.Forms.ComboBox cmBxTaskStatus;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nUpDnTaskNumber;
        private System.Windows.Forms.Label lBlTaskNum;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nUpDnPrioirity;
        private System.Windows.Forms.Button addPerson;
        private System.Windows.Forms.Button btnCopyAnotherProject;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tBxProjects;
    }
}