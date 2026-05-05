using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    enum UserType
    {
        Admin,
        Reader
    }
    public enum Result
    {
        Unknown = -1,
        Sucess,
        Failure,
        Error,
        Canceled,
    }
    public class EarlyExitException : Exception
    {

    }
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        public static Form1 fmMain;
        public static LoadRefreshForm delegatLoadRefreshForm;
        public static ForControl delegatForControl;
        public static ListTasks ListTasksAllPersonToSave = new ListTasks();
        public static ListTasks ListTasksAllPersonToShow = new ListTasks();
        public static ListPersons listPersons = new ListPersons();
        public static Task Task = new Task();
        public static ListNonWorkingDays listNonWorkingDays = new ListNonWorkingDays();
        public static ListHolidays ListHolidays = new ListHolidays();
        public static Color TaskColor = Color.FromArgb(132, 151, 176);
        public static ListPersonButton ListPersonButton = new ListPersonButton();
        public static ListTaskButton ListTaskButtons = new ListTaskButton();
        public static Person Person = new Person(string.Empty);
        //public static List<TextBox> ListTextBoxes = new  List<TextBox>();
        public static PoolTextBox PoolTextBox = new PoolTextBox();
        public static TreeView ProjetTree;
        public static UserType UserType = UserType.Reader;

        public static Result ReadProgramData()
        {
            Program.ListTasksAllPersonToSave.Tasks.Clear();
            string fullFileName = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            ListTasks tasks = new ListTasks();
            Result result = Result.Unknown;
            if (File.Exists(fullFileName))
            {
                (tasks, result) = Dals.GetObjectFromFile(Program.ListTasksAllPersonToSave, fullFileName);
                if (result == Result.Sucess)
                {
                    Program.ListTasksAllPersonToSave = tasks;
                }
                return result;
            }
            return Result.Unknown;
        }
        public static Result SaveProgramData(Action actionWithListTasksAllPersonToSave)
        {
            for (int i = 0; i < Constants.COUNT_OF_TRIES; i++)
            {
                ListTasks tasks = new ListTasks();
                BinaryFormatter bf = new BinaryFormatter();
                string path = Dals.TakeMainPathFile(Constants.TASKS_BIN);
                try
                {

                    using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        if (stream.Length>0)
                        {
                            tasks = (ListTasks)bf.Deserialize(stream);
                        }
                        Program.ListTasksAllPersonToSave = tasks;
                        actionWithListTasksAllPersonToSave();
                    }
                    using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        bf.Serialize(stream, Program.ListTasksAllPersonToSave);
                    }
                    return Result.Sucess;

                }
                catch (Exception ex)
                {
                    if (ex is EarlyExitException)
                    {
                        return Result.Canceled;
                    }
                    System.Threading.Thread.Sleep(300);
                    continue;
                }

                //if (ReadProgramData() == Result.Sucess)
                //{
                //    try
                //    {
                //        action();
                //    }
                //    catch (Exception ex)
                //    {
                //        if(ex is EarlyExitException)
                //        {
                //            return Result.Canceled;
                //        }
                //        return Result.Error;
                //    }

                //}
                //else
                //{
                //    System.Threading.Thread.Sleep(100);
                //    continue;
                //}
                //string fullFileName = Dals.TakeMainPathFile(Constants.TASKS_BIN);
                //if (Dals.binWriteObjectToFile(ListTasksAllPersonToSave, fullFileName) == MethodResultStatus.Ok)
                //{
                //    return Result.Sucess;
                //}
                //System.Threading.Thread.Sleep(100);
            }
            return Result.Failure;
        }
        public static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
