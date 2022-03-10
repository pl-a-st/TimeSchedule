using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule {
   
    public class Dals {
        public static string ProjectFolderPath { get; private set; }
        public static void SetProjectFolderPath(string projectFolder) {
            ProjectFolderPath = projectFolder;
        }
        //public static void WriteProjectFolder() {
        //    WriteProjectFolder("Проект",);
        //}
        public static void WriteProjectFolder(Statuses.WorkWithProject statusWorkWithProject) {
            if (statusWorkWithProject == Statuses.WorkWithProject.ProgramStarted &&
                (!File.Exists(Constants.PROJECT_FILE_NAME) ||
                File.ReadAllLines(Constants.PROJECT_FILE_NAME).Length == 0)) {
                WriteProjectFolder("Проект", statusWorkWithProject);
            }
            else {
                if (File.Exists(Constants.PROJECT_FILE_NAME)) {
                    StreamReader streamReader = new StreamReader(Constants.PROJECT_FILE_NAME);
                    SetProjectFolderPath(streamReader.ReadLine());
                    streamReader.Close();
                }
                else {
                    MessageBox.Show("Не удалось зачитать файл " + Constants.PROJECT_FILE_NAME + "Создайте или выбирите проект.");
                }

            }
        }
        public static void WriteProjectFolder(string targetFolderName, Statuses.WorkWithProject workWithProject) {
            try {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (workWithProject == Statuses.WorkWithProject.NewProject) {
                    folderBrowserDialog.Description = "Выбирете папку для проекта";
                    ChooseFolderWritePath(targetFolderName, folderBrowserDialog);
                }
                    
                if (workWithProject == Statuses.WorkWithProject.LoadProject) {
                    
                    fmProjectChoise fmProjectChoise = new fmProjectChoise();
                    fmProjectChoise.ShowDialog();
                    if (fmProjectChoise.SetTBxAddress().Text == "" ||
                        fmProjectChoise.SetTBxAddress().Text == null ||
                        fmProjectChoise.ChoiceIsMade == ChoiceIsMade.no) {
                        return;
                    }
                    string folderPath = fmProjectChoise.SetTBxAddress().Text;
                    WritePaht(folderPath);
                }
                    
            }
            catch {
                MessageBox.Show("Не удалось произвести запись в файл: " + Constants.PROJECT_FILE_NAME);
            }

        }
        public static void WriteStringToFile(string fileName, string stringForWrite) {
            try {
                StreamWriter streamWriter = new StreamWriter(fileName, true);
                streamWriter.WriteLine(stringForWrite);
                streamWriter.Close();
            }
            catch {
                
            }
        }
        public static void WriteListtFileAppend(string fileName, List<string> listForWrite) {
            try {
                StreamWriter streamWriter = new StreamWriter(fileName, false);
                foreach (string stringForWrite in listForWrite) {
                    streamWriter.WriteLine(stringForWrite);
                }
                streamWriter.Close();
            }
            catch {
                MessageBox.Show("Не удалось произвести запись в файл: " + fileName);
            }
        }
        public static void WriteListProjectFileAppend(string fileName, List<string> listForWrite) {
            fileName = ProjectFolderPath + "\\" + fileName;
            if (!Directory.Exists(ProjectFolderPath))
                Directory.CreateDirectory(ProjectFolderPath);
            WriteListtFileAppend(fileName, listForWrite);
        }
        public static List<string> ReadListFromFile(string fileName) {
            List<string> listFromFile = new List<string>();
            if (File.Exists(fileName)) {
                StreamReader streamReader = new StreamReader(fileName);
                while (!streamReader.EndOfStream) {
                    string stringToWrite = streamReader.ReadLine();
                    if (stringToWrite!="")
                        listFromFile.Add(stringToWrite);
                }
                streamReader.Close();
            }
            else {
                MessageBox.Show("Не удалось зачитать файл " + fileName);
            }
            return listFromFile;
        }
        public static List<string> ReadListFromProjectFile(string fileName) {
            fileName = ProjectFolderPath + "\\" + fileName;
            return ReadListFromFile(fileName);
        }
        public static void ExelWriteListTasks(string pathFileToSave, ListTasks listTasks) {
            string pathModelFileName = Dals.ProjectFolderPath + "\\шаблон.xlsm";
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook=null;
            OpenWorkBook(ref excelApp, ref workbook, pathModelFileName);
            SaveWorkBook(ref workbook, pathFileToSave);
            const int FIRST_SHEET = 1;
            Excel.Worksheet worksheet = workbook.Sheets[FIRST_SHEET];
            WorksheetWriteListTasks(worksheet, listTasks);
            excelApp.Visible = true;
            workbook.Save();
        }
        private static void ChooseFolderWritePath(string targetFolderName, FolderBrowserDialog folderBrowserDialog) {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                string folderName = string.Empty;
                folderName = folderBrowserDialog.SelectedPath + "\\" + targetFolderName;
                WritePaht(folderName);
            }
        }

        private static void WritePaht(string folderName) {
            StreamWriter streamWriter = new StreamWriter(Constants.PROJECT_FILE_NAME, false);
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);
            streamWriter.WriteLine(folderName);
            SetProjectFolderPath(folderName);
            streamWriter.Close();
        }

        private static void SaveWorkBook (ref Excel.Workbook workbook, string pathFileToSave) {
            try {
                workbook.SaveAs(pathFileToSave);
                MessageBox.Show("Файл будет сохранен по адресу: " + pathFileToSave);
            }
            catch {
                MessageBox.Show("Не удалось сохранить файл " + pathFileToSave +
                    ". Заполнение буде произведено в файл шаблона. Необходимо его сохранить самстоятельно",
                    "Проблема", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                workbook.Save();
            }

        }
        private static void OpenWorkBook(
            ref Excel.Application excelApp, 
            ref Excel.Workbook workbook, 
            string pathModelFile) {
            try {
                if (File.Exists(pathModelFile)) {
                    workbook = excelApp.Workbooks.Open(pathModelFile);
                }
                else {
                    workbook = excelApp.Workbooks.Add();
                }
            }
            catch {
                excelApp.Quit();
                MessageBox.Show("Не удалось открыть файл " + pathModelFile +
                    " или проблемы с самим приложением");
                return;
            }
        }
        private static void WorksheetWriteListTasks(Excel.Worksheet worksheet, ListTasks listTasks) {
            const string COLUMN_NUMBER_TASK = "A";
            const string COLUMN_TASK_NAME = "B";
            const string COlUMN_DATE_START = "C";
            const string COLUMN_DATE_FINISH = "D";
            const string COLUMN_COUNT_WORKING = "E";
            const string COLUMN_PERSON_NAME = "F";
            const string COLUMN_TASK_AFTER = "G";
            const int FIRST_ROW_TO_WRITE = 2;
            int i = FIRST_ROW_TO_WRITE;
            foreach (Task task in listTasks.Tasks) {
                worksheet.Cells[i , COLUMN_TASK_NAME].Value = task.Name;
                worksheet.Cells[i , COLUMN_NUMBER_TASK].Value = task.Number;
                worksheet.Cells[i , COlUMN_DATE_START].Value = task.DateStart;
                worksheet.Cells[i , COLUMN_DATE_FINISH].Value = task.DateFinish;
                worksheet.Cells[i , COLUMN_COUNT_WORKING].Value = task.CountWorkingDays;
                worksheet.Cells[i , COLUMN_PERSON_NAME].Value = task.PersonFamaly;
                worksheet.Cells[i , COLUMN_TASK_AFTER].Value = task.TaskNumberAfter;
                i++;
            }
        }
    }
}
