using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace time_schedule
{
    [Serializable]
    public enum TaskStatus
    {
        New,
        Active,
        Closed,
        To_approving
    }
    [Serializable]
    public enum TaskStatusRus
    {
        Новое=TaskStatus.New,
        В_работе= TaskStatus.Active,
        Закрыто=TaskStatus.Closed,
        На_согласовании=TaskStatus.To_approving
    }
    [Serializable]
    public enum DayOfWeekRus {
        Воскресенье = DayOfWeek.Sunday,
        Понедельник = DayOfWeek.Monday,
        Вторник = DayOfWeek.Tuesday,
        Среда = DayOfWeek.Wednesday,
        Четверг = DayOfWeek.Thursday,
        Пятница = DayOfWeek.Friday,
        Суббота = DayOfWeek.Saturday
    }
    [Serializable]
    public class DateTextBox {
        public DateTime Date { get; private set; }
        public TextBox TextBox { get; private set; } = new TextBox();
        public Statuses.LoadingStatus LoadingStatus { get; private set; } = Statuses.LoadingStatus.NotReady;
        public DateTextBox (DateTime dateTime, int TextBoxHight, int locationX) {
            Date = dateTime.Date;
            TextBox.BorderStyle = BorderStyle.FixedSingle;
            TextBox.AutoSize = false;
            TextBox.Size = new Size(Constants.COLUMN_WITH, TextBoxHight);
            TextBox.Multiline = true;
            TextBox.Text = dateTime.ToShortDateString() + "\n" + GetDayOfWeekRus(dateTime.DayOfWeek);
            TextBox.BackColor = Color.AliceBlue;
            TextBox.ForeColor = Color.Black;
            TextBox.TextAlign = HorizontalAlignment.Center;
            TextBox.ReadOnly = true;
            if (dateTime == DateTime.Now.Date)
                TextBox.BackColor = Color.LightBlue;
            TextBox.Location = new Point(locationX, 0);            
        }
        public void SetLoadingStatus (Statuses.LoadingStatus loadingStatus) {
            LoadingStatus = loadingStatus;
        }
        private static DayOfWeekRus GetDayOfWeekRus(DayOfWeek dayOfWeek) {
            DayOfWeekRus dayOfWeekRus = (DayOfWeekRus)Enum.Parse(typeof(DayOfWeek), dayOfWeek.ToString(), true);
            return dayOfWeekRus;
        }
    }
    [Serializable]
    public class PoolTextBox {
        public List<DateTextBox> ListTextBoxes 
            { get; private set; } = new List<DateTextBox>();
        public void SetNewLocationToButtons(int horizontalScroll) {
            foreach (DateTextBox dateTextBox in ListTextBoxes) {
                if (dateTextBox.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad) {
                    dateTextBox.TextBox.Location = new Point(
                        dateTextBox.TextBox.Location.X - horizontalScroll, dateTextBox.TextBox.Location.Y );                   
                }
            }   
        }
        public void CalculateLoadingStatus(int horizontalScroll, int formWith, DateTime minDateStart) {
            DateTime minLoadDate = CalculateMinLoadDate(horizontalScroll, formWith, minDateStart);
            DateTime maxLoadDate = CalculateMaxLoadDate(formWith, minLoadDate);
            foreach (DateTextBox dateTextBox in ListTextBoxes) {
                if (IsReadyToLoad(minLoadDate, maxLoadDate, dateTextBox))
                    dateTextBox.SetLoadingStatus(Statuses.LoadingStatus.ReadyToLoad);
            }
        }
        private static Boolean IsReadyToLoad(DateTime minLoadDate, DateTime maxLoadDate, DateTextBox dateTextBox) {
            if (dateTextBox.LoadingStatus != Statuses.LoadingStatus.NotReady) {
                return false;
            }
            if (IsDateExsistInTargetRange(minLoadDate, maxLoadDate, dateTextBox)) {
                return true;
            }
            return false;
        }
        private static Boolean IsDateExsistInTargetRange(DateTime minLoadDate, DateTime maxLoadDate, DateTextBox dateTextBox) {
            if (dateTextBox.Date >= minLoadDate && dateTextBox.Date <= maxLoadDate)
                return true;
            return false;
        }
        private DateTime CalculateMinLoadDate(int horizontalScroll, int formWith, DateTime minDateStart) {
            int countDaysBeforScroll = horizontalScroll / Constants.COLUMN_WITH-2;
            int reserve = formWith / 2 / Constants.COLUMN_WITH;
            DateTime minLoadDate = minDateStart;
            for (int i = 0; i < countDaysBeforScroll; i++) {
                minLoadDate = minLoadDate.AddDays(1);
                if (IsHolydays(minLoadDate)) {
                    i--;
                }
            }
            return minLoadDate;
        }

        private static bool IsHolydays(DateTime minLoadDate) {
            if (Program.ListHolidays.Holidays.Contains(minLoadDate))
                return true;
            if (minLoadDate.DayOfWeek == DayOfWeek.Sunday)
                return true;
            if (minLoadDate.DayOfWeek == DayOfWeek.Wednesday)
                return true;
            return false;
        }
        private DateTime CalculateMaxLoadDate(int formWith, DateTime minLoadDate) {
            int reserve = formWith / 2 / Constants.COLUMN_WITH;
            int countDaysAfterScroll = formWith / Constants.COLUMN_WITH+2;
            DateTime maxLoadDate = minLoadDate;
            for (int i = 0; i < countDaysAfterScroll; i++) {
                if (maxLoadDate == DateTime.MaxValue)
                    break;
                maxLoadDate = maxLoadDate.AddDays(1);
                if (IsHolydays(maxLoadDate)) {
                    i--;
                }
            }
            return maxLoadDate;
        }
    }
    [Serializable]
    /// <summary>
    /// Исполнители
    /// </summary>
    public  class Person
    {
        public Person(string personFamaly)
        {
            PersonFamaly = personFamaly;
        }
        public Person(string  allParamTab, List<Task> tasks)
        {
            string[] allParam = allParamTab.Split('\t');
            const int NAME_STRING = 0;
            const int NUM_TASK_STRING_AFTER = 1;
            PersonFamaly = allParam[NAME_STRING];
            if (allParam.Length> NUM_TASK_STRING_AFTER)
            {
                for (int i = NUM_TASK_STRING_AFTER;i<allParam.Length;i++)
                {
                    foreach(Task task in tasks)
                    {
                        if (task.Number==Convert.ToInt32(allParam[i]))
                        {
                            AddTasks(task);
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string PersonFamaly
        { get; private set; }
        /// <summary>
        /// Задает фамилию исполнителя
        /// </summary>
        /// <param name="personName">фамилия</param>
        public void SetPersonFamaly(string personFamaly)
        {
            PersonFamaly = personFamaly;
        }
        public ListTasks ListTask
        { get; private set; } = new ListTasks();
        /// <summary>
        /// Список задач исполнителя
        /// </summary>
        /// <param name="task">задача</param>
        public void AddTasks(Task task)
        {
            ListTask.Tasks.Add(task);
        }
        public string GetStringForSave()
        {
            string stringForSave = string.Empty;
            PersonFamaly= PersonFamaly.Replace("\t", " ");
            stringForSave +=  PersonFamaly;
            foreach (Task task in ListTask.Tasks)
            {
                stringForSave = stringForSave + "\t" + task.Number;
            }
            return stringForSave;
        }
        public void setTasks(ListTasks listTasksAllPerson)
        {
            ListTask.Tasks.Clear();
            foreach(Task task in listTasksAllPerson.Tasks)
            {
                if (task.PersonFamaly == PersonFamaly)
                    AddTasks(task);
            }
        }
        public int GetMaxCountSynchTask(ListTasks listTasksAllPerson)
        {
            setTasks(listTasksAllPerson);
            int CountDaysSynchTask1 = 1;
            ListTask.AssingPlace();
            foreach(Task task in ListTask.Tasks)
            {
                if (task.PlaceInSynchTask + 1 > CountDaysSynchTask1)
                    CountDaysSynchTask1 = task.PlaceInSynchTask + 1;
            }
            return CountDaysSynchTask1;
        }
    }
    [Serializable]
    public class ListPersons
    {
        public List<Person> Persons
        { get; private set; } = new List<Person>();
        public void AddPerson(Person person)
        {
            Persons.Add(person);
        }
        public List<string> GetListForSave()
        {
            List<string> listForSave = new List<string>();
            foreach (Person person in Persons)
            {
                listForSave.Add(person.GetStringForSave());
            }
            return listForSave;
        }
        public void SetPersonsFromList(List<string> listString, List<Task> tasks)
        {
            foreach(string stringAllParamTab in listString)
            {
                Person person = new Person(stringAllParamTab, tasks);
                person.setTasks(Program.ListTasksAllPersonToSave);
                AddPerson(person);
            }
        }
        public void SetPersonList(List<Person> ListPerson)
        {
            Persons = ListPerson;
        }
    }
    [Serializable]
    public class ListTasks
    {
        public List<Task> Tasks
        { get; private set; } = new List<Task>();
        public void SetTasks(List<Task> tasks) {
            Tasks = tasks.GetRange(0, tasks.Count);
        }
        /// <summary>
        /// Список задач исполнителя
        /// </summary>
        /// <param name="task">задача</param>
        public void AddTask(Task task)
        {
            Tasks.Add(task);
        }
        public List<string> GetListForSave()
        {
            List<string> listForSave = new List<string>();
            foreach (Task task in Tasks)
            {
                listForSave.Add(task.GetStringForSave());
            }
            return listForSave;
        }
        public void SetTasksFromList(List<string> listStrings)
        {
            foreach (string stringAllParamTab in listStrings)
            {
                Task task = new Task(stringAllParamTab);
                AddTask(task);
            }
        }
        public long GetNextNumForTask()
        {
            long nextNumForTask = 0;
            foreach (Task task in Tasks)
            {
                if (task.Number > nextNumForTask)
                    nextNumForTask = task.Number;
            }
            return nextNumForTask + 1;
        }
        public DateTime GetMinDateStartTasks()
        {
            DateTime dateStartTasks = DateTime.MaxValue.AddDays(-1);
            foreach(Task task in Tasks)
            {
                if (task.DateStart < dateStartTasks)
                    dateStartTasks = task.DateStart;
            }
            return dateStartTasks;
        }
        public DateTime GetMaxDateFinishTasks()
        {
            DateTime dateFinishTasks = DateTime.MinValue;
            foreach (Task task in Tasks)
            {
                if (task.DateFinish > dateFinishTasks)
                    dateFinishTasks = task.DateFinish;
            }
            return dateFinishTasks;
        }
        private List<List<Task>> GetListsSynckTasks()
        {
            foreach (Task task in Tasks)
            {
                task.SetPlaceInSynhTask(0);
            }
            List<List<Task>> listSynckTasks = new List<List<Task>>();
            for (int i = 0; i < Tasks.Count; i++)
            {
                List<Task> synckTasks = new List<Task>();
                synckTasks.Add(Tasks[i]);
                for (int j = 0; j < Tasks.Count; j++)
                {
                    if (i!=j)
                    {
                        if ((Tasks[i].DateStart.Date >= Tasks[j].DateStart.Date && Tasks[i].DateStart.Date <= Tasks[j].DateFinish.Date) ||
                    (Tasks[i].DateStart.Date <= Tasks[j].DateStart.Date && Tasks[i].DateFinish.Date >= Tasks[j].DateStart.Date)
                    )
                        {
                            synckTasks.Add(Tasks[j]);
                        }
                    }
                }
                listSynckTasks.Add(synckTasks);
            }
            return listSynckTasks;
        }
        private void SetPlaceAfterChange(List<List<Task>> listSynckTasks,List<Task> synckTasks,Task changedTask)
        {
            for(int i = 0; i< listSynckTasks.Count; i++)
            {
                if(listSynckTasks[i]!= synckTasks && listSynckTasks[i].Contains(changedTask))
                {
                    for (int j=0;j< listSynckTasks[i].Count;j++)
                    {
                        if (changedTask != listSynckTasks[i][j])
                        {
                            if (changedTask.PlaceInSynchTask == listSynckTasks[i][j].PlaceInSynchTask)
                            {
                                if (changedTask.Priority >= listSynckTasks[i][j].Priority)
                                {
                                    changedTask.SetPlaceInSynhTask(changedTask.PlaceInSynchTask + 1);
                                    SetPlaceAfterChange(listSynckTasks, listSynckTasks[i], changedTask);
                                }
                                    
                                if (changedTask.Priority < listSynckTasks[i][j].Priority)
                                {
                                    listSynckTasks[i][j].SetPlaceInSynhTask(listSynckTasks[i][j].PlaceInSynchTask + 1);
                                    SetPlaceAfterChange(listSynckTasks, listSynckTasks[i], listSynckTasks[i][j]);
                                }

                            }
                        }
                    }
                }
            }
        }
        private void downPlaceAfterChange(List<List<Task>> listsSynckTasks)
        {
            for (int i=0; i<Tasks.Count;i++)
            {
                
               if(DownPlaceAfterChange(listsSynckTasks, Tasks[i]))
                {
                    Tasks[i].SetPlaceInSynhTask(Tasks[i].PlaceInSynchTask - 1);
                    downPlaceAfterChange(listsSynckTasks);
                }
                
            }
        }
        private bool DownPlaceAfterChange(List<List<Task>> listsSynckTasks, Task task)
        {
            foreach (List<Task> synchTasks in listsSynckTasks)
            {
                if (synchTasks[0]== task)
                {
                    foreach (Task targetTask in synchTasks)
                    {
                        if(targetTask != task)
                        {
                            if ( task.PlaceInSynchTask == 0 || targetTask.PlaceInSynchTask == (task.PlaceInSynchTask - 1))
                            {
                                return false;
                            }
                        }
                        if (synchTasks.Count <= 1)
                        {
                            return false;
                        }


                    }
                }
                
            }
            return true;

        }
        public void AssingPlace()
        {
            List<List<Task>> listsSynckTasks = GetListsSynckTasks();
            foreach (List<Task> synckTasks in listsSynckTasks)
            {
                for (int i = 0; i < synckTasks.Count; i++)
                {
                    for (int j = i + 1; j < synckTasks.Count; j++)
                    {
                        if (synckTasks[i].PlaceInSynchTask== synckTasks[j].PlaceInSynchTask)
                        {
                            if (synckTasks[i].Priority >= synckTasks[j].Priority)
                            {
                                synckTasks[i].SetPlaceInSynhTask(synckTasks[i].PlaceInSynchTask + 1);
                                SetPlaceAfterChange(listsSynckTasks, synckTasks, synckTasks[i]);
                            }
                            if (synckTasks[i].Priority < synckTasks[j].Priority)
                            {
                                synckTasks[j].SetPlaceInSynhTask(synckTasks[j].PlaceInSynchTask + 1);
                                SetPlaceAfterChange(listsSynckTasks, synckTasks, synckTasks[j]);
                            }

                        }
                    }
                }
                
            }
            downPlaceAfterChange(listsSynckTasks);
            // прописать метод
        }
    }
    [Serializable]
    public class ListHolidays
    {
        public List<DateTime> Holidays
        { get; private set; } = new List<DateTime>();
        public List<DateTime> GetHolidays()
        {
            return Holidays;
        }
        public List<string> GetListForSave()
        {
            List<string> listForSave = new List<string>();
            foreach (DateTime dateTime in Holidays)
            {
                string strForSave =string.Empty;
                strForSave += dateTime;
                listForSave.Add(strForSave);
            }
            return listForSave;
        }
        public void SetHolidaysFromList(List<string> listStrings)
        {
            List<DateTime> holidaysFromList = new List<DateTime>();
            foreach (string str in listStrings)
            {
                holidaysFromList.Add(Convert.ToDateTime(str));
            }
            Holidays.AddRange(holidaysFromList);
        }
    }
    [Serializable]
    public class ListNonWorkingDays
    {
        public List<DateTime> NonWorkingDays
        { get; private set; } = new List<DateTime>();
        public void AddNonWorkingDay (DateTime dateTime)
        {
            if (!NonWorkingDays.Contains(dateTime))
            NonWorkingDays.Add(dateTime);
        }
        public void AddNonWorkingDay(List<DateTime> listDateTime)
        {
            foreach (DateTime dateTime in listDateTime)
            {
                if (!NonWorkingDays.Contains(dateTime))
                    NonWorkingDays.Add(dateTime);
            }
        }
        public void NonWorkDaysWrite(DateTime вeginningPeriod, DateTime endPeriod)
        {
            for (int i = 0; вeginningPeriod.AddDays(i) <= endPeriod; i++)
            {
                if (вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                    вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    Program.listNonWorkingDays.NonWorkingDays.Add(вeginningPeriod.AddDays(i));
            }
        }

    }
    [Serializable]
    /// <summary>
    /// Задачи
    /// </summary>
    public class Task {
        public TaskStatus Status { get; private set; }
        public string Name { get; private set; }
        public long Number { get; private set; }
        public string PersonFamaly { get; private set; }
        public long TaskNumberAfter { get; private set; }
        public DateTime DateStart { get; private set; }
        public DateTime DateFinish { get; private set; }
        public long CountDays { get; private set; }
        public long CountWorkingDays { get; private set; }
        public Color Color { get; private set; }
        public long Priority { get; private set; }
        public int PlaceInSynchTask { get; private set; } = 0;
        public PoolProjects PoolProjects { get; private set; } = new PoolProjects();
        private TreeProjects TreeProjects  = new TreeProjects();
        public TreeProjects GetTreeProjects() {
            if (TreeProjects == null)
                TreeProjects = new TreeProjects();
            return TreeProjects;
        }
        public void SetTreeProject(TreeProjects treeProjects) {
            TreeProjects.SetTreeViewProjects(treeProjects.ListTreeNode);
        }
        public Task ()
        { }
        public Task(string allParamTab)
        {
            string[] allParam = allParamTab.Split('\t');
            const int TASK_NAME = 0;
            const int PERSON_FAMALY = 1;
            const int TASK_STATUS = 2;
            const int TASK_NUMBER = 3;
            const int TASK_NUMBER_AFTER = 4;
            const int DATE_START = 5;
            const int DATE_FINISH = 6;
            const int COUNT_DAYS = 7;
            const int COUNT_WORKING_DAYS = 8;
            const int TASK_COLOR = 9;
            const int PRIORITY = 10;
            Name = allParam[TASK_NAME];
            PersonFamaly = allParam[PERSON_FAMALY];
            if (allParam[TASK_STATUS] != "")
                Status = (TaskStatus)Enum.Parse(typeof(TaskStatus), allParam[TASK_STATUS], true);
            if (allParam[TASK_NUMBER] != "")
                Number = Convert.ToInt32(allParam[TASK_NUMBER]);
            if (allParam[TASK_NUMBER_AFTER] != "")
                TaskNumberAfter = Convert.ToInt32(allParam[TASK_NUMBER_AFTER]);
            if (allParam[DATE_START] != "")
                DateStart = Convert.ToDateTime(allParam[DATE_START]);
            if (allParam[DATE_FINISH] != "")
                DateFinish = Convert.ToDateTime(allParam[DATE_FINISH]);
            if (allParam[COUNT_DAYS] != "")
                CountDays = Convert.ToInt32(allParam[COUNT_DAYS]);
            if (allParam[COUNT_WORKING_DAYS] != "")
                CountWorkingDays = Convert.ToInt32(allParam[COUNT_WORKING_DAYS]);
            if (allParam[TASK_COLOR] != "")
                Color = Color.FromArgb(Convert.ToInt32(allParam[TASK_COLOR]));
            if (allParam.Length>10&& allParam[PRIORITY] != "")
                Priority = Convert.ToInt32(allParam[PRIORITY]);
        }
        public Task(DateTime dateStart, long countWorksDays)
        {
            dateStart = dateStart.Date;
            SetDateStart(dateStart);
            SetCountWorkingDays(countWorksDays);
            Program.listNonWorkingDays.NonWorkDaysWrite(dateStart, dateStart.AddDays(countWorksDays));
            SetCountDays(Program.listNonWorkingDays);
            SetDateFinish();

        }
        public Task(DateTime dateStart, DateTime dateFinish) {
            dateStart = dateStart.Date;
            dateFinish = dateFinish.Date;
            SetDateStart(dateStart);
            SetDateFinish(dateFinish);
            SetCountDays();
            SetCountWorkingDays(Program.listNonWorkingDays);
        }
        public Task(
                TaskStatus taskStatus, string taskName,
                string personFamaly, long taskNumber, 
                long taskNumberAfter,DateTime dateStart,
                DateTime dateFinish, long countDays,
                long countWorkingDays, Color taskColor,
                long priority
            ) {
            Status = taskStatus;
            Name = taskName;
            PersonFamaly = personFamaly;
            Number = taskNumber;
            TaskNumberAfter = taskNumberAfter;
            DateStart = dateStart;
            DateFinish = dateFinish;
            CountDays = countDays;
            CountWorkingDays = countWorkingDays;
            Color = taskColor;
            Priority = priority;
        }

        public void SetTaskName(string taskName) {
            Name = taskName;
        }
        public void SetTaskStatus(TaskStatus taskStatus) {
            Status = taskStatus;
        }
        static public DateTime GetDateFinish(DateTime dateStart, int workDayCount)
        {
            Task task = new Task(dateStart, workDayCount);
            return task.DateFinish;
        }
        public void ChangeDatesAndCountDays(DateTime dateStart, long countWorksDays)
        {
            dateStart = dateStart.Date;
            SetDateStart(dateStart);
            SetCountWorkingDays(countWorksDays);
            Program.listNonWorkingDays.NonWorkDaysWrite(dateStart, dateStart.AddDays(CountDays + 2));
            SetCountDays(Program.listNonWorkingDays);
            SetDateFinish();
        }
       
       
        public void SetPersonName(Person person)
        {
            PersonFamaly = person.PersonFamaly;
        }
        public void SetPersonFamaly(string personFamaly)
        {
            PersonFamaly = personFamaly;
        }
        
        public void SetTaskNumber(ListTasks listTask)
        {
            Number = listTask.Tasks.Count;
        }
        
        public void SetTaskNumberAfter(Task task)
        {
            TaskNumberAfter = task.Number;
        }
        public void SetTaskNumberAfter(int number)
        {
            TaskNumberAfter = number;
        }
       
        public void SetDateStart(DateTime dateStart)
        {
            DateStart = dateStart;
        }
       
        private void SetDateFinish(DateTime dateFinish)
        {
            DateFinish = dateFinish;
        }
        public void SetDateFinish()
        {
            DateTime dateTime = DateStart;
            DateFinish = dateTime.AddDays(CountDays-1);
        }
       
        private void SetCountDays()
        {
            CountDays = (DateFinish - DateStart).Days+1;
        }
        public void SetCountDays(long countDays)
        {
            CountDays = countDays;
        }
        public void SetCountDays(ListNonWorkingDays listNonWorkingDays)
        {
            DateTime dateTime = DateStart;
            CountDays = CountWorkingDays;
            for(int i=0;i< CountDays; i++)
            {
                if (listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i)))
                {
                    CountDays++;
                }
                if (
                    i == CountDays - 1
                    && (listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i)))
                    )
                {
                    int k = 1;
                    while(
                        listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i+k)))
                    {
                        CountDays++;
                        k++;
                    }
                }
            }
        }
       
        private void SetCountWorkingDays(long countWorkingDays)
        {
            CountWorkingDays = countWorkingDays;
        }
        private void SetCountWorkingDays(ListNonWorkingDays listNonWorkingDays)
        {
            CountWorkingDays = CountDays;
            DateTime date = DateStart;
            while (date <= DateFinish)
            {
                if (listNonWorkingDays.NonWorkingDays.Contains(date))
                    CountWorkingDays--;
                date=date.AddDays(1);
            }
        }
        public void SetTaskColor(Color taskColor)
        {
            Color = taskColor;
        }
        public string GetStringForSave()
        {
            string stringForSave = string.Empty;
            Name = Name.Replace("\t", " ");
            stringForSave += Name;
            PersonFamaly = PersonFamaly.Replace("\t", " "); ;
            stringForSave += "\t";
            stringForSave += PersonFamaly;
            stringForSave += "\t";
            stringForSave += Status;
            stringForSave += "\t";
            stringForSave += Number;
            stringForSave += "\t";            
            stringForSave += TaskNumberAfter;
            stringForSave += "\t";
            stringForSave += DateStart;
            stringForSave += "\t";
            stringForSave += DateFinish;
            stringForSave += "\t";
            stringForSave += CountDays;
            stringForSave += "\t";
            stringForSave += CountWorkingDays;
            stringForSave += "\t";
            stringForSave += Color.ToArgb(); 
            stringForSave += "\t";
            stringForSave += Priority;
            return stringForSave;
        }
        public void SetPriority(int newPriority)
        {
            Priority = newPriority;
        }
        public void SetPlaceInSynhTask(int placeInSynchTask)
        {
            PlaceInSynchTask = placeInSynchTask;
        }
    }
    [Serializable]
    public class ListTaskButton
    {
        public List<TaskButton> TaskButtons
        { get; private set; } = new List<TaskButton>();
        public void AddTaskButtons(TaskButton taskButton)
        {
            TaskButtons.Add(taskButton);
        }
        public void AddTaskButtons(ListTasks listTasks, ListPersonButton listPersonButton)
        {
            foreach (Task task in listTasks.Tasks)
            {
                TaskButton taskButton = new TaskButton(
                    task,
                    listPersonButton,
                    listTasks.GetMinDateStartTasks(),
                    listTasks.GetMaxDateFinishTasks());
                AddTaskButtons(taskButton);
            }
        }
        public void SetNewLocationToButtons (int verticalScroll, int horizontalScroll) {
            foreach (TaskButton taskButton in TaskButtons) {
                if (taskButton.LoadingStatus == Statuses.LoadingStatus.ReadyToLoad)
                foreach(Button button in taskButton.Buttons) {
                    button.Location = new Point(button.Location.X - horizontalScroll, button.Location.Y - verticalScroll);
                }
            }
        }
        public void CalculateLoadingStatus(int horizontalScroll, int formWith, DateTime minDateStart) {
            DateTime minLoadDate = CalculateMinLoadDate(horizontalScroll, formWith, minDateStart);
            DateTime maxLoadDate = CalculateMaxLoadDate(formWith, minLoadDate);
            foreach (TaskButton taskButton in TaskButtons) {
               if (IsReadyToLoad(minLoadDate, maxLoadDate, taskButton))
                    taskButton.SetLoadingStatus(Statuses.LoadingStatus.ReadyToLoad);
            }
        }
        private static Boolean IsReadyToLoad(DateTime minLoadDate, DateTime maxLoadDate, TaskButton taskButton) {
            if (taskButton.LoadingStatus != Statuses.LoadingStatus.NotReady) {
                return false;
            }
            if (IsDateExsistInTargetRange(minLoadDate, maxLoadDate, taskButton)) {
                return true;
            }
            return false;
        }
        private static Boolean IsDateExsistInTargetRange (DateTime minLoadDate, DateTime maxLoadDate, TaskButton taskButton) {
            if (taskButton.Task.DateStart >= minLoadDate && taskButton.Task.DateStart <= maxLoadDate)
                return true;
            if (taskButton.Task.DateFinish >= minLoadDate && taskButton.Task.DateFinish <= maxLoadDate)
                return true;
            if (taskButton.Task.DateStart <= minLoadDate && taskButton.Task.DateFinish >= maxLoadDate)
                return true;
            return false;
        }
        private DateTime CalculateMinLoadDate(int horizontalScroll, int formWith, DateTime minDateStart) {
            int countDaysBeforScroll = horizontalScroll / Constants.COLUMN_WITH - 1;
            int reserve = formWith / 2/ Constants.COLUMN_WITH;
            DateTime minLoadDate = minDateStart;
            try {
                for (int i = 0; i < countDaysBeforScroll; i++) {
                    minLoadDate = minLoadDate.AddDays(1);
                    if (IsHolydays(minLoadDate)) {
                        i--;
                    }
                }
            }
            catch { }
            return minLoadDate;
        }

        private static bool IsHolydays(DateTime minLoadDate) {
            if (Program.ListHolidays.Holidays.Contains(minLoadDate))
                return true;
            if (minLoadDate.DayOfWeek == DayOfWeek.Sunday)
                return true;
            if (minLoadDate.DayOfWeek == DayOfWeek.Wednesday)
                return true;
            return false;
        }
        private DateTime CalculateMaxLoadDate (int formWith, DateTime minLoadDate) {
            int reserve = formWith / 2 / Constants.COLUMN_WITH;
            int countDaysAfterScroll = formWith / Constants.COLUMN_WITH;
            DateTime maxLoadDate = minLoadDate;
            
            for (int i = 0; i < countDaysAfterScroll; i++) {
                if (maxLoadDate == DateTime.MaxValue)
                    break;
                maxLoadDate = maxLoadDate.AddDays(1);
                if (IsHolydays(maxLoadDate)) {
                    i--;
                }
            }
            return maxLoadDate;
        }
    }
    [Serializable]
    public class TaskButton
    {
        
        public Task Task
        { get; private set; }
       
        public List<Button> Buttons
        { get; private set; } = new List<Button>();
        public bool isDown {
            get;
            private set;
        } = false;
        public Statuses.LoadingStatus LoadingStatus {
            get; private set;
        } = Statuses.LoadingStatus.NotReady;
        public void SetTask(Task task) {
            Task = task;
        }
        public void SetLoadingStatus (Statuses.LoadingStatus loadingStatus) {
            LoadingStatus = loadingStatus;
        }
        public void AddButton(int locationX, int locationY, int with, int height)
        {
            Button button = new Button();
            button.Location = new Point(locationX+1, locationY+1);
            button.Width = with-2;
            button.Height = height-1;
            button.Text = Task.Name;
            button.BackColor = Task.Color;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.DimGray;
            button.Click += Button_Click;
            button.MouseHover += Button_MouseHover;
            void Button_MouseHover(object sender, EventArgs e)
            {
               int scroll = Program.fmMain.SetPlMain().Location.Y;
                ToolTip t = new ToolTip();
                string numTaskAfter = string.Empty;
                if (Task.TaskNumberAfter > 0)
                    numTaskAfter = "псоле №" + Task.TaskNumberAfter;
                t.SetToolTip(button,"№"+ Task.Number + "\n" + Task.Name + "\n" +
                    "до "+Task.DateFinish.ToString().Split(' ')[0] + "\n"+
                    "Статус: " + 
                    ((TaskStatusRus)Enum.Parse(typeof(TaskStatus),
                    Task.Status.ToString(),true)).ToString().Replace("_"," ") + "\n" +
                    numTaskAfter);
            }
            button.MouseDown += Button_MouseDown;
            button.MouseUp += Button_MouseUp;
            button.MouseLeave += Button_MouseLeave;
            if (Task.Status == TaskStatus.Closed)
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Strikeout);
            if (Task.Status == TaskStatus.Active)
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Underline);
            if (Task.Status == TaskStatus.To_approving) {
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Italic | FontStyle.Underline);
            }
            if (
                ((Task.Status == TaskStatus.New) && Task.DateStart.Date<DateTime.Now.Date) ||
                (Task.DateFinish.Date < DateTime.Now.Date) && (Task.Status != TaskStatus.Closed)
                )
            {
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, button.Font.Style);
                button.ForeColor = System.Drawing.Color.DarkRed;
            }

            void Button_MouseDown(object sender, MouseEventArgs e) {
                isDown = true;
                if (Program.UserType == UserType.Admin) {
                    Thread thread = new Thread(CursorAndWidth, 0);
                    thread.Start(button);
                }

                    
            }
            Buttons.Add(button);
        }
        private void CursorAndWidth(object button) {
            Thread.Sleep(250);
            if (isDown) {
                (button as Button).BeginInvoke(new Action(delegate () { (button as Button).Width = Constants.COLUMN_WITH; }));
                Program.fmMain.BeginInvoke(new Action(delegate () { Program.fmMain.Cursor = Cursors.NoMove2D; }));
            }
            
        }
        private void Button_MouseLeave(object sender, EventArgs e) {
            
            if (isDown && Program.UserType==UserType.Admin) {
                Program.fmMain.SetPlMain().VerticalScroll.Value = Program.fmMain.SetForm1().VerticalScrollValue;
                Program.fmMain.SetPlMain().VerticalScroll.Value = Program.fmMain.SetForm1().VerticalScrollValue;
                (PersonButton, DateTime) PDT = SetNewDateAndPerson(
                    Program.fmMain.PointToClient(Control.MousePosition),
                    Program.ListPersonButton,
                    Program.PoolTextBox);
                if (PDT.Item1.Person!=null && PDT.Item2 != DateTime.MinValue) {
                    //WriteNewNonWorkigDays();
                    string personFamaly = PDT.Item1.Person.PersonFamaly;
                    DateTime newDateStartTask = PDT.Item2;
                    //MessageBox.Show("" + PDT.Item1.Person.PersonFamaly + " " + PDT.Item2);

                    Task.SetPersonFamaly(PDT.Item1.Person.PersonFamaly);
                    Task.ChangeDatesAndCountDays(PDT.Item2.Date, Task.CountWorkingDays);
                    Task.SetTaskNumberAfter(0);
                    ChekTaskAfter(Task);
                    //Dals.WriteObjectToFile(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
                    Dals.WriteObjectToMainPathFile(Constants.TASKS_BIN, Program.ListTasksAllPersonToSave);
                    Program.fmMain.SetForm1().LoadRefreshForm(Statuses.ProgressBar.Use);
                    Program.fmMain.SetForm1().SetPlMain().Focus();
                }
            }
            isDown = false;
        }
        //private void WriteNewNonWorkigDays() {
        //    Program.listNonWorkingDays.NonWorkingDays.Sort();
        //    int newCountDaysAfterChange = (dTmTaskDateFinish.Value.Date - FinishDateBeforeChange).Days;
        //    if (newCountDaysAfterChange < 0)
        //        return;
        //    const int DIFFRENCE_QUANTITY_LAST_INDEX = 1;
        //    int lastIndex = Program.listNonWorkingDays.NonWorkingDays.Count - DIFFRENCE_QUANTITY_LAST_INDEX;
        //    DateTime newMaxDateFinish =
        //        Program.listNonWorkingDays.NonWorkingDays[lastIndex].AddDays(newCountDaysAfterChange);
        //    Program.listNonWorkingDays.NonWorkDaysWrite(FinishDateBeforeChange, newMaxDateFinish);
        //}
        private void ChekTaskAfter(Task task) {
            for (int i = 0; i < Program.ListTasksAllPersonToSave.Tasks.Count; i++) {
                if (Program.ListTasksAllPersonToSave.Tasks[i].TaskNumberAfter == task.Number) {
                    Program.ListTasksAllPersonToSave.Tasks[i].ChangeDatesAndCountDays(
                        Task.GetDateFinish(task.DateFinish, 2),
                        Program.ListTasksAllPersonToSave.Tasks[i].CountWorkingDays
                        ); // Magic number 2 to do
                    ChekTaskAfter(Program.ListTasksAllPersonToSave.Tasks[i]);
                }
            }
        }
        private (PersonButton, DateTime) SetNewDateAndPerson(Point point, ListPersonButton listPersonButton, PoolTextBox poolTextBox) {
            point.Y = point.Y - Program.fmMain.SetPlMain().Location.Y ;
            point.X = point.X - Program.fmMain.SetPlMain().Location.X ; // -Program.fmMain.SetPlMain().Location.X
            var targetPersonButton = new PersonButton();
            var dateTime = DateTime.MinValue;
            foreach (PersonButton personButton in listPersonButton.PersonButtons) {
                if (point.Y >= personButton.Button.Location.Y &&
                    point.Y <= personButton.Button.Location.Y + personButton.Button.Height) {
                    targetPersonButton = personButton;
                    break;
                }
            }
            int locX = 0;
            foreach (DateTextBox dateTextBox in poolTextBox.ListTextBoxes) {
                if (point.X >= dateTextBox.TextBox.Location.X &&
                    (point.X <= dateTextBox.TextBox.Location.X + dateTextBox.TextBox.Width)&&
                    dateTextBox.LoadingStatus==Statuses.LoadingStatus.Loaded) {
                    locX = dateTextBox.TextBox.Location.X;
                    dateTime = dateTextBox.Date.Date;
                    break;
                }
                
            }
            return (targetPersonButton, dateTime);
        }
        private void Button_MouseUp(object sender, MouseEventArgs e) {
            Program.fmMain.Cursor = Cursors.Default;
        }

        

        //LoadRefreshForm loadRefreshForm;
        private void Button_Click(object sender, EventArgs e)
        {
            Program.fmMain.SetPlForDate().HorizontalScroll.Value = Program.fmMain.SetPlMain().HorizontalScroll.Value;
            isDown = false;
            fmAddChangeTask fmAddTask = new fmAddChangeTask(Program.delegatLoadRefreshForm);
            fmAddTask.GhangeNamebtnCreateTask("Изменить");
            fmAddTask.SetCreateOrChange(CreateOrChange.Change);
            fmAddTask.StartPosition = FormStartPosition.CenterScreen;
            Program.Task = Task;
            fmAddTask.ShowDialog();
            Dals.WriteObjectToMainPathFile(Constants.TASKS, Program.ListTasksAllPersonToSave);
            
        }
        public TaskButton(Task task, ListPersonButton listPersonButton,DateTime minDateStart, DateTime maxDateFinish)
        {
            Task = task;
            int locationY = 0;
            int locationX = 0;
            int width = 0;
            List<Task> listTaskThisPerson = new List<Task>();
            foreach (PersonButton personButton in listPersonButton.PersonButtons)
            {
                if (personButton.Person.PersonFamaly == task.PersonFamaly)
                {
                    listTaskThisPerson = personButton.Person.ListTask.Tasks;
                    locationY = personButton.Button.Location.Y;
                }
            }
            locationY += Task.PlaceInSynchTask * Constants.ROW_HIGHT;
            
            int dateTableLastNumCol = (maxDateFinish- minDateStart).Days;
            DateTime dateTime = minDateStart;
            while ((dateTime.Date != Task.DateStart.Date) && (dateTime < maxDateFinish))
            {
                if (
                    dateTime.DayOfWeek != DayOfWeek.Saturday &&
                    dateTime.DayOfWeek != DayOfWeek.Sunday &&
                    !Program.listNonWorkingDays.NonWorkingDays.Contains(dateTime.Date)
                    )
                    locationX += Constants.COLUMN_WITH;
                dateTime = dateTime.AddDays(1);
            }
            int numberOfDay = 0;
            do
            {
                
                if (
                    dateTime.DayOfWeek != DayOfWeek.Sunday &&
                    dateTime.DayOfWeek != DayOfWeek.Saturday &&
                    !Program.listNonWorkingDays.NonWorkingDays.Contains(dateTime.Date))
                {
                    width += Constants.COLUMN_WITH;
                    numberOfDay++;
                    if ((numberOfDay >= 4 &&
                        dateTime.DayOfWeek == DayOfWeek.Friday ||
                        numberOfDay > 13 ||
                        dateTime.Date == Task.DateFinish.Date)&&
                        dateTime.AddDays(3).Date!= Task.DateFinish.Date) 
                    {
                        AddButton(locationX, locationY, width, Constants.ROW_HIGHT);
                        locationX += width;
                        width = 0;
                        numberOfDay = 0;
                    }
                }
                
                dateTime=dateTime.AddDays(1);
                
            }
            while ((dateTime.Date <= Task.DateFinish.Date) && (dateTime.Date <= maxDateFinish));
        }
    }
    [Serializable]
    public class ListPersonButton
    {
        public List<PersonButton> PersonButtons
        { get; private set; } = new List<PersonButton>();
        public void ListPersonButtonsAdd(PersonButton personButton)
        {
            PersonButtons.Add(personButton);
        }
        public void LoadListPersonButtons(List<Person> persons, ListTasks listTasksAllPerson, int hightRowForTasks,  Form1 form1)
        {
            foreach (Person person in persons)
            {
                PersonButton personButton = new PersonButton(person, listTasksAllPerson, hightRowForTasks, form1);
                ListPersonButtonsAdd(personButton);
            }
        }
    }
    [Serializable]
    public class PersonButton
    {
        public PersonButton() {

        }
        public PersonButton(Person person, ListTasks listTasksAllPerson, int hightRowForTasks, Form1 form1)
        {
            Person = person;
            Person.setTasks(listTasksAllPerson);
            Button.Text = person.PersonFamaly;
            Button.Height = GetHightBooton(listTasksAllPerson, hightRowForTasks);
            Button.BringToFront();
            Button.Click += PersonButton_Click;
            //Form1 form = new Form1();
            Button.Width = form1.GetPersonButtonWith();
            Form1 = form1;
        }
        private Form1 Form1;
        public void SetLocation(int locationХ, int locationY)
        {
            Button.Location = new Point(locationХ, locationY);
        }
        private void PersonButton_Click(object sender, EventArgs e)
        {
            Program.Person = Person;
            fmTasks fmTasks = new fmTasks(Form1);
            fmTasks.SetFmTasksStatusLoad(TaskLoadFor.LoadForPerson);
            fmTasks.Text = "Испольнитель:" + Person.PersonFamaly + "- задачи";
            fmTasks.ShowDialog();
        }
        public Button Button
        { get; private set; } = new Button();
        public Panel Panel
        { get; private set; } = new Panel();
        public Person Person
        { get; private set; }
        public List<Pen> ListMinHorizontLine
        { get; private set; } = new List<Pen>();
        public Pen BigHorizontLine
        { get; private set; } = new Pen(Color.Black, Constants.MIN_ROW_HIGHT);
        public void LoadLinesPerson(ref Panel panel, ListTasks listTasksAllPerson, PaintEventArgs e)
        {
            int height = Button.Height;
            int locationY = Button.Location.Y;
            int length = (listTasksAllPerson.GetMaxDateFinishTasks() - listTasksAllPerson.GetMinDateStartTasks()).Days * Constants.ROW_HIGHT;
            Graphics graphics = panel.CreateGraphics();
            while (height > 0)
            {
                Pen pen = new Pen(Color.LightGray, Constants.LINE_HIGHT);
                graphics.DrawLine(pen, 0, locationY, length, locationY);
                height -= Constants.COLUMN_WITH;
                locationY += Constants.COLUMN_WITH;
            }
            Pen bigLine = new Pen(Color.LightGray, Constants.MIN_ROW_HIGHT);
            graphics.DrawLine(bigLine, 0, locationY, length, locationY);
            graphics.Dispose();
        }
        private int GetHightBooton(ListTasks listTasksAllPerson, int hightRowForTasks)
        {
            int maxCountSynchTask = Person.GetMaxCountSynchTask(listTasksAllPerson);
            return maxCountSynchTask * hightRowForTasks;
        }

    }
    [Serializable]
    public class Project
    {
        public string Name
        { get; private set; }
        public void SetName(string name)
        {
            Name = name;
        }
        public string Address
        { get; private set; }
        public void SetAddress(string address)
        {
            Address = address;
        }
        public Project(string name, string address)
        {
            Name = name;
            Address = address;
        }
        public Project()
        { }
        public string GetStringForSave()
        {
            string stringForSave = string.Empty;
            stringForSave += Name + "\t" + Address;
            return stringForSave;
        }
        public Project(string stringFromFile)
        {
            const int NAME = 0;
            const int ADDRESS = 1;
            string[] arrPfarmetersForProject = stringFromFile.Split('\t');
            Name = arrPfarmetersForProject[NAME];
            Address = arrPfarmetersForProject[ADDRESS];
        }
    }
    [Serializable]
    public class PoolProjects
    {
        public List<Project> Projects
        { get; private set; } = new List<Project>();
        public List<string> GetListForSave()
        {
            List<string> listForSave = new List<string>();
            foreach (Project project in Projects)
            {
                listForSave.Add(project.GetStringForSave());
            }
            return listForSave;
        }
        public void Add(Project project)
        {
            Projects.Add(project);
        }
        public PoolProjects (List<string> listStringFromFile)
        {
            foreach (string stringFromFile in listStringFromFile)
            {
                Projects.Add(new Project(stringFromFile));
            }
        }
        public PoolProjects()
        {
        }
    }
    [Serializable]
    public class TreeProjects {
        public List<TreeNode> ListTreeNode {
            get; private set; } = new List<TreeNode>();
        public TreeProjects() {

        }
        private void CloneTreeView(TreeNode ChangeTreeNode, TreeNode CopyTreeNode) {
            ChangeTreeNode.Nodes.Add(CopyTreeNode.Clone() as TreeNode);
            foreach (TreeNode chTreeNode in CopyTreeNode.Nodes) {
                CloneTreeView(ChangeTreeNode.LastNode, chTreeNode);
            }
        }
        public void SetTreeViewProjects(TreeView treeView) {
            ListTreeNode.Clear();
            foreach (TreeNode treeNode in treeView.Nodes) {
                ListTreeNode.Add(treeNode.Clone() as TreeNode);
            }
        }
        public void SetTreeViewProjects(List<TreeNode> treeNodes) {
            ListTreeNode.Clear();
            foreach (TreeNode treeNode in treeNodes) {
                ListTreeNode.Add(treeNode.Clone() as TreeNode);
            }
        }
        public void SaveTree() {
            Dals.WriteObjectToMainPathFile(Constants.PROJECTS_LIST, this);
        }
        public void SaveSettingsTree() {
            Dals.WriteObjectToUserPathFile(Constants.PROJECTS_LIST, this);
        }
        public void GetTreeFromTask(Task task) {
            TreeProjects mainTree = GetMainTree();
            TreeProjects settingsTree = GetSettingsTreeFromTask(task);
            TreeProjects resultTree = GetResultTree(mainTree, settingsTree);
            this.ListTreeNode = resultTree.ListTreeNode;
        }
        private static TreeProjects GetSettingsTreeFromTask(Task task) {
            TreeProjects settingsTree = new TreeProjects();
            settingsTree.SetTreeViewProjects(task.GetTreeProjects().ListTreeNode);
            return settingsTree;
        }
        public void GetTreeFromFile() {
            TreeProjects mainTree = GetMainTree();
            TreeProjects settingsTree = GetSettingsTreeFromFile();
            TreeProjects resultTree = GetResultTree(mainTree, settingsTree);
            this.ListTreeNode = resultTree.ListTreeNode;
        }
       
        private static TreeProjects GetResultTree(TreeProjects mainTree, TreeProjects settingsTree) {
            TreeProjects resultTree = new TreeProjects();
            if (mainTree.ListTreeNode != null && settingsTree.ListTreeNode != null) {
                foreach (TreeNode mainNode in mainTree.ListTreeNode) {
                    bool isMainNodeInSettings = false;
                    foreach (TreeNode settingsNode in settingsTree.ListTreeNode) {
                        if (mainNode.Text == settingsNode.Text) {
                            resultTree.ListTreeNode.Add(settingsNode.Clone() as TreeNode);
                            resultTree.ListTreeNode.Last().Nodes.Clear();
                            isMainNodeInSettings = true;
                            CheckAndCloneTeeNodes(resultTree.ListTreeNode.Last(), mainNode, settingsNode);
                            break;
                        }
                    }
                    if (!isMainNodeInSettings) {
                        resultTree.ListTreeNode.Add(mainNode.Clone() as TreeNode);
                    }
                }
            }
            return resultTree;
        }

        private static TreeProjects GetSettingsTreeFromFile() {
            TreeProjects settingsTree = new TreeProjects();
            settingsTree.SetTreeViewProjects(Dals.binReadUserPathFileToObject(settingsTree, Constants.PROJECTS_LIST).ListTreeNode);
            return settingsTree;
        }

        private static TreeProjects GetMainTree() {
            TreeProjects mainTree = new TreeProjects();
            mainTree.SetTreeViewProjects(Dals.binReadMainPathFileToObject(mainTree, Constants.PROJECTS_LIST).ListTreeNode);
            if (mainTree.ListTreeNode != null) {
                foreach (TreeNode treeNode in mainTree.ListTreeNode) {
                    ClearSettings(treeNode);
                }
            }
            return mainTree;
        }

        private static void CheckAndCloneTeeNodes(TreeNode resultTree, TreeNode mainNode, TreeNode settingsNode) {
            foreach (TreeNode chMainNode in mainNode.Nodes) {
                bool isChMainNodeInChSetting = false;
                foreach (TreeNode chSettingsNode in settingsNode.Nodes) {
                    if (chMainNode.Text == chSettingsNode.Text) {
                        resultTree.Nodes.Add(chSettingsNode.Clone() as TreeNode);
                        resultTree.LastNode.Nodes.Clear();
                        isChMainNodeInChSetting = true;
                        CheckAndCloneTeeNodes(resultTree.LastNode, chMainNode, chSettingsNode);
                        break;
                    }
                    
                }
                if (!isChMainNodeInChSetting) {
                    resultTree.Nodes.Add(chMainNode.Clone() as TreeNode);
                }
            }
        }

        private static void ClearSettings(TreeNode treeNode) {
            treeNode.Checked = false;
            treeNode.Tag = null;
            treeNode.ExpandAll();
            foreach (TreeNode chTreeNode in treeNode.Nodes) {
                ClearSettings(chTreeNode);
            }

        }
    }
}
