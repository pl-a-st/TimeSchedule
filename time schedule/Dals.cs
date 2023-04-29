using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Syroot.Windows.IO;


namespace time_schedule
{

    public class Dals
    {
        public static string ProjectFolderPath { get; private set; }
        public static void SetProjectFolderPath(string projectFolder)
        {
            ProjectFolderPath = projectFolder;
        }
        //public static void WriteProjectFolder() {
        //    WriteProjectFolder("Проект",);
        //}
        public static void WriteProjectFolder(Statuses.WorkWithProject statusWorkWithProject)
        {
            if (statusWorkWithProject == Statuses.WorkWithProject.ProgramStarted &&
                    (!File.Exists(Constants.PROJECT_FILE_NAME) ||
                    File.ReadAllLines(Constants.PROJECT_FILE_NAME).Length == 0))
            {
                WriteMainPathFolder("Проект", statusWorkWithProject);
            }
            else
            {
                if (File.Exists(Constants.PROJECT_FILE_NAME))
                {
                    StreamReader streamReader = new StreamReader(Constants.PROJECT_FILE_NAME);
                    SetProjectFolderPath(streamReader.ReadLine());
                    streamReader.Close();
                }
                else
                {
                    MessageBox.Show("Не удалось зачитать файл " + Constants.PROJECT_FILE_NAME + "Создайте или выбирите проект.");
                }

            }
        }
        public static void WriteMainPathFolder(string targetFolderName, Statuses.WorkWithProject workWithProject)
        {
            try
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (workWithProject == Statuses.WorkWithProject.NewProject)
                {
                    folderBrowserDialog.Description = "Выбирете папку для проекта";
                    ChooseFolderWritePath(targetFolderName, folderBrowserDialog);
                    return;
                }

                if (workWithProject == Statuses.WorkWithProject.LoadProject)
                {

                    fmMainPathChoise fmProjectChoise = new fmMainPathChoise();
                    fmProjectChoise.ShowDialog();
                    if (fmProjectChoise.SetTBxAddress().Text == "" ||
                        fmProjectChoise.SetTBxAddress().Text == null ||
                        fmProjectChoise.ChoiceIsMade == ChoiceIsMade.no)
                    {
                        return;
                    }
                    string folderPath = fmProjectChoise.SetTBxAddress().Text;
                    WritePaht(folderPath);
                    return;
                }
                WritePaht("Проект");
            }
            catch
            {
                MessageBox.Show("Не удалось произвести запись в файл: " + Constants.PROJECT_FILE_NAME);
            }

        }
        public static void WriteStringToFile(string fileName, string stringForWrite)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(fileName, true);
                streamWriter.WriteLine(stringForWrite);
                streamWriter.Close();
            }
            catch
            {

            }
        }
        public static void WriteListtFileAppend(string fileName, List<string> listForWrite)
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(fileName, false);
                foreach (string stringForWrite in listForWrite)
                {
                    streamWriter.WriteLine(stringForWrite);
                }
                streamWriter.Close();
            }
            catch
            {
                MessageBox.Show("Не удалось произвести запись в файл: " + fileName);
            }
        }
        public static void WriteObjectToFile(string fileName, List<string> listForWrite)
        {
            fileName = TakeMainPathFile(fileName);
            WriteListtFileAppend(fileName, listForWrite);
        }
        public static MethodResultStatus WriteObjectToMainPathFile<Type>(string fileName, Type serObject)
        {
            fileName = TakeMainPathFile(fileName);
            return binWriteObjectToFile(serObject, fileName);
        }
        public static void WriteObjectToBackUpPathFile<Type>(string DirectoryName, Type serObject)
        {
            string fileNameToSave = TakeBackUpPathFile(DirectoryName);
            binWriteObjectToFile(serObject, fileNameToSave);
            while (new DirectoryInfo(TakeBackUpPathDirectory(DirectoryName)).GetFileSystemInfos().Length > Constants.MAX_BACKUP_FILESES)
            {
                FileSystemInfo fileToDelete = new DirectoryInfo(TakeBackUpPathDirectory(DirectoryName)).GetFileSystemInfos().OrderBy(fi => fi.CreationTime).First();
                fileToDelete?.Delete();
            }
        }
        public static void WriteObjectToUserPathFile<Type>(string fileName, Type serObject)
        {
            fileName = TakeUserPath(fileName);
            binWriteObjectToFile(serObject, fileName);
        }
        public static string TakeMainPathFile(string fileName)
        {
            if (ProjectFolderPath == null || ProjectFolderPath == string.Empty)
                ProjectFolderPath = "Проект";
            else
            {
                fileName = ProjectFolderPath + "\\" + fileName;
            }
            if (!Directory.Exists(ProjectFolderPath))
            {
                if (ProjectFolderPath != null && ProjectFolderPath != string.Empty)
                    Directory.CreateDirectory(ProjectFolderPath);
            }
            return fileName;
        }
        public static string TakeBackUpPathFile(string fileNameToBuckUp)
        {
            if (ProjectFolderPath == null || ProjectFolderPath == string.Empty)
                ProjectFolderPath = "Проект";
            if (!Directory.Exists(ProjectFolderPath + "\\BackUp\\" + fileNameToBuckUp))
            {
                Directory.CreateDirectory(ProjectFolderPath + "\\BackUp\\" + fileNameToBuckUp);
            }
            fileNameToBuckUp = ProjectFolderPath + "\\BackUp\\" + fileNameToBuckUp + "\\" + DateTime.Now.ToString("G").Replace(':', '_') + ' ' + Environment.UserName;
            return fileNameToBuckUp;
        }
        public static string TakeBackUpPathDirectory(string fileName)
        {
            string directory;
            if (ProjectFolderPath == null || ProjectFolderPath == string.Empty)
                ProjectFolderPath = "Проект";
            if (!Directory.Exists(ProjectFolderPath + "\\BackUp\\" + fileName))
            {
                Directory.CreateDirectory(ProjectFolderPath + "\\BackUp\\" + fileName);
            }
            directory = ProjectFolderPath + "\\BackUp\\" + fileName;
            return directory;
        }
        public static string TakeUserPath(string fileName)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Time schedule\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path + fileName;
        }
        public static MethodResultStatus binWriteObjectToFile<Type>(Type serObject, string fileName)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    bf.Serialize(stream, serObject);
                }
                return MethodResultStatus.Ok;
            }
            catch
            {
            }
            //MessageBox.Show("Не удалось произвести запись в файл: " + fileName);
            return MethodResultStatus.Fault;
        }
        public static List<string> ReadListFromFile(string fileName)
        {
            List<string> listFromFile = new List<string>();
            if (File.Exists(fileName))
            {
                StreamReader streamReader = new StreamReader(fileName);
                while (!streamReader.EndOfStream)
                {
                    string stringToWrite = streamReader.ReadLine();
                    if (stringToWrite != "")
                        listFromFile.Add(stringToWrite);
                }
                streamReader.Close();
            }
            else
            {
                MessageAboutProblem(fileName);
            }
            return listFromFile;
        }
        public static Type binReadMainPathFileToObject<Type>(Type serObject, string fileName)
        {
            return binReadFileToObject(serObject, Dals.TakeMainPathFile(fileName));
        }
        public static Type binReadUserPathFileToObject<Type>(Type serObject, string fileName)
        {
            return binReadFileToObject(serObject, Dals.TakeUserPath(fileName));
        }
        public static Type binReadFileToObject<Type>(Type serObject, string fullPathFileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            //serObject = default(Type);
            for (int i = 0; i < Constants.COUNT_OF_TRIES; i++)
            {
                try
                {
                    using (FileStream stream = new FileStream(fullPathFileName, FileMode.Open))
                    {
                        serObject = (Type)bf.Deserialize(stream);
                    }
                    return serObject;
                }
                catch
                {
                }
            }
            MessageAboutProblem(fullPathFileName);
            return serObject;
        }
        private static void MessageAboutProblem(string fileName)
        {
            if (fileName.Contains(Constants.PERSONS) ||
                fileName.Contains(Constants.PERSONS_BIN))
            {
                MessageBox.Show("В текущем проекте не создано ни одного исполнителя!", "");
                return;
            }
            if (fileName.Contains(Constants.TASKS) ||
                fileName.Contains(Constants.TASKS_BIN))
            {
                MessageBox.Show("В текущем проекте не создано ни одной задачи!", "");
                return;
            }
            if (fileName.Contains(Constants.HOLYDAYS) ||
                fileName.Contains(Constants.HOLYDAYS_BIN))
            {


                MessageBox.Show("В текущем проекте не занесены праздничные дни!", "");
                return;
            }
            MessageBox.Show("Не удалось зачитать файл " + fileName, "");
        }

        public static List<string> ReadListFromFile(string fileName, string textForError)
        {
            List<string> listFromFile = new List<string>();
            if (File.Exists(fileName))
            {
                StreamReader streamReader = new StreamReader(fileName);
                while (!streamReader.EndOfStream)
                {
                    string stringToWrite = streamReader.ReadLine();
                    if (stringToWrite != "")
                        listFromFile.Add(stringToWrite);
                }
                streamReader.Close();
            }
            else
            {
                MessageBox.Show(textForError);
            }
            return listFromFile;
        }
        public static List<string> ReadListFromMainPathFile(string fileName)
        {
            if (ProjectFolderPath != null && ProjectFolderPath != string.Empty)
                fileName = ProjectFolderPath + "\\" + fileName;
            return ReadListFromFile(fileName);
        }
        public static void ExelWriteListTasks(string pathFileToSave, ListTasks listTasks)
        {
            string pathModelFileName = Dals.ProjectFolderPath + "\\шаблон.xlsm";
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = null;
            OpenWorkBook(ref excelApp, ref workbook, pathModelFileName);
            SaveWorkBook(ref workbook, pathFileToSave);
            const int FIRST_SHEET = 1;
            Excel.Worksheet worksheet = workbook.Sheets[FIRST_SHEET];
            WorksheetWriteListTasks(worksheet, listTasks);
            excelApp.Visible = true;
        }
        private static void ChooseFolderWritePath(string targetFolderName, FolderBrowserDialog folderBrowserDialog)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string folderName = string.Empty;
                folderName = folderBrowserDialog.SelectedPath + "\\" + targetFolderName;
                WritePaht(folderName);
            }
        }

        private static void WritePaht(string folderName)
        {
            StreamWriter streamWriter = new StreamWriter(Constants.PROJECT_FILE_NAME, false);
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            streamWriter.WriteLine(folderName);
            SetProjectFolderPath(folderName);
            streamWriter.Close();
        }

        private static void SaveWorkBook(ref Excel.Workbook workbook, string pathFileToSave)
        {
            try
            {
                workbook.SaveAs(pathFileToSave);
                MessageBox.Show("Файл будет сохранен по адресу: " + pathFileToSave);
            }
            catch
            {
                MessageBox.Show("Не удалось сохранить файл " + pathFileToSave +
                    ". Заполнение будет произведено в файл шаблона. Необходимо его сохранить самстоятельно",
                    "Проблема", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

                workbook.Save();
            }

        }
        private static void OpenWorkBook(
            ref Excel.Application excelApp,
            ref Excel.Workbook workbook,
            string pathModelFile)
        {
            try
            {
                if (File.Exists(pathModelFile))
                {
                    workbook = excelApp.Workbooks.Open(pathModelFile);
                }
                else
                {
                    workbook = excelApp.Workbooks.Add();
                }
            }
            catch
            {
                try
                {
                    pathModelFile = Environment.CurrentDirectory + "\\" + pathModelFile;
                    workbook = excelApp.Workbooks.Open(pathModelFile);
                }
                catch
                {
                    excelApp.Quit();
                    MessageBox.Show("Не удалось открыть файл " + pathModelFile +
                        " или проблемы с самим приложением");
                    return;
                }

            }
        }
        private static void WorksheetWriteListTasks(Excel.Worksheet worksheet, ListTasks listTasks)
        {
            const string COLUMN_NUMBER_TASK = "A";
            const string COLUMN_TASK_NAME = "B";
            const string COlUMN_DATE_START = "C";
            const string COLUMN_DATE_FINISH = "D";
            const string COLUMN_COUNT_WORKING = "E";
            const string COLUMN_PERSON_NAME = "F";
            const string COLUMN_TASK_AFTER = "G";
            const int FIRST_ROW_TO_WRITE = 2;
            int i = FIRST_ROW_TO_WRITE;
            foreach (Task task in listTasks.Tasks)
            {
                worksheet.Cells[i, COLUMN_TASK_NAME].Value = task.Name;
                worksheet.Cells[i, COLUMN_NUMBER_TASK].Value = task.Number;
                worksheet.Cells[i, COlUMN_DATE_START].Value = task.DateStart;
                worksheet.Cells[i, COLUMN_DATE_FINISH].Value = task.DateFinish;
                worksheet.Cells[i, COLUMN_COUNT_WORKING].Value = task.CountWorkingDays;
                worksheet.Cells[i, COLUMN_PERSON_NAME].Value = task.PersonFamaly;
                worksheet.Cells[i, COLUMN_TASK_AFTER].Value = task.TaskNumberAfter;
                i++;
            }
        }
        public static void ReloadListTaskToSave()
        {
            Program.ListTasksAllPersonToSave.Tasks.Clear();
            string fullFileName = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            if (File.Exists(fullFileName))
            {
                Program.ListTasksAllPersonToSave = Dals.binReadFileToObject(
                    Program.ListTasksAllPersonToSave, fullFileName);
            }
            else
            {
                Program.ListTasksAllPersonToSave.SetTasksFromList(Dals.ReadListFromMainPathFile(Constants.TASKS));
            }
        }
    }
}
