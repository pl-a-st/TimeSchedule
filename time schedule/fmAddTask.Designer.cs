namespace time_schedule
{
    partial class fmAddTask
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
            this.button1 = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.bTnColor = new System.Windows.Forms.Button();
            this.cmBxTaskStatus = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnCounWorkDay)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnPreviousTask)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Исполнитель";
            // 
            // cmBxPerson
            // 
            this.cmBxPerson.FormattingEnabled = true;
            this.cmBxPerson.Location = new System.Drawing.Point(12, 66);
            this.cmBxPerson.Name = "cmBxPerson";
            this.cmBxPerson.Size = new System.Drawing.Size(269, 21);
            this.cmBxPerson.TabIndex = 1;
            this.cmBxPerson.SelectedIndexChanged += new System.EventHandler(this.cmBxPerson_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Наименование работ";
            // 
            // tBxTaskName
            // 
            this.tBxTaskName.Location = new System.Drawing.Point(12, 25);
            this.tBxTaskName.Name = "tBxTaskName";
            this.tBxTaskName.Size = new System.Drawing.Size(400, 20);
            this.tBxTaskName.TabIndex = 3;
            // 
            // dTmTaskDateStart
            // 
            this.dTmTaskDateStart.Location = new System.Drawing.Point(6, 116);
            this.dTmTaskDateStart.Name = "dTmTaskDateStart";
            this.dTmTaskDateStart.Size = new System.Drawing.Size(143, 20);
            this.dTmTaskDateStart.TabIndex = 4;
            this.dTmTaskDateStart.ValueChanged += new System.EventHandler(this.dTTaskDateStart_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Дата начала";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Дата завершения";
            // 
            // dTmTaskDateFinish
            // 
            this.dTmTaskDateFinish.Location = new System.Drawing.Point(6, 116);
            this.dTmTaskDateFinish.Name = "dTmTaskDateFinish";
            this.dTmTaskDateFinish.Size = new System.Drawing.Size(140, 20);
            this.dTmTaskDateFinish.TabIndex = 6;
            this.dTmTaskDateFinish.ValueChanged += new System.EventHandler(this.dTmTaskDateFinish_ValueChanged);
            // 
            // nUpDnCounWorkDay
            // 
            this.nUpDnCounWorkDay.Location = new System.Drawing.Point(6, 56);
            this.nUpDnCounWorkDay.Name = "nUpDnCounWorkDay";
            this.nUpDnCounWorkDay.Size = new System.Drawing.Size(140, 20);
            this.nUpDnCounWorkDay.TabIndex = 9;
            this.nUpDnCounWorkDay.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nUpDnCounWorkDay.ValueChanged += new System.EventHandler(this.nUpDnCounWorkDay_ValueChanged);
            // 
            // rBnWorksDay
            // 
            this.rBnWorksDay.AutoSize = true;
            this.rBnWorksDay.Location = new System.Drawing.Point(152, 57);
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
            this.rBnDayFinish.Location = new System.Drawing.Point(152, 116);
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
            this.label5.Location = new System.Drawing.Point(6, 42);
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
            this.groupBox1.Location = new System.Drawing.Point(12, 101);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(269, 148);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Начало работы";
            // 
            // rBnDayStart
            // 
            this.rBnDayStart.AutoSize = true;
            this.rBnDayStart.Location = new System.Drawing.Point(155, 116);
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
            this.rBnPreviousTask.Location = new System.Drawing.Point(155, 39);
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
            this.btnPreviousTask.Location = new System.Drawing.Point(235, 67);
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
            this.tBxPreviousTask.Location = new System.Drawing.Point(6, 67);
            this.tBxPreviousTask.Name = "tBxPreviousTask";
            this.tBxPreviousTask.ReadOnly = true;
            this.tBxPreviousTask.Size = new System.Drawing.Size(217, 20);
            this.tBxPreviousTask.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Задача";
            // 
            // nUpDnPreviousTask
            // 
            this.nUpDnPreviousTask.Enabled = false;
            this.nUpDnPreviousTask.Location = new System.Drawing.Point(6, 39);
            this.nUpDnPreviousTask.Name = "nUpDnPreviousTask";
            this.nUpDnPreviousTask.ReadOnly = true;
            this.nUpDnPreviousTask.Size = new System.Drawing.Size(143, 20);
            this.nUpDnPreviousTask.TabIndex = 9;
            this.nUpDnPreviousTask.ValueChanged += new System.EventHandler(this.nUpDnCounWorkDay_ValueChanged);
            // 
            // btnCreateTask
            // 
            this.btnCreateTask.Location = new System.Drawing.Point(478, 18);
            this.btnCreateTask.Name = "btnCreateTask";
            this.btnCreateTask.Size = new System.Drawing.Size(95, 35);
            this.btnCreateTask.TabIndex = 14;
            this.btnCreateTask.Text = "Создать";
            this.btnCreateTask.UseVisualStyleBackColor = true;
            this.btnCreateTask.Click += new System.EventHandler(this.btnCreateTask_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.nUpDnCounWorkDay);
            this.groupBox2.Controls.Add(this.dTmTaskDateFinish);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.rBnWorksDay);
            this.groupBox2.Controls.Add(this.rBnDayFinish);
            this.groupBox2.Location = new System.Drawing.Point(287, 101);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(286, 148);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Окончание работы";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(478, 59);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 36);
            this.button1.TabIndex = 14;
            this.button1.Text = "Отмена";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bTnColor
            // 
            this.bTnColor.Location = new System.Drawing.Point(418, 18);
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
            this.cmBxTaskStatus.Location = new System.Drawing.Point(287, 66);
            this.cmBxTaskStatus.Name = "cmBxTaskStatus";
            this.cmBxTaskStatus.Size = new System.Drawing.Size(125, 21);
            this.cmBxTaskStatus.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(284, 50);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Статус";
            // 
            // fmAddTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 303);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmBxTaskStatus);
            this.Controls.Add(this.bTnColor);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnCreateTask);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tBxTaskName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmBxPerson);
            this.Controls.Add(this.label1);
            this.Name = "fmAddTask";
            this.Text = "Добавление задачи";
            this.Load += new System.EventHandler(this.fmAddTask_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnCounWorkDay)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nUpDnPreviousTask)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button bTnColor;
        private System.Windows.Forms.ComboBox cmBxTaskStatus;
        private System.Windows.Forms.Label label7;
    }
}