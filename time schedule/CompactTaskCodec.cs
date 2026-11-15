using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace time_schedule
{
    /// <summary>Задача и её положение в исходном списке, независимое от сегмента.</summary>
    internal sealed class CompactTaskRecord
    {
        /// <summary>Позиция задачи при чтении исходного ListTasks.</summary>
        internal int Ordinal { get; set; }

        /// <summary>Задача, восстановленная без изменения сериализуемой модели приложения.</summary>
        internal Task Item { get; set; }

        /// <summary>SHA-256 исходной компактной записи, включая позицию; только для выборочного сохранения.</summary>
        internal string SourceHash { get; set; }
    }

    /// <summary>
    /// Компактный потоковый формат одного сегмента. Не изменяет BinaryFormatter-контракт
    /// старого Tasks.bin и не сериализует элементы WinForms как объектный граф.
    /// </summary>
    internal static class CompactTaskCodec
    {
        /// <summary>Идентификатор формата сегмента.</summary>
        private const int Magic = 0x31534754; // TGS1, little-endian

        /// <summary>Текущая версия компактного формата.</summary>
        private const int Version = 1;

        /// <summary>Ограничение числа узлов в одной задаче для защиты от ошибочного ввода.</summary>
        private const int MaxNodesPerTask = 100000;

        /// <summary>Ограничение вложенности дерева во избежание переполнения стека.</summary>
        private const int MaxDepth = 512;

        /// <summary>Коды типов произвольного TreeNode.Tag.</summary>
        private enum TagKind : byte { Null, Boolean, String, Int32, Int64, LegacyObject }

        /// <summary>Коды представления System.Drawing.Color.</summary>
        private enum ColorKind : byte { Empty, Known, Named, Argb }

        /// <summary>Записывает один сегмент целиком, с завершением и flush файла перед возвратом.</summary>
        internal static void Write(string path, IList<CompactTaskRecord> records)
        {
            using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (var header = new BinaryWriter(output, System.Text.Encoding.UTF8, true))
                {
                    header.Write(Magic);
                    header.Write(Version);
                }
                using (var compression = new DeflateStream(output, CompressionLevel.Fastest, true))
                using (var writer = new BinaryWriter(compression, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(records.Count);
                    foreach (CompactTaskRecord record in records)
                    {
                        writer.Write(record.Ordinal);
                        WriteTask(writer, record.Item);
                    }
                }
                output.Flush(true);
            }
        }
        /// <summary>
        /// Сериализует сегмент в память в том же компактном формате,
        /// который используется при записи сегмента в файл.
        /// </summary>
        internal static byte[] Encode(IList<CompactTaskRecord> records)
        {
            using (var output = new MemoryStream())
            {
                using (var header = new BinaryWriter(
                    output, System.Text.Encoding.UTF8, true))
                {
                    header.Write(Magic);
                    header.Write(Version);
                }

                using (var compression = new DeflateStream(
                    output, CompressionLevel.Fastest, true))
                using (var writer = new BinaryWriter(
                    compression, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(records.Count);

                    foreach (CompactTaskRecord record in records)
                    {
                        writer.Write(record.Ordinal);
                        WriteTask(writer, record.Item);
                    }
                }

                return output.ToArray();
            }
        }

        /// <summary>Читает компактный сегмент в штатном режиме, без дополнительных отпечатков записей.</summary>
        internal static List<CompactTaskRecord> Read(string path, int maximumTasks)
        {
            return Read(path, maximumTasks, false);
        }

        /// <summary>
        /// При выборочном сохранении вместе с задачами получает SHA-256 исходных
        /// распакованных записей. Дополнительной сериализации при чтении нет.
        /// </summary>
        internal static List<CompactTaskRecord> Read(string path, int maximumTasks, bool captureSourceHashes)
        {
            using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var header = new BinaryReader(input, System.Text.Encoding.UTF8, true))
            {
                if (header.ReadInt32() != Magic || header.ReadInt32() != Version)
                    throw new InvalidDataException("Неподдерживаемый формат сегмента: " + path);
                using (var compression = new DeflateStream(input, CompressionMode.Decompress, true))
                using (var hashing = captureSourceHashes ? new RecordHashingStream(compression) : null)
                using (var reader = new BinaryReader(captureSourceHashes ? (Stream)hashing : compression,
                    System.Text.Encoding.UTF8, true))
                {
                    int count = reader.ReadInt32();
                    if (count < 0 || count > maximumTasks)
                        throw new InvalidDataException("Некорректное число задач в сегменте: " + path);
                    var result = new List<CompactTaskRecord>(count);
                    for (int i = 0; i < count; i++)
                    {
                        if (hashing != null) hashing.BeginRecord();
                        int ordinal = reader.ReadInt32();
                        if (ordinal < 0)
                            throw new InvalidDataException("Отрицательный индекс задачи: " + path);
                        Task task = ReadTask(reader);
                        result.Add(new CompactTaskRecord {
                            Ordinal = ordinal,
                            Item = task,
                            SourceHash = hashing == null ? null : hashing.EndRecord()
                        });
                    }
                    if ((hashing == null ? compression : (Stream)hashing).ReadByte() != -1)
                        throw new InvalidDataException("Лишние данные в сегменте: " + path);
                    return result;
                }
            }
        }

        /// <summary>
        /// Вычисляет SHA-256 точно по байтам, прочитанным BinaryReader из потока
        /// отдельной записи. Не буферизует целый сегмент и не изменяет его формат.
        /// </summary>
        private sealed class RecordHashingStream : Stream
        {
            /// <summary>Распакованный поток сегмента, доступный только для чтения.</summary>
            private readonly Stream source;

            /// <summary>Инкрементальный SHA-256 для текущей записи.</summary>
            private readonly IncrementalHash recordHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            /// <summary>Буфер для хеширования одиночного байта.</summary>
            private readonly byte[] singleByte = new byte[1];

            /// <summary>Признак чтения текущей записи, а не заголовка сегмента.</summary>
            private bool active;

            /// <summary>Оборачивает поток без передачи права владения его дескриптором.</summary>
            internal RecordHashingStream(Stream source) { this.source = source; }

            /// <summary>Начинает фиксацию байтов следующей записи.</summary>
            internal void BeginRecord() { active = true; }

            /// <summary>Завершает запись и возвращает её SHA-256.</summary>
            internal string EndRecord()
            {
                active = false;
                return BitConverter.ToString(recordHash.GetHashAndReset()).Replace("-", "");
            }

            /// <summary>Разрешает чтение из базового потока.</summary>
            public override bool CanRead { get { return source.CanRead; } }

            /// <summary>Произвольное позиционирование не поддерживается DeflateStream.</summary>
            public override bool CanSeek { get { return false; } }

            /// <summary>Запись в поток чтения запрещена.</summary>
            public override bool CanWrite { get { return false; } }

            /// <summary>Длина распакованного потока заранее неизвестна.</summary>
            public override long Length { get { throw new NotSupportedException(); } }

            /// <summary>Позиционирование распакованного потока не поддерживается.</summary>
            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            /// <summary>Читает и учитывает байты текущей записи.</summary>
            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = source.Read(buffer, offset, count);
                if (active && read > 0) recordHash.AppendData(buffer, offset, read);
                return read;
            }

            /// <summary>Читает один байт и учитывает его в SHA-256 текущей записи.</summary>
            public override int ReadByte()
            {
                int value = source.ReadByte();
                if (active && value >= 0)
                {
                    singleByte[0] = (byte)value;
                    recordHash.AppendData(singleByte);
                }
                return value;
            }

            /// <summary>Запись запрещена.</summary>
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            /// <summary>Позиционирование запрещено.</summary>
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }

            /// <summary>Изменение длины запрещено.</summary>
            public override void SetLength(long value) { throw new NotSupportedException(); }

            /// <summary>Сброс записи неприменим к потоку чтения.</summary>
            public override void Flush() { throw new NotSupportedException(); }

            /// <summary>Освобождает SHA-256; базовый DeflateStream закрывает вызывающий код.</summary>
            protected override void Dispose(bool disposing)
            {
                if (disposing) recordHash.Dispose();
                base.Dispose(disposing);
            }
        }

        /// <summary>Вычисляет полный отпечаток компактного представления, включая порядок и дерево.</summary>
        internal static string RecordHash(Task task, int ordinal)
        {
            using (var data = new MemoryStream())
            using (var writer = new BinaryWriter(data, System.Text.Encoding.UTF8, true))
            {
                writer.Write(ordinal);
                WriteTask(writer, task);
                writer.Flush();
                using (var sha = System.Security.Cryptography.SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(data.GetBuffer(), 0, (int)data.Length)).Replace("-", "");
            }
        }

        /// <summary>Записывает все сохраняемые поля Task, не включая UI-граф BinaryFormatter.</summary>
        private static void WriteTask(BinaryWriter w, Task task)
        {
            w.Write((int)task.Status);
            WriteNullableString(w, task.Name);
            w.Write(task.Number);
            WriteNullableString(w, task.PersonFamaly);
            w.Write(task.TaskNumberAfter);
            w.Write(task.DateStart.Ticks);
            w.Write((byte)task.DateStart.Kind);
            w.Write(task.DateFinish.Ticks);
            w.Write((byte)task.DateFinish.Kind);
            w.Write(task.CountDays);
            w.Write(task.CountWorkingDays);
            WriteColor(w, task.Color);
            w.Write(task.Priority);
            w.Write(task.PlaceInSynchTask);

            List<Project> projects = task.PoolProjects == null ? null : task.PoolProjects.Projects;
            w.Write(projects != null);
            if (projects != null)
            {
                w.Write(projects.Count);
                foreach (Project p in projects)
                {
                    if (p == null) throw new InvalidDataException("Нулевой проект в задаче " + task.Number);
                    WriteNullableString(w, p.Name);
                    WriteNullableString(w, p.Address);
                }
            }

            TreeProjects tree = task.GetTreeProjects();
            WriteNullableString(w, tree.GetName());
            IList<TreeNode> roots = tree.ListTreeNode;
            w.Write(roots != null);
            if (roots == null) return;
            w.Write(roots.Count);
            int nodeCount = 0;
            foreach (TreeNode node in roots)
                WriteNode(w, node, 0, ref nodeCount);
        }

        /// <summary>Восстанавливает исходную публичную модель Task со всеми полями и деревом.</summary>
        private static Task ReadTask(BinaryReader r)
        {
            TaskStatus status = (TaskStatus)r.ReadInt32();
            string name = ReadNullableString(r);
            long number = r.ReadInt64();
            string person = ReadNullableString(r);
            long after = r.ReadInt64();
            DateTime start = new DateTime(r.ReadInt64(), ReadDateKind(r));
            DateTime finish = new DateTime(r.ReadInt64(), ReadDateKind(r));
            long days = r.ReadInt64();
            long workDays = r.ReadInt64();
            Color color = ReadColor(r);
            long priority = r.ReadInt64();
            int place = r.ReadInt32();

            var task = new Task(status, name, person, number, after, start, finish, days, workDays, color, priority);
            task.SetPlaceInSynhTask(place);
            bool hasProjects = r.ReadBoolean();
            if (hasProjects)
            {
                int count = ReadCount(r, 100000, "проектов");
                for (int i = 0; i < count; i++)
                    task.PoolProjects.Add(new Project(ReadNullableString(r), ReadNullableString(r)));
            }
            string treeName = ReadNullableString(r);
            TreeProjects tree = task.GetTreeProjects();
            tree.SetName(treeName);
            if (r.ReadBoolean())
            {
                int count = ReadCount(r, MaxNodesPerTask, "корней дерева");
                int nodes = 0;
                for (int i = 0; i < count; i++)
                    tree.ListTreeNode.Add(ReadNode(r, 0, ref nodes));
            }
            return task;
        }

        /// <summary>Записывает значимые сериализуемые свойства TreeNode в исходном порядке обхода.</summary>
        private static void WriteNode(BinaryWriter w, TreeNode node, int depth, ref int nodes)
        {
            if (node == null || ++nodes > MaxNodesPerTask || depth > MaxDepth)
                throw new InvalidDataException("Некорректное дерево проектов");
            WriteNullableString(w, node.Text);
            WriteNullableString(w, node.ToolTipText);
            WriteNullableString(w, node.Name);
            w.Write(node.Checked);
            w.Write(node.ImageIndex);
            WriteNullableString(w, node.ImageKey);
            w.Write(node.SelectedImageIndex);
            WriteNullableString(w, node.SelectedImageKey);
            WriteTag(w, node.Tag);
            w.Write(node.Nodes.Count);
            foreach (TreeNode child in node.Nodes)
                WriteNode(w, child, depth + 1, ref nodes);
        }

        /// <summary>Восстанавливает дерево, не прикреплённое к WinForms TreeView.</summary>
        private static TreeNode ReadNode(BinaryReader r, int depth, ref int nodes)
        {
            if (++nodes > MaxNodesPerTask || depth > MaxDepth)
                throw new InvalidDataException("Слишком большое дерево проектов");
            string text = ReadNullableString(r);
            string tip = ReadNullableString(r);
            string name = ReadNullableString(r);
            bool check = r.ReadBoolean();
            int image = r.ReadInt32();
            string imageKey = ReadNullableString(r);
            int selectedImage = r.ReadInt32();
            string selectedImageKey = ReadNullableString(r);
            object tag = ReadTag(r);
            int childCount = ReadCount(r, MaxNodesPerTask, "дочерних узлов");
            var node = new TreeNode(text) { Name = name, Checked = check, ToolTipText = tip, Tag = tag };
            node.ImageIndex = image;
            if (!string.IsNullOrEmpty(imageKey)) node.ImageKey = imageKey;
            node.SelectedImageIndex = selectedImage;
            if (!string.IsNullOrEmpty(selectedImageKey)) node.SelectedImageKey = selectedImageKey;
            for (int i = 0; i < childCount; i++)
                node.Nodes.Add(ReadNode(r, depth + 1, ref nodes));
            return node;
        }

        /// <summary>Сохраняет тип TreeNode.Tag; редкие сериализуемые пользовательские типы имеют fallback.</summary>
        private static void WriteTag(BinaryWriter w, object tag)
        {
            if (tag == null) { w.Write((byte)TagKind.Null); return; }
            if (tag is bool) { w.Write((byte)TagKind.Boolean); w.Write((bool)tag); return; }
            if (tag is string) { w.Write((byte)TagKind.String); w.Write((string)tag); return; }
            if (tag is int) { w.Write((byte)TagKind.Int32); w.Write((int)tag); return; }
            if (tag is long) { w.Write((byte)TagKind.Int64); w.Write((long)tag); return; }
            if (!tag.GetType().IsSerializable)
                throw new InvalidDataException("TreeNode.Tag содержит несериализуемый тип: " + tag.GetType().FullName);
            using (var buffer = new MemoryStream())
            {
                new BinaryFormatter().Serialize(buffer, tag);
                if (buffer.Length > 1048576) throw new InvalidDataException("Слишком большой TreeNode.Tag");
                w.Write((byte)TagKind.LegacyObject);
                w.Write((int)buffer.Length);
                w.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
            }
        }

        /// <summary>Восстанавливает исходный тип метки, применяя BinaryFormatter только для legacy-объекта.</summary>
        private static object ReadTag(BinaryReader r)
        {
            switch ((TagKind)r.ReadByte())
            {
                case TagKind.Null: return null;
                case TagKind.Boolean: return r.ReadBoolean();
                case TagKind.String: return r.ReadString();
                case TagKind.Int32: return r.ReadInt32();
                case TagKind.Int64: return r.ReadInt64();
                case TagKind.LegacyObject:
                    int count = ReadCount(r, 1048576, "размера метки");
                    byte[] bytes = r.ReadBytes(count);
                    if (bytes.Length != count) throw new EndOfStreamException();
                    using (var stream = new MemoryStream(bytes, false))
                        return new BinaryFormatter().Deserialize(stream);
                default: throw new InvalidDataException("Неизвестный тип TreeNode.Tag");
            }
        }

        /// <summary>Сохраняет значение Color вместе с различием известных и именованных цветов.</summary>
        private static void WriteColor(BinaryWriter w, Color color)
        {
            if (color.IsEmpty) { w.Write((byte)ColorKind.Empty); return; }
            if (color.IsKnownColor) { w.Write((byte)ColorKind.Known); w.Write((int)color.ToKnownColor()); return; }
            if (color.IsNamedColor) { w.Write((byte)ColorKind.Named); w.Write(color.Name); return; }
            w.Write((byte)ColorKind.Argb);
            w.Write(color.ToArgb());
        }

        /// <summary>Проверяет DateTimeKind без преобразования календарной даты через другой часовой пояс.</summary>
        private static DateTimeKind ReadDateKind(BinaryReader r)
        {
            byte kind = r.ReadByte();
            if (kind > 2) throw new InvalidDataException("Неизвестный DateTimeKind");
            return (DateTimeKind)kind;
        }

        /// <summary>Восстанавливает значение Color без потери статуса известного цвета.</summary>
        private static Color ReadColor(BinaryReader r)
        {
            switch ((ColorKind)r.ReadByte())
            {
                case ColorKind.Empty: return Color.Empty;
                case ColorKind.Known: return Color.FromKnownColor((KnownColor)r.ReadInt32());
                case ColorKind.Named: return Color.FromName(r.ReadString());
                case ColorKind.Argb: return Color.FromArgb(r.ReadInt32());
                default: throw new InvalidDataException("Неизвестное представление цвета");
            }
        }

        /// <summary>Пишет строку без потери различия между null и пустым значением.</summary>
        private static void WriteNullableString(BinaryWriter w, string value)
        {
            w.Write(value != null);
            if (value != null) w.Write(value);
        }

        /// <summary>Читает nullable-строку.</summary>
        private static string ReadNullableString(BinaryReader r)
        {
            return r.ReadBoolean() ? r.ReadString() : null;
        }

        /// <summary>Проверяет количество элементов перед выделением памяти.</summary>
        private static int ReadCount(BinaryReader r, int maximum, string label)
        {
            int result = r.ReadInt32();
            if (result < 0 || result > maximum)
                throw new InvalidDataException("Некорректное количество " + label + ": " + result);
            return result;
        }
    }
}
