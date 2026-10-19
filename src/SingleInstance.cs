using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace DeskFlow
{
    internal static class SingleInstance
    {
        private const string MutexName = "Local\\DeskFlow.SingleInstance";

        public static Mutex Create(out bool createdNew)
        {
            createdNew = false;
            try
            {
                MutexSecurity security = new MutexSecurity();
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                if (identity != null && identity.User != null)
                {
                    security.AddAccessRule(new MutexAccessRule(identity.User,
                        MutexRights.FullControl, AccessControlType.Allow));
                }
                SecurityIdentifier users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                security.AddAccessRule(new MutexAccessRule(users,
                    MutexRights.Synchronize | MutexRights.Modify, AccessControlType.Allow));

                return new Mutex(true, MutexName, out createdNew, security);
            }
            catch (UnauthorizedAccessException)
            {
                // An instance started with a different token is already running.
                Log.Warn("检测到已有 DeskFlow 实例，当前进程将退出。");
                createdNew = false;
                return null;
            }
            catch (Exception ex)
            {
                Log.Warn("创建单实例互斥体失败，改用默认安全设置: " + ex.Message);
                return new Mutex(true, MutexName, out createdNew);
            }
        }
    }
}
