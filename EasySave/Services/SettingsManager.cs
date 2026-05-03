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
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
            SettingsPath = Path.Combine(appData, "settings.json");
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
