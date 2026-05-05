using System;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Services
{
    public static class SettingsManager
    {
        private static readonly string SettingsPath;
        private static Settings _currentSettings;

        public static Settings CurrentSettings => _currentSettings;

        static SettingsManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Go up 3 levels to reach the EasySave project folder from bin/Debug/net8.0/
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string settingsDir = Path.Combine(projectDir, "Ressources");
            Directory.CreateDirectory(settingsDir);
            SettingsPath = Path.Combine(settingsDir, "appSettings.json");
            LoadSettings();
        }




        public static void LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    _currentSettings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
                catch
                {
                    _currentSettings = new Settings();
                }
            }
            else
            {
                _currentSettings = new Settings();
                SaveSettings(); // Create and populate the file
            }
        }


        public static void SaveSettings()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_currentSettings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
