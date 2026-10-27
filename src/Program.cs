using System;
using System.Threading;
using System.Windows.Forms;

namespace DeskFlow
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            DpiAwareness.Enable();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
            {
                Log.Error("未处理的界面异常", e.Exception);
                try
                {
                    MessageBox.Show("DeskFlow 遇到一个问题，但仍在运行。\n\n" + e.Exception.Message,
                        "DeskFlow", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch { }
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Log.Error("未处理的异常", e.ExceptionObject as Exception);
            };

            bool startHidden = false;
            bool standalone = false;
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--autostart", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--hidden", StringComparison.OrdinalIgnoreCase))
                    startHidden = true;
                else if (string.Equals(arg, "--standalone", StringComparison.OrdinalIgnoreCase))
                    standalone = true;
                else if (arg.StartsWith("--data-dir=", StringComparison.OrdinalIgnoreCase))
                    AppPaths.OverrideDirectory = arg.Substring("--data-dir=".Length).Trim('"');
                else if (string.Equals(arg, "--portable", StringComparison.OrdinalIgnoreCase))
                    AppPaths.OverrideDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            }

            bool createdNew = true;
            Mutex mutex = null;
            if (!standalone)
            {
                try
                {
                    mutex = SingleInstance.Create(out createdNew);
                }
                catch (Exception ex)
                {
                    Log.Error("创建单实例锁失败", ex);
                    createdNew = false;
                }

                if (mutex == null || !createdNew)
                {
                    IntPtr existingWindow = NativeMethods.FindWindow(null, "DeskFlow.Message");
                    if (existingWindow != IntPtr.Zero)
                    {
                        TrayContext.SignalExistingInstance();
                        return;
                    }
                    Log.Warn("检测到残留的单实例锁，但没有运行中的 DeskFlow 窗口，继续启动。");
                }
            }

            try
            {
                if (!Runtime.IsReady)
                {
                    using (PrepareForm prepare = new PrepareForm())
                    {
                        if (prepare.ShowDialog() != DialogResult.OK) return;
                    }
                }
                Runtime.DeleteOldPayloads();

                Log.Info("DeskFlow 启动，版本 1.0.9");
                Application.Run(new TrayContext(startHidden));
            }
            catch (Exception ex)
            {
                Log.Error("启动失败", ex);
                MessageBox.Show("DeskFlow 启动失败：\n\n" + ex.Message + "\n\n日志：" + Log.LogPath, "DeskFlow",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (mutex != null)
                {
                    try { mutex.ReleaseMutex(); } catch { }
                    try { mutex.Dispose(); } catch { }
                }
            }
        }
    }
}
