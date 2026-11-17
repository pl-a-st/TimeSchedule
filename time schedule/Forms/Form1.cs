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
            InitializeCanvasPilot();
            plMain.Paint += ObserveCalendarPaint;
            plForDate.Paint += ObserveCalendarPaint;
            pBForLine.Paint += ObserveCalendarPaint;
            FormClosed += (sender, e) =>
            {
                CancelPendingPaintDiagnostics();
                calendarTaskToolTip.Dispose();
                DisposeUnusedDateTextBoxes();
                DisposeCanvasPilot();
            };
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
            /// <summary>Количество фактически созданных WinForms-кнопок задач после первого присоединения.</summary>
            public int CreatedTaskButtons { get; set; }
            /// <summary>Количество кнопок задач, добавленных в plMain за это обновление.</summary>
            public int AttachedTaskButtons { get; set; }
            /// <summary>Количество геометрических сегментов, рассчитанных для всех задач.</summary>
            public int PlannedTaskButtons { get; set; }
            /// <summary>Количество моделей рабочих дат без WinForms-контролов.</summary>
            public int DateModels { get; set; }
            /// <summary>Количество материализованных текстовых ячеек дат в текущих моделях.</summary>
            public int CreatedDateTextBoxes { get; set; }
            /// <summary>Количество ячеек дат, повторно использованных при присоединении календаря.</summary>
            public int ReusedDateTextBoxes { get; set; }
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
            /// <summary>Время расчёта моделей TaskButton и геометрии их сегментов без создания кнопок.</summary>
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
            /// <summary>Время завершающей подготовки шкалы дат до восстановления прокрутки.</summary>
            public long ScalePrepareMs { get; set; }
            /// <summary>Время восстановления прокрутки до присоединения контролов.</summary>
            public long ScaleScrollMs { get; set; }
            /// <summary>Время изменения видимости панелей после добавления контролов.</summary>
            public long AttachVisibilityMs { get; set; }
            /// <summary>Время установки заголовка выбранного проекта.</summary>
            public long AttachProjectLabelMs { get; set; }
            /// <summary>Время восстановления позиций прокрутки после добавления контролов.</summary>
            public long AttachScrollMs { get; set; }
            /// <summary>Количество событий Paint у прикреплённых кнопок задач.</summary>
            public int TaskPaintEvents { get; set; }
            /// <summary>Количество событий Paint у основных панелей календаря.</summary>
            public int CalendarPaintEvents { get; set; }
            /// <summary>Кнопки с временной подпиской Paint, снимаемой при завершении замера.</summary>
            public List<Button> ObservedButtons { get; } = new List<Button>();
            /// <summary>Время от начала присоединения контролов до их первых Paint-событий.</summary>
            public Stopwatch PaintClock { get; } = new Stopwatch();
            /// <summary>Время первого наблюдавшегося Paint календарных панелей, если он был.</summary>
            public long? FirstCalendarPaintMs { get; set; }
            /// <summary>Время первого наблюдавшегося Paint кнопки задачи, если он был.</summary>
            public long? FirstTaskPaintMs { get; set; }
        }

        /// <summary>Показатели обновления, ожидающего следующего простоя очереди UI.</summary>
        private CalendarRefreshMetrics activePaintMetrics;

        /// <summary>Единственный ToolTip календаря; освобождается при закрытии главного окна.</summary>
        private readonly ToolTip calendarTaskToolTip = new ToolTip();

        /// <summary>Отсоединённые ячейки дат в порядке первоначального присоединения, чтобы сохранять соответствие даты и контрола при повторном использовании.</summary>
        private readonly Queue<TextBox> reusableDateTextBoxes = new Queue<TextBox>();

        /// <summary>Задаёт текст подсказки видимой задачи без создания ToolTip при каждом наведении.</summary>
        /// <param name="button">Полоса задачи, которой назначается подсказка.</param>
        /// <param name="task">Задача, описание которой показывается при наведении.</param>
        internal void RegisterTaskToolTip(Button button, Task task)
        {
            if (!string.IsNullOrEmpty(calendarTaskToolTip.GetToolTip(button)))
                return;
            calendarTaskToolTip.SetToolTip(button, GetTaskToolTipText(task));
        }

        /// <summary>Формирует общую подсказку для WinForms-кнопки и рисуемой полосы задачи.</summary>
        private static string GetTaskToolTipText(Task task)
        {
            string numTaskAfter = task.TaskNumberAfter > 0 ? "псоле №" + task.TaskNumberAfter : string.Empty;
            return "№" + task.Number + "\n" + task.Name + "\n" +
                "до " + task.DateFinish.ToString().Split(' ')[0] + "\n" +
                "Статус: " + ((TaskStatusRus)(int)task.Status).ToString().Replace("_", " ") + "\n" +
                numTaskAfter;
        }

        /// <summary>Обработчик очереди UI, ожидающий завершения ближайшего цикла Paint.</summary>
        private EventHandler pendingRefreshIdleHandler;

        /// <summary>Считает события Paint кнопок задач без вмешательства в их рисование.</summary>
        private void ObserveTaskButtonPaint(object sender, PaintEventArgs e)
        {
            if (activePaintMetrics != null)
            {
                activePaintMetrics.TaskPaintEvents++;
                if (!activePaintMetrics.FirstTaskPaintMs.HasValue)
                    activePaintMetrics.FirstTaskPaintMs = activePaintMetrics.PaintClock.ElapsedMilliseconds;
            }
        }

        /// <summary>Считает события Paint панелей календаря.</summary>
        private void ObserveCalendarPaint(object sender, PaintEventArgs e)
        {
            if (activePaintMetrics != null)
            {
                activePaintMetrics.CalendarPaintEvents++;
                if (!activePaintMetrics.FirstCalendarPaintMs.HasValue)
                    activePaintMetrics.FirstCalendarPaintMs = activePaintMetrics.PaintClock.ElapsedMilliseconds;
            }
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

        /// <summary>Координирует обновление календаря, сохраняя предпросмотр до скрытия старого изображения.</summary>
        public void LoadRefreshForm()
        {
            CancelPendingPaintDiagnostics();
            ResetCanvasPilotPaintMetrics();
            CalendarRefreshMetrics metrics = new CalendarRefreshMetrics();
            Stopwatch timing = Stopwatch.StartNew();
            ListTasks loadedTasks;
            ListPersons loadedPersons;
            if (!TryReadCalendarData(out loadedTasks, out loadedPersons))
            {
                ResetCanvasPilotInteraction();
                return;
            }

            long readMs = timing.ElapsedMilliseconds;
            try
            {
                PrepareCalendarRefresh();
            }
            finally
            {
                // Сбрасываем предпросмотр только после скрытия старого календаря.
                ResetCanvasPilotInteraction();
            }
            Stopwatch phase = Stopwatch.StartNew();
            if (!TryBuildCalendarModels(loadedTasks, loadedPersons, metrics))
                return;

            phase.Restart();
            ClearOldExemplars();
            metrics.ClearMs = phase.ElapsedMilliseconds;
            long modelMs = timing.ElapsedMilliseconds - readMs;
            FinishCalendarRefresh(metrics, phase, timing, readMs, modelMs);
        }

        /// <summary>Читает согласованный снимок задач, сохраняя существующее сообщение при ошибке хранилища.</summary>
        /// <param name="loadedTasks">Загруженные задачи или null при отсутствии нового формата.</param>
        /// <param name="loadedPersons">Загруженные исполнители или null при отсутствии нового формата.</param>
        /// <returns>True при успешном чтении или отсутствии данных нового формата.</returns>
        private bool TryReadCalendarData(out ListTasks loadedTasks, out ListPersons loadedPersons)
        {
            loadedTasks = null;
            loadedPersons = null;
            string tasksPath = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            try
            {
                if (TaskStore.HasTaskData(tasksPath))
                    loadedTasks = TaskStore.ReadProject(tasksPath, out loadedPersons);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException || ex is InvalidCastException ||
                ex is InvalidDataException)
            {
                MessageBox.Show(
                    "Не удалось загрузить задачи проекта. Данные не изменены.\n\n" + ex.Message,
                    "Ошибка хранилища задач", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Подготавливает видимость, прокрутку и готовность календаря перед построением моделей.</summary>
        private void PrepareCalendarRefresh()
        {
            ChangePanelVisibility(Statuses.Visibility.Invisible);
            SaveScrolls();
            if (CalcDivDays() < 0)
                HorizontalScrollAdd(CalcDivDays());
            FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.NotReady;
            plForDate.Enabled = false;
        }

        /// <summary>Создаёт модели, сохраняя возможность возврата предыдущего состояния при ошибке.</summary>
        /// <param name="loadedTasks">Согласованно прочитанные задачи.</param>
        /// <param name="loadedPersons">Согласованно прочитанные исполнители.</param>
        /// <param name="metrics">Показатели обновления.</param>
        /// <returns>True, если новые модели готовы к размещению.</returns>
        private bool TryBuildCalendarModels(ListTasks loadedTasks, ListPersons loadedPersons,
            CalendarRefreshMetrics metrics)
        {
            CalendarModelState previousModels = new CalendarModelState(this);
            try
            {
                InitializeCalendarModels();
                LoadAllPools(loadedTasks, loadedPersons, metrics);
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException || ex is InvalidCastException ||
                ex is InvalidDataException || ex is InvalidOperationException || ex is ArgumentException)
            {
                previousModels.Restore(this);
                plForDate.Enabled = true;
                ChangePanelVisibility(Statuses.Visibility.Visible);
                Trace.WriteLine("TimeSchedule calendar models failed: " + ex);
                MessageBox.Show("Не удалось построить календарь из актуальных данных. " + ex.Message,
                    "Ошибка обновления календаря", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>Размещает подготовленный календарь и сохраняет прежнее поведение при ошибке построения.</summary>
        /// <param name="metrics">Показатели текущего обновления.</param>
        /// <param name="phase">Таймер отдельных фаз.</param>
        /// <param name="timing">Таймер всего обновления.</param>
        /// <param name="readMs">Продолжительность чтения данных.</param>
        /// <param name="modelMs">Продолжительность подготовки моделей.</param>
        private void FinishCalendarRefresh(CalendarRefreshMetrics metrics, Stopwatch phase,
            Stopwatch timing, long readMs, long modelMs)
        {
            try
            {
                BuildCalendarScale(metrics, phase);
                AttachCalendarControls(metrics, phase);
                LogRefreshMetrics(metrics, readMs, modelMs, timing.ElapsedMilliseconds);
                ScheduleRefreshPaintDiagnostics(metrics);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is InvalidDataException ||
                ex is System.Runtime.Serialization.SerializationException ||
                ex is InvalidCastException ||
                ex is InvalidOperationException ||
                ex is ArgumentException ||
                ex is ExternalException)
            {
                CancelPendingPaintDiagnostics();
                FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.NotReady;
                plForDate.Enabled = true;
                ChangePanelVisibility(Statuses.Visibility.Invisible);

                Trace.WriteLine("TimeSchedule refresh failed: " + ex);
                MessageBox.Show(
                    "Не удалось завершить обновление календаря.\n" +
                    "Повторите обновление после устранения причины.\n\n" + ex.Message,
                    "Ошибка обновления календаря",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>Строит шкалу дат и линии календаря, восстанавливая прокрутку перед размещением задач.</summary>
        /// <param name="metrics">Измерения этапов обновления календаря.</param>
        /// <param name="phase">Таймер отдельных этапов построения.</param>
        private void BuildCalendarScale(CalendarRefreshMetrics metrics, Stopwatch phase)
        {
            phase.Restart();
            LoadTextBoxWithDate();
            metrics.BuildDatesMs = phase.ElapsedMilliseconds;
            metrics.DateModels = Program.PoolTextBox.ListTextBoxes.Count;
            phase.Restart();
            LoadHorizontLine();
            DrowVerticalLines();
            metrics.BuildLinesMs = phase.ElapsedMilliseconds;
            phase.Restart();
            SaveMinMaxDate();
            plForDate.Enabled = true;
            metrics.ScalePrepareMs = phase.ElapsedMilliseconds;
            phase.Restart();
            LoadScrolls();
            metrics.ScaleScrollMs = phase.ElapsedMilliseconds;
            FormReadyToBeAddedControl = Statuses.FormReadyToBeAddedControl.Ready;
        }

        /// <summary>Размещает ранее подготовленные элементы календаря и завершает отображение проекта.</summary>
        /// <param name="metrics">Измерения этапов обновления календаря и активной Paint-диагностики.</param>
        /// <param name="phase">Таймер отдельных этапов добавления элементов управления.</param>
        private void AttachCalendarControls(CalendarRefreshMetrics metrics, Stopwatch phase)
        {
            activePaintMetrics = metrics;
            metrics.PaintClock.Start();
            phase.Restart();
            LoadPersonButtons();
            metrics.AttachPersonsMs = phase.ElapsedMilliseconds;
            phase.Restart();
            LoadTaskButtons(metrics);
            metrics.AttachTasksMs = phase.ElapsedMilliseconds;
            metrics.CreatedTaskButtons = Program.ListTaskButtons.TaskButtons.Sum(task => task.Buttons.Count);
            phase.Restart();
            LoadDateTextBoxes(metrics);
            DisposeUnusedDateTextBoxes();
            metrics.AttachDatesMs = phase.ElapsedMilliseconds;
            metrics.CreatedDateTextBoxes = Program.PoolTextBox.ListTextBoxes.Count(date => date.HasCreatedTextBox);
            phase.Restart();
            ChangePanelVisibility(Statuses.Visibility.Visible);
            metrics.AttachVisibilityMs = phase.ElapsedMilliseconds;
            phase.Restart();
            PushLableProject();
            metrics.AttachProjectLabelMs = phase.ElapsedMilliseconds;
            phase.Restart();
            LoadScrolls();
            metrics.AttachScrollMs = phase.ElapsedMilliseconds;
            if (Program.PoolTextBox.ListTextBoxes.Count > 0)
            {
                DateTextBox lastDate = Program.PoolTextBox.ListTextBoxes[Program.PoolTextBox.ListTextBoxes.Count - 1];
                Trace.WriteLine(string.Format(
                    "TimeSchedule calendar-bounds lastDate={0:yyyy-MM-dd} requestedRight={1} " +
                    "gridWidth={2} bitmapWidth={3} datePanelWidth={4} dateScrollMax={5} " +
                    "dateLargeChange={6} mainPanelWidth={7} mainScrollMax={8} " +
                    "mainLargeChange={9} mainScrollValue={10}",
                    lastDate.Date, lastDate.LocationX + Constants.COLUMN_WITH, pBForLine.Width,
                    Bmp == null ? 0 : Bmp.Width, plForDate.ClientSize.Width,
                    plForDate.HorizontalScroll.Maximum, plForDate.HorizontalScroll.LargeChange,
                    plMain.ClientSize.Width, plMain.HorizontalScroll.Maximum,
                    plMain.HorizontalScroll.LargeChange, plMain.HorizontalScroll.Value));
            }
        }

        /// <summary>Записывает ранее собранные измерения обновления календаря без дополнительных чтений данных.</summary>
        /// <param name="metrics">Измерения отдельных этапов построения календаря.</param>
        /// <param name="readMs">Время загрузки данных хранилища.</param>
        /// <param name="modelMs">Время подготовки моделей и компоновки.</param>
        /// <param name="elapsedMs">Общее время до начала диагностического вывода.</param>
        private void LogRefreshMetrics(CalendarRefreshMetrics metrics, long readMs, long modelMs, long elapsedMs)
        {
            long totalMs = elapsedMs;
            long controlsMs = totalMs - readMs - modelMs;
            Trace.WriteLine("TimeSchedule refresh: disk-read-ms=" + readMs +
                " model-and-layout-ms=" + modelMs + " controls-ms=" +
                controlsMs + " total-ms=" + totalMs);
            Trace.WriteLine("TimeSchedule refresh-detail: tasks-total=" + metrics.TotalTasks +
                " tasks-filtered=" + metrics.FilteredTasks +
                " task-models=" + metrics.TaskModels +
                " task-buttons-created=" + metrics.CreatedTaskButtons +
                " task-segments-planned=" + metrics.PlannedTaskButtons +
                " task-buttons-attached=" + metrics.AttachedTaskButtons +
                " date-models=" + metrics.DateModels +
                " date-textboxes-created=" + metrics.CreatedDateTextBoxes +
                " date-textboxes-reused=" + metrics.ReusedDateTextBoxes +
                " controls-main=" + plMain.Controls.Count +
                " controls-dates=" + plForDate.Controls.Count +
                " controls-persons=" + plPersonButton.Controls.Count +
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
            Trace.WriteLine("TimeSchedule refresh-controls-detail: scale-prepare-ms=" + metrics.ScalePrepareMs +
                " scale-scroll-ms=" + metrics.ScaleScrollMs +
                " attach-visibility-ms=" + metrics.AttachVisibilityMs +
                " attach-project-label-ms=" + metrics.AttachProjectLabelMs +
                " attach-scroll-ms=" + metrics.AttachScrollMs +
                " unaccounted-controls-ms=" + (controlsMs - metrics.BuildDatesMs -
                    metrics.BuildLinesMs - metrics.AttachPersonsMs - metrics.AttachTasksMs -
                    metrics.AttachDatesMs - metrics.ScalePrepareMs - metrics.ScaleScrollMs -
                    metrics.AttachVisibilityMs - metrics.AttachProjectLabelMs - metrics.AttachScrollMs));
        }

        /// <summary>Регистрирует одноразовую диагностику Paint до ближайшего простоя UI и снимает подписку.</summary>
        /// <param name="metrics">Измерения завершённого обновления календаря.</param>
        private void ScheduleRefreshPaintDiagnostics(CalendarRefreshMetrics metrics)
        {
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
                    " first-calendar-paint-from-attach-ms=" +
                        (metrics.FirstCalendarPaintMs.HasValue ? metrics.FirstCalendarPaintMs.Value.ToString() : "none") +
                    " first-task-paint-from-attach-ms=" +
                        (metrics.FirstTaskPaintMs.HasValue ? metrics.FirstTaskPaintMs.Value.ToString() : "none") +
                    " note=first-paint-does-not-imply-complete-frame-idle-is-not-pure-paint-time");
                LogCanvasPilotIdleMetrics();
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
        /// <summary>Синхронизирует прокрутку календаря при прокрутке списка исполнителей.</summary>
        private void PlPersonButton_MouseWheel(object sender, MouseEventArgs e)
        {
            try
            {
                plMain.VerticalScroll.Value = plPersonButton.VerticalScroll.Value;
                plMain.VerticalScroll.Value = plPersonButton.VerticalScroll.Value;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule person mouse wheel scroll sync failed: " + ex);
            }


        }
        VScrollBar myScrollBar = new VScrollBar();
        private void PlMain_ClientSizeChanged(object sender, EventArgs e)
        {
            //plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            //plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
        }
        /// <summary>Синхронизирует шкалу дат и список исполнителей с прокруткой календаря.</summary>
        private void CalendarTasks_MouseWheel1(object sender, MouseEventArgs e)
        {
            try
            {
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                int test = plMain.VerticalScroll.Value;
                plPersonButton.VerticalScroll.Value = test;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule calendar wheel scroll sync failed: " + ex);
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        public int GetPersonButtonWith()
        {
            return plMain.Location.X - plPersonButton.Location.X - 2;
        }
        /// <summary>Оркестрирует очистку элементов календаря и записывает измерения прежних этапов.</summary>
        private void ClearOldExemplars()
        {
            Stopwatch clearPhase = Stopwatch.StartNew();
            long taskTooltipTicks, taskRemoveTicks, taskDisposeTicks;
            int taskControlsRemoved = ClearCalendarTaskControls(
                out taskTooltipTicks, out taskRemoveTicks, out taskDisposeTicks);
            long taskClearMs = clearPhase.ElapsedMilliseconds;

            clearPhase.Restart();
            long personRemoveTicks, personDisposeTicks;
            int personControlsRemoved = ClearCalendarPersonControls(
                out personRemoveTicks, out personDisposeTicks);
            long personClearMs = clearPhase.ElapsedMilliseconds;

            clearPhase.Restart();
            int dateControlsRemoved = plForDate.Controls.Count;
            ClearDateControls();
            long dateClearMs = clearPhase.ElapsedMilliseconds;

            clearPhase.Restart();
            ClearCalendarBitmap();
            long imageClearMs = clearPhase.ElapsedMilliseconds;

            Trace.WriteLine("TimeSchedule refresh-clear-detail: task-controls=" + taskControlsRemoved +
                " task-total-ms=" + taskClearMs +
                " task-tooltip-ms=" + taskTooltipTicks * 1000 / Stopwatch.Frequency +
                " task-remove-ms=" + taskRemoveTicks * 1000 / Stopwatch.Frequency +
                " task-dispose-ms=" + taskDisposeTicks * 1000 / Stopwatch.Frequency +
                " person-controls=" + personControlsRemoved +
                " person-total-ms=" + personClearMs +
                " person-remove-ms=" + personRemoveTicks * 1000 / Stopwatch.Frequency +
                " person-dispose-ms=" + personDisposeTicks * 1000 / Stopwatch.Frequency +
                " date-controls=" + dateControlsRemoved +
                " date-total-ms=" + dateClearMs +
                " image-total-ms=" + imageClearMs);
        }

        /// <summary>Снимает прежние кнопки задач с учётом времени удаления подсказок и освобождения ресурсов.</summary>
        /// <param name="tooltipTicks">Время снятия подсказок в отсчётах Stopwatch.</param>
        /// <param name="removeTicks">Время удаления кнопок из панели.</param>
        /// <param name="disposeTicks">Время освобождения кнопок.</param>
        /// <returns>Число удалённых кнопок задач.</returns>
        private int ClearCalendarTaskControls(out long tooltipTicks, out long removeTicks,
            out long disposeTicks)
        {
            tooltipTicks = 0;
            removeTicks = 0;
            disposeTicks = 0;
            int removed = 0;
            plMain.SuspendLayout();
            try
            {
                for (int i = plMain.Controls.Count - 1; i >= 0; i--)
                {
                    Control control = plMain.Controls[i];
                    if (control is Button)
                    {
                        long started = Stopwatch.GetTimestamp();
                        calendarTaskToolTip.SetToolTip(control, null);
                        tooltipTicks += Stopwatch.GetTimestamp() - started;
                        started = Stopwatch.GetTimestamp();
                        plMain.Controls.RemoveAt(i);
                        removeTicks += Stopwatch.GetTimestamp() - started;
                        started = Stopwatch.GetTimestamp();
                        control.Dispose();
                        disposeTicks += Stopwatch.GetTimestamp() - started;
                        removed++;
                    }
                }
            }
            finally
            {
                plMain.ResumeLayout(true);
            }
            return removed;
        }

        /// <summary>Удаляет кнопки исполнителей с сохранением измерений времени освобождения.</summary>
        /// <param name="removeTicks">Время удаления кнопок из панели.</param>
        /// <param name="disposeTicks">Время освобождения кнопок.</param>
        /// <returns>Число удалённых кнопок исполнителей.</returns>
        private int ClearCalendarPersonControls(out long removeTicks, out long disposeTicks)
        {
            removeTicks = 0;
            disposeTicks = 0;
            int removed = 0;
            plPersonButton.SuspendLayout();
            try
            {
                for (int i = plPersonButton.Controls.Count - 1; i >= 0; i--)
                {
                    Control control = plPersonButton.Controls[i];
                    if (control is Button)
                    {
                        long started = Stopwatch.GetTimestamp();
                        plPersonButton.Controls.RemoveAt(i);
                        removeTicks += Stopwatch.GetTimestamp() - started;
                        started = Stopwatch.GetTimestamp();
                        control.Dispose();
                        disposeTicks += Stopwatch.GetTimestamp() - started;
                        removed++;
                    }
                }
            }
            finally
            {
                plPersonButton.ResumeLayout(true);
            }
            return removed;
        }

        /// <summary>Освобождает прежний растр сетки без изменения жизненного цикла PictureBox.</summary>
        private void ClearCalendarBitmap()
        {
            using (Graphics graphics = pBForLine.CreateGraphics())
                graphics.Clear(Color.White);
            Bitmap previousBitmap = Bmp;
            pBForLine.Image = null;
            Bmp = new Bitmap(1, 1);
            previousBitmap?.Dispose();
        }

        /// <summary>Отсоединяет элементы прежней шкалы дат и сохраняет текстовые ячейки для повторного использования.</summary>
        private void ClearDateControls()
        {
            plForDate.SuspendLayout();
            try
            {
                for (int i = plForDate.Controls.Count - 1; i >= 0; i--)
                {
                    Control control = plForDate.Controls[i];
                    plForDate.Controls.RemoveAt(i);
                    if (control is TextBox dateTextBox)
                    {
                        reusableDateTextBoxes.Enqueue(dateTextBox);
                        continue;
                    }
                    if (control is PictureBox pictureBox)
                    {
                        Image image = pictureBox.Image;
                        pictureBox.Image = null;
                        image?.Dispose();
                    }
                    control.Dispose();
                }
            }
            finally
            {
                plForDate.ResumeLayout(true);
            }
        }
        /// <summary>Освобождает невостребованные ячейки после обновления и при закрытии окна.</summary>
        private void DisposeUnusedDateTextBoxes()
        {
            while (reusableDateTextBoxes.Count > 0)
                reusableDateTextBoxes.Dequeue().Dispose();
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
        /// <summary>Рисует линию на текущем растровом изображении календаря.</summary>
        public void Drow(Pen pen, int pX0, int pY0, int pX1, int pY1)
        {
            using (Graphics graphics = Graphics.FromImage(Bmp))
                graphics.DrawLine(pen, pX0, pY0, pX1, pY1);
            pBForLine.Image = Bmp;
        }
        /// <summary>Рисует горизонтальные границы строк календаря одним графическим контекстом.</summary>
        public void LoadHorizontLine()
        {
            using (Graphics graphics = Graphics.FromImage(Bmp))
            using (Pen rowPen = new Pen(Color.FromArgb(225, 225, 230), 1))
            using (Pen separatorPen = new Pen(Color.FromArgb(245, 245, 248), 2))
            {
                foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
                {
                    if (personButton.Person.ListTask.Tasks.Count == 0)
                        continue;

                    int locationY = personButton.Button.Location.Y;
                    while (locationY < (personButton.Button.Location.Y + personButton.Button.Height))
                    {
                        locationY += Constants.ROW_HIGHT;
                        graphics.DrawLine(rowPen, 0, locationY, pBForLine.Width, locationY);
                    }
                    graphics.DrawLine(separatorPen, 0, locationY, pBForLine.Width, locationY + 1);
                }
            }
            pBForLine.Image = Bmp;
        }

        /// <summary>Рисует вертикальные границы дат, выделяя понедельники, одним графическим контекстом.</summary>
        public void DrowVerticalLines()
        {
            using (Graphics graphics = Graphics.FromImage(Bmp))
            using (Pen penGrey = new Pen(Color.FromArgb(225, 225, 230), 1))
            using (Pen penForMonday = new Pen(Color.FromArgb(245, 245, 248), 2))
            {
                foreach (DateTextBox dateTextBox in Program.PoolTextBox.ListTextBoxes)
                {
                    int locationX = dateTextBox.LocationX;
                    graphics.DrawLine(dateTextBox.Date.DayOfWeek == DayOfWeek.Monday ? penForMonday : penGrey,
                        locationX, 0, locationX, pBForLine.Height);
                }
            }
            pBForLine.Image = Bmp;
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
        /// <summary>Восстанавливает позиции прокрутки независимых панелей календаря.</summary>
        public void LoadScrolls()
        {
            try
            {
                plForDate.HorizontalScroll.Value = HorizontalScrollValue;
                plForDate.HorizontalScroll.Value = HorizontalScrollValue;
                plMain.HorizontalScroll.Value = HorizontalScrollValue;
                plMain.HorizontalScroll.Value = HorizontalScrollValue;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule date and calendar horizontal scroll restore failed: " + ex);
            }
            try
            {
                plPersonButton.VerticalScroll.Value = VerticalScrollValue;
                plPersonButton.VerticalScroll.Value = VerticalScrollValue;

            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule person vertical scroll restore failed: " + ex);
            }
            try
            {
                plMain.VerticalScroll.Value = VerticalScrollValue;
                plMain.VerticalScroll.Value = VerticalScrollValue;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule calendar vertical scroll restore failed: " + ex);
            }
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
        /// <summary>Временно задаёт режим полного обновления и восстанавливает прежний режим при любом завершении вызова.</summary>
        /// <param name="refreshType">Запрошенный режим обновления календаря.</param>
        public void LoadRefreshForm(RefreshType refreshType)
        {
            if (refreshType == RefreshType.All)
            {
                RefreshType refreshTypeToSave = RefreshType;
                RefreshType = RefreshType.All;
                try
                {
                    LoadRefreshForm();
                }
                finally
                {
                    RefreshType = refreshTypeToSave;
                }
            }
        }
        public void LoadRefreshFormForDelegat()
        {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }
        /// <summary>Показывает анимированный индикатор в отдельном STA-потоке на время синхронного обновления календаря.</summary>
        /// <param name="statusProgressBar">Режим отображения индикатора.</param>
        public void LoadRefreshForm(Statuses.ProgressBar statusProgressBar)
        {
            timer1.Interval = 900000;
            if (statusProgressBar != Statuses.ProgressBar.Use)
                return;

            RunWithProgressWindow(() => LoadRefreshForm(RefreshType.All));
        }

        /// <summary>Выполняет операцию на прежнем UI-потоке, сохраняя анимацию окна прогресса в отдельном STA-цикле сообщений.</summary>
        /// <param name="refreshAction">Операция обновления, выполняемая вызывающим потоком.</param>
        private void RunWithProgressWindow(Action refreshAction)
        {
            Rectangle mainBounds = Bounds;
            ManualResetEventSlim windowReady = new ManualResetEventSlim(false);
            fmProgressBar progressWindow = null;
            Exception startupError = null;
            int stopRequested = 0;

            Thread progressThread = new Thread(() =>
            {
                try
                {
                    using (fmProgressBar window = new fmProgressBar())
                    {
                        window.SetTextMessege("Идет обновление");
                        window.StartPosition = FormStartPosition.Manual;
                        window.Location = new Point(
                            mainBounds.Left + (mainBounds.Width - window.Width) / 2,
                            mainBounds.Top + (mainBounds.Height - window.Height) / 2);
                        window.Shown += (sender, e) =>
                        {
                            progressWindow = window;
                            windowReady.Set();
                            if (Interlocked.CompareExchange(ref stopRequested, 0, 0) != 0)
                                window.Close();
                        };
                        Application.Run(window);
                    }
                }
                catch (Exception ex)
                {
                    startupError = ex;
                    Trace.WriteLine("TimeSchedule progress window failed: " + ex);
                }
                finally
                {
                    windowReady.Set();
                }
            });
            progressThread.Name = "TimeSchedule progress UI";
            progressThread.IsBackground = true;
            progressThread.SetApartmentState(ApartmentState.STA);
            progressThread.Start();

            if (!windowReady.Wait(TimeSpan.FromSeconds(10)))
            {
                Interlocked.Exchange(ref stopRequested, 1);
                throw new InvalidOperationException("Не удалось открыть окно обновления календаря за отведённое время.");
            }
            if (startupError != null)
            {
                if (progressThread.Join(TimeSpan.FromSeconds(2)))
                    windowReady.Dispose();
                throw new InvalidOperationException("Не удалось открыть окно обновления календаря.", startupError);
            }
            if (progressWindow == null)
            {
                Interlocked.Exchange(ref stopRequested, 1);
                throw new InvalidOperationException("Окно обновления календаря завершилось до начала операции.");
            }

            try
            {
                refreshAction();
            }
            finally
            {
                Interlocked.Exchange(ref stopRequested, 1);
                try
                {
                    progressWindow.BeginInvoke(new Action(progressWindow.Close));
                }
                catch (InvalidOperationException ex)
                {
                    Trace.WriteLine("TimeSchedule progress window already closed: " + ex);
                }
                if (progressThread.Join(TimeSpan.FromSeconds(2)))
                    windowReady.Dispose();
                else
                    Trace.WriteLine("TimeSchedule progress window: timed out while closing.");
            }
        }

        /// <summary>Выполняет полное обновление календаря, инициированное окном прогресса.</summary>
        /// <param name="fmProgressBar">Окно, сопровождающее синхронное обновление.</param>
        public void LoadRefreshForm(fmProgressBar fmProgressBar)
        {
            LoadRefreshForm(RefreshType.All);
        }

        /// <summary>Совместимый вход для запуска обновления с переданным списком форм без ожидания OpenForms.</summary>
        /// <param name="listForm">Список из главной формы и окна прогресса.</param>
        public void LoadRefreshWithProgressBarr(object listForm)
        {
            List<Form> forms = listForm as List<Form>;
            if (forms == null || forms.Count < 2 || !(forms[0] is Form1) || !(forms[1] is fmProgressBar))
                throw new ArgumentException("Ожидается список главной формы и окна прогресса.", "listForm");

            fmProgressBar progressForm = (fmProgressBar)forms[1];
            try
            {
                ((Form1)forms[0]).LoadRefreshForm(progressForm);
            }
            finally
            {
                if (!progressForm.IsDisposed)
                    progressForm.Close();
            }
        }
        /// <summary>Последовательно готовит задачи, исполнителей, календарные дни и кнопки календаря.</summary>
        /// <param name="loadedTasks">Согласованно прочитанные задачи либо null для старого формата.</param>
        /// <param name="loadedPersons">Согласованно прочитанные исполнители либо null для старого формата.</param>
        /// <param name="metrics">Измерения времени и размеров сформированных моделей.</param>
        private void LoadAllPools(ListTasks loadedTasks, ListPersons loadedPersons, CalendarRefreshMetrics metrics)
        {
            Stopwatch phase = Stopwatch.StartNew();
            LoadAndFilterCalendarTasks(loadedTasks, loadedPersons, metrics, phase);
            BuildCalendarPersonsAndTasks(metrics, phase);
        }

        /// <summary>Загружает данные проекта и отбирает задачи, которые нужно показать в календаре.</summary>
        /// <param name="loadedTasks">Уже загруженный список задач либо null для прежнего формата.</param>
        /// <param name="loadedPersons">Уже загруженный список исполнителей либо null для прежнего формата.</param>
        /// <param name="metrics">Показатели чтения и фильтрации задач.</param>
        /// <param name="phase">Счётчик времени этапов построения моделей.</param>
        private void LoadAndFilterCalendarTasks(ListTasks loadedTasks, ListPersons loadedPersons,
            CalendarRefreshMetrics metrics, Stopwatch phase)
        {
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
        }

        /// <summary>Готовит кнопки исполнителей, нерабочие дни и модели кнопок задач.</summary>
        /// <param name="metrics">Показатели количества объектов и времени построения.</param>
        /// <param name="phase">Счётчик времени этапов построения моделей.</param>
        private void BuildCalendarPersonsAndTasks(CalendarRefreshMetrics metrics, Stopwatch phase)
        {
            phase.Restart();
            Program.ListPersonButton.LoadListPersonButtons(
                Program.listPersons.Persons, Program.ListTasksAllPersonToShow,
                Constants.ROW_HIGHT, this);
            int maxButtonLocationY = SetMaxLocationAndAddPersonButton(ref Program.ListPersonButton);
            pBForLine.Height = maxButtonLocationY;
            metrics.BuildPersonButtonsMs = phase.ElapsedMilliseconds;
            metrics.PersonModels = Program.ListPersonButton.PersonButtons.Count;
            if (File.Exists(Dals.TakeMainPathFile(Constants.HOLYDAYS_BIN)))
            {
                Program.ListHolidays = Dals.binReadFileToObject(
                    Program.ListHolidays, Dals.TakeMainPathFile(Constants.HOLYDAYS_BIN));
            }
            else
            {
                Program.ListHolidays.SetHolidaysFromList(
                    Dals.ReadListFromMainPathFile(Constants.HOLYDAYS));
            }
            NonWorkDaysWrite(Program.ListTasksAllPersonToShow.GetMinDateStartTasks(),
                Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks());
            Program.listNonWorkingDays.NonWorkingDays.AddRange(Program.ListHolidays.Holidays);
            phase.Restart();
            Program.ListTaskButtons.AddTaskButtons(
                Program.ListTasksAllPersonToShow, Program.ListPersonButton);
            metrics.BuildTaskButtonsMs = phase.ElapsedMilliseconds;
            metrics.TaskModels = Program.ListTaskButtons.TaskButtons.Count;
            metrics.PlannedTaskButtons = Program.ListTaskButtons.TaskButtons.Sum(task => task.PlannedButtonCount);
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
        /// <summary>Снимок ссылок на модели, необходимый для отката при неудачной подготовке календаря.</summary>
        private sealed class CalendarModelState
        {
            /// <summary>Полный список задач до обновления.</summary>
            private readonly ListTasks savedTasks;
            /// <summary>Отобранные для календаря задачи до обновления.</summary>
            private readonly ListTasks shownTasks;
            /// <summary>Исполнители до обновления.</summary>
            private readonly ListPersons persons;
            /// <summary>Модели кнопок исполнителей до обновления.</summary>
            private readonly ListPersonButton personButtons;
            /// <summary>Модели кнопок задач до обновления.</summary>
            private readonly ListTaskButton taskButtons;
            /// <summary>Пул элементов шкалы дат до обновления.</summary>
            private readonly PoolTextBox dateTextBoxes;
            /// <summary>Нерабочие дни до обновления.</summary>
            private readonly ListNonWorkingDays nonWorkingDays;
            /// <summary>Праздничные дни до обновления.</summary>
            private readonly ListHolidays holidays;
            /// <summary>Дерево проектов, использованное при предыдущем обновлении.</summary>
            private readonly TreeProjects selectionTree;
            /// <summary>Высота области линий до обновления.</summary>
            private readonly int linesHeight;

            /// <summary>Сохраняет ссылки на опубликованные модели, не копируя и не изменяя их содержимое.</summary>
            public CalendarModelState(Form1 form)
            {
                savedTasks = Program.ListTasksAllPersonToSave;
                shownTasks = Program.ListTasksAllPersonToShow;
                persons = Program.listPersons;
                personButtons = Program.ListPersonButton;
                taskButtons = Program.ListTaskButtons;
                dateTextBoxes = Program.PoolTextBox;
                nonWorkingDays = Program.listNonWorkingDays;
                holidays = Program.ListHolidays;
                selectionTree = form.selectionTreeForRefresh;
                linesHeight = form.pBForLine.Height;
            }

            /// <summary>Возвращает прежние ссылки на модели и размер области линий после сбоя подготовки.</summary>
            public void Restore(Form1 form)
            {
                Program.ListTasksAllPersonToSave = savedTasks;
                Program.ListTasksAllPersonToShow = shownTasks;
                Program.listPersons = persons;
                Program.ListPersonButton = personButtons;
                Program.ListTaskButtons = taskButtons;
                Program.PoolTextBox = dateTextBoxes;
                Program.listNonWorkingDays = nonWorkingDays;
                Program.ListHolidays = holidays;
                form.selectionTreeForRefresh = selectionTree;
                form.pBForLine.Height = linesHeight;
            }
        }

        /// <summary>Создаёт независимые пустые коллекции для подготовки нового состояния календаря.</summary>
        private static void InitializeCalendarModels()
        {
            Program.ListTasksAllPersonToSave = new ListTasks();
            Program.ListTasksAllPersonToShow = new ListTasks();
            Program.listPersons = new ListPersons();
            Program.ListPersonButton = new ListPersonButton();
            Program.ListTaskButtons = new ListTaskButton();
            Program.PoolTextBox = new PoolTextBox();
            Program.listNonWorkingDays = new ListNonWorkingDays();
            Program.ListHolidays = new ListHolidays();
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
        /// <summary>Готовит шкалу рабочих дат и изображения, не размещая сами ячейки до этапа Attach.</summary>
        private void LoadTextBoxWithDate()
        {
            DateTime dateToTables = Program.ListTasksAllPersonToShow.GetMinDateStartTasks();
            DateTime dateMaxToTable = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks();
            int height = plMain.Location.Y - plForDate.Location.Y;
            ClearDateControls();
            Bitmap bitmap = new Bitmap(1, 1);
            PictureBox pictureBox = new PictureBox();
            pictureBox.Height = 1;
            pictureBox.Location = new Point(HorizontalScrollValue, 1);
            plForDate.Controls.Add(pictureBox);
            pictureBox.Image = bitmap;

            int locationX = BuildCalendarDateModels(dateToTables, dateMaxToTable, height);
            if (LastMaxHorizonntalScroll >= locationX + Constants.COLUMN_WITH &&
                HorizontalScrollValue > LastMaxHorizonntalScroll - LastLargeChangeHorizonntalScroll)
            {
                HorizontalScrollValue -= LastMaxHorizonntalScroll - locationX;
            }
            pBForLine.Width = locationX;
            ResizeImage(new Size(locationX, pBForLine.Height));
            ResizeCalendarDateImage(pictureBox, bitmap, locationX);
        }

        /// <summary>Формирует объекты рабочих дат и возвращает ширину шкалы в пикселях.</summary>
        /// <param name="firstDate">Начальная дата календаря.</param>
        /// <param name="lastDate">Конечная дата календаря.</param>
        /// <param name="height">Высота объектов даты.</param>
        /// <returns>Ширина сформированной шкалы дат.</returns>
        private static int BuildCalendarDateModels(DateTime firstDate, DateTime lastDate, int height)
        {
            int locationX = 0;
            while (firstDate <= lastDate)
            {
                if (IsNotHolyday(firstDate))
                {
                    DateTextBox dateTextBox = new DateTextBox(firstDate, height, locationX);
                    dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.NotReady);
                    locationX += Constants.COLUMN_WITH;
                    Program.PoolTextBox.ListTextBoxes.Add(dateTextBox);
                }
                firstDate = firstDate.AddDays(1);
            }
            return locationX;
        }

        /// <summary>Заменяет временный растр шкалы дат, освобождая его после успешной подмены.</summary>
        /// <param name="pictureBox">Контрол шкалы дат, владеющий итоговым растром.</param>
        /// <param name="sourceBitmap">Исходный однопиксельный растр.</param>
        /// <param name="width">Ширина итогового изображения.</param>
        private static void ResizeCalendarDateImage(PictureBox pictureBox, Bitmap sourceBitmap, int width)
        {
            Bitmap resizedBitmap = null;
            try
            {
                resizedBitmap = new Bitmap(width, sourceBitmap.Height);
                using (Graphics graphics = Graphics.FromImage(resizedBitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(sourceBitmap, 0, 0, sourceBitmap.Width * 4, sourceBitmap.Height);
                }
                pictureBox.Width = resizedBitmap.Width;
                pictureBox.Image = resizedBitmap;
                sourceBitmap.Dispose();
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) &&
                !(ex is System.Threading.ThreadAbortException) && !(ex is AccessViolationException))
            {
                if (!ReferenceEquals(pictureBox.Image, resizedBitmap))
                    resizedBitmap?.Dispose();
                Trace.WriteLine("TimeSchedule date image resize failed: " + ex);
                throw;
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
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule mouse wheel scroll sync failed: " + ex);
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
        /// <summary>Записывает геометрию правой границы при завершении прокрутки или переходе к последней дате.</summary>
        /// <param name="reason">Пользовательская операция, вызвавшая измерение.</param>
        private void TraceCalendarScrollBounds(string reason)
        {
            if (Program.PoolTextBox.ListTextBoxes.Count == 0)
                return;
            DateTextBox lastDate = Program.PoolTextBox.ListTextBoxes[Program.PoolTextBox.ListTextBoxes.Count - 1];
            Trace.WriteLine(string.Format(
                "TimeSchedule calendar-bounds-scroll reason={0} lastDate={1:yyyy-MM-dd} " +
                "lastRight={2} mainScroll={3} mainMax={4} mainPage={5} " +
                "mainVisibleRight={6} dateScroll={7} dateMax={8} datePage={9} " +
                "dateVisibleRight={10} gridLeft={11} gridWidth={12} bitmapWidth={13}",
                reason, lastDate.Date, lastDate.LocationX + Constants.COLUMN_WITH,
                plMain.HorizontalScroll.Value, plMain.HorizontalScroll.Maximum,
                plMain.HorizontalScroll.LargeChange,
                plMain.HorizontalScroll.Value + plMain.ClientSize.Width,
                plForDate.HorizontalScroll.Value, plForDate.HorizontalScroll.Maximum,
                plForDate.HorizontalScroll.LargeChange,
                plForDate.HorizontalScroll.Value + plForDate.ClientSize.Width,
                pBForLine.Left, pBForLine.Width, Bmp == null ? 0 : Bmp.Width));
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
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule scroll synchronization failed: " + ex);
            }
            VerticalScrollValue = plMain.VerticalScroll.Value;
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
            if (e.Type == ScrollEventType.EndScroll)
                TraceCalendarScrollBounds("end-scroll");
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


        /// <summary>Прокручивает календарь к запрошенной дате, не изменяя модель задач.</summary>
        /// <param name="targetDateTime">Дата, к которой требуется перейти.</param>
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
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule scroll to date failed: " + ex);
            }
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
            if (targetDateTime.Date >= Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks().Date)
                TraceCalendarScrollBounds("scroll-to-last-date");
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
            if (canvasPilotEnabled && canvasPilotPanelMovesLogged < 3)
            {
                canvasPilotPanelMovesLogged++;
                TraceCanvasPilotInput("panel-move", plMain, e.Location);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            LoadRefreshForm(Statuses.ProgressBar.Use);
        }
        /// <summary>Сдвигает календарь по рабочим датам с сохранением текущей синхронизации панелей.</summary>
        /// <param name="value">Сдвиг в количестве столбцов календаря.</param>
        private void HorizontalScrollAdd(int value)
        {
            try
            {
                plMain.HorizontalScroll.Value += value * Constants.COLUMN_WITH;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
                plForDate.HorizontalScroll.Value = plMain.HorizontalScroll.Value;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule horizontal scroll adjustment failed: " + ex);
            }
            HorizontalScrollValue = plMain.HorizontalScroll.Value;
        }
        private void PlusFiveDays(object sender, EventArgs e)
        {
            HorizontalScrollAdd(5);
            LoadTaskButtons();
            LoadDateTextBoxes();
        }
        /// <summary>Создаёт растровую основу линий календаря и освобождает предыдущую, если она не отображается.</summary>
        /// <param name="size">Размер нового растра.</param>
        public void ResizeImage(Size size)
        {
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(size.Width, size.Height);
                Bitmap previousBitmap = Bmp;
                Bmp = bitmap;
                if (!ReferenceEquals(pBForLine.Image, previousBitmap))
                    previousBitmap?.Dispose();
            }
            catch (Exception ex)
            {
                if (!ReferenceEquals(Bmp, bitmap))
                    bitmap?.Dispose();
                Trace.WriteLine("TimeSchedule calendar bitmap resize failed: " + ex);
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

        /// <summary>Сохраняет синхронизацию горизонтальной прокрутки панелей при прокрутке исполнителей.</summary>
        private void plPersonButton_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;
                plMain.HorizontalScroll.Value = plForDate.HorizontalScroll.Value;

            }
            catch (Exception ex)
            {
                Trace.WriteLine("TimeSchedule person panel scroll sync failed: " + ex);
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
#if DEBUG
            if (canvasPilotEnabled)
                this.Text += " [TS-02: интерактивный холст]";
#endif
            this.Activate();
        }
        /// <summary>Присоединяет кнопки исполнителей одним этапом компоновки панели.</summary>
        private void LoadPersonButtons()
        {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            plPersonButton.SuspendLayout();
            try
            {
                foreach (PersonButton personButton in Program.ListPersonButton.PersonButtons)
                {
                    if (personButton.Person.ListTask.Tasks.Count > 0)
                    {
                        plPersonButton.Controls.Add(personButton.Button);
                        personButton.Button.BringToFront();
                    }
                }
            }
            finally
            {
                plPersonButton.ResumeLayout(true);
            }
        }
        /// <summary>Присоединяет впервые материализованные кнопки задач; считает только реально присоединённые сегменты.</summary>
        private void LoadTaskButtons(CalendarRefreshMetrics metrics = null)
        {
            // События Scroll/SizeChanged могут добавить кнопки во время текущего обновления.
            metrics = metrics ?? activePaintMetrics;
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            if (canvasPilotEnabled)
            {
                pBForLine.Invalidate();
                return;
            }
            Program.ListTaskButtons.CalculateLoadingStatus(
                HorizontalScrollValue,
                this.Width,
                Program.ListTasksAllPersonToShow.GetMinDateStartTasks());
            Program.ListTaskButtons.SetNewLocationToButtons(
                VerticalScrollValue,
                HorizontalScrollValue);
            bool layoutSuspended = false;
            try
            {
                foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
                {
                    if (taskButton.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad)
                    {
                        if (!layoutSuspended)
                        {
                            plMain.SuspendLayout();
                            layoutSuspended = true;
                        }
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
            finally
            {
                if (layoutSuspended)
                    plMain.ResumeLayout(true);
            }
        }
        /// <summary>Повторно использует ячейки дат и присоединяет новые видимые столбцы календаря.</summary>
        /// <param name="metrics">Необязательные измерения текущего полного обновления.</param>
        private void LoadDateTextBoxes(CalendarRefreshMetrics metrics = null)
        {
            if (FormReadyToBeAddedControl == Statuses.FormReadyToBeAddedControl.NotReady)
                return;
            Program.PoolTextBox.CalculateLoadingStatus(
                plMain.HorizontalScroll.Value,
                this.Width,
                Program.ListTasksAllPersonToShow.GetMinDateStartTasks());
            if (reusableDateTextBoxes.Count > 0)
            {
                foreach (DateTextBox date in Program.PoolTextBox.ListTextBoxes)
                {
                    if (reusableDateTextBoxes.Count == 0)
                        break;
                    if (date.LoadingStatus != Statuses.LoadingStatus.ReadyToLoad)
                        continue;
                    date.ReuseTextBox(reusableDateTextBoxes.Dequeue());
                    if (metrics != null)
                        metrics.ReusedDateTextBoxes++;
                }
            }
            Program.PoolTextBox.SetNewLocationToButtons(
                plMain.HorizontalScroll.Value);
            bool layoutSuspended = false;
            try
            {
                foreach (DateTextBox dateTextBox in Program.PoolTextBox.ListTextBoxes)
                {
                    if (dateTextBox.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad)
                    {
                        if (!layoutSuspended)
                        {
                            plForDate.SuspendLayout();
                            layoutSuspended = true;
                        }
                        plForDate.Controls.Add(dateTextBox.TextBox);
                        dateTextBox.TextBox.BringToFront();
                        dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.Loaded);
                    }
                }
            }
            finally
            {
                if (layoutSuspended)
                    plForDate.ResumeLayout(true);
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

        /// <summary>Выбирает восстановление из history или независимого архива задач.</summary>
        private void восстановлениеЗадачToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tasksPath = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            if (HandleHeadRecovery(tasksPath)) return;
            ShowTaskBackupSelection(tasksPath);
        }

        /// <summary>
        /// Предлагает проверенный HEAD из history при повреждении основного манифеста.
        /// Возвращает true, если пользователь отменил восстановление или выбрал этот HEAD.
        /// </summary>
        /// <param name="tasksPath">Путь к файлу задач выбранного проекта.</param>
        /// <returns>Признак завершения сценария выбора восстановления.</returns>
        private bool HandleHeadRecovery(string tasksPath)
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
                return false;
            }
            if (candidate == null) return false;

            DialogResult choice = ConfirmHeadRecovery(candidate);
            if (choice == DialogResult.No) return false;
            if (choice == DialogResult.Yes) RestoreHeadFromHistory(tasksPath, candidate);
            return true;
        }

        /// <summary>Запрашивает подтверждение восстановления проверенной ревизии HEAD.</summary>
        /// <param name="candidate">Найденный проверенный манифест из history.</param>
        /// <returns>Выбранный пользователем вариант восстановления.</returns>
        private static DialogResult ConfirmHeadRecovery(CompactHeadRecoveryInfo candidate)
        {
            string message = "Основной манифест задач отсутствует или повреждён.\n\n" +
                "Проверен резервный снимок из history:\n" +
                "Ревизия: " + candidate.Generation + "\n" +
                "Задач: " + candidate.TaskCount + "\n" +
                "Сегментов: " + candidate.SegmentCount + "\n\n" +
                "Изменения, выполненные после этой ревизии, могут быть утрачены.\n" +
                "Восстановить этот снимок?";
            return MessageBox.Show(message +
                "\n\nНет — выбрать независимую резервную копию задач.",
                "Подтверждение восстановления HEAD", MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);
        }

        /// <summary>Восстанавливает подтверждённый HEAD и обновляет календарь.</summary>
        /// <param name="tasksPath">Путь к файлу задач выбранного проекта.</param>
        /// <param name="candidate">Подтверждённый пользователем манифест.</param>
        private void RestoreHeadFromHistory(string tasksPath, CompactHeadRecoveryInfo candidate)
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
        }

        /// <summary>Отображает независимые снимки и исторические архивы для явного восстановления.</summary>
        /// <param name="tasksPath">Путь к файлу задач выбранного проекта.</param>
        private void ShowTaskBackupSelection(string tasksPath)
        {
            fmChangeList fm = CreateTaskRestoreForm();
            List<KeyValuePair<string, string>> availableBackups;
            try { availableBackups = FindAvailableTaskBackups(tasksPath); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is ArgumentException)
            {
                MessageBox.Show("Не удалось прочитать резервные копии задач.\n\n" + ex.Message,
                    "Ошибка восстановления", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (availableBackups.Count == 0)
            {
                MessageBox.Show("Резервные копии задач для этого проекта не найдены.",
                    "Восстановление задач", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            fm.LBx.Items.AddRange(availableBackups.Select(x => x.Key).ToArray());
            fm.LBx.Click += (clickSender, clickArgs) =>
                fm.BtnApply.Enabled = fm.LBx.SelectedIndex != -1;
            string openedRevision;
            try { openedRevision = TaskStore.RevisionForRestore(tasksPath); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is InvalidDataException || ex is System.Runtime.Serialization.SerializationException)
            {
                MessageBox.Show("Не удалось проверить состояние задач: " + ex.Message);
                return;
            }
            fm.ShowDialog();
            RestoreSelectedTaskBackup(fm, availableBackups, tasksPath, openedRevision);
        }

        /// <summary>Настраивает окно выбора резервной копии задач.</summary>
        /// <returns>Окно, готовое к заполнению списком архивов.</returns>
        private static fmChangeList CreateTaskRestoreForm()
        {
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
            return fm;
        }

        /// <summary>Собирает список компактных снимков и архивов старого формата с уникальными отображаемыми именами.</summary>
        /// <param name="tasksPath">Путь к файлу задач выбранного проекта.</param>
        /// <returns>Пары отображаемого имени и полного пути резервной копии.</returns>
        private static List<KeyValuePair<string, string>> FindAvailableTaskBackups(string tasksPath)
        {
            var availableBackups = new List<KeyValuePair<string, string>>();
            var usedBackupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
            string oldBackups = Dals.TakeBackUpPathDirectory(Constants.TASKS_BIN);
            foreach (FileInfo oldFile in new DirectoryInfo(oldBackups)
                .GetFiles().OrderByDescending(f => f.CreationTimeUtc))
            {
                string displayName = "Архивная_" + oldFile.Name;
                string uniqueName = displayName;
                int duplicate = 2;
                while (!usedBackupNames.Add(uniqueName))
                {
                    uniqueName = displayName + "_" + duplicate.ToString("D2");
                    duplicate++;
                }
                availableBackups.Add(new KeyValuePair<string, string>(uniqueName, oldFile.FullName));
            }
            return availableBackups;
        }

        /// <summary>Восстанавливает выбранный архив после подтверждения пользователя.</summary>
        /// <param name="fm">Окно выбора резервной копии.</param>
        /// <param name="availableBackups">Список отображаемых архивов и их путей.</param>
        /// <param name="tasksPath">Путь к файлу задач выбранного проекта.</param>
        /// <param name="openedRevision">Ревизия, прочитанная перед открытием окна.</param>
        private void RestoreSelectedTaskBackup(fmChangeList fm,
            List<KeyValuePair<string, string>> availableBackups, string tasksPath, string openedRevision)
        {
            if (fm.BtnClick != BtnClick.aplly || fm.LBx.SelectedItem == null) return;
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

        /// <summary>Показывает диалог создания проекта и освобождает его после закрытия.</summary>
        private void button2_Click(object sender, EventArgs e)
        {
            using (FrmAddProject addProject = new FrmAddProject())
                addProject.ShowDialog();
        }

        private void NewRefresh_Click(object sender, EventArgs e)
        {
            ShowProgressbarAndMakeRefresh();
        }

        /// <summary>Планирует обновление в UI-потоке, сохраняя возможность подписки на событие после вызова.</summary>
        public void ShowProgressbarAndMakeRefresh()
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed || Disposing)
                    return;
                try
                {
                    LoadRefreshForm(Statuses.ProgressBar.Use);
                    EventRefreshFinished?.Invoke();
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                    ex is System.Runtime.Serialization.SerializationException || ex is InvalidCastException ||
                    ex is InvalidDataException || ex is InvalidOperationException || ex is ArgumentException)
                {
                    Trace.WriteLine("TimeSchedule scheduled refresh failed: " + ex);
                    MessageBox.Show("Не удалось завершить обновление календаря.\n\n" + ex.Message,
                        "Ошибка обновления календаря", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
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

        /// <summary>Включает интерактивный холст в Debug и Release с учётом штатной настройки запуска.</summary>
        private readonly bool canvasPilotEnabled = IsCanvasPilotEnabledByDefault();

        /// <summary>Повторно используемые кисти экспериментальной отрисовки.</summary>
        private readonly Dictionary<int, SolidBrush> canvasPilotBrushes = new Dictionary<int, SolidBrush>();

        /// <summary>Повторно используемые шрифты для отображения статусов задач.</summary>
        private readonly Dictionary<FontStyle, Font> canvasPilotFonts = new Dictionary<FontStyle, Font>();

        /// <summary>Задача под указателем; затемняются все её визуальные сегменты.</summary>
        private TaskButton canvasPilotHoveredTask;

        /// <summary>Задача, на которой была нажата левая кнопка мыши.</summary>
        private TaskButton canvasPilotPressedTask;

        /// <summary>Начальная точка нажатия для различения клика и будущего переноса.</summary>
        private Point canvasPilotPressPoint;

        /// <summary>Задача, для которой открыто контекстное меню.</summary>
        private TaskButton canvasPilotContextTask;

        /// <summary>Единственное контекстное меню всех рисуемых задач календаря.</summary>
        private ContextMenuStrip canvasPilotTaskMenu;

        /// <summary>Подменю изменения статуса выбранной задачи.</summary>
        private ToolStripMenuItem canvasPilotStatusMenuItem;

        /// <summary>Команда дублирования задачи, доступная администратору.</summary>
        private ToolStripMenuItem canvasPilotDuplicateMenuItem;

        /// <summary>Команда удаления, доступность которой зависит от прав пользователя.</summary>
        private ToolStripMenuItem canvasPilotDeleteMenuItem;

        /// <summary>Команда перемещения выбранной задачи с последующей цепочкой через контекстное меню.</summary>
        private ToolStripMenuItem canvasPilotMoveMenuItem;

        /// <summary>Точка правого клика по задаче, сохраняющая выбранный пользователем сегмент при открытии меню.</summary>
        private Point canvasPilotContextPoint;

        /// <summary>Перенос из меню: курсор перемещает цепочку без удержания левой кнопки, клик подтверждает её положение.</summary>
        private bool canvasPilotMoveByClick;

        /// <summary>Фильтр колеса мыши над всеми элементами календаря, включая дочерние контролы.</summary>
        private CanvasCalendarWheelFilter canvasPilotWheelFilter;

        /// <summary>Накопленная неполная дельта горизонтальной прокрутки для точных колёс и тачпадов.</summary>
        private int canvasPilotHorizontalWheelRemainder;

        /// <summary>Накопленная неполная дельта вертикальной прокрутки для точных колёс и тачпадов.</summary>
        private int canvasPilotVerticalWheelRemainder;

        /// <summary>Активный предварительный перенос; не изменяет сохраняемые объекты Task.</summary>
        private CanvasPilotDragPreview canvasPilotDragPreview;

        /// <summary>Корневая задача предварительного переноса.</summary>
        private TaskButton canvasPilotDragRoot;

        /// <summary>Задачи цепочки в порядке от предшественника к зависимым.</summary>
        private List<Task> canvasPilotDragChain;

        /// <summary>Отпечатки всей зависимой цепочки на момент начала переноса.</summary>
        private Dictionary<long, string> canvasPilotDragExpected;

        /// <summary>Количество диагностических записей об изменении предпросмотра за один перенос.</summary>
        private int canvasPilotPreviewLogged;

        /// <summary>Смещение захваченного столбца от начала полосы исходной задачи.</summary>
        private int canvasPilotDragColumnOffset;

        /// <summary>Режим взаимодействия с полосой: перенос или изменение одной из границ.</summary>
        private enum CanvasPilotDragMode
        {
            Move,
            ResizeStart,
            ResizeFinish
        }

        /// <summary>Режим, определённый по месту первоначального нажатия на полосу.</summary>
        private CanvasPilotDragMode canvasPilotPressedMode = CanvasPilotDragMode.Move;

        /// <summary>Режим текущего предпросмотра; не меняется при прокрутке календаря.</summary>
        private CanvasPilotDragMode canvasPilotDragMode = CanvasPilotDragMode.Move;

        /// <summary>Признак удержания мыши при предварительном переносе или изменении длительности.</summary>
        private bool canvasPilotDragActive;

        /// <summary>Уточняет положение предпросмотра после прокрутки и изменения раскладки при зажатом Shift.</summary>
        private readonly System.Windows.Forms.Timer canvasPilotDragTracker = new System.Windows.Forms.Timer { Interval = 33 };

        /// <summary>Экранное положение курсора при предыдущей проверке предпросмотра.</summary>
        private Point canvasPilotLastDragCursor;

        /// <summary>Положение прокрутки календаря при предыдущей проверке предпросмотра.</summary>
        private Point canvasPilotLastDragScroll;

        /// <summary>Ограничитель диагностических записей о прокрутке при переносе.</summary>
        private int canvasPilotShiftTrackLogged;

        /// <summary>Набор задач и новых геометрических сегментов для предпросмотра.</summary>
        private sealed class CanvasPilotDragPreview
        {
            /// <summary>Дата, на которую предварительно переносится выбранная задача.</summary>
            public DateTime StartDate { get; set; }

            /// <summary>Дата окончания выбранной задачи для режима изменения границ.</summary>
            public DateTime FinishDate { get; set; }

            /// <summary>Исполнитель, которому предварительно назначается выбранная задача.</summary>
            public string PersonName { get; set; }

            /// <summary>Номера задач, чьи старые полосы не рисуются во время предпросмотра.</summary>
            public HashSet<long> VisibleTaskNumbers { get; } = new HashSet<long>();

            /// <summary>Новые даты и исполнители всей цепочки, в том числе задач вне текущего фильтра.</summary>
            public Dictionary<long, Task> ProjectedTasks { get; } = new Dictionary<long, Task>();

            /// <summary>Модели рассчитанных полос перемещаемой зависимой цепочки.</summary>
            public List<TaskButton> TaskBands { get; } = new List<TaskButton>();

            /// <summary>Полосы независимых задач, смещённых по вертикали из-за приоритетов.</summary>
            public List<TaskButton> DisplacedTaskBands { get; } = new List<TaskButton>();
        }

        /// <summary>Число записанных первых MouseMove от PictureBox, чтобы не засорять журнал.</summary>
        private int canvasPilotPictureMovesLogged;

        /// <summary>Число записанных первых MouseMove от панели.</summary>
        private int canvasPilotPanelMovesLogged;

        /// <summary>Признак записи первого успешного попадания мышью в задачу.</summary>
        private bool canvasPilotFirstHitLogged;

        /// <summary>Количество рисований экспериментального холста после начала обновления.</summary>
        private int canvasPilotPaintEvents;

        /// <summary>Число сегментов, попавших в последнюю область перерисовки.</summary>
        private int canvasPilotLastSegments;

        /// <summary>Наибольшее измеренное время одного Paint экспериментального холста, мс.</summary>
        private double canvasPilotMaxPaintMs;

        /// <summary>Суммарное время Paint экспериментального холста после начала обновления, мс.</summary>
        private double canvasPilotTotalPaintMs;

        /// <summary>Отключает экспериментальное рисование после ошибки GDI+.</summary>
        private bool canvasPilotPaintFailed;

        /// <summary>Включает подтверждённый интерактивный холст как в Debug, так и в Release.</summary>
        private static bool IsCanvasPilotEnabledByDefault()
        {
            return true;
        }

        /// <summary>Подключает альтернативную отрисовку к существующему PictureBox сетки без новых HWND.</summary>
        private void InitializeCanvasPilot()
        {
            if (canvasPilotEnabled)
            {
                pBForLine.Paint += PaintCanvasPilot;
                pBForLine.MouseMove += CanvasPilot_MouseMove;
                pBForLine.MouseEnter += CanvasPilot_MouseEnter;
                pBForLine.MouseLeave += CanvasPilot_MouseLeave;
                pBForLine.MouseDown += CanvasPilot_MouseDown;
                pBForLine.MouseUp += CanvasPilot_MouseUp;
                pBForLine.MouseCaptureChanged += CanvasPilot_MouseCaptureChanged;
                plMain.MouseDown += CanvasPilot_PanelMouseDown;
                plMain.MouseUp += CanvasPilot_PanelMouseUp;
                InitializeCanvasPilotMenu();
                canvasPilotDragTracker.Tick += CanvasPilotDragTracker_Tick;
                canvasPilotWheelFilter = new CanvasCalendarWheelFilter(this);
                Application.AddMessageFilter(canvasPilotWheelFilter);
                Trace.WriteLine("TimeSchedule canvas-pilot: mode=canvas-interactive-v17 interaction=hover,edit,menu,status,wheel,drag-preview,edge-resize");
            }
#if DEBUG
            else
            {
                Trace.WriteLine("TimeSchedule canvas-pilot: mode=buttons interaction=normal");
            }
#endif
        }

        /// <summary>Создаёт контекстное меню на весь календарь, не порождая контролы для задач.</summary>
        private void InitializeCanvasPilotMenu()
        {
            canvasPilotTaskMenu = new ContextMenuStrip();
            canvasPilotTaskMenu.Items.Add(new ToolStripMenuItem("Изменить", null, (sender, e) =>
            {
                TaskButton selected = canvasPilotContextTask;
                canvasPilotContextTask = null;
                if (selected != null)
                    selected.OpenEditor();
            }));
            canvasPilotStatusMenuItem = new ToolStripMenuItem("Изменить статус");
            foreach (TaskStatus status in Enum.GetValues(typeof(TaskStatus)))
            {
                TaskStatus selectedStatus = status;
                ToolStripMenuItem item = new ToolStripMenuItem(
                    ((TaskStatusRus)(int)status).ToString().Replace('_', ' '), null,
                    (sender, e) =>
                    {
                        TaskButton selected = canvasPilotContextTask;
                        canvasPilotContextTask = null;
                        if (selected != null)
                            ChangeCanvasPilotTaskStatus(selected, selectedStatus);
                    });
                item.Tag = status;
                canvasPilotStatusMenuItem.DropDownItems.Add(item);
            }
            canvasPilotTaskMenu.Items.Add(canvasPilotStatusMenuItem);
            canvasPilotDuplicateMenuItem = new ToolStripMenuItem("Дублировать", null, (sender, e) =>
            {
                TaskButton selected = canvasPilotContextTask;
                canvasPilotContextTask = null;
                if (selected != null)
                    DuplicateCanvasPilotTask(selected);
            });
            canvasPilotTaskMenu.Items.Add(canvasPilotDuplicateMenuItem);
            canvasPilotMoveMenuItem = new ToolStripMenuItem("Переместить", null, (sender, e) =>
            {
                TaskButton selected = canvasPilotContextTask;
                Point startPoint = canvasPilotContextPoint;
                canvasPilotContextTask = null;
                if (selected != null && Program.UserType == UserType.Admin && IsHandleCreated)
                    BeginInvoke(new Action(() => StartCanvasPilotMenuMove(selected, startPoint)));
            });
            canvasPilotTaskMenu.Items.Add(canvasPilotMoveMenuItem);
            canvasPilotDeleteMenuItem = new ToolStripMenuItem("Удалить", null, (sender, e) =>
            {
                TaskButton selected = canvasPilotContextTask;
                canvasPilotContextTask = null;
                if (selected != null)
                    DeleteCanvasPilotTask(selected);
            });
            canvasPilotTaskMenu.Items.Add(canvasPilotDeleteMenuItem);
        }

        /// <summary>Фиксирует новый статус одной задачи в Compact без изменения цвета или других полей.</summary>
        /// <param name="selected">Выбранная задача из отображаемого снимка.</param>
        /// <param name="status">Статус, выбранный пользователем.</param>
        private void ChangeCanvasPilotTaskStatus(TaskButton selected, TaskStatus status)
        {
            if (selected.Task.Status == status)
                return;

            long number = selected.Task.Number;
            Dictionary<long, string> expected;
            try
            {
                expected = TaskStore.Capture(Program.ListTasksAllPersonToSave, new[] { number });
                expected[number] = TaskStore.Fingerprint(selected.Task);
            }
            catch (TaskConflictException ex)
            {
                MessageBox.Show(this, ex.Message, "Изменение статуса", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Result result = Program.SaveProgramData(() =>
            {
                Task task = Program.ListTasksAllPersonToSave.Tasks.FirstOrDefault(item => item.Number == number);
                if (task == null)
                    throw new TaskConflictException("Задача удалена другим пользователем.");
                task.SetTaskStatus(status);
            }, expected);
            if (result == Result.Sucess)
                ShowProgressbarAndMakeRefresh();
            else if (result != Result.Canceled)
                MessageBox.Show(this, Program.SaveFailureMessage, "Изменение статуса",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>Находит верхнюю рисуемую задачу в координатах PictureBox календаря.</summary>
        private TaskButton HitTestCanvasPilot(Point location)
        {
            if (Program.ListTaskButtons == null)
                return null;
            List<TaskButton> tasks = Program.ListTaskButtons.TaskButtons;
            for (int i = tasks.Count - 1; i >= 0; i--)
            {
                foreach (Rectangle planned in tasks[i].CanvasPilotSegments)
                {
                    Rectangle hitArea = new Rectangle(planned.X + 1, planned.Y + 1,
                        planned.Width - 2, planned.Height - 1);
                    if (hitArea.Width > 0 && hitArea.Height > 0 && hitArea.Contains(location))
                        return tasks[i];
                }
            }
            return null;
        }

        /// <summary>Находит внешнюю левую или правую границу задачи, игнорируя стыки её сегментов.</summary>
        /// <param name="task">Задача под указателем.</param>
        /// <param name="location">Координаты указателя на холсте.</param>
        /// <returns>Режим захвата, разрешённый только администратору.</returns>
        private static CanvasPilotDragMode GetCanvasPilotGrabMode(TaskButton task, Point location)
        {
            if (task == null || Program.UserType != UserType.Admin)
                return CanvasPilotDragMode.Move;

            int left = int.MaxValue;
            int right = int.MinValue;
            foreach (Rectangle segment in task.CanvasPilotSegments)
            {
                left = Math.Min(left, segment.Left + 1);
                right = Math.Max(right, segment.Right - 2);
            }
            if (left > right)
                return CanvasPilotDragMode.Move;

            const int edgeSize = 8;
            int startDistance = Math.Abs(location.X - left);
            int finishDistance = Math.Abs(location.X - right);
            if (startDistance <= edgeSize && startDistance <= finishDistance)
                return CanvasPilotDragMode.ResizeStart;
            if (finishDistance <= edgeSize)
                return CanvasPilotDragMode.ResizeFinish;
            return CanvasPilotDragMode.Move;
        }

        /// <summary>Меняет подсветку задачи и курсор при переходе от центра полосы к её краям.</summary>
        private void SetCanvasPilotHoveredTask(TaskButton task)
        {
            if (!ReferenceEquals(canvasPilotHoveredTask, task))
            {
                canvasPilotHoveredTask = task;
                calendarTaskToolTip.SetToolTip(pBForLine,
                    task == null ? null : GetTaskToolTipText(task.Task));
                pBForLine.Invalidate();
            }
            if (!canvasPilotDragActive)
                pBForLine.Cursor = task == null ? Cursors.Default :
                    GetCanvasPilotGrabMode(task, GetCanvasPilotCursorPoint()) == CanvasPilotDragMode.Move
                    ? Cursors.Hand : Cursors.SizeWE;
        }

        /// <summary>Возвращает координаты курсора на холсте без 16-битного переполнения MouseEventArgs на широком PictureBox.</summary>
        private Point GetCanvasPilotCursorPoint()
        {
            return pBForLine.PointToClient(Control.MousePosition);
        }

        /// <summary>Подсвечивает задачу под курсором и ограниченно записывает факт попадания мыши в PictureBox.</summary>
        private void CanvasPilot_MouseMove(object sender, MouseEventArgs e)
        {
            Point location = GetCanvasPilotCursorPoint();
            if (canvasPilotDragActive)
            {
                UpdateCanvasPilotDragPreview(location);
                return;
            }
            if (canvasPilotPressedTask != null && Program.UserType == UserType.Admin &&
                (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
                Size threshold = SystemInformation.DragSize;
                if (Math.Abs(location.X - canvasPilotPressPoint.X) > threshold.Width / 2 ||
                    Math.Abs(location.Y - canvasPilotPressPoint.Y) > threshold.Height / 2)
                {
                    BeginCanvasPilotDrag(location);
                    return;
                }
            }
            TaskButton target = HitTestCanvasPilot(location);
            if (canvasPilotPictureMovesLogged < 3 ||
                (target != null && !canvasPilotFirstHitLogged))
            {
                canvasPilotPictureMovesLogged++;
                if (target != null)
                    canvasPilotFirstHitLogged = true;
                TraceCanvasPilotInput("picture-move", pBForLine, e.Location);
            }
            SetCanvasPilotHoveredTask(target);
        }

        /// <summary>Проверяет, получает ли PictureBox событие входа указателя.</summary>
        private void CanvasPilot_MouseEnter(object sender, EventArgs e)
        {
            TraceCanvasPilotInput("picture-enter", pBForLine,
                pBForLine.PointToClient(Control.MousePosition));
        }

        /// <summary>Запускает режим выбора нового положения для задачи из контекстного меню.</summary>
        /// <param name="selected">Выбранная задача со всеми её зависимыми задачами.</param>
        /// <param name="startPoint">Точка исходного клика в координатах календаря.</param>
        private void StartCanvasPilotMenuMove(TaskButton selected, Point startPoint)
        {
            if (IsDisposed || Program.UserType != UserType.Admin || canvasPilotDragActive)
                return;

            canvasPilotPressedTask = selected;
            canvasPilotPressPoint = startPoint;
            canvasPilotPressedMode = CanvasPilotDragMode.Move;
            BeginCanvasPilotDrag(startPoint, true);
            if (canvasPilotDragActive)
                Trace.WriteLine("TimeSchedule canvas-drag: phase=menu-start root=" +
                    selected.Task.Number);
            else
                canvasPilotPressedTask = null;
        }

        /// <summary>Фиксирует нажатия, направленные непосредственно в свободную область панели.</summary>
        private void CanvasPilot_PanelMouseDown(object sender, MouseEventArgs e)
        {
            TraceCanvasPilotInput("panel-down", plMain, e.Location);
        }

        /// <summary>Фиксирует отпускания кнопок мыши непосредственно в панели.</summary>
        private void CanvasPilot_PanelMouseUp(object sender, MouseEventArgs e)
        {
            TraceCanvasPilotInput("panel-up", plMain, e.Location);
        }

        /// <summary>Проверяет координаты указателя, целевую задачу и контрол под мышью без изменения интерфейса.</summary>
        private void TraceCanvasPilotInput(string eventName, Control source, Point point)
        {
            Point canvasPoint = GetCanvasPilotCursorPoint();
            Point panelPoint = plMain.PointToClient(Control.MousePosition);
            Control topControl = plMain.GetChildAtPoint(panelPoint);
            TaskButton hit = HitTestCanvasPilot(canvasPoint);
            Trace.WriteLine("TimeSchedule canvas-input: event=" + eventName +
                " source=" + source.Name +
                " position=" + point +
                " canvas-position=" + canvasPoint +
                " hit=" + (hit == null ? "none" : hit.Task.Number.ToString()) +
                " top-child=" + (topControl == null ? "none" : topControl.Name) +
                " picture-bounds=" + pBForLine.Bounds +
                " picture-visible=" + pBForLine.Visible +
                " picture-enabled=" + pBForLine.Enabled);
        }

        /// <summary>Сбрасывает подсветку, когда курсор покидает календарь.</summary>
        private void CanvasPilot_MouseLeave(object sender, EventArgs e)
        {
            if (!canvasPilotDragActive)
                SetCanvasPilotHoveredTask(null);
        }

        /// <summary>Запоминает задачу и начальную точку клика; данные задач не изменяются.</summary>
        private void CanvasPilot_MouseDown(object sender, MouseEventArgs e)
        {
            TraceCanvasPilotInput("picture-down-" + e.Button, pBForLine, e.Location);
            if (canvasPilotDragActive && canvasPilotMoveByClick)
                return;
            if (e.Button != MouseButtons.Left)
                return;
            Point location = GetCanvasPilotCursorPoint();
            canvasPilotPressedTask = HitTestCanvasPilot(location);
            canvasPilotPressPoint = location;
            canvasPilotPressedMode = GetCanvasPilotGrabMode(canvasPilotPressedTask, location);
            canvasPilotDragRoot = null;
            canvasPilotDragPreview = null;
            canvasPilotDragChain = null;
            canvasPilotDragExpected = null;
        }

        /// <summary>Обрабатывает контекстное меню, короткий клик и завершение переноса или изменения границ с учётом конечной позиции курсора.</summary>
        private void CanvasPilot_MouseUp(object sender, MouseEventArgs e)
        {
            TraceCanvasPilotInput("picture-up-" + e.Button, pBForLine, e.Location);
            if (canvasPilotDragActive)
            {
                if (canvasPilotMoveByClick && e.Button == MouseButtons.Right)
                    CancelCanvasPilotDragPreview();
                else if (e.Button == MouseButtons.Left)
                {
                    UpdateCanvasPilotDragPreview(GetCanvasPilotCursorPoint());
                    FinishCanvasPilotDragPreview();
                }
                return;
            }
            Point location = GetCanvasPilotCursorPoint();
            TaskButton target = HitTestCanvasPilot(location);
            if (e.Button == MouseButtons.Right)
            {
                canvasPilotPressedTask = null;
                if (target == null)
                    return;
                canvasPilotContextTask = target;
                canvasPilotContextPoint = location;
                canvasPilotMoveMenuItem.Enabled = Program.UserType == UserType.Admin;
                canvasPilotDuplicateMenuItem.Enabled = Program.UserType == UserType.Admin;
                canvasPilotDeleteMenuItem.Enabled = Program.UserType == UserType.Admin;
                foreach (ToolStripMenuItem item in canvasPilotStatusMenuItem.DropDownItems)
                    item.Checked = (TaskStatus)item.Tag == target.Task.Status;
                canvasPilotTaskMenu.Show(pBForLine, location);
                return;
            }
            if (e.Button != MouseButtons.Left)
                return;

            TaskButton pressed = canvasPilotPressedTask;
            Size threshold = SystemInformation.DragSize;
            bool moved =
                Math.Abs(location.X - canvasPilotPressPoint.X) > threshold.Width / 2 ||
                Math.Abs(location.Y - canvasPilotPressPoint.Y) > threshold.Height / 2;

            if (pressed != null && moved && Program.UserType == UserType.Admin)
            {
                BeginCanvasPilotDrag(location);
                if (canvasPilotDragActive)
                {
                    UpdateCanvasPilotDragPreview(location);
                    FinishCanvasPilotDragPreview();
                    return;
                }
            }

            canvasPilotPressedTask = null;
            if (pressed != null && !moved && ReferenceEquals(pressed, target))
                pressed.OpenEditor();
        }

        /// <summary>Начинает предпросмотр переноса зависимой цепочки без изменения сохраняемых задач.</summary>
        /// <param name="location">Координаты исходной точки захвата в системе PictureBox.</param>
        /// <param name="moveByClick">Выбор положения щелчком без удержания кнопки мыши.</param>
        private void BeginCanvasPilotDrag(Point location, bool moveByClick = false)
        {
            TaskButton root = canvasPilotPressedTask;
            if (root == null || Program.UserType != UserType.Admin ||
                Program.ListTasksAllPersonToSave == null)
                return;

            Rectangle firstSegment = root.CanvasPilotSegments.FirstOrDefault();
            if (firstSegment.Width <= 0)
                return;

            CanvasPilotDragMode mode = moveByClick ? CanvasPilotDragMode.Move : canvasPilotPressedMode;
            List<Task> chain = mode == CanvasPilotDragMode.ResizeStart
                ? new List<Task> { root.Task } : CollectCanvasPilotDragChain(root.Task);
            Dictionary<long, string> expected;
            try
            {
                expected = mode == CanvasPilotDragMode.ResizeStart
                    ? TaskStore.Capture(Program.ListTasksAllPersonToSave,
                        new[] { root.Task.Number })
                    : TaskStore.CaptureWithDependents(Program.ListTasksAllPersonToSave,
                        root.Task.Number);
                expected[root.Task.Number] = TaskStore.Fingerprint(root.Task);
                foreach (TaskButton shown in Program.ListTaskButtons.TaskButtons)
                    if (expected.ContainsKey(shown.Task.Number))
                        expected[shown.Task.Number] = TaskStore.Fingerprint(shown.Task);
            }
            catch (TaskConflictException ex)
            {
                MessageBox.Show(this, ex.Message, "Перенос задач", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            canvasPilotDragRoot = root;
            canvasPilotDragMode = mode;
            canvasPilotDragColumnOffset = (canvasPilotPressPoint.X - firstSegment.X) /
                Constants.COLUMN_WITH;
            canvasPilotDragChain = chain;
            canvasPilotDragExpected = expected;
            canvasPilotPreviewLogged = 0;
            canvasPilotDragActive = true;
            canvasPilotMoveByClick = moveByClick;
            canvasPilotLastDragCursor = Control.MousePosition;
            canvasPilotLastDragScroll = plMain.AutoScrollPosition;
            canvasPilotShiftTrackLogged = 0;
            SetCanvasPilotHoveredTask(null);
            if (!moveByClick)
                pBForLine.Capture = true;
            canvasPilotDragTracker.Start();
            pBForLine.Cursor = mode == CanvasPilotDragMode.Move ? Cursors.SizeAll : Cursors.SizeWE;
            Trace.WriteLine("TimeSchedule canvas-drag: phase=start root=" + root.Task.Number +
                " mode=" + mode + " chain=" + canvasPilotDragChain.Count +
                " storage-unchanged=True");
            UpdateCanvasPilotDragPreview(location);
        }

        /// <summary>Собирает последующих зависимых задач единственным проходом по графу связей.</summary>
        /// <param name="root">Задача, которую захватили мышью.</param>
        /// <returns>Список задач в порядке, позволяющем пересчитывать даты потомков.</returns>
        private static List<Task> CollectCanvasPilotDragChain(Task root)
        {
            List<Task> chain = new List<Task> { root };
            HashSet<long> visited = new HashSet<long> { root.Number };
            List<Task> allTasks = Program.ListTasksAllPersonToSave.Tasks;
            for (int i = 0; i < chain.Count; i++)
            {
                long parentNumber = chain[i].Number;
                foreach (Task candidate in allTasks)
                {
                    if (candidate.TaskNumberAfter == parentNumber &&
                        visited.Add(candidate.Number))
                        chain.Add(candidate);
                }
            }
            return chain;
        }

        /// <summary>Обновляет цепочку по дате и исполнителю, не сбрасывая последний предпросмотр при прокрутке между строками.</summary>
        /// <param name="location">Мировые координаты мыши в PictureBox.</param>
        private void UpdateCanvasPilotDragPreview(Point location)
        {
            if (!canvasPilotDragActive || canvasPilotDragChain == null ||
                Program.PoolTextBox == null || Program.ListPersonButton == null)
                return;
            if (canvasPilotDragMode != CanvasPilotDragMode.Move)
            {
                UpdateCanvasPilotResizePreview(location);
                return;
            }
            int index = location.X / Constants.COLUMN_WITH - canvasPilotDragColumnOffset;
            List<DateTextBox> dates = Program.PoolTextBox.ListTextBoxes;
            if (location.X < 0 || index < 0 || index >= dates.Count)
            {
                ClearCanvasPilotDragPreview();
                return;
            }

            PersonButton targetPerson = FindCanvasPilotDragTargetPerson(Control.MousePosition);
            if (targetPerson == null)
            {
                return;
            }

            DateTime startDate = dates[index].Date.Date;
            string personName = targetPerson.Person.PersonFamaly;
            if (canvasPilotDragPreview != null &&
                canvasPilotDragPreview.StartDate == startDate &&
                canvasPilotDragPreview.PersonName == personName)
                return;

            try
            {
                canvasPilotDragPreview = BuildCanvasPilotDragPreview(startDate, personName);
                pBForLine.Invalidate();
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException ||
                ex is InvalidOperationException || ex is OverflowException)
            {
                Trace.WriteLine("TimeSchedule canvas-drag preview failed: " + ex);
                CancelCanvasPilotDragPreview();
            }
        }

        /// <summary>Привязывает изменяемую границу к рабочему столбцу, сохраняя противоположную дату.</summary>
        /// <param name="location">Координаты указателя относительно холста после прокрутки.</param>
        private void UpdateCanvasPilotResizePreview(Point location)
        {
            if (canvasPilotDragRoot == null)
                return;
            List<DateTextBox> dates = Program.PoolTextBox.ListTextBoxes;
            if (dates.Count == 0)
                return;

            int index = location.X < 0 ? 0 : location.X / Constants.COLUMN_WITH;
            index = Math.Min(index, dates.Count - 1);
            DateTime edgeDate = dates[index].Date.Date;
            Task root = canvasPilotDragRoot.Task;
            DateTime start = canvasPilotDragMode == CanvasPilotDragMode.ResizeStart
                ? edgeDate : root.DateStart.Date;
            DateTime finish = canvasPilotDragMode == CanvasPilotDragMode.ResizeFinish
                ? edgeDate : root.DateFinish.Date;
            if (start > finish)
            {
                if (canvasPilotDragMode == CanvasPilotDragMode.ResizeStart)
                    start = finish;
                else
                    finish = start;
            }
            if (canvasPilotDragPreview != null &&
                canvasPilotDragPreview.StartDate == start &&
                canvasPilotDragPreview.FinishDate == finish)
                return;

            try
            {
                canvasPilotDragPreview = BuildCanvasPilotResizePreview(start, finish);
                pBForLine.Invalidate();
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException ||
                ex is InvalidOperationException || ex is OverflowException)
            {
                Trace.WriteLine("TimeSchedule canvas-resize preview failed: " + ex);
                ClearCanvasPilotDragPreview();
            }
        }

        /// <summary>Строит изменение одного края и сдвиг потомков только при изменении окончания.</summary>
        /// <param name="start">Предполагаемая дата начала корневой задачи.</param>
        /// <param name="finish">Предполагаемая дата окончания корневой задачи.</param>
        /// <returns>Независимые временные полосы и раскладка соседей по приоритетам.</returns>
        private CanvasPilotDragPreview BuildCanvasPilotResizePreview(DateTime start, DateTime finish)
        {
            CanvasPilotDragPreview preview = new CanvasPilotDragPreview
            {
                StartDate = start,
                FinishDate = finish,
                PersonName = canvasPilotDragRoot.Task.PersonFamaly
            };
            Task projectedRoot = CloneCanvasPilotTaskForLayout(canvasPilotDragRoot.Task);
            projectedRoot.ChangeDateRange(start, finish);
            preview.ProjectedTasks.Add(projectedRoot.Number, projectedRoot);

            if (canvasPilotDragMode == CanvasPilotDragMode.ResizeFinish)
            {
                for (int i = 1; i < canvasPilotDragChain.Count; i++)
                {
                    Task original = canvasPilotDragChain[i];
                    Task parent;
                    if (!preview.ProjectedTasks.TryGetValue(original.TaskNumberAfter, out parent))
                        throw new InvalidOperationException("Не удалось рассчитать цепочку зависимостей.");
                    DateTime newStart = AddCanvasPilotWorkingDays(parent.DateFinish, 2);
                    preview.ProjectedTasks.Add(original.Number,
                        CloneCanvasPilotTaskAt(original, newStart, original.PersonFamaly));
                }
            }

            CompleteCanvasPilotPreviewLayout(preview);
            if (canvasPilotPreviewLogged++ < 8)
                Trace.WriteLine("TimeSchedule canvas-resize: phase=preview root=" +
                    projectedRoot.Number + " mode=" + canvasPilotDragMode +
                    " start=" + start.ToString("yyyy-MM-dd") +
                    " finish=" + finish.ToString("yyyy-MM-dd") +
                    " chain=" + preview.ProjectedTasks.Count +
                    " bands=" + preview.TaskBands.Count +
                    " shifted-by-priority=" + preview.DisplacedTaskBands.Count);
            return preview;
        }

        /// <summary>Находит строку исполнителя под курсором или ближайшую видимую строку во время прокрутки.</summary>
        /// <param name="cursor">Экранные координаты курсора.</param>
        /// <returns>Исполнитель для магнитной привязки либо null, если подходящих строк нет.</returns>
        private PersonButton FindCanvasPilotDragTargetPerson(Point cursor)
        {
            Rectangle viewport = plPersonButton.RectangleToScreen(plPersonButton.ClientRectangle);
            PersonButton nearest = null;
            int nearestDistance = int.MaxValue;
            foreach (PersonButton candidate in Program.ListPersonButton.PersonButtons)
            {
                if (candidate.Person == null || candidate.Person.ListTask.Tasks.Count == 0)
                    continue;

                Rectangle row = candidate.Button.RectangleToScreen(candidate.Button.ClientRectangle);
                row.Intersect(viewport);
                if (row.IsEmpty)
                    continue;

                int distance = cursor.Y < row.Top ? row.Top - cursor.Y :
                    cursor.Y >= row.Bottom ? cursor.Y - row.Bottom + 1 : 0;
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                    if (distance == 0)
                        break;
                }
            }
            return nearest;
        }

        /// <summary>Снимает только визуальную проекцию при недопустимой цели переноса.</summary>
        private void ClearCanvasPilotDragPreview()
        {
            if (canvasPilotDragPreview == null)
                return;
            canvasPilotDragPreview = null;
            pBForLine.Invalidate();
        }

        /// <summary>Рассчитывает полосы корневой и всех последующих задач на отдельных временных моделях.</summary>
        /// <param name="startDate">Новая дата начала корневой задачи.</param>
        /// <param name="personName">Исполнитель корневой задачи.</param>
        /// <returns>Временный набор геометрии без ссылок на изменяемые задачи хранилища.</returns>
        private CanvasPilotDragPreview BuildCanvasPilotDragPreview(DateTime startDate, string personName)
        {
            CanvasPilotDragPreview preview = new CanvasPilotDragPreview
            {
                StartDate = startDate,
                PersonName = personName
            };
            Dictionary<long, Task> projected = new Dictionary<long, Task>();
            foreach (Task original in canvasPilotDragChain)
            {
                DateTime proposedDate = startDate;
                string proposedPerson = personName;
                if (original.Number != canvasPilotDragRoot.Task.Number)
                {
                    proposedPerson = original.PersonFamaly;
                    Task parent;
                    if (!projected.TryGetValue(original.TaskNumberAfter, out parent))
                        continue;
                    proposedDate = AddCanvasPilotWorkingDays(parent.DateFinish, 2);
                }
                Task clone = CloneCanvasPilotTaskAt(original, proposedDate, proposedPerson);
                if (original.Number == canvasPilotDragRoot.Task.Number &&
                    original.PersonFamaly != personName)
                    clone.SetPlaceInSynhTask(0);
                projected[clone.Number] = clone;
            }

            foreach (Task task in projected.Values)
                preview.ProjectedTasks.Add(task.Number, task);
            CompleteCanvasPilotPreviewLayout(preview);
            if (canvasPilotPreviewLogged++ < 8)
                Trace.WriteLine("TimeSchedule canvas-drag: phase=preview root=" +
                    canvasPilotDragRoot.Task.Number + " start=" +
                    startDate.ToString("yyyy-MM-dd") + " chain=" + projected.Count +
                    " bands=" + preview.TaskBands.Count +
                    " shifted-by-priority=" + preview.DisplacedTaskBands.Count + " dates=" +
                    string.Join(",", projected.Values.Select(task => task.Number + ":" +
                        task.DateStart.ToString("yyyy-MM-dd"))));
            return preview;
        }

        /// <summary>Подготавливает видимые полосы и приоритеты для обоих видов предпросмотра.</summary>
        /// <param name="preview">Предполагаемые даты и исполнители связанных задач.</param>
        private void CompleteCanvasPilotPreviewLayout(CanvasPilotDragPreview preview)
        {
            DateTime calendarStart = Program.ListTasksAllPersonToShow.GetMinDateStartTasks();
            DateTime calendarEnd = Program.ListTasksAllPersonToShow.GetMaxDateFinishTasks();
            foreach (Task projected in preview.ProjectedTasks.Values)
                if (projected.DateFinish > calendarEnd)
                    calendarEnd = projected.DateFinish;

            Dictionary<long, TaskButton> shown = new Dictionary<long, TaskButton>();
            foreach (TaskButton band in Program.ListTaskButtons.TaskButtons)
                shown[band.Task.Number] = band;
            HashSet<string> shownPersons = new HashSet<string>(Program.ListPersonButton.PersonButtons
                .Where(person => person.Person != null && person.Person.ListTask.Tasks.Count > 0 &&
                    ReferenceEquals(person.Button.Parent, plPersonButton))
                .Select(person => person.Person.PersonFamaly));
            ApplyCanvasPilotPriorityLayout(preview, shown, shownPersons,
                calendarStart, calendarEnd);
        }

        /// <summary>Оркестрирует расчёт дорожек затронутых исполнителей по исходным правилам приоритета.</summary>
        /// <param name="preview">Задачи с предполагаемыми датами.</param>
        /// <param name="shown">Исходные полосы для проверки перемещения соседей.</param>
        /// <param name="shownPersons">Исполнители с видимыми строками.</param>
        /// <param name="calendarStart">Начало отображаемого интервала.</param>
        /// <param name="calendarEnd">Окончание отображаемого интервала.</param>
        private void ApplyCanvasPilotPriorityLayout(CanvasPilotDragPreview preview,
            Dictionary<long, TaskButton> shown, HashSet<string> shownPersons,
            DateTime calendarStart, DateTime calendarEnd)
        {
            HashSet<string> affectedPersons = new HashSet<string>(StringComparer.Ordinal);
            foreach (Task original in canvasPilotDragChain)
                affectedPersons.Add(original.PersonFamaly);
            foreach (Task projected in preview.ProjectedTasks.Values)
                affectedPersons.Add(projected.PersonFamaly);

            foreach (string personName in affectedPersons)
            {
                if (!shownPersons.Contains(personName))
                    continue;
                ListTasks layout = CreateCanvasPilotPersonLayout(preview, personName);
                layout.AssingPlace();
                AddCanvasPilotLayoutBands(preview, shown, layout, calendarStart, calendarEnd);
            }
        }

        /// <summary>Собирает копии задач одного исполнителя для безопасного пересчёта дорожек.</summary>
        /// <param name="preview">Изменённые задачи, заменяющие исходные модели.</param>
        /// <param name="personName">Исполнитель, чья строка пересчитывается.</param>
        /// <returns>Отдельный список для штатного алгоритма размещения.</returns>
        private static ListTasks CreateCanvasPilotPersonLayout(CanvasPilotDragPreview preview, string personName)
        {
            ListTasks layout = new ListTasks();
            HashSet<long> included = new HashSet<long>();
            foreach (Task original in Program.ListTasksAllPersonToShow.Tasks)
            {
                Task projected;
                if (preview.ProjectedTasks.TryGetValue(original.Number, out projected))
                {
                    if (projected.PersonFamaly == personName && included.Add(projected.Number))
                        layout.AddTask(projected);
                }
                else if (original.PersonFamaly == personName && included.Add(original.Number))
                {
                    layout.AddTask(CloneCanvasPilotTaskForLayout(original));
                }
            }
            foreach (Task projected in preview.ProjectedTasks.Values)
            {
                if (projected.PersonFamaly == personName && included.Add(projected.Number))
                    layout.AddTask(projected);
            }
            return layout;
        }

        /// <summary>Создаёт полосы изменённых задач и соседей, у которых изменился номер дорожки.</summary>
        /// <param name="preview">Целевые модели и результат отображения.</param>
        /// <param name="shown">Исходные полосы для сравнения дорожек.</param>
        /// <param name="layout">Распределённые штатным алгоритмом задачи.</param>
        /// <param name="calendarStart">Начало отображаемого интервала.</param>
        /// <param name="calendarEnd">Окончание отображаемого интервала.</param>
        private static void AddCanvasPilotLayoutBands(CanvasPilotDragPreview preview,
            Dictionary<long, TaskButton> shown, ListTasks layout,
            DateTime calendarStart, DateTime calendarEnd)
        {
            foreach (Task positioned in layout.Tasks)
            {
                TaskButton existing;
                bool inChain = preview.ProjectedTasks.ContainsKey(positioned.Number);
                if (inChain)
                {
                    preview.VisibleTaskNumbers.Add(positioned.Number);
                    preview.TaskBands.Add(new TaskButton(positioned, Program.ListPersonButton,
                        calendarStart, calendarEnd));
                }
                else if (shown.TryGetValue(positioned.Number, out existing) &&
                    existing.Task.PlaceInSynchTask != positioned.PlaceInSynchTask)
                {
                    preview.VisibleTaskNumbers.Add(positioned.Number);
                    preview.DisplacedTaskBands.Add(new TaskButton(positioned,
                        Program.ListPersonButton, calendarStart, calendarEnd));
                }
            }
        }

        /// <summary>Копирует неизменяемые данные соседней задачи для расчёта расположения без вмешательства в UI-модель.</summary>
        /// <param name="source">Существующая задача, расположение которой может быть пересчитано.</param>
        /// <returns>Независимая временная задача с исходными датами, цветом и приоритетом.</returns>
        private static Task CloneCanvasPilotTaskForLayout(Task source)
        {
            Task copy = new Task(source.Status, source.Name, source.PersonFamaly,
                source.Number, source.TaskNumberAfter, source.DateStart, source.DateFinish,
                source.CountDays, source.CountWorkingDays, source.Color, source.Priority);
            copy.SetPlaceInSynhTask(source.PlaceInSynchTask);
            return copy;
        }

        /// <summary>Копирует только данные, необходимые для независимого расчёта геометрии предпросмотра.</summary>
        /// <param name="source">Исходная задача, не подлежащая изменению.</param>
        /// <param name="dateStart">Предполагаемая дата начала.</param>
        /// <param name="personName">Предполагаемый исполнитель.</param>
        /// <returns>Новая несохраняемая модель задачи.</returns>
        private static Task CloneCanvasPilotTaskAt(Task source, DateTime dateStart, string personName)
        {
            DateTime dateFinish = AddCanvasPilotWorkingDays(dateStart, source.CountWorkingDays);
            long calendarDays = (dateFinish - dateStart).Days + 1;
            Task clone = new Task(source.Status, source.Name, personName, source.Number,
                source.TaskNumberAfter, dateStart, dateFinish, calendarDays,
                source.CountWorkingDays, source.Color, source.Priority);
            clone.SetPlaceInSynhTask(source.PlaceInSynchTask);
            return clone;
        }

        /// <summary>Сдвигает дату на указанное число рабочих дней, включая начальный рабочий день.</summary>
        /// <param name="start">Дата начала отсчёта.</param>
        /// <param name="workingDays">Число рабочих дней, включая стартовый.</param>
        /// <returns>Дата последнего рабочего дня в заданном интервале.</returns>
        private static DateTime AddCanvasPilotWorkingDays(DateTime start, long workingDays)
        {
            if (workingDays <= 0)
                return start.Date;
            DateTime current = start.Date;
            long remaining = workingDays;
            while (remaining > 0)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday &&
                    !Program.listNonWorkingDays.NonWorkingDays.Contains(current))
                    remaining--;
                if (remaining > 0)
                    current = current.AddDays(1);
            }
            return current;
        }

        /// <summary>Следит за геометрией предпросмотра после обработки прокрутки, не полагаясь на MouseMove при Shift.</summary>
        private void CanvasPilotDragTracker_Tick(object sender, EventArgs e)
        {
            if (!canvasPilotDragActive || pBForLine.IsDisposed || !pBForLine.IsHandleCreated)
                return;

            Point cursor = Control.MousePosition;
            Point scroll = plMain.AutoScrollPosition;
            bool shiftHeld = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
            if (cursor == canvasPilotLastDragCursor && scroll == canvasPilotLastDragScroll && !shiftHeld)
                return;

            canvasPilotLastDragCursor = cursor;
            canvasPilotLastDragScroll = scroll;
            Point canvasCursor = GetCanvasPilotCursorPoint();
            UpdateCanvasPilotDragPreview(canvasCursor);
            RedrawCanvasPilotDragViewport();
            if (shiftHeld && canvasPilotShiftTrackLogged++ < 5)
                Trace.WriteLine("TimeSchedule canvas-drag: phase=shift-track cursor=" + cursor +
                    " canvas-cursor=" + canvasCursor + " scroll=" + scroll +
                    " preview=" + (canvasPilotDragPreview != null));
        }

        /// <summary>Перерисовывает видимую часть холста сразу после прокрутки при переносе.</summary>
        private void RedrawCanvasPilotDragViewport()
        {
            Rectangle visible = pBForLine.RectangleToClient(
                plMain.RectangleToScreen(plMain.ClientRectangle));
            visible.Intersect(pBForLine.ClientRectangle);
            if (visible.IsEmpty)
                return;
            pBForLine.Invalidate(visible);
            pBForLine.Update();
        }

        /// <summary>Завершает интерактивную операцию, делегируя сохранение соответствующему сценарию.</summary>
        private void FinishCanvasPilotDragPreview()
        {
            CanvasPilotDragPreview preview = canvasPilotDragPreview;
            TaskButton root = canvasPilotDragRoot;
            Dictionary<long, string> expected = canvasPilotDragExpected;
            if (preview == null || root == null || expected == null ||
                Program.UserType != UserType.Admin)
            {
                CancelCanvasPilotDragPreview();
                return;
            }

            if (canvasPilotDragMode == CanvasPilotDragMode.Move)
                FinishCanvasPilotMovePreview(preview, root, expected);
            else
                FinishCanvasPilotResizePreview(preview, root, expected);
        }

        /// <summary>Проверяет цель переноса и передаёт изменение цепочки в транзакцию хранилища.</summary>
        /// <param name="preview">Согласованный предпросмотр новой геометрии.</param>
        /// <param name="root">Корневая задача, захваченная мышью.</param>
        /// <param name="expected">Версии всех задач, затрагиваемых переносом.</param>
        private void FinishCanvasPilotMovePreview(CanvasPilotDragPreview preview,
            TaskButton root, Dictionary<long, string> expected)
        {
            if (preview.StartDate == root.Task.DateStart.Date &&
                preview.PersonName == root.Task.PersonFamaly)
            {
                CancelCanvasPilotDragPreview();
                return;
            }
            if (!IsCanvasPilotPreviewComplete(preview, expected, "Перенос задач",
                "Не все связанные задачи доступны для предварительного " +
                "просмотра. Отобразите всех затронутых исполнителей и повторите перенос."))
                return;

            StopCanvasPilotDragForSave();
            long rootNumber = root.Task.Number;
            Result result = Program.SaveProgramData(
                () => ApplyCanvasPilotMoveChanges(preview, rootNumber, expected), expected);
            Trace.WriteLine("TimeSchedule canvas-drag: phase=drop root=" + rootNumber +
                " date=" + preview.StartDate.ToString("yyyy-MM-dd") +
                " person=" + preview.PersonName + " chain=" + expected.Count +
                " saved=" + (result == Result.Sucess));
            CompleteCanvasPilotPreviewSave(result, "Перенос задач");
        }

        /// <summary>Проверяет изменение границы и передаёт его в транзакцию хранилища.</summary>
        /// <param name="preview">Новые интервалы корневой и связанных задач.</param>
        /// <param name="root">Корневая задача до изменения длительности.</param>
        /// <param name="expected">Версии затрагиваемых записей до захвата.</param>
        private void FinishCanvasPilotResizePreview(CanvasPilotDragPreview preview,
            TaskButton root, Dictionary<long, string> expected)
        {
            if (preview.StartDate == root.Task.DateStart.Date &&
                preview.FinishDate == root.Task.DateFinish.Date)
            {
                CancelCanvasPilotDragPreview();
                return;
            }
            if (!IsCanvasPilotPreviewComplete(preview, expected, "Изменение длительности",
                "Не все связанные задачи доступны для предварительного " +
                "просмотра. Отобразите их исполнителей и повторите изменение."))
                return;

            CanvasPilotDragMode mode = canvasPilotDragMode;
            long rootNumber = root.Task.Number;
            StopCanvasPilotDragForSave();
            Result result = Program.SaveProgramData(
                () => ApplyCanvasPilotResizeChanges(preview, rootNumber, mode, expected), expected);
            Trace.WriteLine("TimeSchedule canvas-resize: phase=drop root=" + rootNumber +
                " mode=" + mode + " start=" + preview.StartDate.ToString("yyyy-MM-dd") +
                " finish=" + preview.FinishDate.ToString("yyyy-MM-dd") +
                " chain=" + expected.Count + " saved=" + (result == Result.Sucess));
            CompleteCanvasPilotPreviewSave(result, "Изменение длительности");
        }

        /// <summary>Гарантирует полноту предпросмотра перед записью всех затрагиваемых задач.</summary>
        /// <param name="preview">Проекция целевого состояния.</param>
        /// <param name="expected">Снимок версий для записи.</param>
        /// <param name="title">Заголовок предупреждения.</param>
        /// <param name="missingMessage">Пояснение пользователю о недоступных полосах.</param>
        /// <returns>True, если все изменения отражены в доступных полосах.</returns>
        private bool IsCanvasPilotPreviewComplete(CanvasPilotDragPreview preview,
            Dictionary<long, string> expected, string title, string missingMessage)
        {
            if (preview.ProjectedTasks.Count == expected.Count &&
                preview.TaskBands.Count == expected.Count &&
                !preview.TaskBands.Any(band => !band.CanvasPilotSegments.Any()))
                return true;
            CancelCanvasPilotDragPreview();
            MessageBox.Show(this, missingMessage, title,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        /// <summary>Завершает захват мыши, сохраняя временные полосы до обновления календаря.</summary>
        private void StopCanvasPilotDragForSave()
        {
            canvasPilotDragTracker.Stop();
            canvasPilotDragActive = false;
            canvasPilotMoveByClick = false;
            canvasPilotPressedTask = null;
            pBForLine.Cursor = Cursors.Default;
            pBForLine.Capture = false;
        }

        /// <summary>Обновляет календарь только при подтверждённой записи либо возвращает исходный вид.</summary>
        /// <param name="result">Результат штатной операции сохранения.</param>
        /// <param name="title">Заголовок сообщения при отказе сохранения.</param>
        private void CompleteCanvasPilotPreviewSave(Result result, string title)
        {
            if (result == Result.Sucess)
            {
                ShowProgressbarAndMakeRefresh();
                return;
            }
            CancelCanvasPilotDragPreview();
            if (result != Result.Canceled)
                MessageBox.Show(this, Program.SaveFailureMessage, title,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>Под блокировкой сохраняет новые даты и исполнителя, не изменяя предшественников.</summary>
        /// <param name="preview">Новые даты всей зависимой цепочки.</param>
        /// <param name="rootNumber">Номер перемещаемой задачи.</param>
        /// <param name="expected">Версии полного состава цепочки на момент захвата.</param>
        private static void ApplyCanvasPilotMoveChanges(CanvasPilotDragPreview preview,
            long rootNumber, Dictionary<long, string> expected)
        {
            ListTasks current = Program.ListTasksAllPersonToSave;
            Task currentRoot = current.Tasks.FirstOrDefault(task => task.Number == rootNumber);
            if (currentRoot == null)
                throw new TaskConflictException("Перемещаемая задача удалена другим пользователем.");
            List<Task> currentChain = CollectCanvasPilotDragChain(currentRoot);
            if (!new HashSet<long>(currentChain.Select(task => task.Number)).SetEquals(expected.Keys))
                throw new TaskConflictException("Состав зависимых задач изменился. " +
                    "Обновите календарь и повторите перенос.");
            foreach (Task item in currentChain)
            {
                Task projection;
                if (!preview.ProjectedTasks.TryGetValue(item.Number, out projection))
                    throw new TaskConflictException("Не удалось проверить всю цепочку задач.");
                item.ChangeDatesAndCountDays(projection.DateStart, item.CountWorkingDays);
                if (item.DateFinish.Date != projection.DateFinish.Date)
                    throw new TaskConflictException("Изменился календарь нерабочих дней. " +
                        "Обновите календарь и повторите перенос.");
                if (item.Number == rootNumber)
                {
                    item.SetPersonFamaly(preview.PersonName);
                    item.SetTaskNumberAfter(0);
                }
            }
        }

        /// <summary>Под блокировкой изменяет одну границу и даты связанных задач по старому правилу.</summary>
        /// <param name="preview">Новые интервалы и даты зависимостей.</param>
        /// <param name="rootNumber">Номер задачи с изменяемой границей.</param>
        /// <param name="mode">Захваченный край; при изменении начала потомки не сдвигаются.</param>
        /// <param name="expected">Версии записываемых задач на момент захвата.</param>
        private static void ApplyCanvasPilotResizeChanges(CanvasPilotDragPreview preview,
            long rootNumber, CanvasPilotDragMode mode, Dictionary<long, string> expected)
        {
            ListTasks current = Program.ListTasksAllPersonToSave;
            Task currentRoot = current.Tasks.FirstOrDefault(task => task.Number == rootNumber);
            if (currentRoot == null)
                throw new TaskConflictException("Изменяемая задача удалена другим пользователем.");
            List<Task> chain = mode == CanvasPilotDragMode.ResizeStart
                ? new List<Task> { currentRoot } : CollectCanvasPilotDragChain(currentRoot);
            if (!new HashSet<long>(chain.Select(task => task.Number)).SetEquals(expected.Keys))
                throw new TaskConflictException("Состав связанных задач изменился. " +
                    "Обновите календарь и повторите изменение.");
            foreach (Task item in chain)
            {
                Task projected;
                if (!preview.ProjectedTasks.TryGetValue(item.Number, out projected))
                    throw new TaskConflictException("Не удалось проверить все связанные задачи.");
                if (item.Number == rootNumber)
                {
                    item.ChangeDateRange(projected.DateStart, projected.DateFinish);
                    if (item.CountWorkingDays != projected.CountWorkingDays)
                        throw new TaskConflictException("Изменился календарь нерабочих дней. " +
                            "Обновите календарь и повторите изменение.");
                }
                else
                {
                    item.ChangeDatesAndCountDays(projected.DateStart, item.CountWorkingDays);
                    if (item.DateFinish.Date != projected.DateFinish.Date)
                        throw new TaskConflictException("Изменился календарь нерабочих дней. " +
                            "Обновите календарь и повторите изменение.");
                }
            }
        }

        /// <summary>Отменяет предпросмотр, восстанавливая обычный курсор и подсветку.</summary>
        private void CancelCanvasPilotDragPreview()
        {
            canvasPilotDragTracker.Stop();
            bool wasDragging = canvasPilotDragActive;
            bool hadPreview = canvasPilotDragPreview != null;
            canvasPilotDragActive = false;
            canvasPilotMoveByClick = false;
            canvasPilotDragMode = CanvasPilotDragMode.Move;
            canvasPilotPressedMode = CanvasPilotDragMode.Move;
            canvasPilotDragPreview = null;
            canvasPilotDragChain = null;
            canvasPilotDragExpected = null;
            canvasPilotDragRoot = null;
            canvasPilotPressedTask = null;
            if (canvasPilotEnabled && !pBForLine.IsDisposed)
            {
                pBForLine.Cursor = Cursors.Default;
                if (wasDragging)
                {
                    pBForLine.Capture = false;
                    SetCanvasPilotHoveredTask(HitTestCanvasPilot(GetCanvasPilotCursorPoint()));
                }
                if (wasDragging || hadPreview)
                    pBForLine.Invalidate();
            }
        }

        /// <summary>Отменяет перенос при потере мышиного захвата окном.</summary>
        private void CanvasPilot_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (!canvasPilotDragActive || canvasPilotMoveByClick || pBForLine.Capture || !IsHandleCreated)
                return;
            BeginInvoke(new Action(() =>
            {
                if (canvasPilotDragActive && !canvasPilotMoveByClick && !pBForLine.Capture)
                    CancelCanvasPilotDragPreview();
            }));
        }

        /// <summary>Отменяет просмотр переноса клавишей Esc, не перехватывая другие команды.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Escape && canvasPilotDragActive)
            {
                CancelCanvasPilotDragPreview();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Создаёт самостоятельную копию задачи с новым номером под блокировкой Compact.</summary>
        /// <param name="selected">Исходная задача из актуального представления календаря.</param>
        private void DuplicateCanvasPilotTask(TaskButton selected)
        {
            if (Program.UserType != UserType.Admin || selected == null)
                return;

            long number = selected.Task.Number;
            Dictionary<long, string> expected;
            try
            {
                expected = TaskStore.Capture(Program.ListTasksAllPersonToSave, new[] { number });
                expected[number] = TaskStore.Fingerprint(selected.Task);
            }
            catch (TaskConflictException ex)
            {
                MessageBox.Show(this, ex.Message, "Дублирование задачи", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            long newNumber = 0;
            Result result = Program.SaveProgramData(() =>
            {
                ListTasks current = Program.ListTasksAllPersonToSave;
                Task original = current.Tasks.FirstOrDefault(task => task.Number == number);
                if (original == null)
                    throw new TaskConflictException("Исходная задача удалена другим пользователем.");
                newNumber = current.GetNextNumForTask();
                if (newNumber <= 0)
                    throw new InvalidOperationException("Не удалось выделить новый номер задачи.");

                Task duplicate = new Task(original.Status, original.Name, original.PersonFamaly,
                    newNumber, original.TaskNumberAfter, original.DateStart, original.DateFinish,
                    original.CountDays, original.CountWorkingDays, original.Color, original.Priority);
                duplicate.SetPlaceInSynhTask(original.PlaceInSynchTask);
                foreach (Project project in original.PoolProjects.Projects)
                    duplicate.PoolProjects.Add(new Project(project.Name, project.Address));
                TreeProjects originalTree = original.GetTreeProjects();
                TreeProjects copyTree = duplicate.GetTreeProjects();
                copyTree.SetName(originalTree.GetName());
                copyTree.SetTreeViewProjects(originalTree.ListTreeNode);
                current.AddTask(duplicate);
            }, expected);
            if (result == Result.Sucess)
                ShowProgressbarAndMakeRefresh();
            else if (result != Result.Canceled)
                MessageBox.Show(this, Program.SaveFailureMessage, "Дублирование задачи",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>Подтверждает удаление задачи и атомарное отсоединение её непосредственных потомков.</summary>
        /// <param name="selected">Задача из показанного пользователю снимка.</param>
        private void DeleteCanvasPilotTask(TaskButton selected)
        {
            if (Program.UserType != UserType.Admin || selected == null)
                return;

            long number = selected.Task.Number;
            Dictionary<long, string> expected;
            try
            {
                expected = TaskStore.CaptureDeletion(Program.ListTasksAllPersonToSave, new[] { number });
                expected[number] = TaskStore.Fingerprint(selected.Task);
            }
            catch (TaskConflictException ex)
            {
                MessageBox.Show(this, ex.Message, "Удаление задачи", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int dependents = expected.Count - 1;
            string message = dependents == 0
                ? "Удалить задачу №" + number + "?"
                : "Количество непосредственных потомков задачи №" + number + ": " + dependents + "." +
                  "\n\nДа — удалить задачу и разорвать связи с этими потомками." +
                  "\nНет — отменить удаление.\nДаты потомков останутся прежними.";
            if (MessageBox.Show(this, message, "Удаление задачи", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            Result result = Program.SaveProgramData(() =>
                TaskStore.DeleteWithDirectDependents(Program.ListTasksAllPersonToSave,
                    new[] { number }, expected), expected);
            if (result == Result.Sucess)
                ShowProgressbarAndMakeRefresh();
            else if (result != Result.Canceled)
                MessageBox.Show(this, Program.SaveFailureMessage, "Удаление задачи",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Очищает временные ссылки при перестроении календаря.</summary>
        private void ResetCanvasPilotInteraction()
        {
            CancelCanvasPilotDragPreview();
            canvasPilotPressedTask = null;
            canvasPilotContextTask = null;
            if (canvasPilotEnabled)
                SetCanvasPilotHoveredTask(null);
        }

        /// <summary>Сбрасывает показатели, не меняя существующие счетчики обновления.</summary>
        private void ResetCanvasPilotPaintMetrics()
        {
            canvasPilotPaintFailed = false;
            canvasPilotPaintEvents = 0;
            canvasPilotLastSegments = 0;
            canvasPilotMaxPaintMs = 0;
            canvasPilotTotalPaintMs = 0;
        }

        /// <summary>Оркестрирует рисование основной геометрии и временных полос с учётом области Paint.</summary>
        private void PaintCanvasPilot(object sender, PaintEventArgs e)
        {
            if (!canvasPilotEnabled || canvasPilotPaintFailed ||
                Program.ListTaskButtons == null)
                return;

            long started = Stopwatch.GetTimestamp();
            int segments = 0;
            try
            {
                PaintCanvasPilotTaskBands(e.Graphics, e.ClipRectangle, ref segments);
                PaintCanvasPilotPreviewBands(e.Graphics, e.ClipRectangle, ref segments);
            }
            catch (Exception ex) when (ex is ArgumentException ||
                ex is ObjectDisposedException || ex is ExternalException)
            {
                HandleCanvasPilotPaintFailure(ex);
            }
            finally
            {
                double elapsedMs = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
                canvasPilotPaintEvents++;
                canvasPilotLastSegments = segments;
                canvasPilotTotalPaintMs += elapsedMs;
                canvasPilotMaxPaintMs = Math.Max(canvasPilotMaxPaintMs, elapsedMs);
            }
        }

        /// <summary>Рисует существующие полосы, пропуская временно замещённые предпросмотром.</summary>
        /// <param name="graphics">Поверхность текущего Paint.</param>
        /// <param name="clip">Границы области перерисовки.</param>
        /// <param name="segments">Общий счётчик нарисованных сегментов, сохраняемый и при ошибке.</param>
        private void PaintCanvasPilotTaskBands(Graphics graphics, Rectangle clip, ref int segments)
        {
            foreach (TaskButton taskButton in Program.ListTaskButtons.TaskButtons)
            {
                if (canvasPilotDragPreview != null &&
                    canvasPilotDragPreview.VisibleTaskNumbers.Contains(taskButton.Task.Number))
                    continue;
                Task task = taskButton.Task;
                foreach (Rectangle planned in taskButton.CanvasPilotSegments)
                {
                    Rectangle rectangle = new Rectangle(
                        planned.X + 1, planned.Y + 1, planned.Width - 2, planned.Height - 1);
                    if (rectangle.Width <= 0 || rectangle.Height <= 0 ||
                        !clip.IntersectsWith(rectangle))
                        continue;

                    Color color = ReferenceEquals(canvasPilotHoveredTask, taskButton)
                        ? GetCanvasPilotHoverColor(task.Color) : task.Color;
                    graphics.FillRectangle(GetCanvasPilotBrush(color), rectangle);
                    TextRenderer.DrawText(graphics, task.Name ?? string.Empty,
                        GetCanvasPilotFont(task.Status), rectangle,
                        GetCanvasPilotTextColor(task),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    segments++;
                }
            }
        }

        /// <summary>Рисует цепочку предпросмотра и полосы соседей после перераспределения приоритетов.</summary>
        /// <param name="graphics">Поверхность текущего Paint.</param>
        /// <param name="clip">Границы области перерисовки.</param>
        /// <param name="segments">Общий счётчик нарисованных сегментов.</param>
        private void PaintCanvasPilotPreviewBands(Graphics graphics, Rectangle clip, ref int segments)
        {
            if (canvasPilotDragPreview == null)
                return;
            foreach (TaskButton taskBand in canvasPilotDragPreview.TaskBands
                .Concat(canvasPilotDragPreview.DisplacedTaskBands))
                PaintCanvasPilotPreviewBand(graphics, clip, taskBand, ref segments);
        }

        /// <summary>Рисует одну временную полосу в строке соответствующего исполнителя.</summary>
        /// <param name="graphics">Поверхность текущего Paint.</param>
        /// <param name="clip">Границы области перерисовки.</param>
        /// <param name="taskBand">Временная геометрия задачи.</param>
        /// <param name="segments">Общий счётчик видимых сегментов.</param>
        private void PaintCanvasPilotPreviewBand(Graphics graphics, Rectangle clip,
            TaskButton taskBand, ref int segments)
        {
            Task task = taskBand.Task;
            Rectangle firstSegment = taskBand.CanvasPilotSegments.FirstOrDefault();
            if (firstSegment.IsEmpty)
                return;
            PersonButton owner = Program.ListPersonButton.PersonButtons.FirstOrDefault(
                person => person.Person != null &&
                    person.Person.PersonFamaly == task.PersonFamaly);
            if (owner == null)
                return;
            int rowY = pBForLine.PointToClient(owner.Button.PointToScreen(Point.Empty)).Y +
                task.PlaceInSynchTask * Constants.ROW_HIGHT;
            int snapOffsetY = rowY - firstSegment.Y;
            using (Pen outline = new Pen(Color.FromArgb(230, 55, 55, 65), 1))
            {
                outline.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                foreach (Rectangle planned in taskBand.CanvasPilotSegments)
                {
                    Rectangle rectangle = new Rectangle(planned.X + 1,
                        planned.Y + 1 + snapOffsetY, planned.Width - 2, planned.Height - 1);
                    if (rectangle.Width <= 0 || rectangle.Height <= 0 ||
                        !clip.IntersectsWith(rectangle))
                        continue;
                    bool isDependentMove = canvasPilotDragPreview.ProjectedTasks
                        .ContainsKey(task.Number);
                    Color previewColor = isDependentMove
                        ? Color.FromArgb(195, task.Color) : task.Color;
                    graphics.FillRectangle(GetCanvasPilotBrush(previewColor), rectangle);
                    graphics.DrawRectangle(outline, rectangle);
                    TextRenderer.DrawText(graphics, task.Name ?? string.Empty,
                        GetCanvasPilotFont(task.Status), rectangle,
                        GetCanvasPilotTextColor(task),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
                    segments++;
                }
            }
        }

        /// <summary>После ошибки рисования сообщает пользователю о неполном календаре без модального окна внутри Paint.</summary>
        /// <param name="exception">Ошибка, уже зарегистрированная в диагностическом журнале.</param>
        private void HandleCanvasPilotPaintFailure(Exception exception)
        {
            canvasPilotPaintFailed = true;
            Trace.WriteLine("TimeSchedule canvas-pilot paint failed: " + exception);
            if (!IsHandleCreated || IsDisposed || Disposing)
                return;
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (!IsDisposed && !Disposing)
                        MessageBox.Show(this, "Не удалось отобразить задачи календаря. " +
                            "Обновите календарь. Если ошибка повторится, передайте журнал диагностики.\n\n" +
                            exception.Message, "Ошибка отображения задач",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }));
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine("TimeSchedule canvas-pilot paint notification skipped: " + ex);
            }
        }

        /// <summary>Слегка меняет яркость полосы при наведении, сохраняя её исходный цвет и прозрачность.</summary>
        private static Color GetCanvasPilotHoverColor(Color color)
        {
            const int maxChannel = 255;
            const int hoverStrength = 24;
            int luminance = (299 * color.R + 587 * color.G + 114 * color.B) / 1000;
            Color tint = luminance < 100 ? Color.White : Color.Black;
            return Color.FromArgb(color.A,
                (color.R * (maxChannel - hoverStrength) + tint.R * hoverStrength) / maxChannel,
                (color.G * (maxChannel - hoverStrength) + tint.G * hoverStrength) / maxChannel,
                (color.B * (maxChannel - hoverStrength) + tint.B * hoverStrength) / maxChannel);
        }

        /// <summary>Возвращает одну кисть для каждого фактически использованного цвета.</summary>
        private SolidBrush GetCanvasPilotBrush(Color color)
        {
            int key = color.ToArgb();
            if (!canvasPilotBrushes.TryGetValue(key, out SolidBrush brush))
            {
                brush = new SolidBrush(color);
                canvasPilotBrushes.Add(key, brush);
            }
            return brush;
        }

        /// <summary>Воспроизводит оформление статусов, не создавая шрифт при каждом Paint.</summary>
        private Font GetCanvasPilotFont(TaskStatus status)
        {
            FontStyle style = FontStyle.Regular;
            if (status == TaskStatus.Closed)
                style = FontStyle.Strikeout;
            else if (status == TaskStatus.Active)
                style = FontStyle.Underline;
            else if (status == TaskStatus.To_approving)
                style = FontStyle.Italic | FontStyle.Underline;
            if (!canvasPilotFonts.TryGetValue(style, out Font font))
            {
                font = new Font(Control.DefaultFont, style);
                canvasPilotFonts.Add(style, font);
            }
            return font;
        }

        /// <summary>Применяет исходное правило красного текста просроченных задач.</summary>
        private static Color GetCanvasPilotTextColor(Task task)
        {
            return ((task.Status == TaskStatus.New && task.DateStart.Date < DateTime.Today) ||
                (task.DateFinish.Date < DateTime.Today && task.Status != TaskStatus.Closed))
                ? Color.DarkRed : SystemColors.ControlText;
        }

        /// <summary>Записывает показатели контролов, USER/GDI, памяти и размера растра календаря.</summary>
        private void LogCanvasPilotIdleMetrics()
        {
#if DEBUG
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    long bitmapBytes = Bmp == null ? 0 : (long)Bmp.Width * Bmp.Height * 4;
                    Trace.WriteLine("TimeSchedule canvas-pilot-result: mode=" +
                        (canvasPilotEnabled ? "canvas-interactive-v17" : "buttons") +
                        " task-controls=" + (plMain.Controls.Count - 1) +
                        " total-main-controls=" + plMain.Controls.Count +
                        " gdi-handles=" + GetGuiResources(process.Handle, 0) +
                        " user-handles=" + GetGuiResources(process.Handle, 1) +
                        " private-memory-kb=" + process.PrivateMemorySize64 / 1024 +
                        " managed-memory-kb=" + GC.GetTotalMemory(false) / 1024 +
                        " bitmap-size=" + (Bmp == null ? "none" : Bmp.Width + "x" + Bmp.Height) +
                        " bitmap-estimated-kb=" + bitmapBytes / 1024 +
                        " canvas-paint-events=" + canvasPilotPaintEvents +
                        " canvas-last-visible-segments=" + canvasPilotLastSegments +
                        " canvas-paint-total-ms=" + Math.Round(canvasPilotTotalPaintMs, 1) +
                        " canvas-paint-max-ms=" + Math.Round(canvasPilotMaxPaintMs, 1) +
                        " paint-failed=" + canvasPilotPaintFailed);
                }
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception ||
                ex is EntryPointNotFoundException || ex is DllNotFoundException)
            {
                Trace.WriteLine("TimeSchedule canvas-pilot resource diagnostics failed: " + ex);
            }
#endif
        }

        /// <summary>Перехватывает колесо мыши в области календаря до штатной вертикальной прокрутки WinForms.</summary>
        private sealed class CanvasCalendarWheelFilter : IMessageFilter
        {
            /// <summary>Главное окно, которому принадлежит фильтр.</summary>
            private readonly Form1 owner;

            /// <summary>Создаёт фильтр для единственного окна календаря.</summary>
            public CanvasCalendarWheelFilter(Form1 owner)
            {
                this.owner = owner;
            }

            /// <summary>Обрабатывает WM_MOUSEWHEEL только над календарём активного окна.</summary>
            public bool PreFilterMessage(ref Message message)
            {
                return owner.ProcessCanvasCalendarWheel(ref message);
            }
        }

        /// <summary>Перенаправляет колесо над сеткой, датами и исполнителями в одну ось прокрутки.</summary>
        private bool ProcessCanvasCalendarWheel(ref Message message)
        {
            const int wmMouseWheel = 0x020A;
            if (message.Msg != wmMouseWheel || IsDisposed || !Visible ||
                Form.ActiveForm != this || !plMain.Visible ||
                (canvasPilotTaskMenu != null && canvasPilotTaskMenu.Visible))
                return false;

            Point position = Control.MousePosition;
            if (!plMain.RectangleToScreen(plMain.ClientRectangle).Contains(position) &&
                !plForDate.RectangleToScreen(plForDate.ClientRectangle).Contains(position) &&
                !plPersonButton.RectangleToScreen(plPersonButton.ClientRectangle).Contains(position))
                return false;

            int delta = unchecked((short)((message.WParam.ToInt64() >> 16) & 0xFFFF));
            ScrollCanvasCalendarWheel(delta, (Control.ModifierKeys & Keys.Shift) == Keys.Shift);
            return true;
        }

        /// <summary>Прокручивает по рабочим датам, а с Shift — по строкам исполнителей.</summary>
        /// <param name="delta">Дельта сообщения колеса мыши.</param>
        /// <param name="vertical">Вертикальная прокрутка при удержании Shift.</param>
        private void ScrollCanvasCalendarWheel(int delta, bool vertical)
        {
            const int wheelDelta = 120;
            const int columnsPerNotch = 3;
            const int rowsPerNotch = 3;
            int accumulated = (vertical ? canvasPilotVerticalWheelRemainder :
                canvasPilotHorizontalWheelRemainder) + delta;
            int notches = accumulated / wheelDelta;
            if (vertical)
                canvasPilotVerticalWheelRemainder = accumulated % wheelDelta;
            else
                canvasPilotHorizontalWheelRemainder = accumulated % wheelDelta;
            if (notches == 0)
                return;

            int previousX = plMain.HorizontalScroll.Value;
            int previousY = plMain.VerticalScroll.Value;
            ScrollProperties axis;
            if (vertical)
                axis = plMain.VerticalScroll;
            else
                axis = plMain.HorizontalScroll;
            if (!axis.Visible)
                return;
            int limit = Math.Max(axis.Minimum, axis.Maximum - axis.LargeChange + 1);
            int unit = vertical ? Constants.ROW_HIGHT * rowsPerNotch :
                Constants.COLUMN_WITH * columnsPerNotch;
            int next = (int)Math.Max(axis.Minimum,
                Math.Min((long)limit, (long)axis.Value - (long)notches * unit));
            if (next == axis.Value)
                return;

            try
            {
                plMain.AutoScrollPosition = new Point(vertical ? previousX : next,
                    vertical ? next : previousY);
                int actualX = plMain.HorizontalScroll.Value;
                int actualY = plMain.VerticalScroll.Value;
                plForDate.AutoScrollPosition = new Point(actualX, 0);
                plPersonButton.AutoScrollPosition = new Point(0, actualY);
                HorizontalScrollValue = actualX;
                VerticalScrollValue = actualY;
                LoadTaskButtons();
                LoadDateTextBoxes();
                if (canvasPilotDragActive)
                {
                    UpdateCanvasPilotDragPreview(GetCanvasPilotCursorPoint());
                    RedrawCanvasPilotDragViewport();
                }
                Trace.WriteLine("TimeSchedule canvas-wheel: axis=" +
                    (vertical ? "vertical" : "horizontal") + " from=" +
                    (vertical ? previousY : previousX) + " to=" +
                    (vertical ? actualY : actualX));
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException ||
                ex is InvalidOperationException)
            {
                Trace.WriteLine("TimeSchedule canvas-wheel failed: " + ex);
            }
        }

        /// <summary>Освобождает шрифты, кисти и подписку экспериментального режима.</summary>
        private void DisposeCanvasPilot()
        {
            CancelCanvasPilotDragPreview();
            canvasPilotDragTracker.Dispose();
            if (canvasPilotEnabled)
            {
                pBForLine.Paint -= PaintCanvasPilot;
                pBForLine.MouseMove -= CanvasPilot_MouseMove;
                pBForLine.MouseEnter -= CanvasPilot_MouseEnter;
                pBForLine.MouseLeave -= CanvasPilot_MouseLeave;
                pBForLine.MouseDown -= CanvasPilot_MouseDown;
                pBForLine.MouseUp -= CanvasPilot_MouseUp;
                pBForLine.MouseCaptureChanged -= CanvasPilot_MouseCaptureChanged;
                plMain.MouseDown -= CanvasPilot_PanelMouseDown;
                plMain.MouseUp -= CanvasPilot_PanelMouseUp;
                if (canvasPilotWheelFilter != null)
                {
                    Application.RemoveMessageFilter(canvasPilotWheelFilter);
                    canvasPilotWheelFilter = null;
                }
                if (canvasPilotTaskMenu != null)
                    canvasPilotTaskMenu.Dispose();
            }
            foreach (SolidBrush brush in canvasPilotBrushes.Values)
                brush.Dispose();
            foreach (Font font in canvasPilotFonts.Values)
                font.Dispose();
            canvasPilotBrushes.Clear();
            canvasPilotFonts.Clear();
        }

        /// <summary>Получает счётчики оконных и графических ресурсов процесса Windows.</summary>
        [DllImport("user32.dll")]
        private static extern int GetGuiResources(IntPtr processHandle, int kind);
    }

}
