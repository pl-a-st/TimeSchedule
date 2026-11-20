using System;
using System.Diagnostics;
using System.IO;

namespace time_schedule
{
    /// <summary>Хранит выбор темы отдельно для каждого пользователя, вне общего каталога задач.</summary>
    internal static class ThemePreferenceStore
    {
        /// <summary>Путь к локальному пользовательскому предпочтению.</summary>
        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeSchedule", "theme.txt");

        /// <summary>Читает сохранённую тему; при отсутствии настройки выбирает светлую.</summary>
        internal static ThemeKind Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return ThemeKind.Light;
                return string.Equals(File.ReadAllText(FilePath).Trim(), "Dark", StringComparison.OrdinalIgnoreCase)
                    ? ThemeKind.Dark : ThemeKind.Light;
            }
            catch (IOException ex) { Trace.WriteLine("Theme preference read: " + ex.Message); }
            catch (UnauthorizedAccessException ex) { Trace.WriteLine("Theme preference read: " + ex.Message); }
            return ThemeKind.Light;
        }

        /// <summary>Сохраняет тему только для текущего пользователя, не меняя файлы проекта.</summary>
        internal static void Save(ThemeKind kind)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, kind.ToString());
            }
            catch (IOException ex) { Trace.WriteLine("Theme preference write: " + ex.Message); }
            catch (UnauthorizedAccessException ex) { Trace.WriteLine("Theme preference write: " + ex.Message); }
        }
    }
}
