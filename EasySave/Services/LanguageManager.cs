using System;
using System.Collections.Generic;
using System.ComponentModel; // AJOUT: Nécessaire pour INotifyPropertyChanged
using System.IO;
using System.Text.Json;

namespace EasySave.Services
{
    /// Singleton that provides translated UI strings.
    /// Reads translations from JSON files in Ressources/local/ (en.json, fr.json).
    /// Falls back to the key itself if a translation is not found.
    public class LanguageManager : INotifyPropertyChanged // AJOUT: Implémentation de l'interface
    {
        private static LanguageManager instance;
        private static readonly object _lock = new object();

        // AJOUT: Propriété statique pour le binding XAML
        public static LanguageManager Instance => GetInstance();

        // MODIFICATION: Le setter doit notifier l'interface graphique du changement de langue
        private string _currentLanguage = "en";
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    // Notifie la vue que TOUS les textes (via l'indexeur) doivent être rafraîchis
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        private readonly Dictionary<string, Dictionary<string, string>> _catalogs;
        private readonly string _resourcePath;

        // AJOUT: L'événement requis par INotifyPropertyChanged
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

        // AJOUT: L'indexeur pour permettre le binding XAML avec la syntaxe {Binding [ma.cle]}
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

        // AJOUT: Méthode helper pour déclencher l'événement proprement
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
