using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.Models;
using EasyLogDLL;

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
            }

            NormalizeSettings();
            SaveSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                NormalizeSettings();

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(_currentSettings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private static void NormalizeSettings()
        {
            _currentSettings ??= new Settings();

            _currentSettings.ExtensionsToEncrypt ??= new List<string>();
            _currentSettings.BusinessSoftware ??= new List<string>();
            _currentSettings.PriorityExtensions ??= new List<string>();

            if (string.IsNullOrWhiteSpace(_currentSettings.Language))
                _currentSettings.Language = "en";

            if (_currentSettings.Language != "en" && _currentSettings.Language != "fr")
                _currentSettings.Language = "en";

            if (string.IsNullOrWhiteSpace(_currentSettings.LogFormat))
                _currentSettings.LogFormat = "json";

            if (_currentSettings.LogFormat != "json" && _currentSettings.LogFormat != "xml")
                _currentSettings.LogFormat = "json";

            if (_currentSettings.LargeFileThresholdKB < 0)
                _currentSettings.LargeFileThresholdKB = 0;

            if (string.IsNullOrWhiteSpace(_currentSettings.LogMode))
                _currentSettings.LogMode = "local";

            if (_currentSettings.LogMode != "local" &&
                _currentSettings.LogMode != "central" &&
                _currentSettings.LogMode != "mixed")
            {
                _currentSettings.LogMode = "local";
            }

            if (string.IsNullOrWhiteSpace(_currentSettings.CentralLogServerUrl))
                _currentSettings.CentralLogServerUrl = "http://localhost:5000/api/logs";

            ApplyLoggerConfiguration();
        }

        private static void ApplyLoggerConfiguration()
        {
            if (_currentSettings == null) return;

            // Map LogMode string to EasyLogger LogStrategy enum
            if (string.Equals(_currentSettings.LogMode, "local", StringComparison.OrdinalIgnoreCase))
            {
                EasyLogger.CurrentStrategy = LogStrategy.Local;
            }
            else if (string.Equals(_currentSettings.LogMode, "mixed", StringComparison.OrdinalIgnoreCase))
            {
                EasyLogger.CurrentStrategy = LogStrategy.Mixed;
            }
            else
            {
                EasyLogger.CurrentStrategy = LogStrategy.Centralized;
            }

            // Map CentralLogServerUrl to EasyLogger ServerAddress
            string url = _currentSettings.CentralLogServerUrl;
            if (!string.IsNullOrWhiteSpace(url))
            {
                try
                {
                    var uri = new Uri(url);
                    EasyLogger.ServerAddress = $"{uri.Host}:{uri.Port}";
                }
                catch
                {
                    // Fallback to extraction if not a fully formed absolute URI
                    EasyLogger.ServerAddress = url.Replace("http://", "").Replace("https://", "").Split('/')[0];
                }
            }
        }
    }
}