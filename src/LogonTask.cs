using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace DeskFlow
{
    internal static class LogonTask
    {
        private const string TaskName = "DeskFlow Logon";

        public static bool Exists()
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("schtasks.exe",
                    "/query /tn \"" + TaskName + "\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process process = Process.Start(info))
                {
                    string output = process.StandardOutput.ReadToEnd()
                        + process.StandardError.ReadToEnd();
                    process.WaitForExit(5000);
                    return process.HasExited && process.ExitCode == 0
                        && output.IndexOf(TaskName, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch { return false; }
        }

        public static bool ExistsFor(string executable)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo("schtasks.exe",
                    "/query /tn \"" + TaskName + "\" /xml")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process process = Process.Start(info))
                {
                    string output = process.StandardOutput.ReadToEnd()
                        + process.StandardError.ReadToEnd();
                    process.WaitForExit(5000);
                    if (!process.HasExited || process.ExitCode != 0) return false;
                    string escaped = SecurityElement.Escape(executable);
                    return output.IndexOf(escaped, StringComparison.OrdinalIgnoreCase) >= 0
                        || output.IndexOf(executable, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch { return false; }
        }

        public static bool Create(string executable)
        {
            string temp = Path.Combine(Path.GetTempPath(),
                "DeskFlow-task-" + Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                File.WriteAllText(temp, BuildXml(executable), Encoding.Unicode);
                bool ok = RunElevated("schtasks.exe",
                    "/create /tn \"" + TaskName + "\" /xml \"" + temp + "\" /f");
                Thread.Sleep(300);
                return ok && Exists();
            }
            catch
            {
                return false;
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }

        public static bool Delete()
        {
            if (!Exists()) return true;
            bool ok = RunElevated("schtasks.exe", "/delete /tn \"" + TaskName + "\" /f");
            Thread.Sleep(300);
            return ok && !Exists();
        }

        private static bool RunElevated(string file, string arguments)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(file, arguments)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (Process process = Process.Start(info))
                {
                    process.WaitForExit(60000);
                    return process.HasExited && process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string BuildXml(string executable)
        {
            string user = WindowsIdentity.GetCurrent().Name;
            string working = Path.GetDirectoryName(executable);
            string command = SecurityElement.Escape(executable);
            string arguments = SecurityElement.Escape("--autostart");
            string work = SecurityElement.Escape(working);
            string userId = SecurityElement.Escape(user);

            return "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" +
                "<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">" +
                "<RegistrationInfo><Description>DeskFlow early startup</Description></RegistrationInfo>" +
                "<Triggers><LogonTrigger><Enabled>true</Enabled><UserId>" + userId + "</UserId></LogonTrigger></Triggers>" +
                "<Principals><Principal id=\"Author\"><UserId>" + userId + "</UserId>" +
                "<LogonType>InteractiveToken</LogonType><RunLevel>LeastPrivilege</RunLevel></Principal></Principals>" +
                "<Settings><MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>" +
                "<DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>" +
                "<StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>" +
                "<AllowHardTerminate>true</AllowHardTerminate><StartWhenAvailable>true</StartWhenAvailable>" +
                "<ExecutionTimeLimit>PT0S</ExecutionTimeLimit><Enabled>true</Enabled></Settings>" +
                "<Actions Context=\"Author\"><Exec><Command>" + command + "</Command>" +
                "<Arguments>" + arguments + "</Arguments>" +
                "<WorkingDirectory>" + work + "</WorkingDirectory></Exec></Actions></Task>";
        }
    }
}
