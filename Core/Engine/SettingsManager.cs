using System;
using System.IO;
using System.Text.Json;
using DesktopIconLayoutManager.Core.Models;
using Microsoft.Win32;

namespace DesktopIconLayoutManager.Core.Engine
{
    public class SettingsManager
    {
        private readonly string _settingsPath;
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppRegistryName = "ReIconPro";

        public AppSettings CurrentSettings { get; private set; }

        public SettingsManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "ReIconPro");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            _settingsPath = Path.Combine(dir, "settings.json");
            CurrentSettings = LoadSettings();
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch { }

            return new AppSettings();
        }

        public void SaveSettings()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(CurrentSettings, options);
                File.WriteAllText(_settingsPath, json);

                ApplyStartupRegistry(CurrentSettings.StartWithWindows);
            }
            catch { }
        }

        public void ApplyStartupRegistry(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
                if (key == null) return;

                if (enable)
                {
                    string? exePath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        key.SetValue(AppRegistryName, $"\"{exePath}\"");
                    }
                }
                else
                {
                    if (key.GetValue(AppRegistryName) != null)
                    {
                        key.DeleteValue(AppRegistryName, false);
                    }
                }
            }
            catch { }
        }
    }
}
