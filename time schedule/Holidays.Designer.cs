namespace time_schedule
{
    partial class Holidays
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
            this.LBxHolidays = new System.Windows.Forms.ListBox();
            this.btnSetListDays = new System.Windows.Forms.Button();
            this.dTPListDays = new System.Windows.Forms.DateTimePicker();
            this.tXBListDays = new System.Windows.Forms.TextBox();
            this.btnSetIntervalDays = new System.Windows.Forms.Button();
            this.dTPFirstInTheInterval = new System.Windows.Forms.DateTimePicker();
            this.dTPSecondInTheInterval = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dTPOnceDay = new System.Windows.Forms.DateTimePicker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.RemoveDay = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // LBxHolidays
            // 
            this.LBxHolidays.FormattingEnabled = true;
            this.LBxHolidays.Location = new System.Drawing.Point(13, 29);
            this.LBxHolidays.Name = "LBxHolidays";
            this.LBxHolidays.Size = new System.Drawing.Size(95, 238);
            this.LBxHolidays.TabIndex = 0;
            // 
            // btnSetListDays
            // 
            this.btnSetListDays.Location = new System.Drawing.Point(142, 64);
            this.btnSetListDays.Name = "btnSetListDays";
            this.btnSetListDays.Size = new System.Drawing.Size(282, 27);
            this.btnSetListDays.TabIndex = 6;
            this.btnSetListDays.Text = "Отправить";
            this.btnSetListDays.UseVisualStyleBackColor = true;
            // 
            // dTPListDays
            // 
            this.dTPListDays.Location = new System.Drawing.Point(6, 68);
            this.dTPListDays.Name = "dTPListDays";
            this.dTPListDays.Size = new System.Drawing.Size(130, 20);
            this.dTPListDays.TabIndex = 5;
            // 
            // tXBListDays
            // 
            this.tXBListDays.Location = new System.Drawing.Point(6, 38);
            this.tXBListDays.Name = "tXBListDays";
            this.tXBListDays.Size = new System.Drawing.Size(354, 20);
            this.tXBListDays.TabIndex = 4;
            // 
            // btnSetIntervalDays
            // 
            this.btnSetIntervalDays.Location = new System.Drawing.Point(294, 15);
            this.btnSetIntervalDays.Name = "btnSetIntervalDays";
            this.btnSetIntervalDays.Size = new System.Drawing.Size(130, 28);
            this.btnSetIntervalDays.TabIndex = 9;
            this.btnSetIntervalDays.Text = "Отправить";
            this.btnSetIntervalDays.UseVisualStyleBackColor = true;
            // 
            // dTPFirstInTheInterval
            // 
            this.dTPFirstInTheInterval.Location = new System.Drawing.Point(6, 19);
            this.dTPFirstInTheInterval.Name = "dTPFirstInTheInterval";
            this.dTPFirstInTheInterval.Size = new System.Drawing.Size(130, 20);
            this.dTPFirstInTheInterval.TabIndex = 8;
            // 
            // dTPSecondInTheInterval
            // 
            this.dTPSecondInTheInterval.Location = new System.Drawing.Point(158, 19);
            this.dTPSecondInTheInterval.Name = "dTPSecondInTheInterval";
            this.dTPSecondInTheInterval.Size = new System.Drawing.Size(130, 20);
            this.dTPSecondInTheInterval.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dTPOnceDay);
            this.groupBox1.Location = new System.Drawing.Point(114, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(431, 50);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Отправить дату";
            // 
            // dTPOnceDay
            // 
            this.dTPOnceDay.Location = new System.Drawing.Point(6, 19);
            this.dTPOnceDay.Name = "dTPOnceDay";
            this.dTPOnceDay.Size = new System.Drawing.Size(130, 20);
            this.dTPOnceDay.TabIndex = 12;
            this.dTPOnceDay.ValueChanged += new System.EventHandler(this.dTPOnceDay_ValueChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.tXBListDays);
            this.groupBox2.Controls.Add(this.dTPListDays);
            this.groupBox2.Controls.Add(this.btnSetListDays);
            this.groupBox2.Location = new System.Drawing.Point(114, 72);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(431, 100);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Отправить список дат";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.dTPFirstInTheInterval);
            this.groupBox3.Controls.Add(this.btnSetIntervalDays);
            this.groupBox3.Controls.Add(this.dTPSecondInTheInterval);
            this.groupBox3.Location = new System.Drawing.Point(114, 178);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(431, 51);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Отправить интервал дат";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Даты через \", \"";
            // 
            // RemoveDay
            // 
            this.RemoveDay.Location = new System.Drawing.Point(114, 235);
            this.RemoveDay.Name = "RemoveDay";
            this.RemoveDay.Size = new System.Drawing.Size(92, 29);
            this.RemoveDay.TabIndex = 14;
            this.RemoveDay.Text = "Удалить дату";
            this.RemoveDay.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Нерабочие дни";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(142, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "-";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(366, 35);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(26, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "<-";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(398, 35);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(26, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "X";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(183, 11);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(92, 27);
            this.btnApply.TabIndex = 16;
            this.btnApply.Text = "Применить";
            this.btnApply.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnCancel);
            this.groupBox4.Controls.Add(this.btnApply);
            this.groupBox4.Location = new System.Drawing.Point(13, 273);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(532, 44);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(281, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(92, 27);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // Holidays
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 335);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RemoveDay);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.LBxHolidays);
            this.Name = "Holidays";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Нерабочие дни";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LBxHolidays;
        private System.Windows.Forms.Button btnSetListDays;
        private System.Windows.Forms.DateTimePicker dTPListDays;
        private System.Windows.Forms.TextBox tXBListDays;
        private System.Windows.Forms.Button btnSetIntervalDays;
        private System.Windows.Forms.DateTimePicker dTPFirstInTheInterval;
        private System.Windows.Forms.DateTimePicker dTPSecondInTheInterval;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DateTimePicker dTPOnceDay;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button RemoveDay;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnCancel;
    }
}