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

    /// <summary>
    /// Изолированная реализация сегментированного хранилища TS-01.
    /// Используется для испытаний на копии проекта; в рабочий TaskStore пока не подключена.
    /// </summary>
    public static class CompactTaskStore
    {
        /// <summary>Фиксированный диапазон номеров задач в сегменте.</summary>
        public const int SegmentSize = 128;

        /// <summary>Максимальное количество потоков загрузки.</summary>
        private const int MaxReaders = 8;

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

            /// <summary>Хеш старого Tasks.bin на момент миграции; используется для выявления старого клиента.</summary>
            internal string LegacyHash;

            /// <summary>Непустые опубликованные сегменты по номеру диапазона.</summary>
            internal SortedDictionary<long, string> Segments = new SortedDictionary<long, string>();
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
            WriteChangedSegments(testStoreDirectory, source, current, null, null);
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
            if (!string.Equals(Sha(legacyFile), current.LegacyHash, StringComparison.Ordinal))
                throw new IOException("После миграции исходный Tasks.bin изменён старым клиентом. Данные нельзя безопасно объединить автоматически.");
            var records = new ConcurrentBag<CompactTaskRecord>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Min(MaxReaders, Environment.ProcessorCount) };
            Parallel.ForEach(current.Segments, options, entry =>
            {
                string path = SegmentPath(storeDirectory, entry.Key, entry.Value);
                if (!string.Equals(Sha(path), entry.Value, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Повреждён сегмент задач: " + path);
                foreach (CompactTaskRecord record in CompactTaskCodec.Read(path, current.RangeSize))
                {
                    if (SegmentId(record.Item.Number, current.RangeSize) != entry.Key)
                        throw new InvalidDataException("Задача находится не в своём сегменте");
                    records.Add(record);
                }
            });
            var ordered = records.OrderBy(x => x.Ordinal).ToList();
            for (int i = 0; i < ordered.Count; i++)
                if (ordered[i].Ordinal != i)
                    throw new InvalidDataException("Повреждён порядок задач в манифесте");
            var numbers = new HashSet<long>();
            foreach (CompactTaskRecord r in ordered)
                if (!numbers.Add(r.Item.Number))
                    throw new InvalidDataException("Повторяющийся номер задачи: " + r.Item.Number);
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
                var beforeRecords = RecordHashes(before);
                change(before);
                var newHashes = Fingerprints(before);
                foreach (KeyValuePair<long, string> pair in oldHashes)
                {
                    string hash;
                    if ((!newHashes.TryGetValue(pair.Key, out hash) || hash != pair.Value) && !expected.ContainsKey(pair.Key))
                        throw new TaskConflictException("Изменение затронуло неподтверждённую задачу № " + pair.Key);
                }
                var afterRecords = RecordHashes(before);
                if (beforeRecords.Count == afterRecords.Count && beforeRecords.All(x =>
                    afterRecords.ContainsKey(x.Key) && afterRecords[x.Key] == x.Value)) return before;
                var next = new Head { Generation = checked(oldHead.Generation + 1), LegacyHash = oldHead.LegacyHash, RangeSize = oldHead.RangeSize };
                foreach (var entry in oldHead.Segments) next.Segments.Add(entry.Key, entry.Value);
                WriteChangedSegments(storeDirectory, before, next, beforeRecords, afterRecords);
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

        /// <summary>Формирует полный отпечаток компактной записи вместе с порядком задачи.</summary>
        private static Dictionary<long, string> RecordHashes(ListTasks tasks)
        {
            var result = new Dictionary<long, string>();
            for (int i = 0; i < tasks.Tasks.Count; i++)
            {
                Task t = tasks.Tasks[i];
                if (!result.TryAddCompat(t.Number, CompactTaskCodec.RecordHash(t, i)))
                    throw new InvalidDataException("Повторяющийся номер задачи: " + t.Number);
            }
            return result;
        }

        /// <summary>Создаёт новые версии только изменённых групп номеров и сохраняет нетронутые ссылки.</summary>
        private static void WriteChangedSegments(string root, ListTasks tasks, Head head,
            IDictionary<long, string> previousRecordHashes, IDictionary<long, string> newRecordHashes)
        {
            Directory.CreateDirectory(Path.Combine(root, "segments"));
            var all = tasks.Tasks.Select((t, index) => new CompactTaskRecord { Item = t, Ordinal = index })
                .GroupBy(x => SegmentId(x.Item.Number, head.RangeSize)).ToDictionary(x => x.Key, x => x.ToList());
            var changed = new HashSet<long>();
            if (previousRecordHashes == null)
            {
                foreach (long group in all.Keys) changed.Add(group);
            }
            else
            {
                foreach (var group in all)
                    foreach (CompactTaskRecord item in group.Value)
                    {
                        string oldHash;
                        if (!previousRecordHashes.TryGetValue(item.Item.Number, out oldHash) ||
                            oldHash != newRecordHashes[item.Item.Number])
                            changed.Add(group.Key);
                    }
                foreach (long deleted in previousRecordHashes.Keys.Where(x => !newRecordHashes.ContainsKey(x)))
                    changed.Add(SegmentId(deleted, head.RangeSize));
            }
            foreach (long id in changed)
            {
                List<CompactTaskRecord> segment;
                if (!all.TryGetValue(id, out segment)) { head.Segments.Remove(id); continue; }
                string temporary = Path.Combine(root, "segments", ".pending-" + Guid.NewGuid().ToString("N"));
                try
                {
                    CompactTaskCodec.Write(temporary, segment);
                    string hash = Sha(temporary);
                    string target = SegmentPath(root, id, hash);
                    if (!File.Exists(target)) File.Move(temporary, target);
                    else if (Sha(target) != hash) throw new InvalidDataException("Нарушена целостность сегмента: " + target);
                    head.Segments[id] = hash;
                }
                finally { TryDelete(temporary); }
            }
        }

        /// <summary>Публикует манифест с контрольной суммой, сохраняя предыдущий для отката.</summary>
        private static void WriteHead(string root, Head head)
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
                        "HEAD-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff", CultureInfo.InvariantCulture) +
                        "-" + Guid.NewGuid().ToString("N")));
                }
                else File.Move(temporary, destination);
            }
            finally { TryDelete(temporary); }
        }

        /// <summary>Проверяет полный манифест до обращения к его сегментам.</summary>
        private static Head ReadHead(string root)
        {
            byte[] bytes;
            using (var file = new FileStream(HeadPath(root), FileMode.Open, FileAccess.Read,
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

        /// <summary>Проверяет равенство полей исходных и восстановленных задач, включая порядок.</summary>
        private static void VerifyEquivalent(ListTasks original, ListTasks restored)
        {
            if (original.Tasks.Count != restored.Tasks.Count)
                throw new InvalidDataException("Число задач не совпадает после преобразования");
            for (int i = 0; i < original.Tasks.Count; i++)
            {
                Task a = original.Tasks[i];
                Task b = restored.Tasks[i];
                if (a.Number != b.Number ||
                    CompactTaskCodec.RecordHash(a, i) != CompactTaskCodec.RecordHash(b, i))
                    throw new InvalidDataException("После преобразования изменена задача № " + a.Number);
            }
        }

        /// <summary>SHA-256 опубликованного файла без модификации его содержимого.</summary>
        private static string Sha(string file)
        {
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
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
