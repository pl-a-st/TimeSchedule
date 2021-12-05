using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace time_schedule
{
    public enum TaskStatusEnum
    {
        New,
        Active,
        Closed
    }
    public enum TaskStatusRusEnum
    {
        Новое=TaskStatusEnum.New,
        В_работе= TaskStatusEnum.Active,
        Закрыто=TaskStatusEnum.Closed
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
            //foreach (Task task1 in ListTask.Tasks)
            //{
            //    Task SynchTask = task1;
            //    int CountDaysSynchTask2 = 1;
            //    foreach (Task task2 in ListTask.Tasks)
            //    {
            //        if (task1 != task2)
            //        {
            //            if (task2.DateStart.Date >= SynchTask.DateStart.Date && task2.DateStart.Date < SynchTask.DateFinish.Date ||
            //                task2.DateStart.Date <= SynchTask.DateStart.Date && task2.DateFinish.Date > SynchTask.DateStart.Date
            //                )
            //            {
            //                DateTime dateStart;
            //                DateTime dateFinish;
            //                if(task2.DateStart.Date >= SynchTask.DateStart.Date)
            //                {
            //                    dateStart = task2.DateStart.Date;
            //                }
            //                else
            //                {
            //                    dateStart = SynchTask.DateStart.Date;
            //                }
            //                if (task2.DateFinish.Date <= SynchTask.DateFinish.Date)
            //                {
            //                    dateFinish = task2.DateFinish.Date;
            //                }
            //                else
            //                {
            //                    dateFinish = SynchTask.DateFinish.Date;
            //                }
            //                CountDaysSynchTask2++;
            //                SynchTask = new Task(dateStart, dateFinish);
            //            }
            //        }
            //    }
            //    if (CountDaysSynchTask2 >= CountDaysSynchTask1)
            //        CountDaysSynchTask1 = CountDaysSynchTask2;
            //}
            //return CountDaysSynchTask1;
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
        public void SetTasksFromList(List<string> listString)
        {
            foreach (string stringAllParamTab in listString)
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
            DateTime dateStartTasks = DateTime.MaxValue;
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
        public void AssingPlace()
        {
            foreach (Task task in Tasks)
            {
                task.SetPlaceInSynhTask(0);
            }
            for (int i = 0;i<Tasks.Count; i++)
            {
                for (int j=i+1;j< Tasks.Count; j++)
                {
                    if (Tasks[i].DateStart.Date >= Tasks[j].DateStart.Date && Tasks[i].DateStart.Date < Tasks[j].DateFinish.Date ||
                    Tasks[i].DateStart.Date <= Tasks[j].DateStart.Date && Tasks[i].DateFinish.Date > Tasks[j].DateStart.Date
                    )
                    {
                        if (Tasks[i].Priority <= Tasks[j].Priority && Tasks[i].PlaceInSynchTask >= Tasks[j].PlaceInSynchTask)

                        {
                            Tasks[j].SetPlaceInSynhTask(Tasks[i].PlaceInSynchTask + 1);
                        }
                        else
                        {
                            Tasks[i].SetPlaceInSynhTask(Tasks[j].PlaceInSynchTask + 1);
                        }
                    }
                }
            }
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
            if (allParam[TASK_NAME] != null)
                Name = allParam[TASK_NAME];
            if (allParam[PERSON_FAMALY] != null)
                PersonFamaly = allParam[PERSON_FAMALY];
            if (allParam[TASK_STATUS] != null)
                Status = (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), allParam[TASK_STATUS], true);
            if (allParam[TASK_NUMBER] != null)
                Number = Convert.ToInt32(allParam[TASK_NUMBER]);
            if (allParam[TASK_NUMBER_AFTER] != null)
                TaskNumberAfter = Convert.ToInt32(allParam[TASK_NUMBER_AFTER]);
            if (allParam[DATE_START] != null)
                DateStart = Convert.ToDateTime(allParam[DATE_START]);
            if (allParam[DATE_FINISH] != null)
                DateFinish = Convert.ToDateTime(allParam[DATE_FINISH]);
            if (allParam[COUNT_DAYS] != null)
                CountDays = Convert.ToInt32(allParam[COUNT_DAYS]);
                if (allParam[COUNT_WORKING_DAYS] != null)
                    CountWorkingDays = Convert.ToInt32(allParam[COUNT_WORKING_DAYS]);
            if (allParam[TASK_COLOR] != null)
                Color = Color.FromArgb(Convert.ToInt32(allParam[TASK_COLOR]));
            if (allParam.Length>10&& allParam[PRIORITY] != null)
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
        public Task(TaskStatusEnum taskStatus, string taskName, string personFamaly, long taskNumber, long taskNumberAfter,
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

        public TaskStatusEnum Status
        { get; private set; }
        
        public void SetTaskStatus(TaskStatusEnum taskStatus)
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
        private void SetDateFinish()
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
        private void SetCountDays(ListNonWorkingDays listNonWorkingDays)
        {
            DateTime dateTime = DateStart;
            CountDays = CountWorkingDays;
            for(int i=0;i< CountDays; i++)
            {
                if (listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i)))
                {
                    CountDays++;
                }
                if (i == CountDays - 1 && listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i)))
                {
                    int k = 1;
                    while(listNonWorkingDays.NonWorkingDays.Contains(dateTime.AddDays(i+k)))
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

            stringForSave += Name;
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
}
