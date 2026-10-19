using System;
using System.IO;
using System.Text;

namespace DeskFlow
{
    internal static class Log
    {
        private static readonly object Sync = new object();
        private static string _path;

        public static string LogPath
        {
            get
            {
                if (_path != null) return _path;
                try
                {
                    string dir = AppPaths.DataDirectory;
                    Directory.CreateDirectory(dir);
                    _path = Path.Combine(dir, "DeskFlow.log");
                }
                catch
                {
                    _path = Path.Combine(Path.GetTempPath(), "DeskFlow.log");
                }
                return _path;
            }
        }

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Warn(string message)
        {
            Write("WARN", message, null);
        }

        public static void Error(string message, Exception ex = null)
        {
            Write("ERROR", message, ex);
        }

        private static void Write(string level, string message, Exception ex)
        {
            try
            {
                lock (Sync)
                {
                    RotateIfNeeded();
                    StringBuilder line = new StringBuilder();
                    line.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    line.Append(" [").Append(level).Append("] ");
                    line.Append(message);
                    if (ex != null)
                    {
                        line.AppendLine();
                        line.Append(ex.ToString());
                    }
                    File.AppendAllText(LogPath, line.ToString() + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { }
        }

        private static void RotateIfNeeded()
        {
            try
            {
                FileInfo info = new FileInfo(LogPath);
                if (info.Exists && info.Length > 1024 * 1024)
                {
                    string old = LogPath + ".old";
                    if (File.Exists(old)) File.Delete(old);
                    File.Move(LogPath, old);
                }
            }
            catch { }
        }
    }
}
