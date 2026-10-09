using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DesktopIconLayoutManager.Core.Models;

namespace DesktopIconLayoutManager.Core.Engine
{
    public class StorageManager
    {
        private readonly string _storageDir;

        public StorageManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _storageDir = Path.Combine(appData, "ReIconPro", "Profiles");
            if (!Directory.Exists(_storageDir))
            {
                Directory.CreateDirectory(_storageDir);
            }
        }

        public string GetStorageDirectory() => _storageDir;

        public void SaveProfile(IconLayoutProfile profile)
        {
            string filePath = Path.Combine(_storageDir, $"{profile.Id}.json");
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(filePath, json);
        }

        public List<IconLayoutProfile> GetAllProfiles()
        {
            var list = new List<IconLayoutProfile>();
            if (!Directory.Exists(_storageDir)) return list;

            string[] files = Directory.GetFiles(_storageDir, "*.json");
            foreach (var file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var profile = JsonSerializer.Deserialize<IconLayoutProfile>(json);
                    if (profile != null)
                    {
                        list.Add(profile);
                    }
                }
                catch
                {
                    // Ignore corrupted profile files
                }
            }

            list.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            return list;
        }

        public bool DeleteProfile(string id)
        {
            string filePath = Path.Combine(_storageDir, $"{id}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public void ClearAllProfiles()
        {
            if (!Directory.Exists(_storageDir)) return;
            string[] files = Directory.GetFiles(_storageDir, "*.json");
            foreach (var f in files)
            {
                try { File.Delete(f); } catch { }
            }
        }
    }
}
