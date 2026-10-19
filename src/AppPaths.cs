using System;
using System.IO;

namespace DeskFlow
{
    internal static class AppPaths
    {
        public static string OverrideDirectory;
        private static string _dataDirectory;
        private static string _settingsDirectory;

        public static string DataDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OverrideDirectory)) return OverrideDirectory;
                string env = Environment.GetEnvironmentVariable("DESKFLOW_DATA_DIR");
                if (!string.IsNullOrWhiteSpace(env)) return env;
                if (_dataDirectory != null) return _dataDirectory;

                string preferred = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskFlow");
                if (CanWrite(preferred))
                {
                    _dataDirectory = preferred;
                }
                else
                {
                    _dataDirectory = Path.Combine(Path.GetTempPath(), "DeskFlow");
                    Log.Warn("本地应用数据目录不可写，改用 " + _dataDirectory);
                }
                return _dataDirectory;
            }
        }

        public static string SettingsDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OverrideDirectory)) return OverrideDirectory;
                string env = Environment.GetEnvironmentVariable("DESKFLOW_DATA_DIR");
                if (!string.IsNullOrWhiteSpace(env)) return env;
                if (_settingsDirectory != null) return _settingsDirectory;

                string preferred = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeskFlow");
                if (CanWrite(preferred))
                {
                    _settingsDirectory = preferred;
                }
                else
                {
                    _settingsDirectory = DataDirectory;
                }
                return _settingsDirectory;
            }
        }

        private static bool CanWrite(string directory)
        {
            try
            {
                Directory.CreateDirectory(directory);
                string probe = Path.Combine(directory, ".write-test-" + Guid.NewGuid().ToString("N"));
                using (FileStream stream = new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.WriteByte(1);
                }
                try { File.Delete(probe); } catch { }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
