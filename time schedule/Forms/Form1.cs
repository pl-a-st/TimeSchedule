using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using time_schedule.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;



namespace time_schedule
{
    public delegate void LoadRefreshForm();
    public enum RefreshType
    {
        All,
        Minimum
    }
    public partial class Form1 : Form
    {
        public Statuses.FormReadyToBeAddedControl FormReadyToBeAddedControl { get; private set; } = Statuses.FormReadyToBeAddedControl.NotReady;
        public RefreshType RefreshType { get; private set; } = RefreshType.Minimum;
        public int PlMainScrollYSaved { get; private set; } = 0;
        public int PlMainScrollXSaved { get; private set; } = 0;
        public int PlForDateScrollXSaved { get; private set; } = 0;
        public int LastMaxHorizonntalScroll { get; private set; } = 0;
        public int LastLargeChangeHorizonntalScroll { get; private set; } = 0;
        public int PlPersonButtonYSaved { get; private set; } = 0;
        public DateTime MinDateStart { get; private set; } = DateTime.MaxValue;
        public DateTime MaxDateFinish { get; private set; } = DateTime.MinValue;
        public delegate void RefreshFinished();
        public event RefreshFinished EventRefreshFinished;

        public int DiveDays
        {
            get; private set;
        }
        public int VerticalScrollValue
        {
            get;
            private set;
        }
        public int HorizontalScrollValue
        {
            get;
            private set;
        }
        public int OpenFormsCount
        {
            get;
            private set;
        }
        public Bitmap Bmp
        {
            get;
            set;
        } = new Bitmap(1, 1);
        public Statuses.ProgramWindowState ProgramWindowState
        {
            get;
            private set;
        } = Statuses.ProgramWindowState.Normal;
        /// <summary>Инициализирует главное окно и диагностическое наблюдение за Paint без изменения отрисовки.</summary>
        public Form1()
        {
            Program.fmMain = this;
            InitializeComponent();
            plMain.Paint += ObserveCalendarPaint;
            plForDate.Paint += ObserveCalendarPaint;
            pBForLine.Paint += ObserveCalendarPaint;
            FormClosed += (sender, e) => CancelPendingPaintDiagnostics();
            plMain.MouseWheel += CalendarTasks_MouseWheel;
            plMain.ClientSizeChanged += PlMain_ClientSizeChanged;
            plPersonButton.MouseWheel += PlPersonButton_MouseWheel;


        }
        public static Form1 SelfRef
        {
            get;
            set;
        }
        public Form1(Form form)
        {
            SelfRef = this;
        }
        int CalcDivDays()
        {
            DateTime maxDateTime = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks();
            DateTime minDateTime = Program.ListTasksAllPersonToShow.GetMinDateStartTasks();
            return (maxDateTime - MaxDateFinish).Days;
        }
        /// <summary>Временные и количественные показатели одного обновления календаря.</summary>
        private sealed class CalendarRefreshMetrics
        {
            /// <summary>Количество задач в загруженном проекте.</summary>
            public int TotalTasks { get; set; }
            /// <summary>Количество задач, оставшихся после фильтров календаря.</summary>
            public int FilteredTasks { get; set; }
            /// <summary>Количество созданных моделей TaskButton.</summary>
            public int TaskModels { get; set; }
            /// <summary>Количество созданных WinForms-кнопок задач, включая пока не показанные.</summary>
            public int CreatedTaskButtons { get; set; }
            /// <summary>Количество кнопок задач, добавленных в plMain за это обновление.</summary>
            public int AttachedTaskButtons { get; set; }
            /// <summary>Количество моделей исполнителей в календаре.</summary>
            public int PersonModels { get; set; }
            /// <summary>Время очистки прежних элементов и списков.</summary>
            public long ClearMs { get; set; }
            /// <summary>Время отбора задач по выбранным проектам.</summary>
            public long ProjectFilterMs { get; set; }
            /// <summary>Время отбора задач по актуальности.</summary>
            public long RelevanceFilterMs { get; set; }
            /// <summary>Время создания кнопок исполнителей.</summary>
            public long BuildPersonButtonsMs { get; set; }
            /// <summary>Время создания всех объектов TaskButton и их WinForms-кнопок.</summary>
            public long BuildTaskButtonsMs { get; set; }
            /// <summary>Время построения календарной шкалы дат.</summary>
            public long BuildDatesMs { get; set; }
            /// <summary>Время формирования горизонтальных и вертикальных линий.</summary>
            public long BuildLinesMs { get; set; }
            /// <summary>Время добавления кнопок исполнителей в панель.</summary>
            public long AttachPersonsMs { get; set; }
            /// <summary>Время расчёта видимости, размещения и добавления кнопок задач.</summary>
            public long AttachTasksMs { get; set; }
            /// <summary>Время добавления элементов шкалы дат.</summary>
            public long AttachDatesMs { get; set; }
            /// <summary>Количество событий Paint у прикреплённых кнопок задач.</summary>
            public int TaskPaintEvents { get; set; }
            /// <summary>Количество событий Paint у основных панелей календаря.</summary>
            public int CalendarPaintEvents { get; set; }
            /// <summary>Кнопки с временной подпиской Paint, снимаемой при завершении замера.</summary>
            public List<Button> ObservedButtons { get; } = new List<Button>();
        }

        /// <summary>Показатели обновления, ожидающего следующего простоя очереди UI.</summary>
        private CalendarRefreshMetrics activePaintMetrics;

        /// <summary>Обработчик очереди UI, ожидающий завершения ближайшего цикла Paint.</summary>
        private EventHandler pendingRefreshIdleHandler;

        /// <summary>Считает события Paint кнопок задач без вмешательства в их рисование.</summary>
        private void ObserveTaskButtonPaint(object sender, PaintEventArgs e)
        {
            if (activePaintMetrics != null)
                activePaintMetrics.TaskPaintEvents++;
        }

