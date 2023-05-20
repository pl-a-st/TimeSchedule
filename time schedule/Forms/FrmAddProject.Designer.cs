
namespace time_schedule
{
    partial class FrmAddProject
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
            this.butAddTask = new System.Windows.Forms.Button();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.butDelete = new System.Windows.Forms.Button();
            this.butUplift = new System.Windows.Forms.Button();
            this.butDown = new System.Windows.Forms.Button();
            this.butNum = new System.Windows.Forms.Button();
            this.butTaskName = new System.Windows.Forms.Button();
            this.butDateStart = new System.Windows.Forms.Button();
            this.butDateFinish = new System.Windows.Forms.Button();
            this.butWorkDaysCount = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // butAddTask
            // 
            this.butAddTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butAddTask.Location = new System.Drawing.Point(1139, 58);
            this.butAddTask.Name = "butAddTask";
            this.butAddTask.Size = new System.Drawing.Size(110, 31);
            this.butAddTask.TabIndex = 1;
            this.butAddTask.Text = "Добавить задачу";
            this.butAddTask.UseVisualStyleBackColor = true;
            this.butAddTask.Click += new System.EventHandler(this.butAddTask_Click);
            // 
            // pnlMain
            // 
            this.pnlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlMain.AutoScroll = true;
            this.pnlMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMain.Location = new System.Drawing.Point(25, 58);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1047, 584);
            this.pnlMain.TabIndex = 2;
            // 
            // butDelete
            // 
            this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDelete.Location = new System.Drawing.Point(1139, 95);
            this.butDelete.Name = "butDelete";
            this.butDelete.Size = new System.Drawing.Size(110, 31);
            this.butDelete.TabIndex = 1;
            this.butDelete.Text = "Удалить задачу";
            this.butDelete.UseVisualStyleBackColor = true;
            this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
            // 
            // butUplift
            // 
            this.butUplift.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butUplift.Location = new System.Drawing.Point(1087, 319);
            this.butUplift.Name = "butUplift";
            this.butUplift.Size = new System.Drawing.Size(29, 23);
            this.butUplift.TabIndex = 3;
            this.butUplift.Text = "▲";
            this.butUplift.UseVisualStyleBackColor = true;
            this.butUplift.Click += new System.EventHandler(this.butUplift_Click);
            // 
            // butDown
            // 
            this.butDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butDown.Location = new System.Drawing.Point(1087, 348);
            this.butDown.Name = "butDown";
            this.butDown.Size = new System.Drawing.Size(29, 23);
            this.butDown.TabIndex = 3;
            this.butDown.Text = "▼";
            this.butDown.UseVisualStyleBackColor = true;
            this.butDown.Click += new System.EventHandler(this.butDown_Click);
            // 
            // butNum
            // 
            this.butNum.Location = new System.Drawing.Point(23, 32);
            this.butNum.Name = "butNum";
            this.butNum.Size = new System.Drawing.Size(40, 23);
            this.butNum.TabIndex = 4;
            this.butNum.Text = "№";
            this.butNum.UseVisualStyleBackColor = true;
            // 
            // butTaskName
            // 
            this.butTaskName.Location = new System.Drawing.Point(66, 32);
            this.butTaskName.Name = "butTaskName";
            this.butTaskName.Size = new System.Drawing.Size(400, 23);
            this.butTaskName.TabIndex = 4;
            this.butTaskName.Text = "Задача";
            this.butTaskName.UseVisualStyleBackColor = true;
            // 
            // butDateStart
            // 
            this.butDateStart.Location = new System.Drawing.Point(468, 32);
            this.butDateStart.Name = "butDateStart";
            this.butDateStart.Size = new System.Drawing.Size(134, 23);
            this.butDateStart.TabIndex = 4;
            this.butDateStart.Text = "Начало";
            this.butDateStart.UseVisualStyleBackColor = true;
            // 
            // butDateFinish
            // 
            this.butDateFinish.Location = new System.Drawing.Point(604, 32);
            this.butDateFinish.Name = "butDateFinish";
            this.butDateFinish.Size = new System.Drawing.Size(134, 23);
            this.butDateFinish.TabIndex = 4;
            this.butDateFinish.Text = "Окончание";
            this.butDateFinish.UseVisualStyleBackColor = true;
            // 
            // butWorkDaysCount
            // 
            this.butWorkDaysCount.Location = new System.Drawing.Point(740, 32);
            this.butWorkDaysCount.Name = "butWorkDaysCount";
            this.butWorkDaysCount.Size = new System.Drawing.Size(72, 23);
            this.butWorkDaysCount.TabIndex = 4;
            this.butWorkDaysCount.Text = "Раб. дни";
            this.butWorkDaysCount.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(814, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Исполнитель";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1016, 32);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(53, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "После";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // FrmAddProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1261, 684);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.butWorkDaysCount);
            this.Controls.Add(this.butDateFinish);
            this.Controls.Add(this.butDateStart);
            this.Controls.Add(this.butTaskName);
            this.Controls.Add(this.butNum);
            this.Controls.Add(this.butDown);
            this.Controls.Add(this.butUplift);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.butDelete);
            this.Controls.Add(this.butAddTask);
            this.Name = "FrmAddProject";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.FrmAddProject_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button butAddTask;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Button butDelete;
        private System.Windows.Forms.Button butUplift;
        private System.Windows.Forms.Button butDown;
        private System.Windows.Forms.Button butNum;
        private System.Windows.Forms.Button butTaskName;
        private System.Windows.Forms.Button butDateStart;
        private System.Windows.Forms.Button butDateFinish;
        private System.Windows.Forms.Button butWorkDaysCount;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}