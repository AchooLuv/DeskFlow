using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace DeskFlow
{
    internal static class AppIcon
    {
        private static Icon _icon;

        public static Icon Get()
        {
            if (_icon != null) return _icon;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("DeskFlow.AppIcon.ico"))
                {
                    if (stream != null)
                    {
                        _icon = new Icon(stream);
                        return _icon;
                    }
                }
            }
            catch { }
            _icon = SystemIcons.Application;
            return _icon;
        }
    }
}
