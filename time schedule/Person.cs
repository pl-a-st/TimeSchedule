﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    public enum TaskStatus
    {
        New,
        Active,
        Closed
    }
    public enum TaskStatusRus
    {
        Новое=TaskStatus.New,
        В_работе= TaskStatus.Active,
        Закрыто=TaskStatus.Closed
    }
    
   
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
                person.setTasks(Program.ListTasksAllPerson);
                AddPerson(person);
            }
        }
        public void SetPersonList(List<Person> ListPerson)
        {
            Persons = ListPerson;
        }
    }
    public class ListTasks
    {
        public List<Task> Tasks
        { get; private set; } = new List<Task>();
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
            int i = 0;
            while (вeginningPeriod.AddDays(i) < endPeriod)
            {
                if (вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Sunday ||
                    вeginningPeriod.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    Program.listNonWorkingDays.NonWorkingDays.Add(вeginningPeriod.AddDays(i));
                i++;
            }
        }

    }
   
    /// <summary>
    /// Задачи
    /// </summary>
    public class Task
    {
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
            if (allParam[TASK_NAME] != "")
                Name = allParam[TASK_NAME];
            if (allParam[PERSON_FAMALY] != "")
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
            SetCountDays(Program.listNonWorkingDays);
            SetDateFinish();
        }
        static public DateTime GetDateFinish(DateTime dateStart, int workDayCount)
        {
            Task task = new Task(dateStart, workDayCount);
            return task.DateFinish;
        }
        public void ChangeDatesCountDays(DateTime dateStart, long countWorksDays)
        {
            dateStart = dateStart.Date;
            SetDateStart(dateStart);
            SetCountWorkingDays(countWorksDays);
            SetCountDays(Program.listNonWorkingDays);
            SetDateFinish();
        }
        public Task(DateTime dateStart, DateTime dateFinish)
        {
            dateStart = dateStart.Date;
            dateFinish = dateFinish.Date;
            SetDateStart(dateStart);
            SetDateFinish(dateFinish);
            SetCountDays();
            SetCountWorkingDays(Program.listNonWorkingDays);
        }
        public Task(TaskStatus taskStatus, string taskName, string personFamaly, long taskNumber, long taskNumberAfter,
            DateTime dateStart, DateTime dateFinish, long countDays, long countWorkingDays, Color taskColor, long priority)
        {
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

        public TaskStatus Status
        { get; private set; }
        
        public void SetTaskStatus(TaskStatus taskStatus)
        {
            Status = taskStatus; 
        }
        public string Name
        { get; private set; }
        public void SetTaskName(string taskName)
        {
            Name = taskName;
        }
        public string PersonFamaly
        { get; private set; }
        public void SetPersonName(Person person)
        {
            PersonFamaly = person.PersonFamaly;
        }
        public void SetPersonFamaly(string personFamaly)
        {
            PersonFamaly = personFamaly;
        }
        public long Number
        { get; private set; }
        public void SetTaskNumber(ListTasks listTask)
        {
            Number = listTask.Tasks.Count;
        }
        public long TaskNumberAfter
        { get; private set; }
        public void SetTaskNumberAfter(Task task)
        {
            TaskNumberAfter = task.Number;
        }
        public void SetTaskNumberAfter(int number)
        {
            TaskNumberAfter = number;
        }
        public DateTime DateStart
        { get; private set; }
        public void SetDateStart(DateTime dateStart)
        {
            DateStart = dateStart;
        }
        public DateTime DateFinish
        { get; private set; }
        private void SetDateFinish(DateTime dateFinish)
        {
            DateFinish = dateFinish;
        }
        public void SetDateFinish()
        {
            DateTime dateTime = DateStart;
            DateFinish = dateTime.AddDays(CountDays-1);
        }
        public long CountDays
        { get; private set; }
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
                if (i == CountDays - 1 && (listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i))))
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
        public long CountWorkingDays
        { get; private set; }
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
        public Color Color
        { get; private set; }
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
        public long Priority
        { get; private set;}
        public void SetPriority(int newPriority)
        {
            Priority = newPriority;
        }
        public int PlaceInSynchTask
        { get; private set; } = 0;
        public void SetPlaceInSynhTask(int placeInSynchTask)
        {
            PlaceInSynchTask = placeInSynchTask;
        }
    }
    public class ListTaskButton
    {
        public List<TaskButton> TaskButtons
        { get; private set; } = new List<TaskButton>();
        public void LoadTaskButtons(TaskButton taskButton)
        {
            TaskButtons.Add(taskButton);
        }
        public void LoadTaskButtons(ListTasks listTasks, ListPersonButton listPersonButton)
        {
            foreach (Task task in listTasks.Tasks)
            {
                TaskButton taskButton = new TaskButton(
                    task,
                    listPersonButton,
                    listTasks.GetMinDateStartTasks(),
                    listTasks.GetMaxDateFinishTasks());

                LoadTaskButtons(taskButton);
            }
        }
    }

    public class TaskButton
    {
        public Task Task
        { get; private set; }
        public void SetTask(Task task)
        {
            Task = task;
        }
        public List<Button> Buttons
        { get; private set; } = new List<Button>();
        public void AddButton(int locationX, int locationY, int with, int height)
        {
            Button button = new Button();
            button.Location = new Point(locationX+1, locationY+1);
            button.Width = with-2;
            button.Height = height-2;
            button.Text = Task.Name;
            button.BackColor = Task.Color;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.DimGray;
            button.Click += Button_Click;
            button.MouseHover += Button_MouseHover;
            void Button_MouseHover(object sender, EventArgs e)
            {
                ToolTip t = new ToolTip();
                t.SetToolTip(button, Task.Name + "\n" +
                    "до "+Task.DateFinish.ToString().Split(' ')[0] + "\n"+
                    "Статус: " + 
                    ((TaskStatusRus)Enum.Parse(typeof(TaskStatus),
                    Task.Status.ToString(),true)).ToString().Replace("_"," "));
            }


            if (Task.Status == TaskStatus.Closed)
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Strikeout);
            if (Task.Status == TaskStatus.Active)
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Underline);
            if (Task.Status == TaskStatus.New && Task.DateStart.Date<DateTime.Now.Date)
            {
                button.Font = new Font(button.Font.FontFamily, button.Font.Size, FontStyle.Bold);
                button.ForeColor = System.Drawing.Color.DarkRed;
            }
               
            Buttons.Add(button);
        }
        //LoadRefreshForm loadRefreshForm;
        private void Button_Click(object sender, EventArgs e)
        {

            fmAddTask fmAddTask = new fmAddTask(Program.delegatLoadRefreshForm);
            fmAddTask.GhangeNamebtnCreateTask("Изменить");
            fmAddTask.SetCreateOrChange(CreateOrChange.Change);
            fmAddTask.StartPosition = FormStartPosition.CenterScreen;
            Program.Task = Task;
            fmAddTask.ShowDialog();
            Dals.WriteListProjectFileAppend(Constants.TASKS, Program.ListTasksAllPerson.GetListForSave());
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
            do
            {
                if(
                    dateTime.DayOfWeek != DayOfWeek.Sunday &&
                    dateTime.DayOfWeek != DayOfWeek.Saturday &&
                    !Program.listNonWorkingDays.NonWorkingDays.Contains(dateTime.Date))
                {
                    width += Constants.COLUMN_WITH;
                    if (dateTime.DayOfWeek == DayOfWeek.Friday ||
                        dateTime.Date == Task.DateFinish.Date)
                    {

                        AddButton(locationX, locationY, width, Constants.ROW_HIGHT);
                        locationX += width;
                        width = 0;
                    }
                }
                dateTime=dateTime.AddDays(1);
            }
            while ((dateTime.Date <= Task.DateFinish.Date) && (dateTime.Date <= maxDateFinish));
        }
    }
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
    public class PersonButton
    {

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
            fmTasks.Load -= fmTasks.fmTasks_Load;
            fmTasks.Load += FmTasks_Load;
            void FmTasks_Load(object sender1, EventArgs e1)
            {
                fmTasks.SetFmTasksStatusLoad(FmTasksStatusLoad.loadForPerson);
                fmTasks.SetFilterDateStart(DateTime.Now.Date);
                fmTasks.SetFilterDateFinish(Program.ListTasksAllPerson.GetMaxDateFinishTasks().Date);
                fmTasks.Text = "Испольнитель:" + Person.PersonFamaly + "- задачи";
                //foreach (Task task in Program.ListTasksAllPerson.Tasks)
                //{
                //    if (task.PersonFamaly == Person.PersonFamaly)
                //        fmTasks.RetunlBxTasks().Items.Add(task.Number.ToString() + "\t" + task.Name);
                //}
                //if (Program.UserType != UserType.Admin)
                //{
                //    fmTasks.GetBtnDeleteTask().Enabled = false;
                //    fmTasks.GetBtnNewTask().Enabled = false;
                //}
            }
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
    public class ListProjects
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
        public ListProjects (List<string> listStringFromFile)
        {
            foreach (string stringFromFile in listStringFromFile)
            {
                Projects.Add(new Project(stringFromFile));
            }
        }
        public ListProjects()
        {
        }
    }
}
