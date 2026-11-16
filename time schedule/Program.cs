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

        /// <summary>Описание последнего отказа сохранения для интерфейса; не скрывает конфликты.</summary>
        public static string LastSaveError { get; private set; }

        /// <summary>Загружает целый согласованный снимок задач, не очищая память до успеха.</summary>
        public static Result ReadProgramData()
        {
            string file = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            if (!TaskStore.HasTaskData(file)) return Result.Unknown;
            try
            {
                ListTasksAllPersonToSave = TaskStore.Read(file);
                return Result.Sucess;
            }
            catch (Exception ex) when (ex is IOException || ex is System.Runtime.Serialization.SerializationException || ex is UnauthorizedAccessException)
            {
                LastSaveError = ex.Message;
                return Result.Error;
            }
        }

        /// <summary>Сохраняет только добавление новой задачи; существующие записи менять без снимка запрещено.</summary>
        public static Result SaveProgramData(Action action)
        {
            return SaveProgramData(action, new Dictionary<long, string>());
        }

        /// <summary>Выполняет callback ровно один раз над свежим снимком под общей файловой блокировкой.</summary>
        /// <param name="action">Мутация текущего снимка; без модальных окон и блокирующей работы UI.</param>
        /// <param name="expected">Отпечатки всех допускаемых к изменению существующих задач.</param>
        public static Result SaveProgramData(Action action, IDictionary<long, string> expected)
        {
            LastSaveError = null;
            ListTasks previous = ListTasksAllPersonToSave;
            try
            {
                ListTasks result = TaskStore.Update(Dals.TakeMainPathFile(Constants.TASKS_BIN), current =>
                {
                    ListTasksAllPersonToSave = current;
                    action();
                }, expected);
                ListTasksAllPersonToSave = result;
                return Result.Sucess;
            }
            catch (EarlyExitException)
            {
                ListTasksAllPersonToSave = previous;
                return Result.Canceled;
            }
            catch (TaskConflictException ex)
            {
                ListTasksAllPersonToSave = previous;
                LastSaveError = ex.Message;
                return Result.Failure;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException ||
                ex is System.Runtime.Serialization.SerializationException || ex is InvalidOperationException)
            {
                ListTasksAllPersonToSave = previous;
                LastSaveError = "Сохранение не подтверждено: " + ex.Message +
                    ". Не повторяйте операцию до проверки данных в общей папке.";
                return Result.Error;
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) &&
                !(ex is System.Threading.ThreadAbortException) && !(ex is AccessViolationException))
            {
                ListTasksAllPersonToSave = previous;
                LastSaveError = "Сохранение не выполнено: " + ex.Message;
                return Result.Error;
            }
        }

        /// <summary>Выводит конкретную причину, а не необоснованное сообщение об успешной записи.</summary>
        public static string SaveFailureMessage => LastSaveError ?? "Изменения не сохранены.";

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
        /// <summary>
        /// Запускает приложение со штатным компактным хранилищем задач.
        /// Диагностика времени включена в Debug и доступна по запросу в Release.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            bool timingEnabled = true;
#else
            bool timingEnabled = args.Any(arg =>
                string.Equals(arg, "--timing", StringComparison.OrdinalIgnoreCase));
#endif
            timingEnabled |= Environment.GetEnvironmentVariable("TS01_TIMING") == "1";
            if (timingEnabled)
            {
                string logPath = Path.Combine(Path.GetTempPath(),
                    "TimeSchedule-TS01-" + System.Diagnostics.Process.GetCurrentProcess().Id + ".log");
                System.Diagnostics.Trace.Listeners.Add(
                    new System.Diagnostics.TextWriterTraceListener(logPath));
                System.Diagnostics.Trace.AutoFlush = true;
                TaskStore.TimingSink = message => System.Diagnostics.Trace.WriteLine("TimeSchedule store: " + message);
                System.Diagnostics.Trace.WriteLine("TimeSchedule storage-mode=compact");
            }
            Form1 mainForm = new Form1();
            Application.Run(mainForm);
        }
    }
}
