using System;
using System.IO;

namespace DeskFlow
{
    internal static class AppPaths
    {
        public static string OverrideDirectory;

        public static string DataDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OverrideDirectory)) return OverrideDirectory;
                string overridePath = Environment.GetEnvironmentVariable("DESKFLOW_DATA_DIR");
                if (!string.IsNullOrWhiteSpace(overridePath)) return overridePath;
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeskFlow");
            }
        }

        public static string SettingsDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OverrideDirectory)) return OverrideDirectory;
                string overridePath = Environment.GetEnvironmentVariable("DESKFLOW_DATA_DIR");
                if (!string.IsNullOrWhiteSpace(overridePath)) return overridePath;
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeskFlow");
            }
        }
    }
}
