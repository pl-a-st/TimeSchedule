using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Ошибка согласования: сохранённая задача изменилась или исчезла после открытия редактора.</summary>
    public sealed class TaskConflictException : IOException
    {
        /// <summary>Создаёт исключение с описанием конфликта для пользователя.</summary>
        public TaskConflictException(string message) : base(message) { }
    }

    /// <summary>
    /// Согласованный доступ к компактному хранилищу задач и двоичному списку исполнителей.
    /// Синхронизация между компьютерами требует общего файла блокировки.
    /// </summary>
    public static class TaskStore
    {
        /// <summary>
        /// Проверяет наличие задач; каталог компактного хранилища без HEAD
        /// также требует попытки чтения, чтобы сообщить об ошибке, а не скрыть её.
        /// </summary>
        public static bool HasTaskData(string file)
        {
            return Directory.Exists(CompactTaskStore.StoreDirectory(file)) || File.Exists(file);
        }

        /// <summary>Определяет принадлежность файла к основному списку задач.</summary>
        private static bool IsTaskFile(string file)
        {
            return string.Equals(Path.GetFileName(file), Constants.TASKS_BIN,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Число попыток открытия сетевого файла блокировки.</summary>
        private const int LockAttempts = 40;
        /// <summary>Пауза между попытками при занятости файла блокировки (мс).</summary>
        private const int LockDelayMs = 150;
        /// <summary>Имя файла синхронизации; удалять его во время работы клиентов нельзя.</summary>
        private const string LockSuffix = ".lock";
        /// <summary>Формат меток неизменяемого отсутствующего файла.</summary>
        private const string MissingRevision = "absent";

        /// <summary>Необязательный получатель обезличенных измерений времени чтения и сохранения.</summary>
        public static Action<string> TimingSink { get; set; }

        /// <summary>Отправляет замер только при явном подключении диагностики.</summary>
        private static void ReportTiming(string data)
        {
            Action<string> sink = TimingSink;
            if (sink == null) return;
            try { sink(data); }
            catch (Exception ex) when (!(ex is OutOfMemoryException) &&
                !(ex is ThreadAbortException) && !(ex is AccessViolationException))
            {
                // Диагностический получатель не вправе превращать уже выполненный commit в отказ.
                Debug.WriteLine("Ошибка получателя замеров TS-01: " + ex.Message);
            }
        }

        /// <summary>Записывает детали компактной публикации через общий безопасный диагностический канал.</summary>
        internal static void ReportCompactTiming(string data)
        {
            ReportTiming("compact " + data);
        }

        /// <summary>Захватывает файловую блокировку на SMB/другой общей файловой системе.</summary>
        private static FileStream Acquire(string file)
        {
            string lockFile = file + LockSuffix;
            IOException last = null;
            for (int attempt = 0; attempt < LockAttempts; attempt++)
            {
                try
                {
                    return new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException ex)
                {
                    last = ex;
                    if (attempt != LockAttempts - 1)
                        Thread.Sleep(LockDelayMs);
                }
            }
            throw new IOException("Не удалось получить блокировку общей папки: " + lockFile, last);
        }

        /// <summary>Читает проверенную опубликованную ревизию компактного хранилища задач.</summary>
        public static ListTasks Read(string file)
        {
            Stopwatch sw = Stopwatch.StartNew();
            ListTasks result;
            long waitMs;
            using (Acquire(file))
            {
                waitMs = sw.ElapsedMilliseconds;
                result = CompactTaskStore.ReadLocked(file);
            }
            if (TimingSink != null)
                ReportTiming("read tasks=" + result.Tasks.Count + " storage=" +
                    "compact" + " lock-wait-ms=" + waitMs +
                    " deserialize-ms=" + (sw.ElapsedMilliseconds - waitMs));
            return result;
        }

        /// <summary>Читает два связанных файла в согласованном с записью порядке блокировок.</summary>
        public static ListTasks ReadProject(string taskFile, out ListPersons people)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string personFile = Path.Combine(Path.GetDirectoryName(taskFile), Constants.PERSONS_BIN);
            ListTasks tasks;
            long waitMs, tasksMs;
            using (Acquire(personFile))
            using (Acquire(taskFile))
            {
                waitMs = sw.ElapsedMilliseconds;
                tasks = CompactTaskStore.ReadLocked(taskFile);
                tasksMs = sw.ElapsedMilliseconds - waitMs;
                people = ReadOrImportPersons(personFile);
            }
            if (TimingSink != null)
                ReportTiming("read-project tasks=" + tasks.Tasks.Count + " storage=" +
                    "compact" + " lock-wait-ms=" + waitMs +
                    " tasks-deserialize-ms=" + tasksMs + " persons-deserialize-ms=" +
                    (sw.ElapsedMilliseconds - waitMs - tasksMs));
            return tasks;
        }

        /// <summary>
        /// Получает ревизию для явного восстановления, даже если текущий HEAD повреждён.
        /// Не запускает миграцию или чтение повреждённых сегментов.
        /// </summary>
        public static string RevisionForRestore(string file)
        {
            using (Acquire(file))
                return IsTaskFile(file)
                    ? CompactTaskStore.CurrentRevision(file) : RevisionUnsafe(file);
        }

        /// <summary>Получает версию файла для проверки перед подтверждённым восстановлением.</summary>
        public static string Revision(string file)
        {
            using (Acquire(file))
            {
                if (IsTaskFile(file))
                {
                    CompactTaskStore.InitializeLocked(file); // миграция только под межмашинной блокировкой
                    return CompactTaskStore.CurrentRevision(file);
                }
                return RevisionUnsafe(file);
            }
        }

        /// <summary>Под межмашинной блокировкой выбирает проверенную резервную ревизию HEAD без записи данных.</summary>
        internal static CompactHeadRecoveryInfo FindCompactHeadRecovery(string taskFile)
        {
            using (Acquire(taskFile))
                return CompactTaskStore.FindHeadRecoveryLocked(taskFile);
        }

        /// <summary>Явно восстанавливает выбранную ревизию HEAD с защитой от конкурирующих изменений.</summary>
        internal static ListTasks RestoreCompactHead(string taskFile, CompactHeadRecoveryInfo approved)
        {
            using (Acquire(taskFile))
                return CompactTaskStore.RestoreHeadLocked(taskFile, approved);
        }

        /// <summary>Сохраняет изменение поверх актуального снимка с проверкой ранее показанных задач.</summary>
        /// <param name="file">Полный путь Tasks.bin выбранного проекта.</param>
        /// <param name="change">Изменение текущего списка; вызывается ровно один раз.</param>
        /// <param name="expected">Отпечатки всех существующих задач, которые разрешено менять или удалять.</param>
        public static ListTasks Update(string file, Action<ListTasks> change, IDictionary<long, string> expected)
        {
            if (change == null) throw new ArgumentNullException(nameof(change));
            Stopwatch sw = Stopwatch.StartNew();
            ListTasks current;
            Dictionary<long, string> compactSourceHashes = null;
            long waitMs, readMs, checkMs, changeMs, verifyMs, writeMs;
            using (Acquire(file))
            {
                waitMs = sw.ElapsedMilliseconds;
                current = CompactTaskStore.ReadLocked(file, out compactSourceHashes);
                readMs = sw.ElapsedMilliseconds - waitMs;
                Dictionary<long, string> original = Fingerprints(current);
                ValidateExpected(original, expected);
                checkMs = sw.ElapsedMilliseconds - waitMs - readMs;
                change(current);
                changeMs = sw.ElapsedMilliseconds - waitMs - readMs - checkMs;
                ValidateChanges(original, current, expected);
                verifyMs = sw.ElapsedMilliseconds - waitMs - readMs - checkMs - changeMs;
                CompactTaskStore.CommitLocked(file, current, compactSourceHashes);
                writeMs = sw.ElapsedMilliseconds - waitMs - readMs - checkMs - changeMs - verifyMs;
            }
            if (TimingSink != null)
                ReportTiming("save tasks=" + current.Tasks.Count + " storage=" +
                    "compact" + " lock-wait-ms=" + waitMs +
                    " deserialize-ms=" + readMs + " check-ms=" + checkMs +
                    " change-ms=" + changeMs + " verify-ms=" + verifyMs +
                    " serialize-publish-backup-ms=" + writeMs);
            return current;
        }

        /// <summary>
        /// Создаёт резервную копию только опубликованного состояния под общей файловой блокировкой.
        /// Не использует потенциально устаревший список задач из интерфейса.
        /// </summary>
        public static string CreateBackup(string file)
        {
            using (Acquire(file))
                return CompactTaskStore.CreateBackupLocked(file);
        }

        /// <summary>Восстанавливает компактный либо исторический архив с проверкой актуальной ревизии.</summary>
        public static ListTasks Restore(string file, string backupFile, string expectedRevision)
        {
            using (Acquire(file))
            {
                if (Directory.Exists(backupFile))
                    return CompactTaskStore.RestoreBackupLocked(file, backupFile, expectedRevision);

                CompactTaskStore.ReadLocked(file);
                string actualRevision = CompactTaskStore.CurrentRevision(file);
                if (actualRevision != expectedRevision)
                    throw new TaskConflictException("За время выбора резервной копии задачи были изменены. Обновите список и повторите восстановление.");
                ListTasks backup = ReadExisting<ListTasks>(backupFile);
                Fingerprints(backup);
                CompactTaskStore.CommitLocked(file, backup);
                return backup;
            }
        }

        /// <summary>
        /// Изменяет исполнителей и задачи под общими файловыми блокировками,
        /// проверяет исходные ревизии и согласованно публикует оба хранилища.
        /// Порядок блокировок: Persons.bin, затем Tasks.bin; общей атомарности нет.
        /// </summary>
        /// <param name="taskFile">Путь к Tasks.bin, определяющий каталог проекта.</param>
        /// <param name="change">Действие, изменяющее оба списка под блокировками.</param>
        /// <param name="expected">Ожидаемые отпечатки задач.</param>
        /// <param name="expectedPersonsRevision">Ожидаемая ревизия Persons.bin или null.</param>
        /// <returns>Список задач после успешного сохранения.</returns>
        public static ListTasks UpdatePersonsAndTasks(string taskFile, Action<ListPersons, ListTasks> change,
            IDictionary<long, string> expected, string expectedPersonsRevision = null)
        {
            string personFile = Path.Combine(Path.GetDirectoryName(taskFile), Constants.PERSONS_BIN);
            using (Acquire(personFile))
            using (Acquire(taskFile))
            {
                if (expectedPersonsRevision != null && RevisionUnsafe(personFile) != expectedPersonsRevision)
                    throw new TaskConflictException("Список исполнителей изменён другим пользователем. Обновите список.");
                ListPersons people = ReadOrImportPersons(personFile);
                ListTasks tasks = CompactTaskStore.ReadLocked(taskFile);
                Dictionary<long, string> before = Fingerprints(tasks);
                ValidateExpected(before, expected);
                change(people, tasks);
                ValidateChanges(before, tasks, expected);
                PublishPersonsAndTasksLocked(personFile, taskFile, people, tasks);
                return tasks;
            }
        }

        /// <summary>
        /// Публикует сначала Persons.bin, затем компактную ревизию задач в уже взятых
        /// блокировках. При ошибке до публикации задач компенсирует изменение Persons.bin.
        /// </summary>
        /// <param name="personFile">Путь к файлу исполнителей.</param>
        /// <param name="taskFile">Путь к Tasks.bin, определяющий каталог компактного хранилища.</param>
        /// <param name="people">Подготовленный список исполнителей.</param>
        /// <param name="tasks">Подготовленный список задач.</param>
        private static void PublishPersonsAndTasksLocked(string personFile, string taskFile,
            ListPersons people, ListTasks tasks)
        {
            byte[] previousPeople = File.Exists(personFile) ? File.ReadAllBytes(personFile) : null;
            string personTemp = Stage(personFile, people);
            bool publishedPeople = false;
            bool publishedTasks = false;
            try
            {
                string stagedPeopleHash = RevisionUnsafe(personTemp);
                string compactRevisionBefore = CompactTaskStore.CurrentRevision(taskFile);
                try { PublishStaged(personFile, personTemp); }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    if (RevisionUnsafe(personFile) != stagedPeopleHash) throw;
                }
                publishedPeople = true;
                personTemp = null;
                try
                {
                    CompactTaskStore.CommitLocked(taskFile, tasks);
                    publishedTasks = true;
                }
                catch
                {
                    if (CompactTaskStore.CurrentRevision(taskFile) != compactRevisionBefore)
                        publishedTasks = true;
                    throw;
                }
                Prune(personFile);
            }
            catch
            {
                if (publishedPeople && !publishedTasks)
                    RestorePersonsAfterFailedCommit(personFile, previousPeople);
                throw;
            }
            finally { TryDelete(personTemp); }
        }

        /// <summary>
        /// Возвращает прежние байты Persons.bin или удаляет вновь созданный файл,
        /// когда публикация изменений задач не состоялась.
        /// </summary>
        /// <param name="personFile">Путь к файлу исполнителей.</param>
        /// <param name="previousPeople">Прежнее содержимое файла или null, если его не было.</param>
        private static void RestorePersonsAfterFailedCommit(string personFile, byte[] previousPeople)
        {
            if (previousPeople == null)
                File.Delete(personFile);
            else
            {
                string rollback = StageBytes(personFile, previousPeople);
                try { PublishStaged(personFile, rollback); }
                finally { TryDelete(rollback); }
            }
        }

        /// <summary>Сохраняет изменение Persons.bin без перезаписи устаревшего списка из UI.</summary>
        public static ListPersons UpdatePersons(string file, Action<ListPersons> change,
            string expectedRevision = null)
        {
            using (Acquire(file))
            {
                if (expectedRevision != null && RevisionUnsafe(file) != expectedRevision)
                    throw new TaskConflictException("Список исполнителей был изменён. Обновите его и повторите действие.");
                ListPersons people = ReadOrImportPersons(file);
                change(people);
                Publish(file, people);
                return people;
            }
        }

        /// <summary>Читает текущую версию Persons.bin под блокировкой для отображения.</summary>
        public static ListPersons ReadPersons(string file)
        {
            using (Acquire(file)) return ReadOrImportPersons(file);
        }

        /// <summary>Читает список и его версию в одной критической секции.</summary>
        public static ListPersons ReadPersonsWithRevision(string file, out string revision)
        {
            using (Acquire(file))
            {
                ListPersons result = ReadOrImportPersons(file);
                revision = RevisionUnsafe(file);
                return result;
            }
        }

        /// <summary>Отпечатки только выбранных задач по сохранённой модели, без временного UI-размещения.</summary>
        public static Dictionary<long, string> Capture(ListTasks tasks, IEnumerable<long> numbers)
        {
            var required = new HashSet<long>(numbers);
            var result = new Dictionary<long, string>();
            foreach (Task task in tasks.Tasks)
                if (required.Contains(task.Number)) result.Add(task.Number, Fingerprint(task));
            if (result.Count != required.Count)
                throw new TaskConflictException("Часть выбранных задач отсутствует. Обновите список.");
            return result;
        }

        /// <summary>Получает отпечатки задачи и всех её рекурсивно зависимых задач из исходного снимка.</summary>
        public static Dictionary<long, string> CaptureWithDependents(ListTasks tasks, long number)
        {
            var ids = new HashSet<long> { number };
            bool added;
            do
            {
                added = false;
                foreach (Task task in tasks.Tasks)
                    if (ids.Contains(task.TaskNumberAfter) && ids.Add(task.Number)) added = true;
            } while (added);
            return Capture(tasks, ids);
        }

        /// <summary>Вычисляет устойчивый отпечаток сохраняемых полей задачи, исключая UI-раскладку.</summary>
        public static string Fingerprint(Task task)
        {
            var b = new StringBuilder();
            Append(b, task.Number.ToString(CultureInfo.InvariantCulture));
            Append(b, task.Name);
            Append(b, task.PersonFamaly);
            Append(b, ((int)task.Status).ToString(CultureInfo.InvariantCulture));
            Append(b, task.TaskNumberAfter.ToString(CultureInfo.InvariantCulture));
            Append(b, task.DateStart.ToBinary().ToString(CultureInfo.InvariantCulture));
            Append(b, task.DateFinish.ToBinary().ToString(CultureInfo.InvariantCulture));
            Append(b, task.CountDays.ToString(CultureInfo.InvariantCulture));
            Append(b, task.CountWorkingDays.ToString(CultureInfo.InvariantCulture));
            Append(b, task.Color.ToArgb().ToString(CultureInfo.InvariantCulture));
            Append(b, task.Priority.ToString(CultureInfo.InvariantCulture));
            if (task.PoolProjects != null && task.PoolProjects.Projects != null)
                foreach (Project project in task.PoolProjects.Projects)
                {
                    Append(b, project.Name);
                    Append(b, project.Address);
                }
            Append(b, "projects-end");
            if (task.GetTreeProjects().ListTreeNode != null)
                foreach (TreeNode node in task.GetTreeProjects().ListTreeNode)
                    AppendNode(b, node);
            Append(b, "tree-end");
            return Hash(Encoding.UTF8.GetBytes(b.ToString()));
        }

        /// <summary>Записывает строку с длиной, чтобы избежать неоднозначного склеивания полей.</summary>
        private static void Append(StringBuilder b, string value)
        {
            value = value ?? "<null>";
            b.Append(value.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(value);
        }

        /// <summary>Учитывает структуру дерева, отметки и сохранённые параметры узлов.</summary>
        private static void AppendNode(StringBuilder b, TreeNode node)
        {
            Append(b, "node");
            Append(b, node.Name);
            Append(b, node.Text);
            Append(b, node.Checked ? "1" : "0");
            Append(b, node.Tag == null ? null : node.Tag.GetType().FullName + ":" + node.Tag);
            Append(b, node.ImageIndex.ToString(CultureInfo.InvariantCulture));
            Append(b, node.ImageKey);
            Append(b, node.ToolTipText);
            foreach (TreeNode child in node.Nodes) AppendNode(b, child);
            Append(b, "end-node");
        }

        /// <summary>Возвращает контрольную сумму SHA-256 для двоичной последовательности.</summary>
        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        /// <summary>Проверяет, что исходные версии всех затрагиваемых задач актуальны.</summary>
        private static void ValidateExpected(IDictionary<long, string> before, IDictionary<long, string> expected)
        {
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            foreach (var entry in expected)
            {
                string currentFingerprint;
                if (!before.TryGetValue(entry.Key, out currentFingerprint) || currentFingerprint != entry.Value)
                    throw new TaskConflictException("Задача № " + entry.Key + " изменена или удалена другим пользователем. Ваши данные не сохранены. Обновите список и повторите изменение.");
            }
        }

        /// <summary>Не допускает изменения непроверенных задач и дублирования номеров.</summary>
        private static void ValidateChanges(Dictionary<long, string> before, ListTasks after,
            IDictionary<long, string> expected)
        {
            var now = Fingerprints(after);
            foreach (var entry in before)
                if ((!now.ContainsKey(entry.Key) || now[entry.Key] != entry.Value) && !expected.ContainsKey(entry.Key))
                    throw new TaskConflictException("Изменение затронуло задачу № " + entry.Key + " вне подтверждённого снимка. Сохранение отменено.");
        }

        /// <summary>Формирует отпечатки и отклоняет дубли номеров в исходном или новом списке.</summary>
        private static Dictionary<long, string> Fingerprints(ListTasks tasks)
        {
            var result = new Dictionary<long, string>();
            foreach (Task task in tasks.Tasks)
            {
                if (result.ContainsKey(task.Number))
                    throw new InvalidDataException("В Tasks.bin повторяется номер задачи: " + task.Number);
                result.Add(task.Number, Fingerprint(task));
            }
            return result;
        }

        /// <summary>Поддерживает старый Persons.txt при первом сохранении проекта.</summary>
        private static ListPersons ReadOrImportPersons(string file)
        {
            if (File.Exists(file)) return ReadExisting<ListPersons>(file);
            var people = new ListPersons();
            string legacy = Path.Combine(Path.GetDirectoryName(file), Constants.PERSONS);
            if (File.Exists(legacy))
                foreach (string line in File.ReadAllLines(legacy).Where(s => !string.IsNullOrWhiteSpace(s)))
                    people.AddPerson(new Person(line.Split('\t')[0]));
            return people;
        }

        /// <summary>Строго десериализует файл; пустота, порча и недоступность являются ошибкой.</summary>
        private static T ReadExisting<T>(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length == 0) throw new InvalidDataException("Файл пуст: " + file);
                var value = (T)new BinaryFormatter().Deserialize(stream);
                if (value == null) throw new InvalidDataException("Файл не содержит объекта: " + file);
                return value;
            }
        }

        /// <summary>Получает хеш текущей публикации только внутри файловой блокировки.</summary>
        private static string RevisionUnsafe(string file)
        {
            return File.Exists(file) ? Hash(File.ReadAllBytes(file)) : MissingRevision;
        }

        /// <summary>Сериализует полную новую версию в отдельный файл на том же томе.</summary>
        private static string Stage<T>(string target, T obj)
        {
            string temp = target + ".pending-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    new BinaryFormatter().Serialize(stream, obj);
                    stream.Flush(true);
                }
                return temp;
            }
            catch { TryDelete(temp); throw; }
        }

        /// <summary>Готовит двоичную копию для компенсационного восстановления.</summary>
        private static string StageBytes(string target, byte[] contents)
        {
            string temp = target + ".pending-" + Guid.NewGuid().ToString("N");
            try
            {
                using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(contents, 0, contents.Length);
                    stream.Flush(true);
                }
                return temp;
            }
            catch { TryDelete(temp); throw; }
        }

        /// <summary>
        /// Публикует готовую сериализацию, при диагностике отдельно измеряет стадии,
        /// а временный файл удаляет при любом исходе операции.
        /// </summary>
        private static void Publish<T>(string file, T obj)
        {
            Stopwatch timing = TimingSink == null ? null : Stopwatch.StartNew();
            string temp = Stage(file, obj);
            long stageMs = timing == null ? 0 : timing.ElapsedMilliseconds;
            try
            {
                PublishStaged(file, temp);
                long replaceMs = timing == null ? 0 : timing.ElapsedMilliseconds - stageMs;
                Prune(file);
                if (timing != null)
                    ReportTiming("publish stage-ms=" + stageMs + " replace-ms=" + replaceMs +
                        " prune-ms=" + (timing.ElapsedMilliseconds - stageMs - replaceMs));
            }
            finally { TryDelete(temp); }
        }

        /// <summary>
        /// Публикует только через атомарную замену на поддерживающей её ФС.
        /// При отсутствии File.Replace отказывает, не выполняя неатомарную перезапись.
        /// </summary>
        private static string PublishStaged(string file, string temp)
        {
            if (!File.Exists(file))
            {
                File.Move(temp, file); // новый файл на той же ФС
                return null;
            }
            string backupDir = Path.Combine(Path.GetDirectoryName(file), "BackUp", Path.GetFileName(file));
            Directory.CreateDirectory(backupDir);
            string backup = Path.Combine(backupDir,
                DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss_fffffff", CultureInfo.InvariantCulture) + " " + Guid.NewGuid().ToString("N"));
            File.Replace(temp, file, backup); // на поддерживаемых SMB/NTFS замена без промежуточной пустоты
            return backup;
        }

        /// <summary>Сокращает число старых резервных копий без влияния на результат commit.</summary>
        private static void Prune(string file)
        {
            try
            {
                string dir = Path.Combine(Path.GetDirectoryName(file), "BackUp", Path.GetFileName(file));
                if (!Directory.Exists(dir)) return;
                foreach (FileInfo backup in new DirectoryInfo(dir).GetFiles()
                    .OrderByDescending(f => f.CreationTimeUtc).Skip(Constants.MAX_BACKUP_FILESES))
                    backup.Delete();
            }
            catch (IOException) { /* резервные копии остаются; успешная публикация не отменяется */ }
            catch (UnauthorizedAccessException) { /* сохранённые задачи не откатываем */ }
        }

        /// <summary>Пытается удалить только вспомогательный файл после завершения операции.</summary>
        private static void TryDelete(string file)
        {
            if (string.IsNullOrEmpty(file)) return;
            try { if (File.Exists(file)) File.Delete(file); }
            catch (IOException) { /* оставшийся .pending не читается как основной */ }
            catch (UnauthorizedAccessException) { /* удаляется администратором после проверки */ }
        }
    }
}
