namespace time_schedule
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.plMain = new System.Windows.Forms.Panel();
            this.plForDate = new System.Windows.Forms.Panel();
            this.btnTask = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.сенитьПользователяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.проектToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPersons = new System.Windows.Forms.ToolStripMenuItem();
            this.нерабочиеДниToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plPeraonButton = new System.Windows.Forms.Panel();
            this.bTnToDay = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.pBForLine = new System.Windows.Forms.PictureBox();
            this.plMain.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBForLine)).BeginInit();
            this.SuspendLayout();
            // 
            // plMain
            // 
            this.plMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plMain.AutoScroll = true;
            this.plMain.Controls.Add(this.pBForLine);
            this.plMain.Location = new System.Drawing.Point(114, 46);
            this.plMain.Name = "plMain";
            this.plMain.Size = new System.Drawing.Size(777, 474);
            this.plMain.TabIndex = 0;
            this.plMain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollChange);
            this.plMain.MouseEnter += new System.EventHandler(this.plMain_MouseEnter);
            this.plMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.plMain_MouseMove);
            // 
            // plForDate
            // 
            this.plForDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plForDate.AutoScroll = true;
            this.plForDate.Location = new System.Drawing.Point(114, 12);
            this.plForDate.Name = "plForDate";
            this.plForDate.Size = new System.Drawing.Size(758, 52);
            this.plForDate.TabIndex = 1;
            // 
            // btnTask
            // 
            this.btnTask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTask.Cursor = System.Windows.Forms.Cursors.AppStarting;
            this.btnTask.Location = new System.Drawing.Point(12, 535);
            this.btnTask.Name = "btnTask";
            this.btnTask.Size = new System.Drawing.Size(111, 23);
            this.btnTask.TabIndex = 3;
            this.btnTask.Text = "Задачи";
            this.btnTask.UseVisualStyleBackColor = true;
            this.btnTask.Click += new System.EventHandler(this.btnTask_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сенитьПользователяToolStripMenuItem,
            this.проектToolStripMenuItem,
            this.menuPersons,
            this.нерабочиеДниToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(79, 20);
            this.toolStripMenuItem1.Text = "Настройки";
            // 
            // сенитьПользователяToolStripMenuItem
            // 
            this.сенитьПользователяToolStripMenuItem.Name = "сенитьПользователяToolStripMenuItem";
            this.сенитьПользователяToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.сенитьПользователяToolStripMenuItem.Text = "Сенить пользователя";
            // 
            // проектToolStripMenuItem
            // 
            this.проектToolStripMenuItem.Name = "проектToolStripMenuItem";
            this.проектToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.проектToolStripMenuItem.Text = "Проект";
            this.проектToolStripMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem_Click);
            // 
            // menuPersons
            // 
            this.menuPersons.Name = "menuPersons";
            this.menuPersons.Size = new System.Drawing.Size(191, 22);
            this.menuPersons.Text = "Исполнители";
            this.menuPersons.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // нерабочиеДниToolStripMenuItem
            // 
            this.нерабочиеДниToolStripMenuItem.Name = "нерабочиеДниToolStripMenuItem";
            this.нерабочиеДниToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.нерабочиеДниToolStripMenuItem.Text = "Нерабочие дни";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(903, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // plPeraonButton
            // 
            this.plPeraonButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.plPeraonButton.AutoScroll = true;
            this.plPeraonButton.Location = new System.Drawing.Point(12, 46);
            this.plPeraonButton.Name = "plPeraonButton";
            this.plPeraonButton.Size = new System.Drawing.Size(200, 454);
            this.plPeraonButton.TabIndex = 4;
            // 
            // bTnToDay
            // 
            this.bTnToDay.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bTnToDay.Location = new System.Drawing.Point(435, 535);
            this.bTnToDay.Name = "bTnToDay";
            this.bTnToDay.Size = new System.Drawing.Size(75, 23);
            this.bTnToDay.TabIndex = 5;
            this.bTnToDay.Text = "Сегодня";
            this.bTnToDay.UseVisualStyleBackColor = true;
            this.bTnToDay.Click += new System.EventHandler(this.button3_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(242, 550);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 6;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(591, 549);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 7;
            this.button4.Text = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // pBForLine
            // 
            this.pBForLine.BackColor = System.Drawing.SystemColors.Window;
            this.pBForLine.Location = new System.Drawing.Point(0, 0);
            this.pBForLine.Name = "pBForLine";
            this.pBForLine.Size = new System.Drawing.Size(100, 50);
            this.pBForLine.TabIndex = 0;
            this.pBForLine.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 658);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.bTnToDay);
            this.Controls.Add(this.btnTask);
            this.Controls.Add(this.plMain);
            this.Controls.Add(this.plForDate);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.plPeraonButton);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.plMain.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBForLine)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel plMain;
        private System.Windows.Forms.Panel plForDate;
        private System.Windows.Forms.Button btnTask;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem сенитьПользователяToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem проектToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuPersons;
        private System.Windows.Forms.ToolStripMenuItem нерабочиеДниToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Panel plPeraonButton;
        private System.Windows.Forms.Button bTnToDay;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.PictureBox pBForLine;
    }
}

