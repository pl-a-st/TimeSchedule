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
        public static void WriteProjectFolder() {
            WriteProjectFolder("Проект");
        }
        public static void WriteProjectFolder(bool startProgram) {
            if (startProgram && (!File.Exists(Constants.PROJECT_FILE_NAME) || File.ReadAllLines(Constants.PROJECT_FILE_NAME).Length == 0)) {
                WriteProjectFolder("Проект");
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
        public static void WriteProjectFolder(string targetFolderName) {
            try {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.Description = "Выбирете папку проекта";

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                    StreamWriter streamWriter = new StreamWriter(Constants.PROJECT_FILE_NAME, false);
                    string folderName = string.Empty;
                    folderName = folderBrowserDialog.SelectedPath + "\\" + targetFolderName;
                    if (!Directory.Exists(folderName))
                        Directory.CreateDirectory(folderName);
                    streamWriter.WriteLine(folderName);
                    SetProjectFolderPath(folderName);
                    streamWriter.Close();
                }

            }
            catch {
                MessageBox.Show("Не удалось произвести запись в файл: " + Constants.PROJECT_FILE_NAME);
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
                    listFromFile.Add(streamReader.ReadLine());
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
        public static void ExelWriteListTasks(string fileToSaveName,ListTasks listTasks) {
            const int FIRST_ROW_TO_WRITE = 2;
            const string COLUMN_NUMBER_TASK = "A";
            const string COLUMN_TASK_NAME = "B";
            const string COlUMN_DATE_START = "C";
            const string COLUMN_DATE_FINISH = "D";
            const string COLUMN_COUNT_WORKING = "E";
            const string COLUMN_PERSON_NAME = "F";
            const string COLUMN_TASK_AFTER = "G";
            string modelFileName = Dals.ProjectFolderPath + "\\шаблон.xlsm";
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook;
            if (!File.Exists(modelFileName)) {
                workbook = excelApp.Workbooks.Add();
            }
            else {
                workbook = excelApp.Workbooks.Open(modelFileName);
            }
            
            workbook.SaveAs(Dals.ProjectFolderPath + "\\" + fileToSaveName);
            Excel.Worksheet worksheet = workbook.Sheets[1];
            int i = FIRST_ROW_TO_WRITE;
            foreach (Task task in listTasks.Tasks) {
                worksheet.Cells[COLUMN_NUMBER_TASK, i].Value = task.Number;
                worksheet.Cells[COLUMN_TASK_NAME, i].Value = task.Name;
                worksheet.Cells[COlUMN_DATE_START, i].Value = task.DateStart;
                worksheet.Cells[COLUMN_DATE_FINISH, i].Value = task.DateFinish;
                worksheet.Cells[COLUMN_COUNT_WORKING, i].Value = task.CountWorkingDays;
                worksheet.Cells[COLUMN_PERSON_NAME, i].Value = task.PersonFamaly;
                worksheet.Cells[COLUMN_TASK_AFTER, i].Value = task.TaskNumberAfter;
                i++;
            }
            workbook.Save();
            excelApp.Visible = true;
        }
    }
}
