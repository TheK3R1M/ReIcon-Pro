using System;
using System.IO;

namespace DesktopIconLayoutManager.Core.Helpers
{
    public static class ShortcutHelper
    {
        public static bool CreateDesktopShortcut(string profileName, out string shortcutPath)
        {
            shortcutPath = string.Empty;
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string cleanName = string.Join("_", profileName.Split(Path.GetInvalidFileNameChars()));
                shortcutPath = Path.Combine(desktop, $"{cleanName} - ReIcon.lnk");

                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return false;

                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Environment.ProcessPath;
                shortcut.Arguments = $"/restore \"{profileName}\"";
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                shortcut.IconLocation = $"{Environment.ProcessPath},0";
                shortcut.Description = $"ReIcon Pro - {profileName} Geri Yükle";
                shortcut.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
