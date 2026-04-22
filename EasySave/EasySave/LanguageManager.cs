using System;
using System.Collections.Generic;

namespace EasySave.Utils
{
    public class LanguageManager
    {
        private static LanguageManager? _instance;
        private Dictionary<string, string> _translations;
        public string CurrentLanguage { get; private set; }

        private LanguageManager()
        {
            _translations = new Dictionary<string, string>();
            CurrentLanguage = "en"; 
            LoadLanguage(CurrentLanguage);
        }

        public static LanguageManager Instance
        {
            get
            {
                if (_instance == null) _instance = new LanguageManager();
                return _instance;
            }
        }

        public void SetLanguage(string lang)
        {
            CurrentLanguage = lang.ToLower();
            LoadLanguage(CurrentLanguage);
        }

        private void LoadLanguage(string lang)
        {

            if (lang == "fr")
            {
                _translations = new Dictionary<string, string> {
                    { "job_name", "Nom du travail" },
                    { "source", "Répertoire source" },
                    { "target", "Répertoire cible" },
                    { "type", "Type de sauvegarde" },
                    { "progress", "Progression" },
                    { "success", "Sauvegarde terminée avec succès." },
                    { "error", "Une erreur est survenue." }
                };
            }
            else
            {
                _translations = new Dictionary<string, string> {
                    { "job_name", "Job Name" },
                    { "source", "Source Directory" },
                    { "target", "Target Directory" },
                    { "type", "Backup Type" },
                    { "progress", "Progress" },
                    { "success", "Backup completed successfully." },
                    { "error", "An error has occurred." }
                };
            }
        }

        public string GetText(string key)
        {
            return _translations.ContainsKey(key) ? _translations[key] : key;
        }
    }
}