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
            this.pBForLine = new System.Windows.Forms.PictureBox();
            this.plForDate = new System.Windows.Forms.Panel();
            this.btnTask = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.сенитьПользователяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.проектToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.задачиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPersons = new System.Windows.Forms.ToolStripMenuItem();
            this.нерабочиеДниToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plPersonButton = new System.Windows.Forms.Panel();
            this.bTnToDay = new System.Windows.Forms.Button();
            this.addFiveDays = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.minusFiveDay = new System.Windows.Forms.Button();
            this.plMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pBForLine)).BeginInit();
            this.menuStrip1.SuspendLayout();
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
            this.plMain.Size = new System.Drawing.Size(777, 534);
            this.plMain.TabIndex = 0;
            this.plMain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollChange);
            this.plMain.MouseEnter += new System.EventHandler(this.plMain_MouseEnter);
            this.plMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.plMain_MouseMove);
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
            // plForDate
            // 
            this.plForDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plForDate.AutoScroll = true;
            this.plForDate.Enabled = false;
            this.plForDate.Location = new System.Drawing.Point(114, 12);
            this.plForDate.Name = "plForDate";
            this.plForDate.Size = new System.Drawing.Size(758, 52);
            this.plForDate.TabIndex = 1;
            // 
            // btnTask
            // 
            this.btnTask.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnTask.Cursor = System.Windows.Forms.Cursors.AppStarting;
            this.btnTask.Location = new System.Drawing.Point(503, 597);
            this.btnTask.Name = "btnTask";
            this.btnTask.Size = new System.Drawing.Size(110, 49);
            this.btnTask.TabIndex = 3;
            this.btnTask.Text = "Новая задача";
            this.btnTask.UseVisualStyleBackColor = true;
            this.btnTask.Click += new System.EventHandler(this.btnTask_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сенитьПользователяToolStripMenuItem,
            this.проектToolStripMenuItem,
            this.задачиToolStripMenuItem,
            this.menuPersons,
            this.нерабочиеДниToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(53, 20);
            this.toolStripMenuItem1.Text = "Меню";
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
            // задачиToolStripMenuItem
            // 
            this.задачиToolStripMenuItem.Name = "задачиToolStripMenuItem";
            this.задачиToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.задачиToolStripMenuItem.Text = "Задачи";
            this.задачиToolStripMenuItem.Click += new System.EventHandler(this.задачиToolStripMenuItem_Click);
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
            // plPersonButton
            // 
            this.plPersonButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.plPersonButton.AutoScroll = true;
            this.plPersonButton.Location = new System.Drawing.Point(12, 46);
            this.plPersonButton.Name = "plPersonButton";
            this.plPersonButton.Size = new System.Drawing.Size(200, 514);
            this.plPersonButton.TabIndex = 4;
            // 
            // bTnToDay
            // 
            this.bTnToDay.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bTnToDay.Location = new System.Drawing.Point(374, 597);
            this.bTnToDay.Name = "bTnToDay";
            this.bTnToDay.Size = new System.Drawing.Size(75, 23);
            this.bTnToDay.TabIndex = 5;
            this.bTnToDay.Text = "Сегодня";
            this.bTnToDay.UseVisualStyleBackColor = true;
            this.bTnToDay.Click += new System.EventHandler(this.button3_Click);
            // 
            // addFiveDays
            // 
            this.addFiveDays.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.addFiveDays.Location = new System.Drawing.Point(455, 597);
            this.addFiveDays.Name = "addFiveDays";
            this.addFiveDays.Size = new System.Drawing.Size(42, 23);
            this.addFiveDays.TabIndex = 7;
            this.addFiveDays.Text = "+5";
            this.addFiveDays.UseVisualStyleBackColor = true;
            this.addFiveDays.Click += new System.EventHandler(this.button4_Click);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.dateTimePicker1.Location = new System.Drawing.Point(325, 626);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(172, 20);
            this.dateTimePicker1.TabIndex = 8;
            this.dateTimePicker1.CloseUp += new System.EventHandler(this.dateTimePicker1_CloseUp);
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            this.dateTimePicker1.Enter += new System.EventHandler(this.dateTimePicker1_Enter);
            this.dateTimePicker1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dateTimePicker1_KeyUp);
            // 
            // minusFiveDay
            // 
            this.minusFiveDay.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.minusFiveDay.Location = new System.Drawing.Point(325, 597);
            this.minusFiveDay.Name = "minusFiveDay";
            this.minusFiveDay.Size = new System.Drawing.Size(43, 23);
            this.minusFiveDay.TabIndex = 9;
            this.minusFiveDay.Text = "-5 ";
            this.minusFiveDay.UseVisualStyleBackColor = true;
            this.minusFiveDay.Click += new System.EventHandler(this.minusFiveDay_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(903, 658);
            this.Controls.Add(this.minusFiveDay);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.addFiveDays);
            this.Controls.Add(this.bTnToDay);
            this.Controls.Add(this.btnTask);
            this.Controls.Add(this.plMain);
            this.Controls.Add(this.plForDate);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.plPersonButton);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.plMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pBForLine)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
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
        private System.Windows.Forms.Panel plPersonButton;
        private System.Windows.Forms.Button bTnToDay;
        private System.Windows.Forms.Button addFiveDays;
        private System.Windows.Forms.PictureBox pBForLine;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button minusFiveDay;
        private System.Windows.Forms.ToolStripMenuItem задачиToolStripMenuItem;
    }
}

