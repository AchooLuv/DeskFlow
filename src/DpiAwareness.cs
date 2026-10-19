using System;
using System.Runtime.InteropServices;

namespace DeskFlow
{
    internal static class DpiAwareness
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int value);

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        public static void Enable()
        {
            try
            {
                // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2
                if (SetProcessDpiAwarenessContext(new IntPtr(-4))) return;
            }
            catch { }
            try
            {
                // PROCESS_PER_MONITOR_DPI_AWARE
                if (SetProcessDpiAwareness(2) == 0) return;
            }
            catch { }
            try { SetProcessDPIAware(); } catch { }
        }
    }
}
