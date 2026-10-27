using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Diagnostics;
using System.Text;

namespace DeskFlow
{
    internal sealed class RuntimeProgress
    {
        public int Percent;
        public string Message;
    }

    internal static class Runtime
    {
#if SLIM_RUNTIME
        public const string PayloadVersion = "vlc-3.0.21-slim2";
#else
        public const string PayloadVersion = "vlc-3.0.21";
#endif
        public const string ResourceName = "DeskFlow.Payload.runtime.zip";

        private static string _baseDir;

        public static string DataDirectory
        {
            get { return AppPaths.DataDirectory; }
        }

        public static string RuntimeRoot
        {
            get { return Path.Combine(DataDirectory, "runtime"); }
        }

        public static string RuntimeDir
        {
            get { return Path.Combine(RuntimeRoot, PayloadVersion); }
        }

        public static string VlcDir
        {
            get { return Path.Combine(RuntimeDir, "vlc"); }
        }

        public static string PluginsDir
        {
            get { return Path.Combine(VlcDir, "plugins"); }
        }

        public static string WebpTool
        {
            get { return Path.Combine(RuntimeDir, "tools", "dwebp.exe"); }
        }

        public static string BaseDirectory
        {
            get
            {
                if (_baseDir == null)
                    _baseDir = AppDomain.CurrentDomain.BaseDirectory;
                return _baseDir;
            }
        }

        private static string ReadyMarker
        {
            get { return Path.Combine(RuntimeDir, ".ready"); }
        }

        public static bool IsReady
        {
            get
            {
                try
                {
                    string libvlc = Path.Combine(VlcDir, "libvlc.dll");
                    string core = Path.Combine(VlcDir, "libvlccore.dll");
                    if (!File.Exists(ReadyMarker)
                        || !File.Exists(libvlc)
                        || new FileInfo(libvlc).Length < 100 * 1024
                        || !File.Exists(core)
                        || new FileInfo(core).Length < 1024 * 1024
                        || !Directory.Exists(PluginsDir)
                        || !File.Exists(WebpTool))
                        return false;

                    // Detect a runtime copied by an administrator but unreadable
                    // to the current account.
                    using (FileStream stream = new FileStream(libvlc, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (stream.ReadByte() < 0) return false;
                    }
                    return true;
                }
                catch { return false; }
            }
        }

        public static bool Prepare(IProgress<RuntimeProgress> progress, out string error)
        {
            error = null;
            try
            {
                using (Stream resource = OpenPayload())
                {
                    if (resource == null)
                    {
                        error = "安装包中缺少播放引擎组件。请重新下载完整的 DeskFlow.exe。";
                        return false;
                    }

                    int processId = Process.GetCurrentProcess().Id;
                    string tempRoot = RuntimeDir + ".tmp-" + processId;
                    if (Directory.Exists(tempRoot))
                    {
                        try { Directory.Delete(tempRoot, true); } catch { }
                    }
                    Directory.CreateDirectory(tempRoot);

                    Report(progress, 1, "正在准备播放引擎…");
                    string tempZip = Path.Combine(Path.GetTempPath(), "DeskFlow-runtime-" + processId + ".zip");
                    using (FileStream file = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[128 * 1024];
                        long total = resource.Length;
                        long done = 0;
                        int read;
                        while ((read = resource.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            file.Write(buffer, 0, read);
                            done += read;
                            if (total > 0)
                                Report(progress, 1 + (int)(done * 8 / total), "正在准备播放引擎…");
                        }
                    }

                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(tempZip))
                        {
                            int count = archive.Entries.Count;
                            int index = 0;
                            string rootFull = Path.GetFullPath(tempRoot + Path.DirectorySeparatorChar);
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                index++;
                                if (string.IsNullOrEmpty(entry.Name))
                                {
                                    string dirPath = Path.GetFullPath(Path.Combine(tempRoot, entry.FullName));
                                    if (dirPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                                        Directory.CreateDirectory(dirPath);
                                }
                                else
                                {
                                    string destination = Path.GetFullPath(Path.Combine(tempRoot, entry.FullName));
                                    if (!destination.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                                        continue;
                                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                                    entry.ExtractToFile(destination, true);
                                }

                                if ((index % 12) == 0 || index == count)
                                {
                                    int pct = 9 + (int)((long)index * 88 / Math.Max(1, count));
                                    Report(progress, pct, "正在释放播放组件 " + index + " / " + count);
                                }
                            }
                        }
                    }
                    finally
                    {
                        try { File.Delete(tempZip); } catch { }
                    }

                    Report(progress, 97, "正在完成安装…");
                    string finalDir = RuntimeDir;
                    if (Directory.Exists(finalDir))
                    {
                        try { Directory.Delete(finalDir, true); }
                        catch (Exception ex)
                        {
                            throw new IOException("无法替换旧的播放引擎目录（可能被占用或权限不足）: " + finalDir, ex);
                        }
                    }
                    Directory.CreateDirectory(RuntimeRoot);
                    Directory.Move(tempRoot, finalDir);
                    File.WriteAllText(ReadyMarker, DateTime.Now.ToString("o"));
                    Report(progress, 100, "准备完成");
                    Log.Info("播放引擎已释放到 " + finalDir);
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name + ": " + ex.Message;
                Log.Error("释放播放引擎失败", ex);
                WriteFailureLog(ex);
                return false;
            }
        }

        private static void WriteFailureLog(Exception ex)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                + " runtime=" + RuntimeDir
                + Environment.NewLine + ex.ToString()
                + Environment.NewLine;
            string[] candidates = new string[]
            {
                Path.Combine(Path.GetTempPath(), "DeskFlow-error.log"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskFlow-error.log"),
                Path.Combine(BaseDirectory, "DeskFlow-error.log")
            };
            foreach (string path in candidates)
            {
                try
                {
                    File.AppendAllText(path, line, Encoding.UTF8);
                    return;
                }
                catch { }
            }
        }

        private static Stream OpenPayload()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(ResourceName);
                if (stream != null) return stream;

                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.EndsWith(".runtime.zip", StringComparison.OrdinalIgnoreCase))
                        return assembly.GetManifestResourceStream(name);
                }
            }
            catch (Exception ex)
            {
                Log.Error("读取内嵌播放引擎失败", ex);
            }
            return null;
        }

        private static void Report(IProgress<RuntimeProgress> progress, int percent, string message)
        {
            if (progress == null) return;
            progress.Report(new RuntimeProgress
            {
                Percent = Math.Max(0, Math.Min(100, percent)),
                Message = message
            });
        }

        public static void DeleteOldPayloads()
        {
            try
            {
                if (!Directory.Exists(RuntimeRoot)) return;
                foreach (string dir in Directory.GetDirectories(RuntimeRoot))
                {
                    string name = Path.GetFileName(dir);
                    if (!string.Equals(name, PayloadVersion, StringComparison.OrdinalIgnoreCase)
                        && !name.StartsWith(PayloadVersion + ".tmp-", StringComparison.OrdinalIgnoreCase))
                    {
                        try { Directory.Delete(dir, true); } catch { }
                    }
                }
            }
            catch { }
        }
    }
}
