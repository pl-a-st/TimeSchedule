using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    enum UserType
    {
        Admin,
        Reader
    }
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        public static Form1 fmMain ;
        public static LoadRefreshForm delegatLoadRefreshForm;
        public static ListTasks ListTasksAllPerson = new ListTasks();
        public static ListPersons listPersons = new ListPersons();
        public static Task Task = new Task();
        public static ListNonWorkingDays listNonWorkingDays= new ListNonWorkingDays();
        public static Color TaskColor = Color.FromArgb(132, 151, 176);
        public static ListPersonButton ListPersonButton = new ListPersonButton();
        public static ListTaskButton ListTaskButtons = new ListTaskButton();
        public static Person Person = new Person(string.Empty);
        public static UserType UserType = UserType.Reader;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
