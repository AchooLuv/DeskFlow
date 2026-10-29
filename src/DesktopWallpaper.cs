using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DeskFlow
{
    internal static class DesktopWallpaper
    {
        private const uint SPI_SETDESKWALLPAPER = 0x0014;
        private const uint SPIF_UPDATEINIFILE = 0x0001;
        private const uint SPIF_SENDCHANGE = 0x0002;

        public static string CurrentPath
        {
            get
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                    {
                        string path = key == null ? null : key.GetValue("WallPaper") as string;
                        if (string.IsNullOrWhiteSpace(path)) return null;
                        path = Environment.ExpandEnvironmentVariables(path);
                        return File.Exists(path) ? path : null;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public static Bitmap LoadForScreen()
        {
            string path = CurrentPath;
            if (path == null) return null;
            string error;
            Bitmap bitmap = ImageLoader.LoadForSize(path,
                Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, out error);
            if (bitmap == null)
                Log.Warn("读取系统壁纸失败: " + (error ?? string.Empty));
            else
                Log.Info("已加载系统壁纸作为过渡背景: " + path);
            return bitmap;
        }

        public static void Restore()
        {
            try
            {
                string path = CurrentPath;
                if (path != null)
                {
                    NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                        SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                    Log.Info("已请求恢复系统壁纸: " + path);
                }
                IntPtr progman = NativeMethods.FindWindow("Progman", null);
                if (progman != IntPtr.Zero)
                {
                    NativeMethods.RedrawWindow(progman, IntPtr.Zero, IntPtr.Zero,
                        NativeMethods.RDW_INVALIDATE | NativeMethods.RDW_ERASE |
                        NativeMethods.RDW_ALLCHILDREN | NativeMethods.RDW_UPDATENOW);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("恢复系统壁纸失败: " + ex.Message);
            }
        }
    }
}
