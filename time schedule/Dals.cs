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
        /// <summary>Записывает прочие файлы; обход согласованного Tasks.bin запрещён.</summary>
        public static MethodResultStatus WriteObjectToMainPathFile<Type>(string fileName, Type serObject)
        {
            if (fileName == Constants.TASKS_BIN || fileName == Constants.PERSONS_BIN)
                throw new InvalidOperationException("Для Tasks.bin и Persons.bin обязателен TaskStore.");
            fileName = TakeMainPathFile(fileName);
            return binWriteObjectToFile(serObject, fileName);
        }
        /// <summary>Создаёт резервную копию вспомогательного файла; для задач копию делает TaskStore.</summary>
        public static void WriteObjectToBackUpPathFile<Type>(string DirectoryName, Type serObject)
        {
            if (DirectoryName == Constants.TASKS_BIN)
                throw new InvalidOperationException("Резервная копия задач создаётся только из опубликованного файла TaskStore.");
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
        /// <summary>Записывает вспомогательные бинарные файлы, но не Tasks.bin.</summary>
        public static MethodResultStatus binWriteObjectToFile<Type>(Type serObject, string fileName)
        {
            if (string.Equals(Path.GetFileName(fileName), Constants.TASKS_BIN, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Path.GetFileName(fileName), Constants.PERSONS_BIN, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Прямая запись Tasks.bin/Persons.bin запрещена; используйте TaskStore.");
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
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
        /// <summary>Читает прочие файлы и согласованный Tasks.bin без возврата устаревших данных при ошибке.</summary>
        public static Type binReadFileToObject<Type>(Type serObject, string fullPathFileName)
        {
            if (string.Equals(Path.GetFileName(fullPathFileName), Constants.TASKS_BIN, StringComparison.OrdinalIgnoreCase))
                return (Type)(object)TaskStore.Read(fullPathFileName);
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
                    System.Threading.Thread.Sleep(100);
                }
            }
            MessageAboutProblem(fullPathFileName);
            return serObject;
        }
        /// <summary>Читает Tasks.bin под общим файловым протоколом, сохраняя статус результата.</summary>
        public static (Type, Result) GetObjectFromFile<Type>(Type serObject, string fullPathFileName)
        {
            if (string.Equals(Path.GetFileName(fullPathFileName), Constants.TASKS_BIN, StringComparison.OrdinalIgnoreCase))
            {
                try { return ((Type)(object)TaskStore.Read(fullPathFileName), Result.Sucess); }
                catch (IOException) { return (serObject, Result.Error); }
                catch (System.Runtime.Serialization.SerializationException) { return (serObject, Result.Error); }
            }
            BinaryFormatter bf = new BinaryFormatter();
            for (int i = 0; i < Constants.COUNT_OF_TRIES; i++)
            {
                try
                {
                    using (FileStream stream = new FileStream(fullPathFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        serObject = (Type)bf.Deserialize(stream);
                    }
                    return (serObject, Result.Sucess);
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            return (serObject, Result.Error);
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
                MessageBox.Show("Не удалось зачитать задачи!", "");
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
        /// <summary>Обновляет список из целого снимка; прежний список сохраняется при ошибке чтения.</summary>
        public static void ReloadListTaskToSave()
        {
            string fullFileName = Dals.TakeMainPathFile(Constants.TASKS_BIN);
            if (TaskStore.HasTaskData(fullFileName))
                Program.ListTasksAllPersonToSave = TaskStore.Read(fullFileName);
            else
            {
                var legacy = new ListTasks();
                legacy.SetTasksFromList(Dals.ReadListFromMainPathFile(Constants.TASKS));
                Program.ListTasksAllPersonToSave = legacy;
            }
        }
    }
}
