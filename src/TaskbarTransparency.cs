using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeskFlow
{
    internal static class TaskbarTransparency
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<IntPtr, NativeMethods.AccentPolicy> Original =
            new Dictionary<IntPtr, NativeMethods.AccentPolicy>();
        private static bool _enabled;
        private static DateTime _lastApply = DateTime.MinValue;

        public static bool Enabled
        {
            get { return _enabled; }
        }

        public static void Initialize(bool enabled)
        {
            _enabled = enabled;
            if (_enabled) Apply(true);
        }

        public static void SetEnabled(bool enabled)
        {
            if (_enabled == enabled)
            {
                if (enabled) Apply(false);
                return;
            }
            _enabled = enabled;
            if (enabled) Apply(true);
            else Restore();
        }

        public static void ApplyIfNeeded()
        {
            if (!_enabled) return;
            // Start menu and other shell surfaces can reset the taskbar's
            // accent policy. Re-assert it regularly instead of waiting 30s.
            // Windows 11's DWM message is heavier, so use a slightly longer
            // interval there.
            double minimum = NativeMethods.IsWindows11 ? 2.0 : 0.6;
            if ((DateTime.UtcNow - _lastApply).TotalSeconds < minimum) return;
            Apply(false);
        }

        public static void Apply(bool force)
        {
            if (!_enabled) return;
            lock (Sync)
            {
                List<IntPtr> taskbars = NativeMethods.FindTaskbars();
                if (taskbars.Count == 0) return;
                bool windows11 = NativeMethods.IsWindows11;

                foreach (IntPtr hwnd in taskbars)
                {
                    if (!Original.ContainsKey(hwnd))
                    {
                        NativeMethods.AccentPolicy current;
                        if (TryGet(hwnd, out current))
                            Original[hwnd] = current;
                        else
                            Original[hwnd] = new NativeMethods.AccentPolicy
                            {
                                AccentState = (int)NativeMethods.AccentState.EnableBlurBehind,
                                AccentFlags = 2,
                                GradientColor = 0x99000000,
                                AnimationId = 0
                            };
                    }

                    NativeMethods.AccentPolicy policy = new NativeMethods.AccentPolicy
                    {
                        AccentState = (int)NativeMethods.AccentState.EnableTransparentGradient,
                        AccentFlags = 2,
                        GradientColor = 0x00FFFFFF,
                        AnimationId = 0
                    };
                    Set(hwnd, policy);

                    if (windows11)
                    {
                        IntPtr result;
                        NativeMethods.SendMessageTimeout(hwnd, NativeMethods.WM_DWMCOMPOSITIONCHANGED,
                            IntPtr.Zero, IntPtr.Zero, NativeMethods.SMTO_ABORTIFHUNG, 120, out result);
                    }
                }

                // Forget handles that no longer exist.
                List<IntPtr> stale = new List<IntPtr>();
                foreach (IntPtr hwnd in Original.Keys)
                {
                    if (!NativeMethods.IsWindow(hwnd)) stale.Add(hwnd);
                }
                foreach (IntPtr hwnd in stale) Original.Remove(hwnd);

                _lastApply = DateTime.UtcNow;
                if (force) Log.Info("已应用任务栏完全透明样式");
            }
        }

        public static void Restore()
        {
            lock (Sync)
            {
                foreach (KeyValuePair<IntPtr, NativeMethods.AccentPolicy> pair in Original)
                {
                    if (!NativeMethods.IsWindow(pair.Key)) continue;
                    Set(pair.Key, pair.Value);
                    if (NativeMethods.IsWindows11)
                    {
                        IntPtr result;
                        NativeMethods.SendMessageTimeout(pair.Key, NativeMethods.WM_DWMCOMPOSITIONCHANGED,
                            IntPtr.Zero, IntPtr.Zero, NativeMethods.SMTO_ABORTIFHUNG, 120, out result);
                    }
                }
                Original.Clear();
                Log.Info("已恢复任务栏样式");
            }
        }

        private static bool TryGet(IntPtr hwnd, out NativeMethods.AccentPolicy policy)
        {
            policy = new NativeMethods.AccentPolicy();
            IntPtr data = IntPtr.Zero;
            try
            {
                data = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.AccentPolicy)));
                Marshal.StructureToPtr(policy, data, false);
                NativeMethods.WindowCompositionAttributeData attribute = new NativeMethods.WindowCompositionAttributeData
                {
                    Attribute = NativeMethods.WCA_ACCENT_POLICY,
                    Data = data,
                    SizeOfData = Marshal.SizeOf(typeof(NativeMethods.AccentPolicy))
                };
                int result = NativeMethods.GetWindowCompositionAttribute(hwnd, ref attribute);
                if (result == 0) return false;
                policy = (NativeMethods.AccentPolicy)Marshal.PtrToStructure(data, typeof(NativeMethods.AccentPolicy));
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (data != IntPtr.Zero) Marshal.FreeHGlobal(data);
            }
        }

        private static void Set(IntPtr hwnd, NativeMethods.AccentPolicy policy)
        {
            IntPtr data = IntPtr.Zero;
            try
            {
                data = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.AccentPolicy)));
                Marshal.StructureToPtr(policy, data, false);
                NativeMethods.WindowCompositionAttributeData attribute = new NativeMethods.WindowCompositionAttributeData
                {
                    Attribute = NativeMethods.WCA_ACCENT_POLICY,
                    Data = data,
                    SizeOfData = Marshal.SizeOf(typeof(NativeMethods.AccentPolicy))
                };
                NativeMethods.SetWindowCompositionAttribute(hwnd, ref attribute);
            }
            catch (Exception ex)
            {
                Log.Warn("设置任务栏样式失败: " + ex.Message);
            }
            finally
            {
                if (data != IntPtr.Zero) Marshal.FreeHGlobal(data);
            }
        }
    }
}
