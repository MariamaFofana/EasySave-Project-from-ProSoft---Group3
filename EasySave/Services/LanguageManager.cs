using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    
    /// Singleton that provides translated UI strings.
    /// Reads translations from JSON files in Ressources/local/ (en.json, fr.json).
    /// Falls back to the key itself if a translation is not found.
   
    public class LanguageManager
    {
        private static LanguageManager instance;
        private static readonly object _lock = new object();

        /// Current language code ("en" or "fr").
        public string CurrentLanguage { get; set; } = "en";

        /// Loaded translations keyed by language then by text key.
        private readonly Dictionary<string, Dictionary<string, string>> _catalogs;

        /// Base path where en.json and fr.json are located.
        private readonly string _resourcePath;

        private LanguageManager()
        {
            // Determine where the JSON files live relative to the executable
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _resourcePath = Path.Combine(baseDir, "Ressources", "local");

            _catalogs = new Dictionary<string, Dictionary<string, string>>();

            // Load all available language files
            LoadLanguageFile("en");
            LoadLanguageFile("fr");
        }

        
        /// Singleton access point. Thread-safe.
        
        public static LanguageManager GetInstance()
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                        instance = new LanguageManager();
                }
            }
            return instance;
        }

       
        /// Returns the translated text for a key in the current language.
        /// Falls back to the key itself if not found (so missing translations are visible).
        
        public string GetText(string key)
        {
            if (_catalogs.TryGetValue(CurrentLanguage, out var dict) &&
                dict.TryGetValue(key, out string value))
            {
                return value;
            }
            return key;
        }

        /// Loads a single language file (e.g. en.json) into the catalogs.
        private void LoadLanguageFile(string culture)
        {
            string filePath = Path.Combine(_resourcePath, culture + ".json");

            if (!File.Exists(filePath))
            {
                _catalogs[culture] = new Dictionary<string, string>();
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                _catalogs[culture] = dict ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                _catalogs[culture] = new Dictionary<string, string>();
            }
        }
    }
}
