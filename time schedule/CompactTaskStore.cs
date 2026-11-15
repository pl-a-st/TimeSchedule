using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Статистика испытательного преобразования без содержимого пользовательских задач.</summary>
    public sealed class CompactMigrationResult
    {
        /// <summary>Число преобразованных задач.</summary>
        public int TaskCount { get; internal set; }

        /// <summary>Число непустых сегментов.</summary>
        public int SegmentCount { get; internal set; }

        /// <summary>Размер исходного бинарного файла в байтах.</summary>
        public long LegacyBytes { get; internal set; }

        /// <summary>Общий размер файлов сегментов в байтах.</summary>
        public long SegmentBytes { get; internal set; }

        /// <summary>Время чтения и десериализации исходного формата, мс.</summary>
        public long LegacyReadMs { get; internal set; }

        /// <summary>Время подготовки нового компактного хранилища, мс.</summary>
        public long MigrationMs { get; internal set; }

        /// <summary>Время загрузки сегментов в параллельном режиме, мс.</summary>
        public long ParallelReadMs { get; internal set; }
    }

    /// <summary>Подтверждённый для явного восстановления резервный манифест компактного хранилища.</summary>
    internal sealed class CompactHeadRecoveryInfo
    {
        /// <summary>Полный путь резервного манифеста в каталоге history.</summary>
        internal string BackupPath { get; set; }

        /// <summary>SHA-256 резервного манифеста на момент предварительной проверки.</summary>
        internal string BackupRevision { get; set; }

        /// <summary>Ревизия текущего HEAD либо метка отсутствующего файла на момент выбора.</summary>
        internal string ExpectedCurrentRevision { get; set; }

        /// <summary>Номер проверенной резервной ревизии.</summary>
        internal long Generation { get; set; }

        /// <summary>Число задач в проверенном резервном снимке.</summary>
        internal int TaskCount { get; set; }

        /// <summary>Число сегментов в проверенном резервном снимке.</summary>
        internal int SegmentCount { get; set; }
    }

    /// <summary>
    /// Сегментированное хранилище TS-01. В Debug используется автоматически
    /// на тестовом проекте; прежний Tasks.bin не изменяется.
    /// </summary>
    public static class CompactTaskStore
    {
        /// <summary>Фиксированный диапазон номеров задач в сегменте.</summary>
        public const int SegmentSize = 128;

        /// <summary>Максимальное количество потоков загрузки и кодирования сегментов.</summary>
        private const int MaxReaders = 8;

        /// <summary>Ограничение числа одновременных операций записи независимых файлов сегментов.</summary>
        private const int MaxParallelWrites = 4;

        /// <summary>Формат манифеста: 1-я версия.</summary>
        private const int HeadVersion = 1;

        /// <summary>Сигнатура манифеста.</summary>
        private const int HeadMagic = 0x31534854; // THS1

        /// <summary>Таймаут ожидания совместной файловой блокировки.</summary>
        private const int LockAttempts = 40;

        /// <summary>Задержка между повторными попытками получения блокировки.</summary>
        private const int LockDelayMs = 150;

        /// <summary>Текущий контрольный снимок хранилища.</summary>
        private sealed class Head
        {
            /// <summary>Номер опубликованной ревизии.</summary>
            internal long Generation;

            /// <summary>Размер числового диапазона сегмента в этой опубликованной версии.</summary>
            internal int RangeSize;

            /// <summary>Контрольная сумма исходного Tasks.bin, записанная при однократной миграции; только метаданные.</summary>
            internal string LegacyHash;

            /// <summary>Непустые опубликованные сегменты по номеру диапазона.</summary>
            internal SortedDictionary<long, string> Segments = new SortedDictionary<long, string>();
        }

        /// <summary>Имя изолированного каталога компактного хранилища рядом с исходным Tasks.bin.</summary>
        private const string StoreDirectoryName = "Tasks.compact";

        /// <summary>Определяет место компактного хранилища без доступа к данным пользователя.</summary>
        internal static string StoreDirectory(string legacyFile)
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(legacyFile)), StoreDirectoryName);
        }

        /// <summary>Признак опубликованного компактного хранилища для выбранного проекта.</summary>
        internal static bool Exists(string legacyFile)
        {
            return File.Exists(HeadPath(StoreDirectory(legacyFile)));
        }

        /// <summary>Возвращает версию опубликованного HEAD, не трогая исходный Tasks.bin.</summary>
        internal static string CurrentRevision(string legacyFile)
        {
            string path = HeadPath(StoreDirectory(legacyFile));
            return File.Exists(path) ? Sha(path) : "absent";
        }

        /// <summary>
        /// Под общей блокировкой находит самый поздний доступный и полностью проверенный
        /// резервный HEAD. Не изменяет файлы и не запускает повторную миграцию.
        /// </summary>
        internal static CompactHeadRecoveryInfo FindHeadRecoveryLocked(string legacyFile)
        {
            string root = StoreDirectory(legacyFile);
            if (!Directory.Exists(root) || !HeadNeedsRecovery(root)) return null;
            string history = Path.Combine(root, "history");
            if (!Directory.Exists(history))
                throw new InvalidDataException("Каталог history не найден. Восстановление HEAD невозможно.");

            string originalRevision = CurrentRevision(legacyFile);
            foreach (string backup in Directory.GetFiles(history, "HEAD-*").OrderByDescending(name => Path.GetFileName(name), StringComparer.Ordinal))
            {
                try
                {
                    Head candidate = ReadHeadFile(backup);
                    if (candidate.Generation <= 0 || candidate.Generation == long.MaxValue)
                        throw new InvalidDataException("Некорректная ревизия резервного HEAD");
                    ListTasks tasks = ReadSegments(root, candidate);
                    return new CompactHeadRecoveryInfo {
                        BackupPath = backup,
                        BackupRevision = Sha(backup),
                        ExpectedCurrentRevision = originalRevision,
                        Generation = candidate.Generation,
                        TaskCount = tasks.Tasks.Count,
                        SegmentCount = candidate.Segments.Count
                    };
                }
                catch (Exception ex) when (IsDamagedRecoveryCandidate(ex))
                {
                    continue;
                }
            }
            throw new InvalidDataException("В history нет резервного HEAD с целыми сегментами. Данные не изменены.");
        }

        /// <summary>
        /// После явного подтверждения публикует резервный снимок как новую ревизию.
        /// Перед заменой повторно проверяет исходное состояние, кандидата и все сегменты.
        /// </summary>
        internal static ListTasks RestoreHeadLocked(string legacyFile, CompactHeadRecoveryInfo approved)
        {
            if (approved == null) throw new ArgumentNullException(nameof(approved));
            string root = StoreDirectory(legacyFile);
            string history = Path.GetFullPath(Path.Combine(root, "history"));
            string backup = Path.GetFullPath(approved.BackupPath);
            if (!string.Equals(Path.GetDirectoryName(backup), history, StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(backup).StartsWith("HEAD-", StringComparison.Ordinal))
                throw new InvalidDataException("Резервный манифест не принадлежит выбранному проекту");
            if (!string.Equals(CurrentRevision(legacyFile), approved.ExpectedCurrentRevision, StringComparison.OrdinalIgnoreCase) ||
                !HeadNeedsRecovery(root))
                throw new TaskConflictException("HEAD изменился после подтверждения восстановления. Повторите выбор резервной копии.");
            if (!string.Equals(Sha(backup), approved.BackupRevision, StringComparison.OrdinalIgnoreCase))
                throw new TaskConflictException("Резервный HEAD изменился после выбора. Повторите восстановление.");

            Head candidate = ReadHeadFile(backup);
            ListTasks tasks = ReadSegments(root, candidate);
            if (candidate.Generation != approved.Generation ||
                candidate.Segments.Count != approved.SegmentCount || tasks.Tasks.Count != approved.TaskCount)
                throw new TaskConflictException("Состав резервного снимка изменился. Повторите выбор.");
            var published = new Head {
                Generation = checked(candidate.Generation + 1),
                RangeSize = candidate.RangeSize,
                LegacyHash = candidate.LegacyHash
            };
            foreach (var entry in candidate.Segments)
                published.Segments.Add(entry.Key, entry.Value);
            WriteHead(root, published, true);
            TaskStore.ReportCompactTiming("head-recovery generation=" + published.Generation +
                " tasks=" + tasks.Tasks.Count + " segments=" + published.Segments.Count);
            return tasks;
        }

        /// <summary>Проверяет только HEAD, отличая повреждение манифеста от повреждения сегмента.</summary>
        private static bool HeadNeedsRecovery(string root)
        {
            if (!File.Exists(HeadPath(root))) return true;
            try { ReadHead(root); return false; }
            catch (Exception ex) when (IsInvalidHeadException(ex)) { return true; }
        }

        /// <summary>Определяет ожидаемые повреждения формата манифеста, не скрывая ошибок доступа.</summary>
        private static bool IsInvalidHeadException(Exception ex)
        {
            return ex is InvalidDataException || ex is EndOfStreamException ||
                ex is FormatException || ex is ArgumentException || ex is OverflowException;
        }

        /// <summary>Определяет ошибки целостности архивного снимка, включая ошибки в параллельных читателях.</summary>
        private static bool IsDamagedRecoveryCandidate(Exception ex)
        {
            if (IsInvalidHeadException(ex) || ex is FileNotFoundException || ex is DirectoryNotFoundException ||
                ex is System.Runtime.Serialization.SerializationException)
                return true;
            AggregateException aggregate = ex as AggregateException;
            return aggregate != null && aggregate.Flatten().InnerExceptions.All(IsDamagedRecoveryCandidate);
        }

        /// <summary>
        /// Под блокировкой Tasks.bin.lock безопасно создаёт первое компактное состояние.
        /// Вызывающий код отвечает за сетевую блокировку и разрешение тестового режима.
        /// </summary>
        internal static void InitializeLocked(string legacyFile)
        {
            if (Exists(legacyFile)) return;
            string root = StoreDirectory(legacyFile);
            // Отсутствие HEAD в уже созданном каталоге может означать утрату
            // опубликованного манифеста. Нельзя молча заменить актуальные задачи
            // архивным Tasks.bin, даже если он сохранился после миграции.
            if (Directory.Exists(root))
                throw new InvalidDataException(
                    "Найден каталог Tasks.compact без HEAD. Автоматическая миграция отменена; " +
                    "проверьте целостность хранилища и восстановите манифест из резервной копии.");
            string baseline = LegacyRevision(legacyFile);
            ListTasks old;
            if (File.Exists(legacyFile))
            {
                using (var input = new FileStream(legacyFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    old = (ListTasks)new BinaryFormatter().Deserialize(input);
            }
            else
            {
                // Новый проект: допускается создание компактной базы без пустого Tasks.bin.
                old = new ListTasks();
                string textFile = Path.Combine(Path.GetDirectoryName(legacyFile), Constants.TASKS);
                if (File.Exists(textFile))
                    old.SetTasksFromList(File.ReadAllLines(textFile).Where(x => !string.IsNullOrWhiteSpace(x)).ToList());
            }
            var head = new Head { Generation = 1, LegacyHash = baseline, RangeSize = SegmentSize };
            WriteChangedSegments(root, old, head);
            // Публикация возможна только после проверки обратного преобразования.
            ListTasks converted = ReadSegments(root, head);
            VerifyEquivalent(old, converted);
            if (LegacyRevision(legacyFile) != baseline)
                throw new IOException("Исходный Tasks.bin изменён во время преобразования");
            WriteHead(root, head);
        }

        /// <summary>
        /// Читает актуальную компактную версию из HEAD и сегментов.
        /// К старому Tasks.bin обращается только при самой первой миграции.
        /// Требует удержания внешней блокировки Tasks.bin.lock при миграции.
        /// </summary>
        internal static ListTasks ReadLocked(string legacyFile)
        {
            InitializeLocked(legacyFile);
            string root = StoreDirectory(legacyFile);
            return ReadSegments(root, ReadHead(root));
        }

        /// <summary>
        /// Читает тот же согласованный снимок с отпечатками исходных записей.
        /// Вызывается только перед изменением для последующего выбора сегментов.
        /// </summary>
        internal static ListTasks ReadLocked(string legacyFile, out Dictionary<long, string> sourceHashes)
        {
            InitializeLocked(legacyFile);
            string root = StoreDirectory(legacyFile);
            return ReadSegments(root, ReadHead(root), true, out sourceHashes);
        }

        /// <summary>Публикует полный снимок в сценариях без исходных отпечатков (например, восстановление).</summary>
        internal static void CommitLocked(string legacyFile, ListTasks changed)
        {
            CommitLocked(legacyFile, changed, null);
        }

        /// <summary>
        /// Для обычного изменения вычисляет полные отпечатки текущих записей и
        /// кодирует только изменённые сегменты. При отсутствии исходных отпечатков
        /// сохраняет прежний полный алгоритм публикации.
        /// </summary>
        internal static void CommitLocked(string legacyFile, ListTasks changed,
            IDictionary<long, string> sourceHashes)
        {
            string root = StoreDirectory(legacyFile);
            Head previous = ReadHead(root);
            Stopwatch timing = TaskStore.TimingSink == null ? null : Stopwatch.StartNew();
            var next = new Head {
                Generation = checked(previous.Generation + 1),
                LegacyHash = previous.LegacyHash,
                RangeSize = previous.RangeSize
            };
            foreach (var entry in previous.Segments) next.Segments.Add(entry.Key, entry.Value);
            HashSet<long> candidates = sourceHashes == null
                ? null : FindChangedSegments(changed, sourceHashes, previous.RangeSize);
            long selectionMs = timing == null ? 0 : timing.ElapsedMilliseconds;
            int changedSegments;
            long changedBytes;
            bool changedContent = WriteChangedSegments(root, changed, next, candidates,
                out changedSegments, out changedBytes);
            long segmentsMs = timing == null ? 0 : timing.ElapsedMilliseconds - selectionMs;
            string selectionInfo = " select-segments-ms=" + selectionMs +
                " candidate-segments=" + (candidates == null ? "all" : candidates.Count.ToString(CultureInfo.InvariantCulture));
            if (!changedContent)
            {
                if (timing != null)
                    TaskStore.ReportCompactTiming("commit legacy-check-ms=0 compare-segments-ms=" + segmentsMs +
                        selectionInfo + " second-legacy-check-ms=0 published-segments=0");
                return;
            }
            WriteHead(root, next);
            if (timing != null)
                TaskStore.ReportCompactTiming("commit legacy-check-ms=0 compare-segments-ms=" + segmentsMs +
                    selectionInfo + " second-legacy-check-ms=0 publish-head-ms=" +
                    (timing.ElapsedMilliseconds - selectionMs - segmentsMs) +
                    " published-segments=" + changedSegments + " published-bytes=" + changedBytes);
        }

        /// <summary>
        /// Находит сегменты по изменению полного компактного представления задачи,
        /// её исходному/новому номеру и позиции. Учитывает поля, отсутствующие
        /// в отпечатках TaskStore, удаление, добавление и перестановку записей.
        /// </summary>
        private static HashSet<long> FindChangedSegments(ListTasks changed,
            IDictionary<long, string> sourceHashes, int segmentSize)
        {
            var touched = new ConcurrentDictionary<long, byte>();
            var currentNumbers = new ConcurrentDictionary<long, byte>();
            var entries = changed.Tasks.Select((task, index) =>
                new CompactTaskRecord { Item = task, Ordinal = index }).ToArray();
            var options = new ParallelOptions {
                MaxDegreeOfParallelism = Math.Min(MaxReaders, Environment.ProcessorCount)
            };
            try
            {
                Parallel.ForEach(entries, options, record =>
                {
                    long number = record.Item.Number;
                    if (!currentNumbers.TryAdd(number, 0))
                        throw new InvalidDataException("Повторяющийся номер задачи: " + number);
                    string initial;
                    string actual = CompactTaskCodec.RecordHash(record.Item, record.Ordinal);
                    if (!sourceHashes.TryGetValue(number, out initial) ||
                        !string.Equals(initial, actual, StringComparison.OrdinalIgnoreCase))
                        touched.TryAdd(SegmentId(number, segmentSize), 0);
                });
            }
            catch (AggregateException error)
            {
                AggregateException flattened = error.Flatten();
                if (flattened.InnerExceptions.Count == 1)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(flattened.InnerExceptions[0]).Throw();
                throw;
            }
            foreach (long removed in sourceHashes.Keys)
                if (!currentNumbers.ContainsKey(removed))
                    touched.TryAdd(SegmentId(removed, segmentSize), 0);
            return new HashSet<long>(touched.Keys);
        }

        /// <summary>Получает номер сегмента из устойчивого номера задачи.</summary>
        private static long SegmentId(long number, int size)
        {
            if (number <= 0) throw new InvalidDataException("У задачи недопустимый номер: " + number);
            if (size < 32 || size > 512) throw new InvalidDataException("Некорректный размер сегмента");
            return (number - 1) / size;
        }

        /// <summary>Размещает каталог хранилища строго по указанному пути.</summary>
        private static string HeadPath(string root) { return Path.Combine(root, "HEAD"); }

        /// <summary>Формирует только допустимое имя файла по идентификатору и хешу.</summary>
        private static string SegmentPath(string root, long id, string sha)
        {
            if (id < 0 || sha == null || sha.Length != 64 || !sha.All(Uri.IsHexDigit))
                throw new InvalidDataException("Некорректная ссылка на сегмент");
            return Path.Combine(root, "segments", "part-" + id.ToString("D8", CultureInfo.InvariantCulture) + "-" + sha + ".tseg");
        }

        /// <summary>Захватывает ту же сетевую блокировку Tasks.bin.lock, что и существующий TaskStore.</summary>
        private static FileStream Acquire(string legacyFile)
        {
            IOException last = null;
            for (int i = 0; i < LockAttempts; i++)
            {
                try { return new FileStream(legacyFile + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
                catch (IOException ex)
                {
                    last = ex;
                    if (i + 1 < LockAttempts) Thread.Sleep(LockDelayMs);
                }
            }
            throw new IOException("Не удалось получить блокировку общего хранилища", last);
        }

        /// <summary>
        /// Создаёт компактную копию старого файла исключительно в указанном тестовом каталоге.
        /// Не изменяет исходный Tasks.bin и не переключает рабочее приложение.
        /// </summary>
        public static CompactMigrationResult BuildPreview(string legacyFile, string testStoreDirectory, int segmentSize = SegmentSize)
        {
            if (segmentSize < 32 || segmentSize > 512) throw new ArgumentOutOfRangeException(nameof(segmentSize));
            if (!File.Exists(legacyFile)) throw new FileNotFoundException("Исходный файл не найден", legacyFile);
            if (File.Exists(HeadPath(testStoreDirectory)))
                throw new IOException("Тестовое хранилище уже существует; выберите новую папку");
            if (Path.GetFullPath(testStoreDirectory).TrimEnd(Path.DirectorySeparatorChar) ==
                Path.GetFullPath(Path.GetDirectoryName(legacyFile)).TrimEnd(Path.DirectorySeparatorChar))
                throw new InvalidOperationException("Для эксперимента нужен отдельный каталог");
            var result = new CompactMigrationResult { LegacyBytes = new FileInfo(legacyFile).Length };
            var watch = Stopwatch.StartNew();
            ListTasks source;
            string baseline;
            using (Acquire(legacyFile))
            {
                baseline = Sha(legacyFile);
                using (var stream = File.Open(legacyFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    source = (ListTasks)new BinaryFormatter().Deserialize(stream);
            }
            result.LegacyReadMs = watch.ElapsedMilliseconds;
            watch.Restart();
            var current = new Head { Generation = 1, LegacyHash = baseline, RangeSize = segmentSize };
            WriteChangedSegments(testStoreDirectory, source, current);
            WriteHead(testStoreDirectory, current);
            result.MigrationMs = watch.ElapsedMilliseconds;
            result.TaskCount = source.Tasks.Count;
            result.SegmentCount = current.Segments.Count;
            result.SegmentBytes = current.Segments.Sum(x => new FileInfo(SegmentPath(testStoreDirectory, x.Key, x.Value)).Length);
            watch.Restart();
            ListTasks converted = ReadPreview(legacyFile, testStoreDirectory);
            result.ParallelReadMs = watch.ElapsedMilliseconds;
            VerifyEquivalent(source, converted);
            return result;
        }

        /// <summary>Загружает согласованный снимок сегментов параллельно и проверяет исходную базу миграции.</summary>
        public static ListTasks ReadPreview(string legacyFile, string storeDirectory)
        {
            Head current = ReadHead(storeDirectory);
            if (!string.Equals(LegacyRevision(legacyFile), current.LegacyHash, StringComparison.Ordinal))
                throw new IOException("После миграции исходный Tasks.bin изменён старым клиентом. Данные нельзя безопасно объединить автоматически.");
            return ReadSegments(storeDirectory, current);
        }

        /// <summary>Читает сегменты без дополнительных отпечатков для обычного просмотра.</summary>
        private static ListTasks ReadSegments(string root, Head head)
        {
            Dictionary<long, string> ignored;
            return ReadSegments(root, head, false, out ignored);
        }

        /// <summary>Читает валидированные сегменты, при необходимости сохраняя SHA-256 исходных записей.</summary>
        private static ListTasks ReadSegments(string root, Head head, bool captureSourceHashes,
            out Dictionary<long, string> sourceHashes)
        {
            var records = new ConcurrentBag<CompactTaskRecord>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(MaxReaders, Environment.ProcessorCount) };
            try
            {
                Parallel.ForEach(head.Segments, options, entry =>
                {
                    string path = SegmentPath(root, entry.Key, entry.Value);
                    if (!string.Equals(Sha(path), entry.Value, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Повреждён сегмент задач: " + path);
                    foreach (CompactTaskRecord record in CompactTaskCodec.Read(path, head.RangeSize, captureSourceHashes))
                    {
                        if (SegmentId(record.Item.Number, head.RangeSize) != entry.Key)
                            throw new InvalidDataException("Задача находится не в своём сегменте");
                        records.Add(record);
                    }
                });
            }
            catch (AggregateException error)
            {
                AggregateException flattened = error.Flatten();
                if (flattened.InnerExceptions.Count == 1)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(flattened.InnerExceptions[0]).Throw();
                if (flattened.InnerExceptions.All(IsDamagedRecoveryCandidate))
                    throw new InvalidDataException("Повреждены сегменты компактного хранилища", error);
                throw;
            }
            var ordered = records.OrderBy(x => x.Ordinal).ToList();
            for (int i = 0; i < ordered.Count; i++)
                if (ordered[i].Ordinal != i)
                    throw new InvalidDataException("Повреждён порядок задач в манифесте");
            var numbers = new HashSet<long>();
            foreach (CompactTaskRecord r in ordered)
                if (!numbers.Add(r.Item.Number))
                    throw new InvalidDataException("Повторяющийся номер задачи: " + r.Item.Number);
            sourceHashes = captureSourceHashes
                ? ordered.ToDictionary(x => x.Item.Number, x => x.SourceHash)
                : null;
            var tasks = new ListTasks();
            tasks.SetTasks(ordered.Select(x => x.Item).ToList());
            return tasks;
        }

        /// <summary>
        /// Испытательное согласованное изменение компактного снимка: проверяет показанные версии,
        /// сохраняет только затронутые сегменты и публикует один манифест.
        /// </summary>
        public static ListTasks UpdatePreview(string legacyFile, string storeDirectory,
            Action<ListTasks> change, IDictionary<long, string> expected)
        {
            if (change == null) throw new ArgumentNullException(nameof(change));
            if (expected == null) throw new ArgumentNullException(nameof(expected));
            using (Acquire(legacyFile))
            {
                Head oldHead = ReadHead(storeDirectory);
                ListTasks before = ReadPreview(legacyFile, storeDirectory);
                var oldHashes = Fingerprints(before);
                foreach (KeyValuePair<long, string> pair in expected)
                {
                    string hash;
                    if (!oldHashes.TryGetValue(pair.Key, out hash) || hash != pair.Value)
                        throw new TaskConflictException("Задача № " + pair.Key + " была изменена другим пользователем");
                }
                change(before);
                var newHashes = Fingerprints(before);
                foreach (KeyValuePair<long, string> pair in oldHashes)
                {
                    string hash;
                    if ((!newHashes.TryGetValue(pair.Key, out hash) || hash != pair.Value) && !expected.ContainsKey(pair.Key))
                        throw new TaskConflictException("Изменение затронуло неподтверждённую задачу № " + pair.Key);
                }
                var next = new Head { Generation = checked(oldHead.Generation + 1), LegacyHash = oldHead.LegacyHash, RangeSize = oldHead.RangeSize };
                foreach (var entry in oldHead.Segments) next.Segments.Add(entry.Key, entry.Value);
                if (WriteChangedSegments(storeDirectory, before, next))
                    WriteHead(storeDirectory, next);
                return before;
            }
        }

        /// <summary>Возвращает отпечатки всех задач, отклоняя дубли номеров.</summary>
        private static Dictionary<long, string> Fingerprints(ListTasks tasks)
        {
            var result = new Dictionary<long, string>();
            foreach (Task t in tasks.Tasks)
                if (!result.TryAddCompat(t.Number, TaskStore.Fingerprint(t)))
                    throw new InvalidDataException("Повторяющийся номер задачи: " + t.Number);
            return result;
        }

        /// <summary>
        /// Подготовленное неизменяемое содержимое сегмента и его контрольная сумма.
        /// </summary>
        private sealed class PreparedSegment
        {
            /// <summary>Сериализованные байты сегмента для проверки и публикации.</summary>
            internal byte[] Payload;

            /// <summary>SHA-256 сериализованных байтов сегмента.</summary>
            internal string Hash;
        }

        /// <summary>Готовит новые версии только для сегментов, чьи сериализованные байты изменились.</summary>
        private static bool WriteChangedSegments(string root, ListTasks tasks, Head head)
        {
            int count;
            long size;
            return WriteChangedSegments(root, tasks, head, null, out count, out size);
        }

        /// <summary>
        /// Сравнивает SHA-256 готовых сегментов с HEAD и записывает независимые изменённые
        /// файлы параллельно при наличии нескольких изменений. HEAD публикуется
        /// вызывающим методом только после успешной подготовки всех файлов.
        /// </summary>
        private static bool WriteChangedSegments(string root, ListTasks tasks, Head head,
            HashSet<long> candidates, out int publishedSegments, out long publishedBytes)
        {
            string segmentsPath = Path.Combine(root, "segments");
            Directory.CreateDirectory(segmentsPath);
            var all = tasks.Tasks.Select((task, index) => new CompactTaskRecord { Item = task, Ordinal = index })
                .GroupBy(x => SegmentId(x.Item.Number, head.RangeSize))
                .ToDictionary(x => x.Key, x => x.ToList());
            publishedSegments = 0;
            publishedBytes = 0;
            bool changed = false;
            foreach (long removed in head.Segments.Keys.Where(id => !all.ContainsKey(id)).ToList())
            {
                head.Segments.Remove(removed);
                changed = true;
            }
            // Без исходного снимка кодируем все сегменты (миграция, восстановление).
            // В обычном сохранении кодируем только сегменты с изменёнными записями.
            // Каждый файл сегмента неизменяем; HEAD публикуется отдельно и последним.
            var selected = candidates == null ? all :
                all.Where(x => candidates.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
            var prepared = new ConcurrentDictionary<long, PreparedSegment>();
            var options = new ParallelOptions {
                MaxDegreeOfParallelism = Math.Min(MaxReaders, Environment.ProcessorCount)
            };
            try
            {
                Parallel.ForEach(selected, options, group =>
                {
                    byte[] bytes = CompactTaskCodec.Encode(group.Value);
                    string digest = Sha(bytes);
                    if (!prepared.TryAdd(group.Key, new PreparedSegment { Payload = bytes, Hash = digest }))
                        throw new InvalidDataException("Повторяющийся номер сегмента");
                });
            }
            catch (AggregateException error)
            {
                AggregateException flattened = error.Flatten();
                if (flattened.InnerExceptions.Count == 1)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(flattened.InnerExceptions[0]).Throw();
                throw;
            }
            // Определяем новые версии до обращения к диску: HEAD пока не изменяется.
            var changes = new List<KeyValuePair<long, PreparedSegment>>();
            foreach (var group in selected)
            {
                PreparedSegment encoded = prepared[group.Key];
                string oldHash;
                if (head.Segments.TryGetValue(group.Key, out oldHash) &&
                    string.Equals(oldHash, encoded.Hash, StringComparison.OrdinalIgnoreCase))
                    continue;
                changes.Add(new KeyValuePair<long, PreparedSegment>(group.Key, encoded));
            }

            int newFiles = 0;
            bool parallelWrite = changes.Count > 1 && Environment.ProcessorCount > 1;
            Stopwatch writeTimer = TaskStore.TimingSink == null ? null : Stopwatch.StartNew();
            Action<KeyValuePair<long, PreparedSegment>> writeOne = entry =>
            {
                byte[] payload = entry.Value.Payload;
                string hash = entry.Value.Hash;
                string target = SegmentPath(root, entry.Key, hash);
                if (!File.Exists(target))
                {
                    string temporary = Path.Combine(segmentsPath, ".pending-" + Guid.NewGuid().ToString("N"));
                    try
                    {
                        using (var output = new FileStream(temporary, FileMode.CreateNew,
                            FileAccess.Write, FileShare.None))
                        {
                            output.Write(payload, 0, payload.Length);
                            output.Flush(true);
                        }
                        File.Move(temporary, target);
                        Interlocked.Increment(ref newFiles);
                    }
                    finally { TryDelete(temporary); }
                }
                else if (!string.Equals(Sha(target), hash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Нарушена целостность сегмента: " + target);
            };

            // Записываются только независимые файлы; изменять общий HEAD из потоков нельзя.
            try
            {
                if (parallelWrite)
                {
                    var writeOptions = new ParallelOptions {
                        MaxDegreeOfParallelism = Math.Min(MaxParallelWrites, Environment.ProcessorCount)
                    };
                    Parallel.ForEach(changes, writeOptions, writeOne);
                }
                else
                    foreach (var entry in changes) writeOne(entry);
            }
            catch (AggregateException error)
            {
                AggregateException flattened = error.Flatten();
                if (flattened.InnerExceptions.Count == 1)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(flattened.InnerExceptions[0]).Throw();
                throw;
            }

            // ТОЛЬКО после успешной записи всех файлов обновляем снимок HEAD в памяти.
            // Если хотя бы один поток завершился ошибкой, вызывающий код HEAD не публикует.
            foreach (var entry in changes)
            {
                head.Segments[entry.Key] = entry.Value.Hash;
                publishedSegments++;
                publishedBytes += entry.Value.Payload.Length;
                changed = true;
            }
            if (writeTimer != null)
                TaskStore.ReportCompactTiming("segment-write changed-segments=" + changes.Count +
                    " new-files=" + newFiles + " parallel=" + (parallelWrite ? 1 : 0) +
                    " write-ms=" + writeTimer.ElapsedMilliseconds);
            return changed;
        }

        /// <summary>Публикует манифест с контрольной суммой, сохраняя предыдущий для отката.</summary>
        private static void WriteHead(string root, Head head)
        {
            WriteHead(root, head, false);
        }

        /// <summary>Публикует HEAD и при восстановлении отдельно сохраняет повреждённый исходник.</summary>
        private static void WriteHead(string root, Head head, bool recovering)
        {
            Directory.CreateDirectory(root);
            string destination = HeadPath(root);
            string temporary = Path.Combine(root, ".HEAD.pending-" + Guid.NewGuid().ToString("N"));
            try
            {
                byte[] data;
                using (var buffer = new MemoryStream())
                using (var writer = new BinaryWriter(buffer, Encoding.UTF8, true))
                {
                    writer.Write(HeadMagic);
                    writer.Write(HeadVersion);
                    writer.Write(head.Generation);
                    writer.Write(head.RangeSize);
                    writer.Write(head.LegacyHash);
                    writer.Write(head.Segments.Count);
                    foreach (var segment in head.Segments)
                    {
                        writer.Write(segment.Key);
                        writer.Write(segment.Value);
                    }
                    writer.Flush();
                    byte[] payload = buffer.ToArray();
                    using (var sha = SHA256.Create())
                    {
                        byte[] hash = sha.ComputeHash(payload);
                        data = new byte[payload.Length + hash.Length];
                        Buffer.BlockCopy(payload, 0, data, 0, payload.Length);
                        Buffer.BlockCopy(hash, 0, data, payload.Length, hash.Length);
                    }
                }
                using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    output.Write(data, 0, data.Length);
                    output.Flush(true);
                }
                if (File.Exists(destination))
                {
                    string backups = Path.Combine(root, "history");
                    Directory.CreateDirectory(backups);
                    File.Replace(temporary, destination, Path.Combine(backups,
                        (recovering ? "damaged-HEAD-" : "HEAD-") + DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff", CultureInfo.InvariantCulture) +
                        "-" + Guid.NewGuid().ToString("N")));
                }
                else File.Move(temporary, destination);
            }
            finally { TryDelete(temporary); }
        }

        /// <summary>Читает текущий HEAD, применяя общую проверку формата и контрольной суммы.</summary>
        private static Head ReadHead(string root)
        {
            return ReadHeadFile(HeadPath(root));
        }

        /// <summary>Читает текущий или резервный манифест с одинаковой полной проверкой.</summary>
        private static Head ReadHeadFile(string path)
        {
            byte[] bytes;
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            {
                if (file.Length < 60 || file.Length > 10000000)
                    throw new InvalidDataException("Манифест пустой или слишком большой");
                bytes = new byte[(int)file.Length];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    int n = file.Read(bytes, offset, bytes.Length - offset);
                    if (n == 0) throw new EndOfStreamException("Манифест неполный");
                    offset += n;
                }
            }
            if (bytes.Length < 32 + 28 || bytes.Length > 10000000)
                throw new InvalidDataException("Манифест пустой или слишком большой");
            using (var sha = SHA256.Create())
            {
                byte[] checksum = sha.ComputeHash(bytes, 0, bytes.Length - 32);
                for (int i = 0; i < 32; i++)
                    if (bytes[bytes.Length - 32 + i] != checksum[i])
                        throw new InvalidDataException("Нарушена контрольная сумма манифеста");
            }
            using (var input = new MemoryStream(bytes, 0, bytes.Length - 32, false))
            using (var reader = new BinaryReader(input))
            {
                if (reader.ReadInt32() != HeadMagic || reader.ReadInt32() != HeadVersion)
                    throw new InvalidDataException("Неизвестный формат манифеста");
                var result = new Head { Generation = reader.ReadInt64(), RangeSize = reader.ReadInt32(), LegacyHash = reader.ReadString() };
                if (result.RangeSize < 32 || result.RangeSize > 512)
                    throw new InvalidDataException("Некорректный размер сегмента в манифесте");
                int count = reader.ReadInt32();
                if (count < 0 || count > 100000)
                    throw new InvalidDataException("Некорректное число сегментов");
                for (int i = 0; i < count; i++)
                    result.Segments.Add(reader.ReadInt64(), reader.ReadString());
                if (input.Position != input.Length)
                    throw new InvalidDataException("Лишние данные в манифесте");
                return result;
            }
        }

        /// <summary>
        /// Независимо от кодека сверяет все поля модели Task и значимое состояние дерева.
        /// Неизвестные метки и нестандартные параметры TreeNode блокируют миграцию.
        /// </summary>
        private static void VerifyEquivalent(ListTasks original, ListTasks restored)
        {
            if (original.Tasks.Count != restored.Tasks.Count)
                throw new InvalidDataException("Число задач не совпадает после преобразования");
            for (int i = 0; i < original.Tasks.Count; i++)
            {
                Task a = original.Tasks[i];
                Task b = restored.Tasks[i];
                if (a.Number != b.Number || a.Status != b.Status ||
                    a.Name != b.Name || a.PersonFamaly != b.PersonFamaly ||
                    a.TaskNumberAfter != b.TaskNumberAfter ||
                    a.DateStart.Ticks != b.DateStart.Ticks || a.DateStart.Kind != b.DateStart.Kind ||
                    a.DateFinish.Ticks != b.DateFinish.Ticks || a.DateFinish.Kind != b.DateFinish.Kind ||
                    a.CountDays != b.CountDays || a.CountWorkingDays != b.CountWorkingDays ||
                    a.Priority != b.Priority || a.PlaceInSynchTask != b.PlaceInSynchTask ||
                    a.Color != b.Color)
                    throw new InvalidDataException("Отличаются основные поля задачи № " + a.Number);
                var ap = a.PoolProjects == null ? null : a.PoolProjects.Projects;
                var bp = b.PoolProjects == null ? null : b.PoolProjects.Projects;
                if ((ap == null) != (bp == null) || (ap != null && ap.Count != bp.Count))
                    throw new InvalidDataException("Отличается список проектов задачи № " + a.Number);
                if (ap != null)
                    for (int j = 0; j < ap.Count; j++)
                        if ((ap[j] == null) != (bp[j] == null) ||
                            (ap[j] != null && (ap[j].Name != bp[j].Name || ap[j].Address != bp[j].Address)))
                            throw new InvalidDataException("Отличается проект задачи № " + a.Number);
                TreeProjects at = a.GetTreeProjects();
                TreeProjects bt = b.GetTreeProjects();
                if (at.GetName() != bt.GetName() ||
                    (at.ListTreeNode == null) != (bt.ListTreeNode == null) ||
                    (at.ListTreeNode != null && at.ListTreeNode.Count != bt.ListTreeNode.Count))
                    throw new InvalidDataException("Отличается дерево задачи № " + a.Number);
                if (at.ListTreeNode != null)
                    for (int j = 0; j < at.ListTreeNode.Count; j++)
                        VerifyNode(at.ListTreeNode[j], bt.ListTreeNode[j], a.Number);
            }
        }

        /// <summary>Сравнивает содержимое и иерархию узлов, отказывая при непереносимых свойствах.</summary>
        private static void VerifyNode(TreeNode a, TreeNode b, long taskNumber)
        {
            if (a == null || b == null || a.Text != b.Text || a.Name != b.Name ||
                a.ToolTipText != b.ToolTipText || a.Checked != b.Checked ||
                a.ImageIndex != b.ImageIndex || a.ImageKey != b.ImageKey ||
                a.SelectedImageIndex != b.SelectedImageIndex ||
                a.SelectedImageKey != b.SelectedImageKey ||
                a.StateImageIndex != b.StateImageIndex || a.StateImageKey != b.StateImageKey ||
                a.ForeColor != b.ForeColor || a.BackColor != b.BackColor ||
                !object.Equals(a.NodeFont, b.NodeFont) || !TagsEqual(a.Tag, b.Tag) ||
                a.Nodes.Count != b.Nodes.Count)
                throw new InvalidDataException("При миграции изменилось дерево задачи № " + taskNumber);
            for (int i = 0; i < a.Nodes.Count; i++)
                VerifyNode(a.Nodes[i], b.Nodes[i], taskNumber);
        }

        /// <summary>Проверяет точное равенство поддерживаемых значений метки проекта.</summary>
        private static bool TagsEqual(object a, object b)
        {
            if (a == null || b == null) return a == b;
            if (a.GetType() != b.GetType()) return false;
            if (a is bool || a is string || a is int || a is long)
                return a.Equals(b);
            throw new InvalidDataException("Для TreeNode.Tag типа " + a.GetType().FullName +
                " требуется отдельная проверка совместимости перед миграцией");
        }

        /// <summary>Контрольная сумма старого Tasks.bin либо однозначная метка его отсутствия.</summary>
        private static string LegacyRevision(string file)
        {
            return File.Exists(file) ? Sha(file) : "absent";
        }

        /// <summary>SHA-256 готового сегмента в памяти, без повторной записи на диск.</summary>
        private static string Sha(byte[] payload)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(payload)).Replace("-", "");
        }

        /// <summary>SHA-256 опубликованного файла без модификации его содержимого.</summary>
        private static string Sha(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Sha(stream);
        }

        /// <summary>SHA-256 уже открытого файла; вызывающий код управляет временем закрытия дескриптора.</summary>
        private static string Sha(Stream stream)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        /// <summary>Удаляет только непубликованный временный файл, не изменяя основной снимок.</summary>
        private static void TryDelete(string file)
        {
            try { if (File.Exists(file)) File.Delete(file); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>Совместимая с .NET Framework 4.7.2 реализация добавления в словарь без замены значения.</summary>
    internal static class CompactDictionaryExtensions
    {
        /// <summary>Добавляет уникальную пару и возвращает false при повторном ключе.</summary>
        internal static bool TryAddCompat<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue value)
        {
            if (source.ContainsKey(key)) return false;
            source.Add(key, value);
            return true;
        }
    }
}
