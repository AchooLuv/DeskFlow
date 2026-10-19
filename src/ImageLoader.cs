using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace DeskFlow
{
    internal static class ImageLoader
    {
        private static readonly HashSet<string> Supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".webp", ".bmp", ".gif"
        };

        public static bool IsSupported(string path)
        {
            return !string.IsNullOrEmpty(path) && Supported.Contains(Path.GetExtension(path));
        }

        public static List<string> ScanFolder(string folder)
        {
            List<string> files = new List<string>();
            try
            {
                if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return files;
                foreach (string file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    if (IsSupported(file)) files.Add(file);
                }
                files.Sort(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.Warn("扫描图片文件夹失败: " + ex.Message);
            }
            return files;
        }

        public static Bitmap LoadForSize(string path, int width, int height, out string error)
        {
            error = null;
            if (width < 1 || height < 1)
            {
                error = "无效的壁纸尺寸。";
                return null;
            }

            Image source = null;
            string tempFile = null;
            try
            {
                source = LoadSource(path, out tempFile);
                if (source == null)
                {
                    error = "无法读取图片。";
                    return null;
                }

                ApplyOrientation(source);

                Bitmap target = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.Clear(Color.Black);

                    float scale = Math.Max((float)width / source.Width, (float)height / source.Height);
                    float drawWidth = source.Width * scale;
                    float drawHeight = source.Height * scale;
                    float x = (width - drawWidth) / 2f;
                    float y = (height - drawHeight) / 2f;
                    g.DrawImage(source, x, y, drawWidth, drawHeight);
                }
                return target;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Log.Warn("加载图片失败 " + path + ": " + ex.Message);
                return null;
            }
            finally
            {
                if (source != null) source.Dispose();
                if (tempFile != null)
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        private static Image LoadSource(string path, out string tempFile)
        {
            tempFile = null;
            if (!File.Exists(path))
                throw new FileNotFoundException("找不到图片文件。", path);

            if (string.Equals(Path.GetExtension(path), ".webp", StringComparison.OrdinalIgnoreCase))
            {
                try { return LoadFromFileStream(path); }
                catch (Exception ex)
                {
                    Log.Info("系统 WebP 解码不可用，改用 dwebp: " + ex.Message);
                }
                return DecodeWebp(path, out tempFile);
            }
            return LoadFromFileStream(path);
        }

        private static Image LoadFromFileStream(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                Image image = Image.FromStream(stream, true, true);
                // Force GDI+ to decode the image before the stream is closed.
                using (Bitmap probe = new Bitmap(1, 1))
                using (Graphics g = Graphics.FromImage(probe))
                    g.DrawImage(image, 0, 0, 1, 1);
                return image;
            }
        }

        private static Image DecodeWebp(string path, out string tempFile)
        {
            tempFile = Path.Combine(Path.GetTempPath(), "DeskFlow-webp-" + Guid.NewGuid().ToString("N") + ".bmp");
            if (!File.Exists(Runtime.WebpTool))
                throw new InvalidOperationException("缺少 WebP 解码组件。");

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = Runtime.WebpTool,
                Arguments = Quote(path) + " -bmp -o " + Quote(tempFile) + " -quiet",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(Runtime.WebpTool)
            };
            using (Process process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException("无法启动 WebP 解码器。");
                if (!process.WaitForExit(20000))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException("WebP 解码超时。");
                }
                if (process.ExitCode != 0 || !File.Exists(tempFile))
                    throw new InvalidOperationException("WebP 文件无法解码。");
            }

            try
            {
                Image image = Image.FromFile(tempFile);
                // Load a copy so the temporary file can be released immediately.
                Bitmap copy = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
                image.Dispose();
                return copy;
            }
            catch
            {
                try { File.Delete(tempFile); } catch { }
                tempFile = null;
                throw;
            }
        }

        private static void ApplyOrientation(Image image)
        {
            try
            {
                if (!image.PropertyIdList.Contains(0x0112)) return;
                PropertyItem item = image.GetPropertyItem(0x0112);
                if (item == null || item.Value == null || item.Value.Length < 2) return;
                int orientation = BitConverter.ToUInt16(item.Value, 0);
                switch (orientation)
                {
                    case 2: image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                    case 3: image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    case 4: image.RotateFlip(RotateFlipType.RotateNoneFlipY); break;
                    case 5: image.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                    case 6: image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                    case 7: image.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                    case 8: image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                }
            }
            catch { }
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }
    }
}
