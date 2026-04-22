using System;

namespace EasySave.Services
{
    public class LanguageManager
    {
        private static LanguageManager instance;
        
        public string CurrentLanguage { get; set; }

        private LanguageManager()
        {
        }

        public static LanguageManager GetInstance()
        {
            return instance ?? (instance = new LanguageManager());
        }

        public string GetText(string key)
        {
            return string.Empty;
        }
    }
}
