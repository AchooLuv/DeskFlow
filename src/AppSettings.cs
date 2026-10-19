using System;
using System.IO;
using System.Web.Script.Serialization;

namespace DeskFlow
{
    internal sealed class AppSettings
    {
        public int Schema { get; set; }
        public string Mode { get; set; }
        public string ImageFolder { get; set; }
        public int IntervalSeconds { get; set; }
        public bool Shuffle { get; set; }
        public string VideoPath { get; set; }
        public double PlaybackRate { get; set; }
        public int Volume { get; set; }
        public bool Loop { get; set; }
        public bool TaskbarTransparent { get; set; }
        public bool MinimizeToTray { get; set; }
        public bool AutoStart { get; set; }
        public bool PauseFullscreen { get; set; }
        public bool PauseOnBattery { get; set; }
        public int LastPage { get; set; }

        public AppSettings()
        {
            Schema = 1;
            Mode = "none";
            ImageFolder = string.Empty;
            IntervalSeconds = 30;
            Shuffle = false;
            VideoPath = string.Empty;
            PlaybackRate = 1.0;
            Volume = 30;
            Loop = true;
            TaskbarTransparent = true;
            MinimizeToTray = true;
            AutoStart = false;
            PauseFullscreen = true;
            PauseOnBattery = false;
            LastPage = 0;
        }

        public static string SettingsDirectory
        {
            get { return AppPaths.SettingsDirectory; }
        }

        public static string SettingsPath
        {
            get { return Path.Combine(SettingsDirectory, "settings.json"); }
        }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            try
            {
                if (File.Exists(SettingsPath))
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string json = File.ReadAllText(SettingsPath);
                    AppSettings loaded = serializer.Deserialize<AppSettings>(json);
                    if (loaded != null) settings = loaded;
                }
            }
            catch (Exception ex)
            {
                Log.Warn("读取设置失败，使用默认值: " + ex.Message);
                try
                {
                    if (File.Exists(SettingsPath))
                        File.Copy(SettingsPath, SettingsPath + ".bad", true);
                }
                catch { }
            }

            settings.Normalize();
            return settings;
        }

        public void Normalize()
        {
            if (Schema <= 0) Schema = 1;
            if (Mode != "slideshow" && Mode != "video") Mode = "none";
            if (ImageFolder == null) ImageFolder = string.Empty;
            if (VideoPath == null) VideoPath = string.Empty;
            if (IntervalSeconds < 3) IntervalSeconds = 3;
            if (IntervalSeconds > 3600) IntervalSeconds = 3600;
            if (PlaybackRate < 0.25) PlaybackRate = 0.25;
            if (PlaybackRate > 2.0) PlaybackRate = 2.0;
            if (Volume < 0) Volume = 0;
            if (Volume > 100) Volume = 100;
            if (LastPage < 0 || LastPage > 2) LastPage = 0;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(this);
                string temp = SettingsPath + ".tmp";
                File.WriteAllText(temp, json);
                if (File.Exists(SettingsPath))
                {
                    // File.Replace needs DELETE access, which is denied in some
                    // sandboxed profiles. Copy + delete is more forgiving.
                    File.Copy(temp, SettingsPath, true);
                    try { File.Delete(temp); } catch { }
                }
                else
                {
                    File.Move(temp, SettingsPath);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("保存设置失败: " + ex.Message);
            }
        }

        public AppSettings Clone()
        {
            AppSettings copy = (AppSettings)MemberwiseClone();
            return copy;
        }
    }
}
