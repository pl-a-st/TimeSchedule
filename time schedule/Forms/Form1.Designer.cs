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
            this.восстановлениеЗадачToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cBxShowTask = new System.Windows.Forms.ToolStripComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plPersonButton = new System.Windows.Forms.Panel();
            this.bTnToDay = new System.Windows.Forms.Button();
            this.addFiveDays = new System.Windows.Forms.Button();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.minusFiveDay = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnRefresh = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cBxSeetingsProjects = new System.Windows.Forms.ComboBox();
            this.lblProjects = new System.Windows.Forms.Label();
            this.btnSetings = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
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
            this.plMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.plMain.Controls.Add(this.pBForLine);
            this.plMain.Location = new System.Drawing.Point(181, 85);
            this.plMain.Name = "plMain";
            this.plMain.Size = new System.Drawing.Size(977, 645);
            this.plMain.TabIndex = 0;
            this.plMain.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollChange);
            this.plMain.MouseEnter += new System.EventHandler(this.plMain_MouseEnter);
            this.plMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.plMain_MouseMove);
            this.plMain.Resize += new System.EventHandler(this.plMain_Resize);
            // 
            // pBForLine
            // 
            this.pBForLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(194)))), ((int)(((byte)(183)))));
            this.pBForLine.Location = new System.Drawing.Point(0, 0);
            this.pBForLine.Name = "pBForLine";
            this.pBForLine.Size = new System.Drawing.Size(100, 51);
            this.pBForLine.TabIndex = 0;
            this.pBForLine.TabStop = false;
            // 
            // plForDate
            // 
            this.plForDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.plForDate.AutoScroll = true;
            this.plForDate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.plForDate.Enabled = false;
            this.plForDate.Location = new System.Drawing.Point(181, 53);
            this.plForDate.Name = "plForDate";
            this.plForDate.Size = new System.Drawing.Size(977, 50);
            this.plForDate.TabIndex = 1;
            // 
            // btnNewTask
            // 
            this.btnNewTask.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnNewTask.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnNewTask.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnNewTask.FlatAppearance.BorderSize = 0;
            this.btnNewTask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewTask.ForeColor = System.Drawing.SystemColors.Control;
            this.btnNewTask.Location = new System.Drawing.Point(654, 736);
            this.btnNewTask.Name = "btnNewTask";
            this.btnNewTask.Size = new System.Drawing.Size(110, 49);
            this.btnNewTask.TabIndex = 3;
            this.btnNewTask.Text = "Новая задача";
            this.btnNewTask.UseVisualStyleBackColor = false;
            this.btnNewTask.Click += new System.EventHandler(this.btnTask_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.сенитьПользователяToolStripMenuItem,
            this.проектToolStripMenuItem,
            this.задачиToolStripMenuItem,
            this.menuPersons,
            this.нерабочиеДниToolStripMenuItem,
            this.восстановлениеЗадачToolStripMenuItem,
            this.cBxShowTask});
            this.toolStripMenuItem1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
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
            this.сенитьПользователяToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.сенитьПользователяToolStripMenuItem.Text = "Сменить пользователя";
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
            this.проектToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
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
            this.задачиToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.задачиToolStripMenuItem.Text = "Задачи";
            this.задачиToolStripMenuItem.Click += new System.EventHandler(this.задачиToolStripMenuItem_Click);
            // 
            // menuPersons
            // 
            this.menuPersons.Name = "menuPersons";
            this.menuPersons.Size = new System.Drawing.Size(200, 22);
            this.menuPersons.Text = "Исполнители";
            this.menuPersons.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // нерабочиеДниToolStripMenuItem
            // 
            this.нерабочиеДниToolStripMenuItem.Name = "нерабочиеДниToolStripMenuItem";
            this.нерабочиеДниToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.нерабочиеДниToolStripMenuItem.Text = "Нерабочие дни";
            this.нерабочиеДниToolStripMenuItem.Click += new System.EventHandler(this.нерабочиеДниToolStripMenuItem_Click);
            // 
            // восстановлениеЗадачToolStripMenuItem
            // 
            this.восстановлениеЗадачToolStripMenuItem.Name = "восстановлениеЗадачToolStripMenuItem";
            this.восстановлениеЗадачToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.восстановлениеЗадачToolStripMenuItem.Text = "Восстановление задач";
            this.восстановлениеЗадачToolStripMenuItem.Click += new System.EventHandler(this.восстановлениеЗадачToolStripMenuItem_Click);
            // 
            // cBxShowTask
            // 
            this.cBxShowTask.BackColor = System.Drawing.Color.White;
            this.cBxShowTask.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.cBxShowTask.Items.AddRange(new object[] {
            "Актуальные задачи",
            "Все задачи"});
            this.cBxShowTask.Name = "cBxShowTask";
            this.cBxShowTask.Size = new System.Drawing.Size(121, 23);
            this.cBxShowTask.Text = "Актуальные задачи";
            this.cBxShowTask.SelectedIndexChanged += new System.EventHandler(this.cBxShowTask_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
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
            this.plPersonButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.plPersonButton.Location = new System.Drawing.Point(11, 85);
            this.plPersonButton.Name = "plPersonButton";
            this.plPersonButton.Size = new System.Drawing.Size(200, 624);
            this.plPersonButton.TabIndex = 4;
            this.plPersonButton.Scroll += new System.Windows.Forms.ScrollEventHandler(this.plPersonButton_Scroll);
            // 
            // bTnToDay
            // 
            this.bTnToDay.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.bTnToDay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.bTnToDay.FlatAppearance.BorderSize = 0;
            this.bTnToDay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bTnToDay.ForeColor = System.Drawing.SystemColors.Control;
            this.bTnToDay.Location = new System.Drawing.Point(525, 736);
            this.bTnToDay.Name = "bTnToDay";
            this.bTnToDay.Size = new System.Drawing.Size(75, 23);
            this.bTnToDay.TabIndex = 5;
            this.bTnToDay.Text = "Сегодня";
            this.bTnToDay.UseVisualStyleBackColor = false;
            this.bTnToDay.Click += new System.EventHandler(this.button3_Click);
            // 
            // addFiveDays
            // 
            this.addFiveDays.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.addFiveDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.addFiveDays.FlatAppearance.BorderSize = 0;
            this.addFiveDays.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addFiveDays.ForeColor = System.Drawing.SystemColors.Control;
            this.addFiveDays.Location = new System.Drawing.Point(606, 736);
            this.addFiveDays.Name = "addFiveDays";
            this.addFiveDays.Size = new System.Drawing.Size(42, 23);
            this.addFiveDays.TabIndex = 7;
            this.addFiveDays.Text = "+5";
            this.addFiveDays.UseVisualStyleBackColor = false;
            this.addFiveDays.Click += new System.EventHandler(this.PlusFiveDays);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.dateTimePicker1.CalendarMonthBackground = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.dateTimePicker1.Location = new System.Drawing.Point(476, 765);
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
            this.minusFiveDay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.minusFiveDay.FlatAppearance.BorderSize = 0;
            this.minusFiveDay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.minusFiveDay.ForeColor = System.Drawing.SystemColors.Control;
            this.minusFiveDay.Location = new System.Drawing.Point(476, 736);
            this.minusFiveDay.Name = "minusFiveDay";
            this.minusFiveDay.Size = new System.Drawing.Size(43, 23);
            this.minusFiveDay.TabIndex = 9;
            this.minusFiveDay.Text = "-5 ";
            this.minusFiveDay.UseVisualStyleBackColor = false;
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
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnRefresh.Location = new System.Drawing.Point(360, 736);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 49);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.button1.Location = new System.Drawing.Point(11, 53);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 30);
            this.button1.TabIndex = 10;
            this.button1.Text = "Проекты";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.Projects_Click_2);
            // 
            // cBxSeetingsProjects
            // 
            this.cBxSeetingsProjects.FormattingEnabled = true;
            this.cBxSeetingsProjects.Location = new System.Drawing.Point(44, 27);
            this.cBxSeetingsProjects.Name = "cBxSeetingsProjects";
            this.cBxSeetingsProjects.Size = new System.Drawing.Size(135, 21);
            this.cBxSeetingsProjects.TabIndex = 11;
            this.cBxSeetingsProjects.SelectedIndexChanged += new System.EventHandler(this.cBxSeetingsProgects_SelectedIndexChanged);
            this.cBxSeetingsProjects.Click += new System.EventHandler(this.comboBox1_Click);
            // 
            // lblProjects
            // 
            this.lblProjects.AutoSize = true;
            this.lblProjects.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblProjects.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblProjects.Location = new System.Drawing.Point(187, 27);
            this.lblProjects.Name = "lblProjects";
            this.lblProjects.Size = new System.Drawing.Size(46, 17);
            this.lblProjects.TabIndex = 12;
            this.lblProjects.Text = "label1";
            // 
            // btnSetings
            // 
            this.btnSetings.Location = new System.Drawing.Point(11, 27);
            this.btnSetings.Name = "btnSetings";
            this.btnSetings.Size = new System.Drawing.Size(30, 21);
            this.btnSetings.TabIndex = 13;
            this.btnSetings.Text = "...";
            this.btnSetings.UseVisualStyleBackColor = true;
            this.btnSetings.Click += new System.EventHandler(this.btnSetings_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(181, 765);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(80, 24);
            this.button2.TabIndex = 14;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(1171, 820);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnSetings);
            this.Controls.Add(this.lblProjects);
            this.Controls.Add(this.cBxSeetingsProjects);
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
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.TransparencyKey = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
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
        private System.Windows.Forms.ComboBox cBxSeetingsProjects;
        private System.Windows.Forms.Label lblProjects;
        private System.Windows.Forms.ToolStripComboBox cBxShowTask;
        private System.Windows.Forms.Button btnSetings;
        private System.Windows.Forms.ToolStripMenuItem восстановлениеЗадачToolStripMenuItem;
        private System.Windows.Forms.Button button2;
    }
}

