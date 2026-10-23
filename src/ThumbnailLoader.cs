using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DeskFlow
{
    internal static class ThumbnailLoader
    {
        private const uint BiggerSizeOk = 0x1;
        private const uint ThumbnailOnly = 0x8;

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;
        }

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            int GetImage(SIZE size, uint flags, out IntPtr bitmap);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            string path, IntPtr bindContext, ref Guid iid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory factory);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr handle);

        public static Bitmap Load(string path, int width, int height)
        {
            if (string.IsNullOrEmpty(path) || width < 1 || height < 1) return null;
            try
            {
                Guid iid = typeof(IShellItemImageFactory).GUID;
                IShellItemImageFactory factory;
                SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out factory);
                if (factory == null) return null;

                SIZE size = new SIZE { cx = width, cy = height };
                IntPtr handle;
                int hr = factory.GetImage(size, BiggerSizeOk | ThumbnailOnly, out handle);
                if (hr != 0 || handle == IntPtr.Zero)
                {
                    hr = factory.GetImage(size, BiggerSizeOk, out handle);
                    if (hr != 0 || handle == IntPtr.Zero) return null;
                }

                try
                {
                    using (Bitmap source = Image.FromHbitmap(handle))
                        return new Bitmap(source);
                }
                finally
                {
                    DeleteObject(handle);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