        /// <summary>Считает события Paint панелей календаря.</summary>
        private void ObserveCalendarPaint(object sender, PaintEventArgs e)
        {
            if (activePaintMetrics != null)
                activePaintMetrics.CalendarPaintEvents++;
        }

        /// <summary>Убирает отложенную диагностику предыдущего обновления.</summary>
        private void CancelPendingPaintDiagnostics()
        {
            if (pendingRefreshIdleHandler != null)
            {
                Application.Idle -= pendingRefreshIdleHandler;
                pendingRefreshIdleHandler = null;
            }
            if (activePaintMetrics != null)
            {
                foreach (Button button in activePaintMetrics.ObservedButtons)
                    button.Paint -= ObserveTaskButtonPaint;
                activePaintMetrics = null;
            }
        }

        /// <summary>Дерево отбора проектов для текущего цикла обновления; повторное чтение с диска не требуется.</summary>
        private TreeProjects selectionTreeForRefresh;

        /// <summary>Обновляет интерфейс только из согласованно загруженных данных.</summary>
        public void LoadRefreshForm()
        {
            CancelPendingPaintDiagnostics();
            CalendarRefreshMetrics metrics = new CalendarRefreshMetrics();
            Stopwatch timing = Stopwatch.StartNew();
            ListTasks loadedTasks = null;
            ListPersons loadedPersons = null;
            string tasksPath = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            try
            {
                if (TaskStore.HasTaskData(tasksPath))
                    loadedTasks = TaskStore.ReadProject(tasksPath, out loadedPersons);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException || ex is InvalidCastException ||
                ex is InvalidDataException)
            {
                MessageBox.Show(
                    "Не удалось загрузить задачи проекта. Данные не изменены.\n\n" + ex.Message,
                    "Ошибка хранилища задач", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            long readMs = timing.ElapsedMilliseconds;
            ChangePanelVisibility(Statuses.Visibility.Invisible);
            SaveScrolls();
            if (CalcDivDays() < 0)
            {
                HorizontalScrollAdd(CalcDivDays());
            }
            FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.NotReady;
            plForDate.Enabled = false;
            Stopwatch phase = Stopwatch.StartNew();
            ClearOldExemplars();
            ClearAllPools();
            metrics.ClearMs = phase.ElapsedMilliseconds;
            try
            {
                LoadAllPools(loadedTasks, loadedPersons, metrics);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException || ex is InvalidCastException)
            {
                plForDate.Enabled = true;
                ChangePanelVisibility(Statuses.Visibility.Visible);
                MessageBox.Show("Не удалось построить календарь из актуальных данных. " + ex.Message);
                return;
            }
            long modelMs = timing.ElapsedMilliseconds - readMs;
            phase.Restart();
            LoadTextBoxWithDate();//493
            metrics.BuildDatesMs = phase.ElapsedMilliseconds;
            phase.Restart();
            LoadHorizontLine();
            DrowVerticalLines();
            metrics.BuildLinesMs = phase.ElapsedMilliseconds;
            SaveMinMaxDate();
            plForDate.Enabled = true;
            LoadScrolls();
            FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.Ready;
            activePaintMetrics = metrics;
            phase.Restart();
            LoadPersonButtons();
            metrics.AttachPersonsMs = phase.ElapsedMilliseconds;
            phase.Restart();
            LoadTaskButtons(metrics);
            metrics.AttachTasksMs = phase.ElapsedMilliseconds;
            phase.Restart();
            LoadDateTextBoxes();
            metrics.AttachDatesMs = phase.ElapsedMilliseconds;
            ChangePanelVisibility(Statuses.Visibility.Visible);//313
            PushLableProject();
            LoadScrolls();
            long totalMs = timing.ElapsedMilliseconds;
            long controlsMs = totalMs - readMs - modelMs;
            Trace.WriteLine("TimeSchedule refresh: disk-read-ms=" + readMs +
                " model-and-layout-ms=" + modelMs + " controls-ms=" +
                controlsMs + " total-ms=" + totalMs);
            Trace.WriteLine("TimeSchedule refresh-detail: tasks-total=" + metrics.TotalTasks +
                " tasks-filtered=" + metrics.FilteredTasks +
                " task-models=" + metrics.TaskModels +
                " task-buttons-created=" + metrics.CreatedTaskButtons +
                " task-buttons-attached=" + metrics.AttachedTaskButtons +
                " persons=" + metrics.PersonModels +
                " clear-ms=" + metrics.ClearMs +
                " project-filter-ms=" + metrics.ProjectFilterMs +
                " relevance-filter-ms=" + metrics.RelevanceFilterMs +
                " build-person-buttons-ms=" + metrics.BuildPersonButtonsMs +
                " build-task-buttons-ms=" + metrics.BuildTaskButtonsMs +
                " build-dates-ms=" + metrics.BuildDatesMs +
                " build-lines-ms=" + metrics.BuildLinesMs +
                " attach-persons-ms=" + metrics.AttachPersonsMs +
                " attach-tasks-ms=" + metrics.AttachTasksMs +
                " attach-dates-ms=" + metrics.AttachDatesMs +
                " other-model-ms=" + (modelMs - metrics.ClearMs -
                    metrics.ProjectFilterMs - metrics.RelevanceFilterMs -
                    metrics.BuildPersonButtonsMs - metrics.BuildTaskButtonsMs) +
                " other-controls-ms=" + (controlsMs - metrics.BuildDatesMs -
                    metrics.BuildLinesMs - metrics.AttachPersonsMs -
                    metrics.AttachTasksMs - metrics.AttachDatesMs));
            // Отложенный Paint может выполниться только после выхода из LoadRefreshForm.
            Stopwatch idleWait = Stopwatch.StartNew();
            EventHandler idleHandler = null;
            idleHandler = (sender, e) =>
            {
                Application.Idle -= idleHandler;
                if (ReferenceEquals(pendingRefreshIdleHandler, idleHandler))
                    pendingRefreshIdleHandler = null;
                if (ReferenceEquals(activePaintMetrics, metrics))
                    CancelPendingPaintDiagnostics();
                Trace.WriteLine("TimeSchedule refresh-paint: idle-after-refresh-ms=" +
                    idleWait.ElapsedMilliseconds +
                    " task-paint-events=" + metrics.TaskPaintEvents +
                    " calendar-paint-events=" + metrics.CalendarPaintEvents +
                    " note=idle-latency-is-not-pure-paint-time");
            };
            pendingRefreshIdleHandler = idleHandler;
            Application.Idle += idleHandler;
        }
        private void PushLableProject()
        {
            TreeProjects treeProjects = selectionTreeForRefresh;
            if (treeProjects == null)
            {
                treeProjects = new TreeProjects();
                treeProjects.GetTreeFromFile();
            }
            lblProjects.Text = "";
            foreach (TreeNode treeNode in treeProjects.ListTreeNode)
            {
                if (treeNode.Checked)
                {
                    lblProjects.Text += "<" + treeNode.Text + ">";
                    break;
                }
                PushLableProject(treeNode);
            }
        }
        private void PushLableProject(TreeNode treeNode)
        {
            foreach (TreeNode chTreeNode in treeNode.Nodes)
            {
                if (chTreeNode.Checked)
                {
                    lblProjects.Text += "<" + chTreeNode.Text + "> ";
                }
                else
                {
                    PushLableProject(chTreeNode);
                }
            }
        }

        public Panel SetPlMain()
        {
            return plMain;
        }
        public Panel SetPlForDate()
        {
            return plForDate;
        }
        public Panel SetPlForPerson()
        {
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
            return plMain.Location.X - plPersonButton.Location.X - 2;
        }
        private void ClearOldExemplars()
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
            Bmp = new Bitmap(1, 1);
        }
        private int SetMaxLocationAndAddPersonButton(ref ListPersonButton listPersonButton)
        {
            int maxButtonLocationY = 0;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    personButton.SetLocation(0, maxButtonLocationY);
                    maxButtonLocationY += (personButton.Button.Height + Constants.MIN_ROW_HIGHT);
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
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    int locationY = personButton.Button.Location.Y;
                    Pen pen = new Pen(Color.FromArgb(225, 225, 230), 1);
                    while (locationY < (personButton.Button.Location.Y + personButton.Button.Height))
                    {
                        locationY += Constants.ROW_HIGHT;
                        Drow(pen, 0, locationY, pBForLine.Width, locationY);

                    }

                    Pen pen1 = new Pen(Color.FromArgb(245, 245, 248), 2);
                    Drow(pen1, 0, locationY, pBForLine.Width, locationY + 1);
                }
            }
        }
        public void DrowVerticalLines()
        {
            Pen penGrey = new Pen(Color.FromArgb(225, 225, 230), 1);
            Pen penForMonday = new Pen(Color.FromArgb(245, 245, 248), 2);
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
            LastMaxHorizonntalScroll = plMain.HorizontalScroll.Maximum;
            LastLargeChangeHorizonntalScroll = plMain.HorizontalScroll.LargeChange;
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
            try
            {
                plPersonButton.VerticalScroll.Value = VerticalScrollValue;
                plPersonButton.VerticalScroll.Value = VerticalScrollValue;

            }
            catch { }
            try
            {
                plMain.VerticalScroll.Value = VerticalScrollValue;
                plMain.VerticalScroll.Value = VerticalScrollValue;
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
            MaxDateFinish = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks();
            MinDateStart = Program.ListTasksAllPersonToShow.GetMinDateStartTasks();
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
        public void LoadRefreshFormForDelegat()
        {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }
        public void LoadRefreshForm(Statuses.ProgressBar statusProgressBar)
        {
            timer1.Interval = 900000;
            if (statusProgressBar == Statuses.ProgressBar.Use)
            {
                fmProgressBar fmProgressBar = new fmProgressBar();
                fmProgressBar.SetTextMessege("Идет обновление");
                fmProgressBar.StartPosition = FormStartPosition.Manual;
                fmProgressBar.Location = new Point(
                    this.Left + this.Width / 2 - fmProgressBar.Width / 2,
                    this.Top + this.Height / 2 - fmProgressBar.Height / 2);

                List<Form> listForm = new List<Form>() { this, fmProgressBar };
                //Thread thread = new Thread(LoadRefreshWithProgressBarr, 0);
                fmProgressBar.TopLevel = true;
                System.Threading.Tasks.Task.Run(() => fmProgressBar.ShowDialog());
                LoadRefreshWithProgressBarr(listForm);
                //LoadRefreshWithProgressBarr(listForm);
                OpenFormsCount = Application.OpenForms.Count;
                //Test 2
            }
        }
        public void LoadRefreshForm(fmProgressBar fmProgressBar)
        {
            LoadRefreshForm(RefreshType.All);
            //fmProgressBar.BeginInvoke(new Action(()=>fmProgressBar.Close()));
            //fmProgressBar.Close();
        }
        public void LoadRefreshWithProgressBarr(object listForm)
        {
            //await System.Threading.Tasks.Task.Delay(50);
            List<Form> result = new List<Form>();
            do
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form.Visible)
                        result.Add(form);
                }
                Thread.Sleep(50);
            }
            while (result.Count <= OpenFormsCount);
            fmProgressBar fmProgressBar = (listForm as List<Form>)[1] as fmProgressBar;
            Form1 form1 = (listForm as List<Form>)[0] as Form1;
            LoadRefreshForm(fmProgressBar);
            Invoke(new Action(() => fmProgressBar.Close()));
        }
        /// <summary>Строит элементы календаря из уже прочитанного согласованного снимка.</summary>
        private void LoadAllPools(ListTasks loadedTasks, ListPersons loadedPersons, CalendarRefreshMetrics metrics)
        {
            Stopwatch phase = Stopwatch.StartNew();
            if (loadedTasks != null)
            {
                Program.ListTasksAllPersonToSave = loadedTasks;
                Program.listPersons = loadedPersons;
            }
            else if (TaskStore.HasTaskData(Dals.TakeMainPathFile(Constants.TASKS_BIN)))
            {
                Program.ListTasksAllPersonToSave = TaskStore.ReadProject(
                    Dals.TakeMainPathFile(Constants.TASKS_BIN), out Program.listPersons);
            }
            else
            {
                Program.ListTasksAllPersonToSave.SetTasksFromList(Dals.ReadListFromMainPathFile(Constants.TASKS));
            }
            metrics.TotalTasks = Program.ListTasksAllPersonToSave.Tasks.Count;
            phase.Restart();
            Program.ListTasksAllPersonToShow.SetTasks(GetProjectEntryTasks(Program.ListTasksAllPersonToSave.Tasks));
            metrics.ProjectFilterMs = phase.ElapsedMilliseconds;
            phase.Restart();
            if (cBxShowTask.Text == cBxShowTask.Items[0].ToString())
                Program.ListTasksAllPersonToShow.SetTasks(GetRelevantTasks(Program.ListTasksAllPersonToShow.Tasks));
            metrics.RelevanceFilterMs = phase.ElapsedMilliseconds;
            metrics.FilteredTasks = Program.ListTasksAllPersonToShow.Tasks.Count;
            if (!TaskStore.HasTaskData(Dals.TakeMainPathFile(Constants.TASKS_BIN)))
                ReReadListPerson();

            phase.Restart();
            Program.ListPersonButton.LoadListPersonButtons(
                Program.listPersons.Persons,
                Program.ListTasksAllPersonToShow,
                Constants.ROW_HIGHT,
                this);
            int maxButtonLocationY = SetMaxLocationAndAddPersonButton(ref Program.ListPersonButton);
            pBForLine.Height = maxButtonLocationY;
            metrics.BuildPersonButtonsMs = phase.ElapsedMilliseconds;
            metrics.PersonModels = Program.ListPersonButton.PersonButtons.Count;
            if (File.Exists(Dals.TakeMainPathFile(Constants.HOLYDAYS_BIN)))
            {
                Program.ListHolidays = Dals.binReadFileToObject(
                    Program.ListHolidays,
                    Dals.TakeMainPathFile(Constants.HOLYDAYS_BIN));
            }
            else
            {
                Program.ListHolidays.SetHolidaysFromList(
                    Dals.ReadListFromMainPathFile(Constants.HOLYDAYS));
            }

            NonWorkDaysWrite(Program.ListTasksAllPersonToShow.GetMinDateStartTasks(), Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks());
            Program.listNonWorkingDays.NonWorkingDays.AddRange(Program.ListHolidays.Holidays);
            phase.Restart();
            Program.ListTaskButtons.AddTaskButtons(
                Program.ListTasksAllPersonToShow,
                Program.ListPersonButton);
            metrics.BuildTaskButtonsMs = phase.ElapsedMilliseconds;
            metrics.TaskModels = Program.ListTaskButtons.TaskButtons.Count;
            metrics.CreatedTaskButtons = Program.ListTaskButtons.TaskButtons.Sum(task => task.Buttons.Count);
        }

        /// <summary>Читает согласованную версию списка исполнителей.</summary>
        public void ReReadListPerson()
        {
            if (TaskStore.HasTaskData(Dals.TakeMainPathFile(Constants.TASKS_BIN)))
            {
                Program.listPersons = TaskStore.ReadPersons(
                    Dals.TakeMainPathFile(Constants.PERSONS_BIN));
            }
            else
            {
                Program.listPersons.SetPersonsFromList(
                 Dals.ReadListFromMainPathFile(Constants.PERSONS),
                 Program.ListTasksAllPersonToShow.Tasks);
            }
        }

        private List<Task> GetProjectEntryTasks(List<Task> tasks)
        {
            var resultListTasks = new List<Task>();
            TreeProjects treeProjects = new TreeProjects();
            treeProjects.GetTreeFromFile();
            selectionTreeForRefresh = treeProjects;
            if (treeProjects.ListTreeNode.Count == 0)
            {
                return resultListTasks = tasks.GetRange(0, tasks.Count);
            }
            foreach (TreeNode treeNodeProject in treeProjects.ListTreeNode)
            {
                if (treeNodeProject.Text == "Все" && treeNodeProject.Checked != false)
                {
                    return resultListTasks = tasks.GetRange(0, tasks.Count);
                }
            }
            foreach (Task task in tasks)
            {

                TreeProjects treeTask = new TreeProjects();
                treeTask = task.GetTreeProjects();
                if (CheckEntryProject(treeProjects, treeTask))
                {
                    resultListTasks.Add(task);
                }


            }
            return resultListTasks;
        }
        private List<Task> GetRelevantTasks(List<Task> tasks)
        {
            var resultListTasks = new List<Task>();
            foreach (Task task in tasks)
            {
                if (IsDateSuitable(task))
                {
                    resultListTasks.Add(task);
                }
            }
            return resultListTasks;
        }
        private bool IsDateSuitable(Task task)
        {
            return IsRelevant(task) || IsNotStarted(task) || IsOverdue(task);
        }
        private static bool IsRelevant(Task task)
        {
            return task.DateFinish >= DateTime.Now.Date.AddMonths(-1);
        }
        private bool IsOverdue(Task task)
        {
            return (task.DateFinish.Date < DateTime.Now.Date.AddMonths(-1)) && (task.Status != TaskStatus.Closed);
        }
        private bool IsNotStarted(Task task)
        {
            return (task.DateStart.Date < DateTime.Now.Date.AddMonths(-1)) && (task.Status == TaskStatus.New);
        }
        private static bool CheckEntryProject(TreeProjects treeProjects, TreeProjects treeTask)
        {
            bool isEntry = false;
            foreach (TreeNode taskNode in treeTask.ListTreeNode)
            {
                foreach (TreeNode projectNode in treeProjects.ListTreeNode)
                {
                    if (taskNode.Text == projectNode.Text &&
                        taskNode.Checked == true &&
                        projectNode.Checked == true)
                    {
                        isEntry = true;
                        return isEntry;
                    }
                    if (taskNode.Text == projectNode.Text)
                        isEntry = CheckEntryProject(projectNode, taskNode, isEntry);
                }
            }
            return isEntry;
        }
        private static bool CheckEntryProject(TreeNode treeProjects, TreeNode treeTask, bool isEntry)
        {
            foreach (TreeNode taskNode in treeTask.Nodes)
            {
                foreach (TreeNode projectNode in treeProjects.Nodes)
                {
                    if (taskNode.Text == projectNode.Text &&
                        taskNode.Checked == true &&
                        projectNode.Checked == true)
                    {
                        isEntry = true;
                        return isEntry;
                    }
                    if (taskNode.Text == projectNode.Text)
                        isEntry = CheckEntryProject(projectNode, taskNode, isEntry);
                }
            }
            return isEntry;
        }
        private static void ClearAllPools()
        {
            Program.PoolTextBox.ListTextBoxes.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListTasksAllPersonToSave.Tasks.Clear();
            Program.ListTasksAllPersonToShow.Tasks.Clear();
            Program.listPersons.Persons.Clear();
            Program.ListPersonButton.PersonButtons.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
            Program.ListHolidays.Holidays.Clear();
            Program.listNonWorkingDays.NonWorkingDays.Clear();
            Program.ListTaskButtons.TaskButtons.Clear();
        }
        private void ChangePanelVisibility(Statuses.Visibility visibility)
        {
            if (visibility == Statuses.Visibility.Visible)
            {
                plMain.Visible = true;
                plForDate.Visible = true;
                plPersonButton.Visible = true;
            }
            if (visibility == Statuses.Visibility.Invisible)
            {
                plForDate.Visible = false;
                plPersonButton.Visible = false;
                plMain.Visible = false;
            }
        }
        private void LoadTextBoxWithDate()
        {
            int scrollValue = HorizontalScrollValue;
            DateTime dateToTables = Program.ListTasksAllPersonToShow.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks();
            int height = plMain.Location.Y - plForDate.Location.Y;
            int locationX = 0;
            plForDate.Controls.Clear();
            Bitmap bmp = new Bitmap(1, 1);

            PictureBox pb = new PictureBox();
            pb.Height = 1;
            pb.Location = new Point(HorizontalScrollValue, 1);
            plForDate.Controls.Add(pb);
            pb.Image = bmp;

            DateTextBox dateTextBox = new DateTextBox(dateToTables, height, locationX);
            while (dateToTables <= dateMaxToTable)
            {
                if (IsNotHolyday(dateToTables))
                {
                    dateTextBox = new DateTextBox(dateToTables, height, locationX);
                    dateTextBox.TextBox.BringToFront();
                    dateTextBox.LocationX = locationX;
                    dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.NotReady);
                    locationX += Constants.COLUMN_WITH;
                    Program.PoolTextBox.ListTextBoxes.Add(dateTextBox);
                }
                dateToTables = dateToTables.AddDays(1);
            }

            if (LastMaxHorizonntalScroll >= locationX + Constants.COLUMN_WITH && HorizontalScrollValue > LastMaxHorizonntalScroll - LastLargeChangeHorizonntalScroll)
            {
                HorizontalScrollValue -= LastMaxHorizonntalScroll - locationX;
            }
            ResizePlForTaskes();
            ResizePlForDate();
            void ResizePlForDate()
            {
                try
                {
                    var b = new Bitmap(locationX, bmp.Height);
                    using (Graphics g = Graphics.FromImage((Image)b))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmp, 0, 0, bmp.Width * 4, bmp.Height);
                    }
                    bmp = b;
                    pb.Width = bmp.Width;
                    pb.Image = bmp;
                }
                catch
                {

                }

            }
            void ResizePlForTaskes()
            {
                pBForLine.Width = locationX;
                ResizeImage(new Size(locationX, pBForLine.Height));
            }
        }
        private static bool IsNotHolyday(DateTime dateToTables)
        {
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
            SetNewTextForForm();
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
            while (вeginningPeriod.AddDays(i) < endPeriod ||
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
            //Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
            //Dals.WriteObjectToMainPathFile(Constants.PERSONS_BIN, Program.listPersons);

        }
        private void ToolStripMenuProject_Click(object sender, EventArgs e)
        {

        }


        public void ScrollToDate(DateTime targetDateTime)
        {
            if (!plForDate.HorizontalScroll.Visible)
            {
                return;
            }
            int locationX = 0;
            foreach (DateTextBox DateTextBox in Program.PoolTextBox.ListTextBoxes)
            {
                if (DateTextBox.Date.Date < targetDateTime)
                {
                    locationX += Constants.COLUMN_WITH;
                }
                else
                {
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
        private void HorizontalScrollAdd(int value)
        {
            try
            {
                plMain.HorizontalScroll.Value += value * Constants.COLUMN_WITH;
                plMain.HorizontalScroll.Value += value * Constants.COLUMN_WITH;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            }
            catch
            {

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
                    g.DrawImage(b, 0, 0, size.Width, size.Height);
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
            MessageBox.Show("" + plForDate.HorizontalScroll.Value);
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
            if (Form.ActiveForm == this)
            {
                ShowProgressbarAndMakeRefresh();
            }
            else
            {
                LoadRefreshForm();
            }
            timer1.Interval = 900000;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dals.WriteMainPathFolder("Проект", Statuses.WorkWithProject.NewProject);
            SetNewTextForForm();
            ZeroingScrolss();
            LoadRefreshForm(Statuses.ProgressBar.Use);
            ScrollToDate(DateTime.Now.Date);
        }
        private void выбратьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dals.WriteMainPathFolder("", Statuses.WorkWithProject.LoadProject);
            SetNewTextForForm();
            ZeroingScrolss();
            LoadRefreshForm(Statuses.ProgressBar.Use);
            ScrollToDate(DateTime.Now.Date);
        }
        private void ZeroingScrolss()
        {
            HorizontalScrollValue = 0;
            VerticalScrollValue = 0;
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
            PlForDateScrollXSaved = 0;
            PlMainScrollXSaved = 0;
            PlMainScrollYSaved = 0;
            PlPersonButtonYSaved = 0;

        }
        /// <summary>Обновляет заголовок окна при открытии или смене проекта.</summary>
        private void SetNewTextForForm()
        {
            this.Text = Dals.ProjectFolderPath.Replace("\\Проект", string.Empty);
            this.Activate();
        }
        private void LoadPersonButtons()
        {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
            {
                if (personButton.Person.ListTask.Tasks.Count > 0)
                {
                    plPersonButton.Controls.Add(personButton.Button);
                    personButton.Button.BringToFront();
                }

            }
        }
        /// <summary>Присоединяет готовые кнопки задач; при диагностике отдельно считает их количество.</summary>
        private void LoadTaskButtons(CalendarRefreshMetrics metrics = null)
        {
            // События Scroll/SizeChanged могут добавить кнопки во время текущего обновления.
            metrics = metrics ?? activePaintMetrics;
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            Program.ListTaskButtons.CalculateLoadingStatus(
                HorizontalScrollValue,
                this.Width,
                Program.ListTasksAllPersonToShow.GetMinDateStartTasks());
            Program.ListTaskButtons.SetNewLocationToButtons(
                VerticalScrollValue,
                HorizontalScrollValue);
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
            {
                if (taskButton.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad)
                {
                    foreach (Button button in taskButton.Buttons)
                    {
                        if (metrics != null)
                        {
                            button.Paint += ObserveTaskButtonPaint;
                            metrics.ObservedButtons.Add(button);
                        }
                        plMain.Controls.Add(button);
                        button.BringToFront();
                        if (metrics != null)
                            metrics.AttachedTaskButtons++;
                    }
                    taskButton.SetLoadingStatus(Statuses.LoadingStatus.Loaded);
                }
            }
        }
        private void LoadDateTextBoxes()
        {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            Program.PoolTextBox.CalculateLoadingStatus(
                plMain.HorizontalScroll.Value,
                this.Width,
                Program.ListTasksAllPersonToShow.GetMinDateStartTasks());
            Program.PoolTextBox.SetNewLocationToButtons(
            plMain.HorizontalScroll.Value);
            foreach (DateTextBox dateTextBox in Program.PoolTextBox.ListTextBoxes)
            {
                if (dateTextBox.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad)
                {
                    plForDate.Controls.Add(dateTextBox.TextBox);
                    dateTextBox.TextBox.BringToFront();
                    dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.Loaded);
                }
            }

        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            LoadTaskButtons();
            LoadDateTextBoxes();
            this.VerticalScrollValue = this.plMain.VerticalScroll.Value;
            if (this.WindowState == FormWindowState.Minimized)
            {
                ProgramWindowState = Statuses.ProgramWindowState.Minimized;
                timer1.Interval = 100;
                timer1.Enabled = false;
                return;
            }
            if (this.WindowState == FormWindowState.Normal || this.WindowState == FormWindowState.Maximized)
            {
                ProgramWindowState = Statuses.ProgramWindowState.Normal;
                timer1.Enabled = true;
            }

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void Projects_Click_2(object sender, EventArgs e)
        {
            fmProjectTree fmProjectTree = new fmProjectTree();
            fmProjectTree.StartPosition = FormStartPosition.CenterParent;
            //fmProjectTree.SetTreeView(Dals.binReadMainPathFileToObject(
            //    new TreeProjects(),
            //    Constants.PROJECTS_LIST));
            TreeProjects treeProjectsFromFile = new TreeProjects();
            treeProjectsFromFile.GetTreeFromFile();
            fmProjectTree.SetTreeView(treeProjectsFromFile);
            fmProjectTree.SetHasLoad(HasLoad.Yes);
            fmProjectTree.ShowDialog();
            TreeProjects treeProjects = new TreeProjects();
            treeProjects.SetTreeViewProjects(fmProjectTree.projectTreeView);
            if (fmProjectTree.ClickButton == ClickButton.Aplly)
            {
                cBxSeetingsProjects.Items.Clear();
                cBxSeetingsProjects.Text = "";
                treeProjects.SaveTree();
                treeProjects.SaveSettingsTree();
                ZeroingScrolss();
                EventRefreshFinished += Form1_EventRefreshFinished;
                ShowProgressbarAndMakeRefresh();

                //Dals.WriteObjectToMainPathFile(Constants.PROJECTS_LIST, treeProjects);
            }
            void Form1_EventRefreshFinished()
            {
                Program.fmMain.ScrollToDate(DateTime.Now.Date);
                EventRefreshFinished -= Form1_EventRefreshFinished;
            }

        }



        private void plMain_Resize(object sender, EventArgs e)
        {
            if (plMain.VerticalScroll.Visible == true)
            {
                plForDate.Width = plMain.Width - 18;
            }
            if (plMain.VerticalScroll.Visible == false)
            {
                plForDate.Width = plMain.Width;
            }
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            cBxSeetingsProjects.Items.Clear();
            cBxSeetingsProjects.Text = "";
            string fileName = Dals.ProjectFolderPath.Replace('\\', '_');
            fileName = fileName.Replace(':', '+');
            List<TreeProjects> listTreeProject = new List<TreeProjects>();
            listTreeProject = Dals.binReadUserPathFileToObject(listTreeProject, fileName);
            List<string> projectNames = new List<string>();
            foreach (TreeProjects treeProjects in listTreeProject)
            {
                projectNames.Add(treeProjects.GetName());
            }
            cBxSeetingsProjects.Items.AddRange(projectNames.ToArray());
        }
        private void cBxSeetingsProgects_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fileName = Dals.ProjectFolderPath.Replace('\\', '_');
            fileName = fileName.Replace(':', '+');
            List<TreeProjects> listTreeProject = new List<TreeProjects>();
            listTreeProject = Dals.binReadUserPathFileToObject(listTreeProject, fileName);
            foreach (TreeProjects treeProjects in listTreeProject)
            {
                if (cBxSeetingsProjects.SelectedItem == null)
                {
                    break;
                }
                if (treeProjects.GetName() == cBxSeetingsProjects.SelectedItem.ToString())
                {
                    treeProjects.SaveSettingsTree();
                }
            }
            ZeroingScrolss();
            EventRefreshFinished += Form1_EventRefreshFinished;
            ShowProgressbarAndMakeRefresh();

            //Dals.WriteObjectToMainPathFile(Constants.PROJECTS_LIST, treeProjects);

            void Form1_EventRefreshFinished()
            {
                Program.fmMain.ScrollToDate(DateTime.Now.Date);
                EventRefreshFinished -= Form1_EventRefreshFinished;
            }

        }

        private void cBxShowTask_SelectedIndexChanged(object sender, EventArgs e)
        {
            ZeroingScrolss();
            ShowProgressbarAndMakeRefresh();
            Program.fmMain.ScrollToDate(DateTime.Now.Date);
        }

        private void btnSetings_Click(object sender, EventArgs e)
        {
            fmChangeList fm = new fmChangeList();
            fm.Text = "Сохраненные настройки";
            fm.StartPosition = FormStartPosition.CenterParent;
            fm.TopMost = true;
            fm.BtnChange.Text = "Изменить название";
            fm.BtnNew.Text = "Добавить";
            fm.BtnNew.Enabled = false;
            fm.BtnDelete.Text = "Удалить";
            fm.LblReplace.Text = "Переместить \n настройки";
            string fileName = Dals.ProjectFolderPath.Replace('\\', '_');
            fileName = fileName.Replace(':', '+');
            SetLbx(fm, fileName);
            var listStr = new List<string>();

            fm.ShowDialog();
            if (fm.BtnClick == BtnClick.aplly)
            {
                SaveListProjects(fm, fileName);
            }
        }

        private static void SaveListProjects(fmChangeList fm, string fileName)
        {
            List<TreeProjects> listTreeForChange = new List<TreeProjects>();
            List<TreeProjects> listTreeForSave = new List<TreeProjects>();
            if (File.Exists(Dals.TakeUserPath(fileName)))
            {
                listTreeForChange = Dals.binReadUserPathFileToObject(listTreeForChange, fileName);
            }
            foreach (StringNumChange stringNumChange in fm.ListStringNumChange)
            {
                listTreeForChange[stringNumChange.Num].SetName(stringNumChange.Str);
            }
            foreach (string str in fm.LBx.Items)
            {
                foreach (TreeProjects tree in listTreeForChange)
                {
                    if (tree.GetName() == str)
                    {
                        listTreeForSave.Add(tree);
                        break;
                    }
                }
            }
            Dals.WriteObjectToUserPathFile(fileName, listTreeForSave);
        }

        private static void SetLbx(fmChangeList fm, string fileName)
        {
            List<TreeProjects> listTreeProjects = new List<TreeProjects>();
            if (File.Exists(Dals.TakeUserPath(fileName)))
            {
                listTreeProjects = Dals.binReadUserPathFileToObject(listTreeProjects, fileName);
            }
            List<string> listString = new List<string>();
            foreach (TreeProjects targetTreeProjects in listTreeProjects)
            {
                listString.Add(targetTreeProjects.GetName());
            }
            fm.LBx.Items.AddRange(listString.ToArray());
        }

        /// <summary>Восстанавливает повреждённый HEAD или выбранную независимую копию задач.</summary>
        private void восстановлениеЗадачToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tasksPath = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            if (TaskStore.UseCompactStore)
            {
                CompactHeadRecoveryInfo candidate;
                try { candidate = TaskStore.FindCompactHeadRecovery(tasksPath); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                    ex is InvalidDataException || ex is System.Runtime.Serialization.SerializationException ||
                    ex is AggregateException || ex is ArgumentException || ex is FormatException)
                {
                    MessageBox.Show("Восстановление HEAD из history недоступно. " +
                        "Можно выбрать независимую резервную копию задач.\n\n" + ex.Message,
                        "Восстановление задач", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    candidate = null;
                }
                if (candidate != null)
                {
                    string message = "Основной манифест задач отсутствует или повреждён.\n\n" +
                        "Проверен резервный снимок из history:\n" +
                        "Ревизия: " + candidate.Generation + "\n" +
                        "Задач: " + candidate.TaskCount + "\n" +
                        "Сегментов: " + candidate.SegmentCount + "\n\n" +
                        "Изменения, выполненные после этой ревизии, могут быть утрачены.\n" +
                        "Восстановить этот снимок?";
                    DialogResult choice = MessageBox.Show(message +
                        "\n\nНет — выбрать независимую резервную копию задач.",
                        "Подтверждение восстановления HEAD", MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);
                    if (choice == DialogResult.Cancel) return;
                    if (choice == DialogResult.Yes)
                    {
                        try
                        {
                            Program.ListTasksAllPersonToSave = TaskStore.RestoreCompactHead(tasksPath, candidate);
                            LoadRefreshForm(Statuses.ProgressBar.Use);
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                            ex is InvalidDataException || ex is System.Runtime.Serialization.SerializationException ||
                            ex is AggregateException || ex is ArgumentException || ex is FormatException)
                        {
                            MessageBox.Show("Восстановление HEAD не выполнено.\n\n" + ex.Message,
                                "Ошибка восстановления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        return;
                    }
                }
            }
            fmChangeList fm = new fmChangeList();
            fm.Text = "Файлы";
            fm.StartPosition = FormStartPosition.CenterParent;
            fm.TopMost = true;
            fm.BtnChange.Visible = false;
            fm.BtnNew.Visible = false;
            fm.BtnDelete.Visible = false;
            fm.LblReplace.Visible = false;
            fm.BtnUp.Visible = false;
            fm.BtnDown.Visible = false;
            fm.BtnApply.Text = "Восстановить";
            fm.BtnApply.Enabled = false;
            fm.GroupBox1.Visible = false;
            fm.LBx.Width = fm.Width - fm.LBx.Location.X * 3;
            var availableBackups = new List<KeyValuePair<string, string>>();
            var usedBackupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (TaskStore.UseCompactStore)
            {
                string compactBackups = CompactTaskStore.BackupDirectory(tasksPath);
                if (Directory.Exists(compactBackups))
                    foreach (DirectoryInfo snapshot in new DirectoryInfo(compactBackups)
                        .GetDirectories("*.snapshot").OrderByDescending(d => d.CreationTimeUtc)
                        .ThenByDescending(d => d.Name, StringComparer.Ordinal))
                    {
                        string displayName = CompactTaskStore.BackupDisplayName(snapshot);
                        string uniqueName = displayName;
                        int duplicate = 2;
                        while (!usedBackupNames.Add(uniqueName))
                        {
                            uniqueName = displayName + "_" + duplicate.ToString("D2");
                            duplicate++;
                        }
                        availableBackups.Add(new KeyValuePair<string, string>(uniqueName, snapshot.FullName));
                    }
            }
            string oldBackups = Dals.TakeBackUpPathDirectory(Constants.TASKS_BIN);
            foreach (FileInfo oldFile in new DirectoryInfo(oldBackups)
                .GetFiles().OrderByDescending(f => f.CreationTimeUtc))
            {
                string displayName = TaskStore.UseCompactStore ? "Архивная_" + oldFile.Name :
                    oldFile.Name.Replace('_', ':');
                string uniqueName = displayName;
                int duplicate = 2;
                while (!usedBackupNames.Add(uniqueName))
                {
                    uniqueName = displayName + "_" + duplicate.ToString("D2");
                    duplicate++;
                }
                availableBackups.Add(new KeyValuePair<string, string>(uniqueName, oldFile.FullName));
            }
            if (availableBackups.Count == 0)
            {
                MessageBox.Show("Резервные копии задач для этого проекта не найдены.",
                    "Восстановление задач", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            fm.LBx.Items.AddRange(availableBackups.Select(x => x.Key).ToArray());
            fm.LBx.Click += LBx_Click;
            void LBx_Click(object newSender, EventArgs newE)
            {
                if (fm.LBx.SelectedIndex != -1)
                {
                    fm.BtnApply.Enabled = true;
                }
                if (fm.LBx.SelectedIndex == -1)
                {
                    fm.BtnApply.Enabled = false;
                }
            }
            string openedRevision;
            try { openedRevision = TaskStore.RevisionForRestore(tasksPath); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is InvalidDataException || ex is System.Runtime.Serialization.SerializationException)
            {
                MessageBox.Show("Не удалось проверить состояние задач: " + ex.Message);
                return;
            }
            fm.ShowDialog();
            if (fm.BtnClick == BtnClick.aplly && fm.LBx.SelectedItem != null)
            {
                string fileName = availableBackups[fm.LBx.SelectedIndex].Value;
                if (MessageBox.Show("Заменить все текущие задачи выбранной копией? " +
                    "Если текущее хранилище исправно, его состояние будет сохранено в резервной копии. " +
                    "Повреждённые файлы, когда возможно, сохраняются для диагностики.",
                    "Подтверждение восстановления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                try
                {
                    Program.ListTasksAllPersonToSave = TaskStore.Restore(tasksPath, fileName, openedRevision);
                    Program.fmMain.LoadRefreshForm(Statuses.ProgressBar.Use);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                    ex is InvalidDataException || ex is System.Runtime.Serialization.SerializationException ||
                    ex is AggregateException || ex is ArgumentException || ex is FormatException)
                {
                    MessageBox.Show("Восстановление не выполнено: " + ex.Message);
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FrmAddProject addProject = new FrmAddProject();
            addProject.ShowDialog();
        }

        private void NewRefresh_Click(object sender, EventArgs e)
        {
            ShowProgressbarAndMakeRefresh();
        }

        public void ShowProgressbarAndMakeRefresh()
        {
            fmProgressBar frm = new fmProgressBar();
            frm.SetTextMessege("Идет обновление");
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = new Point(
                this.Left + this.Width / 2 - frm.Width / 2,
                this.Top + this.Height / 2 - frm.Height / 2);
            System.Threading.Tasks.Task tsk = new System.Threading.Tasks.Task(() =>
            {
                Invoke(new Action(() =>
                {
                    LoadRefreshForm(RefreshType.All);
                    EventRefreshFinished?.Invoke();
                }));
                Thread.Sleep(10);
                frm.Invoke(new Action(() => frm.Close()));
            });
            frm.Load += Frm_EventLoadFinished;
            System.Threading.Tasks.Task.Run(()=>frm.ShowDialog());
             void Frm_EventLoadFinished(object s, EventArgs ev)
            {
                tsk.Start();
                
            }

        }

        private void butTest_Click(object sender, EventArgs e)
        {
            var txtbox = Program.PoolTextBox.ListTextBoxes[Program.PoolTextBox.ListTextBoxes.Count - 1].TextBox;
            txtbox.Location = new Point(plForDate.ClientSize.Width - Constants.COLUMN_WITH, txtbox.Location.Y);
            plForDate.Controls.Add(txtbox);
        }

        private void cBxShowTask_Click(object sender, EventArgs e)
        {

        }
    }

}
