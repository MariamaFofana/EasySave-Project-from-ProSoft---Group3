namespace EasySave.Core
{
    public class LanguageManager
    {
        private static LanguageManager _instance;
        private string _currentCulture;

        private LanguageManager()
        {
        }

        public static LanguageManager GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LanguageManager();
            }
            return _instance;
        }

        public string GetString(string key)
        {
            return string.Empty;
        }

        public void SetLanguage(string culture)
        {
        }
    }
}
