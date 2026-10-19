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
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--autostart", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--hidden", StringComparison.OrdinalIgnoreCase))
                    startHidden = true;
                else if (arg.StartsWith("--data-dir=", StringComparison.OrdinalIgnoreCase))
                    AppPaths.OverrideDirectory = arg.Substring("--data-dir=".Length).Trim('"');
                else if (string.Equals(arg, "--portable", StringComparison.OrdinalIgnoreCase))
                    AppPaths.OverrideDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            }

            bool createdNew;
            using (Mutex mutex = new Mutex(true, "Local\\DeskFlow.SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    TrayContext.SignalExistingInstance();
                    return;
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
                    else
                    {
                        Runtime.DeleteOldPayloads();
                    }

                    Log.Info("DeskFlow 启动，版本 1.0.0");
                    Application.Run(new TrayContext(startHidden));
                }
                catch (Exception ex)
                {
                    Log.Error("启动失败", ex);
                    MessageBox.Show("DeskFlow 启动失败：\n\n" + ex.Message, "DeskFlow",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    try { mutex.ReleaseMutex(); } catch { }
                }
            }
        }
    }
}
