using System;
using System.Collections.Generic;
using System.ComponentModel; // ADDED: Required for INotifyPropertyChanged
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    /// Singleton that provides translated UI strings.
    /// Reads translations from JSON files in Ressources/local/ (en.json, fr.json).
    /// Falls back to the key itself if a translation is not found.
    public class LanguageManager : INotifyPropertyChanged // ADDED: Interface implementation
    {
        private static LanguageManager instance;
        private static readonly object _lock = new object();

        // ADDED: Static property for XAML binding
        public static LanguageManager Instance => GetInstance();

        // MODIFICATION: The setter must notify the GUI of language changes
        private string _currentLanguage = "en";
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    // Notifies the view that ALL texts (via indexer) must be refreshed
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        private readonly Dictionary<string, Dictionary<string, string>> _catalogs;
        private readonly string _resourcePath;

        // ADDED: Event required by INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..")); _resourcePath = Path.Combine(projectDir, "Ressources", "local");

            _catalogs = new Dictionary<string, Dictionary<string, string>>();

            LoadLanguageFile("en");
            LoadLanguageFile("fr");
        }

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

        // ADDED: Indexer to allow XAML binding using the {Binding [my.key]} syntax
        public string this[string key] => GetText(key);

        public string GetText(string key)
        {
            if (_catalogs.TryGetValue(CurrentLanguage, out var dict) &&
                dict.TryGetValue(key, out string value))
            {
                return value;
            }
            return key;
        }

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

        // ADDED: Helper method to raise the event properly
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
