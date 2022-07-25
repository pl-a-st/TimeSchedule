namespace time_schedule
{
    public partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.plMain = new System.Windows.Forms.Panel();
            this.pBForLine = new System.Windows.Forms.PictureBox();
            this.plForDate = new System.Windows.Forms.Panel();
            this.btnNewTask = new System.Windows.Forms.Button();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.сенитьПользователяToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.adminToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.проектToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.выбратьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.создатьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.задачиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPersons = new System.Windows.Forms.ToolStripMenuItem();
            this.нерабочиеДниToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plPersonButton = new System.Windows.Forms.Panel();
            this.bTnToDay = new System.Windows.Forms.Button();
            this.addFiveDays = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.minusFiveDay = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnRefresh = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
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
            this.plMain.Location = new System.Drawing.Point(179, 60);
            this.plMain.Name = "plMain";
            this.plMain.Size = new System.Drawing.Size(977, 641);
            this.plMain.TabIndex = 0;
            this.plMain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollChange);
            this.plMain.MouseEnter += new System.EventHandler(this.plMain_MouseEnter);
            this.plMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.plMain_MouseMove);
            // 
            // pBForLine
            // 
            this.pBForLine.BackColor = System.Drawing.SystemColors.Window;
            this.pBForLine.Location = new System.Drawing.Point(0, 1);
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
            this.plForDate.Location = new System.Drawing.Point(179, 26);
            this.plForDate.Name = "plForDate";
            this.plForDate.Size = new System.Drawing.Size(958, 52);
            this.plForDate.TabIndex = 1;
            // 
            // btnNewTask
            // 
            this.btnNewTask.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnNewTask.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnNewTask.Location = new System.Drawing.Point(634, 707);
            this.btnNewTask.Name = "btnNewTask";
            this.btnNewTask.Size = new System.Drawing.Size(110, 49);
            this.btnNewTask.TabIndex = 3;
            this.btnNewTask.Text = "Новая задача";
            this.btnNewTask.UseVisualStyleBackColor = true;
            this.btnNewTask.Click += new System.EventHandler(this.btnTask_Click);
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
            this.сенитьПользователяToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.adminToolStripMenuItem});
            this.сенитьПользователяToolStripMenuItem.Name = "сенитьПользователяToolStripMenuItem";
            this.сенитьПользователяToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.сенитьПользователяToolStripMenuItem.Text = "Сенить пользователя";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(108, 22);
            this.toolStripMenuItem2.Text = "reader";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click_1);
            // 
            // adminToolStripMenuItem
            // 
            this.adminToolStripMenuItem.Name = "adminToolStripMenuItem";
            this.adminToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
            this.adminToolStripMenuItem.Text = "admin";
            this.adminToolStripMenuItem.Click += new System.EventHandler(this.adminToolStripMenuItem_Click);
            // 
            // проектToolStripMenuItem
            // 
            this.проектToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.выбратьToolStripMenuItem,
            this.создатьToolStripMenuItem});
            this.проектToolStripMenuItem.Name = "проектToolStripMenuItem";
            this.проектToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.проектToolStripMenuItem.Text = "Файл";
            this.проектToolStripMenuItem.Click += new System.EventHandler(this.ToolStripMenuProject_Click);
            // 
            // выбратьToolStripMenuItem
            // 
            this.выбратьToolStripMenuItem.Name = "выбратьToolStripMenuItem";
            this.выбратьToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.выбратьToolStripMenuItem.Text = "Выбрать";
            this.выбратьToolStripMenuItem.Click += new System.EventHandler(this.выбратьToolStripMenuItem_Click);
            // 
            // создатьToolStripMenuItem
            // 
            this.создатьToolStripMenuItem.Name = "создатьToolStripMenuItem";
            this.создатьToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.создатьToolStripMenuItem.Text = "Создать";
            this.создатьToolStripMenuItem.Click += new System.EventHandler(this.создатьToolStripMenuItem_Click);
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
            this.нерабочиеДниToolStripMenuItem.Click += new System.EventHandler(this.нерабочиеДниToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1171, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // plPersonButton
            // 
            this.plPersonButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.plPersonButton.AutoScroll = true;
            this.plPersonButton.Location = new System.Drawing.Point(9, 60);
            this.plPersonButton.Name = "plPersonButton";
            this.plPersonButton.Size = new System.Drawing.Size(200, 621);
            this.plPersonButton.TabIndex = 4;
            this.plPersonButton.Scroll += new System.Windows.Forms.ScrollEventHandler(this.plPersonButton_Scroll);
            // 
            // bTnToDay
            // 
            this.bTnToDay.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bTnToDay.Location = new System.Drawing.Point(505, 707);
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
            this.addFiveDays.Location = new System.Drawing.Point(586, 707);
            this.addFiveDays.Name = "addFiveDays";
            this.addFiveDays.Size = new System.Drawing.Size(42, 23);
            this.addFiveDays.TabIndex = 7;
            this.addFiveDays.Text = "+5";
            this.addFiveDays.UseVisualStyleBackColor = true;
            this.addFiveDays.Click += new System.EventHandler(this.PlusFiveDays);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.dateTimePicker1.Location = new System.Drawing.Point(456, 736);
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
            this.minusFiveDay.Location = new System.Drawing.Point(456, 707);
            this.minusFiveDay.Name = "minusFiveDay";
            this.minusFiveDay.Size = new System.Drawing.Size(43, 23);
            this.minusFiveDay.TabIndex = 9;
            this.minusFiveDay.Text = "-5 ";
            this.minusFiveDay.UseVisualStyleBackColor = true;
            this.minusFiveDay.Click += new System.EventHandler(this.MinusFiveDays_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 900000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnRefresh.Location = new System.Drawing.Point(340, 707);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 49);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(9, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(170, 37);
            this.button1.TabIndex = 10;
            this.button1.Text = "Проекты";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Projects_Click_2);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1171, 764);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.minusFiveDay);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.addFiveDays);
            this.Controls.Add(this.bTnToDay);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnNewTask);
            this.Controls.Add(this.plMain);
            this.Controls.Add(this.plForDate);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.plPersonButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
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
        private System.Windows.Forms.Button btnNewTask;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem adminToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ToolStripMenuItem выбратьToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem создатьToolStripMenuItem;
        private System.Windows.Forms.Button button1;
    }
}

