using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;


namespace time_schedule {
    public delegate void LoadRefreshForm();
    public enum RefreshType
    {
        All,
        Minimum
    }
    public partial class Form1 : Form
    {
        public Statuses.FormReadyToBeAddedControl FormReadyToBeAddedControl
            { get; private set; } = Statuses.FormReadyToBeAddedControl.NotReady;
        public RefreshType RefreshType
        { get; private set; } = RefreshType.Minimum;
        public int PlMainScrollYSaved
        { get; private set; } = 0;
        public int PlMainScrollXSaved
        { get; private set; } = 0;
        public int PlForDateScrollXSaved
        { get; private set; } = 0;
        public int PlPersonButtonYSaved
        { get; private set; } = 0;
        public DateTime MinDateStart
        { get; private set; } = DateTime.MaxValue;
        public DateTime MaxDateFinish
        { get; private set; } = DateTime.MinValue;
        public int DiveDays {
            get; private set;}
        public int VerticalScrollValue {
            get;
            private set;
        }
        public int HorizontalScrollValue {
            get;
            private set;
        }
        public int OpenFormsCount {
            get;
            private set;
        }
        public Bitmap Bmp { 
            get;
            set;
        } = new Bitmap(1, 1);
        public Statuses.ProgramWindowState ProgramWindowState {
            get;
            private set;
        } = Statuses.ProgramWindowState.Normal;
        public Form1()
        {
            Program.fmMain = this;
            InitializeComponent();
            plMain.MouseWheel += CalendarTasks_MouseWheel;
            plMain.ClientSizeChanged += PlMain_ClientSizeChanged;
            plPersonButton.MouseWheel += PlPersonButton_MouseWheel;
        }
        public static Form1 SelfRef {
            get; 
            set;
        }
        public Form1(Form form) {
            SelfRef = this;
        }
        int CalcDivDays() {
            DateTime maxDateTime = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            DateTime minDateTime = Program.ListTasksAllPerson.GetMinDateStartTasks();
            return (maxDateTime - MaxDateFinish).Days;
        }
        public void LoadRefreshForm() {
            
            
            ChangePanelVisibility(Statuses.Visibility.Invisible);
            SaveScrolls();
            if (CalcDivDays() < 0) {
                HorizontalScrollAdd(CalcDivDays());
            }
            FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.NotReady;
            plForDate.Enabled = false;
            CleanOldExemplars(); // 245 мс
            ClearAllPools();
            LoadAllPools();
            
            LoadTextBoxWithDate(); //281 мс
            LoadHorizontLine();
            DrowVerticalLines();
            SaveMinMaxDate();
            plForDate.Enabled = true;
            LoadScrolls();
            FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.Ready;
            LoadPersonButtons();
            LoadTaskButtons();//285 мс
            LoadDateTextBoxes();
            ChangePanelVisibility(Statuses.Visibility.Visible);
            //plPersonButton.VerticalScroll.Value = VerticalScrollValue;
            //plPersonButton.VerticalScroll.Value = VerticalScrollValue;
        }
        public Panel SetPlMain() {
            return plMain;
        }
        public Panel SetPlForDate() {
            return plForDate;
        }
        public Panel SetPlForPerson() {
            return plPersonButton;
        }
        private void PlPersonButton_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                plMain.VerticalScroll.Value = plPersonButton.VerticalScroll.Value;
                plMain.VerticalScroll.Value = plPersonButton.VerticalScroll.Value;
            }
            catch
            {

            }


        }
        VScrollBar myScrollBar = new VScrollBar();
        private void PlMain_ClientSizeChanged(object sender, EventArgs e)
        {
            //plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            //plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }
        private void CalendarTasks_MouseWheel1(object sender, MouseEventArgs e)
        {
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                int test = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = test;
            }
            catch
            {
            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        public int GetPersonButtonWith()
        {
            return plMain.Location.X - plPersonButton.Location.X;
        }
        private void CleanOldExemplars()
        {
            
            for (int i = 0; i < plMain.Controls.Count; i++)
            {
                if (plMain.Controls[i] is Button)
                {
                    plMain.Controls.Remove(plMain.Controls[i]);
                    i--;
                }

            }
            for (int i = 0; i < plPersonButton.Controls.Count; i++)
            {
                if (plPersonButton.Controls[i] is Button)
                {
                    plPersonButton.Controls.Remove(this.plPersonButton.Controls[i]);
                    i--;
                }
            }
            plForDate.Controls.Clear();
            pBForLine.CreateGraphics().Clear(Color.White);
            Bmp = new Bitmap(1,1);
        }
        private int SetMaxLocationAndAddPersonButton(ref ListPersonButton listPersonButton)
        {
            int maxButtonLocationY = 0;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    personButton.SetLocation(0, maxButtonLocationY);
                    maxButtonLocationY += (personButton.Button.Height + Constants.MIN_ROW_HIGHT +1);
                }
            }
            return maxButtonLocationY; 
        }
        public void Drow(Pen pen, int pX0, int pY0, int pX1, int pY1)
        {
            Graphics graphics = Graphics.FromImage(Bmp);
            graphics.DrawLine(pen, pX0, pY0, pX1, pY1);
            pBForLine.Image = Bmp;
            graphics.Dispose();
        }
        public void LoadHorizontLine()
        {
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if(personButton.Person.ListTask.Tasks.Count>0)
                {
                    int locationY = personButton.Button.Location.Y;
                    Pen pen = new Pen(Color.LightGray, 1);
                    do
                    {
                        Drow(pen, 0, locationY, pBForLine.Width, locationY);
                        locationY += Constants.ROW_HIGHT;
                    }
                    while (locationY < (personButton.Button.Location.Y + personButton.Button.Height));
                    Pen pen1 = new Pen(Color.Black, 5);
                    Drow(pen1, 0, locationY + 1, pBForLine.Width, locationY);
                }  
            }
        }
        public void DrowVerticalLines()
        {
            Pen penGrey = new Pen(Color.LightGray, 1);
            Pen penForMonday = new Pen(Color.FromArgb(32,55,100), 2);
            foreach (DateTextBox dateTextBox in Program.PoolTextBox.ListTextBoxes)
            {             
                int locationX = dateTextBox.TextBox.Location.X;
                if (dateTextBox.Date.DayOfWeek == DayOfWeek.Monday)
                {
                    Drow(penForMonday, locationX, 0, locationX, pBForLine.Height);
                }
                else
                {
                    Drow(penGrey, locationX, 0, locationX, pBForLine.Height);
                }
            }
        }
        public void SaveScrolls()
        {
            PlForDateScrollXSaved = plForDate.HorizontalScroll.Value;
            PlMainScrollXSaved = plMain.HorizontalScroll.Value;
            PlMainScrollYSaved = plMain.VerticalScroll.Value;
            PlPersonButtonYSaved = plPersonButton.VerticalScroll.Value;
        }
        public void LoadScrolls()
        {
            try
            {
                plForDate.HorizontalScroll.Value = HorizontalScrollValue;
                plForDate.HorizontalScroll.Value = HorizontalScrollValue;
                plMain.HorizontalScroll.Value = HorizontalScrollValue;
                plMain.HorizontalScroll.Value = HorizontalScrollValue;
            }
            catch { }            
        }
        public void ScrollToZero()
        {
            plForDate.HorizontalScroll.Value = 0;
            plMain.HorizontalScroll.Value = 0;
            plMain.VerticalScroll.Value = 0;
            plPersonButton.VerticalScroll.Value = 0;
        }
        public void SaveMinMaxDate()
        {
            MaxDateFinish = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            MinDateStart = Program.ListTasksAllPerson.GetMinDateStartTasks();
        }
        public void LoadRefreshForm(RefreshType refreshType)
        {
            if (refreshType == RefreshType.All)
            {
                RefreshType refreshTypeToSave = RefreshType;
                RefreshType = RefreshType.All;
                LoadRefreshForm();
                RefreshType = refreshTypeToSave;
            }
        }
        public void LoadRefreshFormForDelegat() {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }
        public void LoadRefreshForm(Statuses.ProgressBar statusProgressBar) {
            timer1.Interval = 900000;
            if (statusProgressBar == Statuses.ProgressBar.Use) {
                fmProgressBar fmProgressBar = new fmProgressBar();
                fmProgressBar.SetTextMessege("Идет обновление");
                fmProgressBar.StartPosition = FormStartPosition.Manual;
                fmProgressBar.Location = new Point(
                    this.Left + this.Width/2- fmProgressBar.Width/2,
                    this.Top + this.Height/2-fmProgressBar.Height/2);
                fmProgressBar.TopLevel = true;
                List<Form> listForm = new List<Form>() { this, fmProgressBar };
                Thread thread = new Thread(LoadRefreshWithProgressBarr,0);
                thread.Start(listForm);
                OpenFormsCount = Application.OpenForms.Count;
                fmProgressBar.ShowDialog();
            }
        }
        public void LoadRefreshForm(fmProgressBar fmProgressBar) {
            LoadRefreshForm(RefreshType.All);
            fmProgressBar.Close();
        }
        public void LoadRefreshWithProgressBarr(object listForm) {
            List<Form> result = new List<Form>();
            do {
                foreach (Form form in Application.OpenForms) {
                    if (form.Visible)
                        result.Add(form);
                }
                Thread.Sleep(50);
            }
            while (result.Count <= OpenFormsCount);
            fmProgressBar fmProgressBar = (listForm as List<Form>)[1] as fmProgressBar;
            Form1 form1 = (listForm as List<Form>)[0] as Form1;
            form1.BeginInvoke(new Action(delegate () { LoadRefreshForm(fmProgressBar); }));
        }
        private void LoadAllPools() {
            if (File.Exists(Dals.TakeProjectPath(Constants.TASKS_BIN))){
                Program.ListTasksAllPerson = Dals.binReadFileToObject(
                    Program.ListTasksAllPerson,
                    Dals.TakeProjectPath(Constants.TASKS_BIN));
            }
            else {
                Program.ListTasksAllPerson.SetTasksFromList(Dals.ReadListFromProjectFile(Constants.TASKS));
            }
            if (File.Exists(Dals.TakeProjectPath(Constants.TASKS_BIN))) {
                Program.listPersons = Dals.binReadFileToObject(
                    Program.listPersons,
                    Dals.TakeProjectPath(Constants.PERSONS_BIN));
            }
            else {
                Program.listPersons.SetPersonsFromList(
                 Dals.ReadListFromProjectFile(Constants.PERSONS),
                 Program.ListTasksAllPerson.Tasks);
            }
            
            Program.ListPersonButton.LoadListPersonButtons(
                Program.listPersons.Persons,
                Program.ListTasksAllPerson,
                Constants.ROW_HIGHT,
                this);
            int maxButtonLocationY = SetMaxLocationAndAddPersonButton(ref Program.ListPersonButton);
            pBForLine.Height = maxButtonLocationY;
            if (File.Exists(Dals.TakeProjectPath(Constants.HOLYDAYS_BIN))) {
                Program.ListHolidays = Dals.binReadFileToObject(
                    Program.ListHolidays,
                    Dals.TakeProjectPath(Constants.HOLYDAYS_BIN));
            }
            else {
                Program.ListHolidays.SetHolidaysFromList(
                    Dals.ReadListFromProjectFile(Constants.HOLYDAYS));
            }
            
            NonWorkDaysWrite(Program.ListTasksAllPerson.GetMinDateStartTasks(), Program.ListTasksAllPerson.GetMaxDateFinishTasks());
            Program.listNonWorkingDays.NonWorkingDays.AddRange(Program.ListHolidays.Holidays);
            Program.ListTaskButtons.AddTaskButtons(
                Program.ListTasksAllPerson,
                Program.ListPersonButton);
        }
        private static void ClearAllPools() {
            Program.PoolTextBox.ListTextBoxes.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListTasksAllPerson.Tasks.Clear();
            Program.listPersons.Persons.Clear();
            Program.ListPersonButton.PersonButtons.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListHolidays.Holidays.Clear();
            Program.listNonWorkingDays.NonWorkingDays.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
        }
        private void ChangePanelVisibility(Statuses.Visibility visibility) {
            if (visibility == Statuses.Visibility.Visible) {
                plMain.Visible = true;
                plForDate.Visible = true;
                plPersonButton.Visible = true;
            }
            if (visibility == Statuses.Visibility.Invisible) {
                plForDate.Visible = false;
                plPersonButton.Visible = false;
                plMain.Visible = false;
            }
        }
        private void LoadTextBoxWithDate() {
            DateTime dateToTables = Program.ListTasksAllPerson.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPerson.GetMaxDateFinishTasks();
            int height = plMain.Location.Y - plForDate.Location.Y;
            int locationX = 0;
            plForDate.Controls.Clear();
            while (dateToTables <= dateMaxToTable) {
                if (IsNotHolyday(dateToTables)) {
                    DateTextBox dateTextBox = new DateTextBox(dateToTables, height, locationX);
                   
                    if (dateToTables == Program.ListTasksAllPerson.GetMinDateStartTasks()) {
                        dateTextBox.TextBox.Location = new Point(
                            dateTextBox.TextBox.Location.X - HorizontalScrollValue, 0);
                        plForDate.Controls.Add(dateTextBox.TextBox);
                        dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.Loaded);
                    }
                    if (dateToTables == dateMaxToTable) {
                        dateTextBox.TextBox.Location = new Point(
                            dateTextBox.TextBox.Location.X, 0);
                        plForDate.Controls.Add(dateTextBox.TextBox);
                        dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.ReadyToLoad);
                    }
                    locationX += Constants.COLUMN_WITH;
                    Program.PoolTextBox.ListTextBoxes.Add(dateTextBox);
                }
                dateToTables = dateToTables.AddDays(1);
            }
            pBForLine.Width = locationX;
            ResizeImage(new Size(locationX, pBForLine.Height));
        }
        private static bool IsNotHolyday(DateTime dateToTables) {
            if (dateToTables.DayOfWeek == DayOfWeek.Saturday)
                return false;
            if (dateToTables.DayOfWeek == DayOfWeek.Sunday)
                return false;
            if (Program.listNonWorkingDays.NonWorkingDays.Contains(dateToTables))
                return false;
            return true;
        }
        public Panel GetPlPeraonButton()
        {
            return plPersonButton;
        }
        public Panel GetPlMain()
        {
            return plMain;
        }
        public Bitmap GetBmp()
        {
            return Bmp;
        }
        public Form1 SetForm1()
        {
            return this;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            Program.delegatLoadRefreshForm = LoadRefreshFormForDelegat;
            myScrollBar.Height = plMain.Height;
            myScrollBar.Left = plMain.Width - myScrollBar.Width;
            myScrollBar.Top = 0;
            myScrollBar.Enabled = false;
            Dals.WriteProjectFolder(Statuses.WorkWithProject.ProgramStarted);
            this.Text = Dals.ProjectFolderPath;
            this.Text = this.Text.Replace("\\Проект",string.Empty);
            this.Activate();
            LoadRefreshForm();
            plForDate.Enabled = true;
            btnNewTask.Visible = false;
        }

        private void CalendarTasks_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                plMain.Focus();
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                int test = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = test;
                plPersonButton.VerticalScroll.Value = test;
            }
            catch
            {
                
            }
            VerticalScrollValue = plMain.VerticalScroll.Value;
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
            LoadTaskButtons();
            LoadDateTextBoxes();
        }
        public void ScrollToBottom(Panel p)
        {
            using (Control c = new Control() { Parent = p, Dock = DockStyle.Bottom })
            {
                p.ScrollControlIntoView(c);
                c.Parent = null;
            }
        }
        public void NonWorkDaysWrite(DateTime вeginningPeriod, DateTime endPeriod)
        {
            int i = 0;
            while(вeginningPeriod.AddDays(i) < endPeriod ||
                вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday ||
                Program.ListHolidays.Holidays.Contains(вeginningPeriod.AddDays(i))
                )
            {
                if (вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                    вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    Program.listNonWorkingDays.NonWorkingDays.Add(вeginningPeriod.AddDays(i));
                i++;
            }
        }
        private void ScrollChange(object sender, ScrollEventArgs e)
        {
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plPersonButton.VerticalScroll.Value = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = plMain.VerticalScroll.Value;
            }
            catch
            {
                //MessageBox.Show("1");
            }
            VerticalScrollValue = plMain.VerticalScroll.Value;
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
            LoadTaskButtons();
            LoadDateTextBoxes();
            //label1.Text = "текущее " + plForDate.HorizontalScroll.Value;
            //label2.Text = "максимальное   " + plForDate.HorizontalScroll.Maximum;

        }
        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            fmWorkWithPersons fmWorkWithPersons = new fmWorkWithPersons(Program.delegatLoadRefreshForm);
            fmWorkWithPersons.ShowDialog();
            

        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            fmAddChangeTask fmAddTask = new fmAddChangeTask(Program.delegatLoadRefreshForm);
            fmAddTask.StartPosition = FormStartPosition.CenterScreen;
            fmAddTask.SetCreateOrChange(CreateOrChange.Create);
            fmAddTask.Show();
            //Dals.WriteObjectToFile(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
            Dals.WriteObjectToFile(Constants.TASKS_BIN, Program.ListTasksAllPerson);
        }
        private void ToolStripMenuProject_Click(object sender, EventArgs e)
        {
           
        }

        
        public void ScrollToDate(DateTime targetDateTime)
        {
            int locationX = 0;
            foreach (DateTextBox DateTextBox in Program.PoolTextBox.ListTextBoxes) {
                if (DateTextBox.Date.Date < targetDateTime) {
                    locationX += Constants.COLUMN_WITH;
                }
                else {
                    break;
                }
            }
            int NewLocationX = locationX - 5 * Constants.COLUMN_WITH;
            try
            {
                if (NewLocationX > 0)
                {
                    plMain.HorizontalScroll.Value = NewLocationX;
                    plMain.HorizontalScroll.Value = NewLocationX;
                    plForDate.HorizontalScroll.Value = NewLocationX;
                    plForDate.HorizontalScroll.Value = NewLocationX;

                }
                else
                {
                    plMain.HorizontalScroll.Value = locationX;
                    plMain.HorizontalScroll.Value = locationX;
                    plForDate.HorizontalScroll.Value = locationX;
                    plForDate.HorizontalScroll.Value = locationX;
                }
            }
            catch { }
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
            LoadTaskButtons();
            LoadDateTextBoxes();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            ScrollToDate(DateTime.Now.Date);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ScrollToDate(DateTime.Now.Date);
        }
        private void LoadDataGridPersonButton(ref DataGridView dataGridView, ListPersonButton listPersonButton)
        {
            //Point startPositionPersonButton;

        }

        private void plMain_MouseEnter(object sender, EventArgs e)
        {

        }

        private void plMain_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }
        private void HorizontalScrollAdd(int value) {
            try {
                plMain.HorizontalScroll.Value += value * Constants.COLUMN_WITH;
                plMain.HorizontalScroll.Value += value * Constants.COLUMN_WITH;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            }
            catch {

            }
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
        }
        private void PlusFiveDays(object sender, EventArgs e)
        {
            HorizontalScrollAdd(5);
            LoadTaskButtons();
            LoadDateTextBoxes();
        }
        public void ResizeImage(Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(Bmp, 0, 0, size.Width, size.Height);
                }
                Bmp = b;
            }
            catch
            {
            }
        }

        private void MinusFiveDays_Click(object sender, EventArgs e)
        {
            HorizontalScrollAdd(-5);
            LoadTaskButtons();
            LoadDateTextBoxes();

        }

        private void dateTimePicker1_CloseUp(object sender, EventArgs e)
        {
            ScrollToDate(dateTimePicker1.Value.Date);
        }

        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
            
        }

        private void dateTimePicker1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                ScrollToDate(dateTimePicker1.Value.Date);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(""+plForDate.HorizontalScroll.Value);
        }
        //LoadRefreshForm ThisloadRefreshForm;
        private void задачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            fmTasks fmTasks = new fmTasks(this);
            
            fmTasks.ShowDialog();
        }

        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            Program.UserType = UserType.Reader;
            btnNewTask.Visible = false;
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Program.UserType = UserType.Admin;
            btnNewTask.Visible = true;
        }

        private void plPersonButton_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;
                plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;

            }
            catch
            {

            }
        }

        private void нерабочиеДниToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fmHolidays holidays = new fmHolidays(LoadRefreshForm);
            holidays.ShowDialog();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadRefreshForm(Statuses.ProgressBar.Use);
            timer1.Interval = 900000;
        }

        private void btnRefresh_Click(object sender, EventArgs e) {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e) {
            Dals.WriteProjectFolder("Проект", Statuses.WorkWithProject.NewProject);
            SetNewTextForForm();
            ZeroingScrolss();
            LoadRefreshForm(Statuses.ProgressBar.Use);
            ScrollToDate(DateTime.Now.Date);
        }
        private void выбратьToolStripMenuItem_Click(object sender, EventArgs e) {
            Dals.WriteProjectFolder("", Statuses.WorkWithProject.LoadProject);
            SetNewTextForForm();
            ZeroingScrolss();
            LoadRefreshForm(Statuses.ProgressBar.Use);
            ScrollToDate(DateTime.Now.Date);
        }
        private void ZeroingScrolss() {
            plForDate.HorizontalScroll.Value = 0;
            plForDate.HorizontalScroll.Value = 0;
            plMain.HorizontalScroll.Value = 0;
            plMain.HorizontalScroll.Value = 0;
            HorizontalScrollValue = 0;
            plMain.VerticalScroll.Value = 0;
            plMain.VerticalScroll.Value = 0;
            VerticalScrollValue = 0;
            plPersonButton.VerticalScroll.Value = 0;
            plPersonButton.VerticalScroll.Value = 0;
        }
        private void SetNewTextForForm() {
            this.Text = Dals.ProjectFolderPath;
            this.Text = this.Text.Replace("\\Проект", string.Empty);
            this.Activate();
        }
        private void LoadPersonButtons() {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons) {
                if (personButton.Person.ListTask.Tasks.Count > 0) {
                    plPersonButton.Controls.Add(personButton.Button);
                    personButton.Button.BringToFront();
                }
                   
            }
        }
        private void LoadTaskButtons() {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            Program.ListTaskButtons.CalculateLoadingStatus(
                HorizontalScrollValue,
                this.Width, 
                Program.ListTasksAllPerson.GetMinDateStartTasks());
            Program.ListTaskButtons.SetNewLocationToButtons(
                VerticalScrollValue,
                HorizontalScrollValue);
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons) {
                if (taskButton.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad) {
                    foreach(Button button in taskButton.Buttons) {
                        plMain.Controls.Add(button);
                        button.BringToFront();
                    }
                    taskButton.SetLoadingStatus(Statuses.LoadingStatus.Loaded);
                }
            }
        }
        private void LoadDateTextBoxes() {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            Program.PoolTextBox.CalculateLoadingStatus(
                plMain.HorizontalScroll.Value,
                this.Width,
                Program.ListTasksAllPerson.GetMinDateStartTasks());
            Program.PoolTextBox.SetNewLocationToButtons(
            plMain.HorizontalScroll.Value);
            foreach (DateTextBox dateTextBox in Program.PoolTextBox.ListTextBoxes) {
                if (dateTextBox.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad) {
                    plForDate.Controls.Add(dateTextBox.TextBox);
                    dateTextBox.TextBox.BringToFront();
                    dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.Loaded);
                }
            }
            
        }
        private void Form1_SizeChanged(object sender, EventArgs e) {
            LoadTaskButtons();
            LoadDateTextBoxes();
            this.VerticalScrollValue = this.plMain.VerticalScroll.Value;
            if (this.WindowState == FormWindowState.Minimized) {
                ProgramWindowState = Statuses.ProgramWindowState.Minimized;
                timer1.Interval = 100;
                timer1.Enabled = false;
                return;
            }
            if (ProgramWindowState == Statuses.ProgramWindowState.Minimized) {
                ProgramWindowState = Statuses.ProgramWindowState.Normal;
                timer1.Enabled = true;
            }
                
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) {

        }

        private void button1_Click_2(object sender, EventArgs e) {

            fmProjectTree fmProjectTree = new fmProjectTree();
            fmProjectTree.StartPosition = FormStartPosition.CenterParent;
            if (Program.ProjetTree != null) {
                fmProjectTree.SetTreeView(Program.ProjetTree);
            }
            fmProjectTree.ShowDialog();
        }
    }
   
}
