// TS-01: исключительно синтетические данные. Запуск на КОПИИ, не на живом Tasks.bin.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using time_schedule;

internal static class CompactStorePreviewTests
{
    /// <summary>Число успешно пройденных синтетических проверок.</summary>
    private static int passed;

    /// <summary>Число неуспешных синтетических проверок.</summary>
    private static int failed;

    /// <summary>Проверяет обязательное условие сценария.</summary>
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    /// <summary>Выполняет независимую синтетическую проверку.</summary>
    private static void Run(string name, Action test)
    {
        try { test(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
    }

    /// <summary>Формирует задачу с многоуровневым деревом и проектами.</summary>
    private static time_schedule.Task NewTask(long id)
    {
        var t = new time_schedule.Task(TaskStatus.Active, "Synthetic task " + id,
            "Synthetic employee", id, 0,
            new DateTime(2025, 3, 1, 0, 0, 0, (id % 2 == 0 ? DateTimeKind.Local : DateTimeKind.Unspecified)),
            new DateTime(2025, 4, 1, 0, 0, 0, (id % 2 == 0 ? DateTimeKind.Local : DateTimeKind.Unspecified)),
            32, 23, Color.Blue, 7);
        t.PoolProjects.Add(new Project("Synthetic project", "P/1"));
        var root = new TreeNode("Synthetic parent")
        {
            Name = "parent", Checked = (id % 2) == 0, Tag = true,
            ToolTipText = "Help", ImageIndex = 2, SelectedImageIndex = 3
        };
        root.Nodes.Add(new TreeNode("Synthetic child") { Name = "child", Checked = (id % 3) == 0 });
        t.GetTreeProjects().SetName("Synthetic tree");
        t.GetTreeProjects().ListTreeNode.Add(root);
        return t;
    }

    /// <summary>Создаёт старый формат в искусственном тестовом каталоге.</summary>
    private static string Seed(string root, int count)
    {
        Directory.CreateDirectory(root);
        string file = Path.Combine(root, "Tasks.bin");
        var list = new ListTasks();
        for (int i = 1; i <= count; i++) list.AddTask(NewTask(i));
        using (var stream = new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            new BinaryFormatter().Serialize(stream, list);
        return file;
    }

    /// <summary>Хеширует входной файл для проверки неизменности его байтов после миграции.</summary>
    private static string Sha(string path)
    {
        using (var sha = SHA256.Create())
        using (var s = File.OpenRead(path))
            return Convert.ToBase64String(sha.ComputeHash(s));
    }

    /// <summary>Проверяет создание сегментов, неизменность старого файла и полное чтение.</summary>
    private static void TestMigration(string root, int count)
    {
        string file = Seed(root, count);
        string initial = Sha(file);
        string store = Path.Combine(root, "compact-preview");
        CompactMigrationResult stats = CompactTaskStore.BuildPreview(file, store);
        Check(initial == Sha(file), "Legacy Tasks.bin modified");
        Check(stats.TaskCount == count, "Task count");
        Check(stats.SegmentCount == (count + CompactTaskStore.SegmentSize - 1) / CompactTaskStore.SegmentSize,
            "Number of segments");
        var data = CompactTaskStore.ReadPreview(file, store);
        Check(data.Tasks.Count == count, "Roundtrip count");
        for (int i = 0; i < count; i++)
        {
            Check(data.Tasks[i].Number == i + 1, "Roundtrip ID");
            Check(data.Tasks[i].Color == Color.Blue, "Preserve named color");
            Check(data.Tasks[i].DateStart.Kind == (i % 2 == 1 ? DateTimeKind.Local : DateTimeKind.Unspecified), "Preserve DateTimeKind");
            Check(data.Tasks[i].DateStart.Ticks == new DateTime(2025, 3, 1).Ticks, "Preserve local wall-clock date");
            Check(data.Tasks[i].GetTreeProjects().GetName() == "Synthetic tree", "Tree name");
            Check(data.Tasks[i].GetTreeProjects().ListTreeNode[0].Nodes.Count == 1, "Subtree");
            Check(data.Tasks[i].GetTreeProjects().ListTreeNode[0].Tag is bool, "Tag type");
        }
        Console.WriteLine("METRIC count=" + count + " legacyBytes=" + stats.LegacyBytes +
            " segmentBytes=" + stats.SegmentBytes + " segments=" + stats.SegmentCount +
            " legacyReadMs=" + stats.LegacyReadMs + " migrateMs=" + stats.MigrationMs +
            " parallelReadMs=" + stats.ParallelReadMs);
    }

    /// <summary>Проверяет сохранение разных задач и конфликт одной задачи.</summary>
    private static void TestUpdate(string root)
    {
        string file = Seed(root, 260);
        string store = Path.Combine(root, "compact-preview");
        CompactTaskStore.BuildPreview(file, store);
        string initial = Sha(file);
        var first = CompactTaskStore.ReadPreview(file, store);
        var expectedFirst = TaskStore.Capture(first, new long[] { 2 });
        var expectedSecond = TaskStore.Capture(first, new long[] { 258 });
        CompactTaskStore.UpdatePreview(file, store, list =>
            list.Tasks.First(t => t.Number == 2).SetTaskName("changed first"), expectedFirst);
        CompactTaskStore.UpdatePreview(file, store, list =>
            list.Tasks.First(t => t.Number == 258).SetTaskName("changed second"), expectedSecond);
        var after = CompactTaskStore.ReadPreview(file, store);
        Check(after.Tasks.First(t => t.Number == 2).Name == "changed first", "First change retained");
        Check(after.Tasks.First(t => t.Number == 258).Name == "changed second", "Second change retained");
        Check(initial == Sha(file), "Original legacy remains unchanged after update");
        bool conflict = false;
        try
        {
            CompactTaskStore.UpdatePreview(file, store, list =>
                list.Tasks.First(t => t.Number == 2).SetTaskName("stale change"), expectedFirst);
        }
        catch (TaskConflictException) { conflict = true; }
        Check(conflict, "Stale change must conflict");
        Check(CompactTaskStore.ReadPreview(file, store).Tasks.First(t => t.Number == 2).Name == "changed first",
            "Conflict must not destroy prior data");
    }

    /// <summary>Обнаруживает изменение старого Tasks.bin другим несовместимым клиентом.</summary>
    private static void TestLegacyChanged(string root)
    {
        string file = Seed(root, 3);
        string store = Path.Combine(root, "compact-preview");
        CompactTaskStore.BuildPreview(file, store);
        using (var s = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.None)) s.WriteByte(0);
        bool detected = false;
        try { CompactTaskStore.ReadPreview(file, store); }
        catch (IOException) { detected = true; }
        Check(detected, "Must reject conflicting changes by old client");
    }

    /// <summary>Проверяет отказ при повреждении опубликованного сегмента.</summary>
    private static void TestCorruption(string root)
    {
        string file = Seed(root, 2);
        string store = Path.Combine(root, "compact-preview");
        CompactTaskStore.BuildPreview(file, store);
        string segment = Directory.GetFiles(Path.Combine(store, "segments"), "*.tseg").Single();
        using (var stream = new FileStream(segment, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            stream.Position = stream.Length - 1;
            int old = stream.ReadByte();
            stream.Position = stream.Length - 1;
            stream.WriteByte((byte)(old ^ 0xFF));
        }
        bool detected = false;
        try { CompactTaskStore.ReadPreview(file, store); }
        catch (Exception ex) when (ex is System.AggregateException || ex is InvalidDataException) { detected = true; }
        Check(detected, "Damaged segment must be rejected");
    }

    /// <summary>Независимый процесс, изменяющий только выбранную синтетическую задачу.</summary>
    private static int Worker(string[] args)
    {
        string file = args[1];
        string store = args[2];
        long id = long.Parse(args[3]);
        var start = CompactTaskStore.ReadPreview(file, store);
        var expected = TaskStore.Capture(start, new long[] { id });
        CompactTaskStore.UpdatePreview(file, store, list =>
            list.Tasks.First(t => t.Number == id).SetTaskName("worker change " + id), expected);
        return 0;
    }

    /// <summary>Запускает два независимых дочерних процесса, использующих общую папку.</summary>
    private static void TestTwoProcesses(string root)
    {
        string file = Seed(root, 130);
        string store = Path.Combine(root, "compact-preview");
        CompactTaskStore.BuildPreview(file, store);
        string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var a = Process.Start(new ProcessStartInfo(executable, "--worker \"" + file + "\" \"" + store + "\" 1")
            { UseShellExecute = false, CreateNoWindow = true });
        var b = Process.Start(new ProcessStartInfo(executable, "--worker \"" + file + "\" \"" + store + "\" 130")
            { UseShellExecute = false, CreateNoWindow = true });
        Check(a.WaitForExit(30000) && b.WaitForExit(30000), "Worker timeout");
        Check(a.ExitCode == 0 && b.ExitCode == 0, "Worker failed");
        var after = CompactTaskStore.ReadPreview(file, store);
        Check(after.Tasks.Single(t => t.Number == 1).Name == "worker change 1", "Worker 1 lost");
        Check(after.Tasks.Single(t => t.Number == 130).Name == "worker change 130", "Worker 2 lost");
    }

    /// <summary>Выполняет всё на искусственных данных под системным временным каталогом.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--preview-copy")
        {
            try
            {
                int size = args.Length >= 4 ? int.Parse(args[3]) : CompactTaskStore.SegmentSize;
                var stats = CompactTaskStore.BuildPreview(args[1], args[2], size);
                Console.WriteLine("PRIVATE_COPY_OK segmentRange=" + size + " count=" + stats.TaskCount +
                    " segments=" + stats.SegmentCount +
                    " legacyBytes=" + stats.LegacyBytes +
                    " compactBytes=" + stats.SegmentBytes +
                    " legacyReadMs=" + stats.LegacyReadMs +
                    " convertMs=" + stats.MigrationMs +
                    " parallelReadMs=" + stats.ParallelReadMs);
                return 0;
            }
            catch (Exception ex) { Console.Error.WriteLine("PRIVATE_COPY_FAILED: " + ex); return 3; }
        }
        if (args.Length > 0 && args[0] == "--worker")
        {
            try { return Worker(args); }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 2; }
        }
        string root = Path.Combine(Path.GetTempPath(), "TimeSchedule-CompactPreview-SYNTHETIC-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Console.WriteLine("TEMP SYNTHETIC TEST ROOT: " + root);
        Run("compact-10", () => TestMigration(Path.Combine(root, "ten"), 10));
        Run("compact-130", () => TestMigration(Path.Combine(root, "hundredthirty"), 130));
        Run("compact-936", () => TestMigration(Path.Combine(root, "ninehundred"), 936));
        Run("independent-tasks-and-conflict", () => TestUpdate(Path.Combine(root, "changes")));
        Run("legacy-client-conflict", () => TestLegacyChanged(Path.Combine(root, "legacyChanged")));
        Run("corrupt-segment", () => TestCorruption(Path.Combine(root, "corruption")));
        Run("two-independent-processes", () => TestTwoProcesses(Path.Combine(root, "processes")));
        Console.WriteLine("TOTAL passed=" + passed + " failed=" + failed);
        return failed == 0 ? 0 : 1;
    }
}
