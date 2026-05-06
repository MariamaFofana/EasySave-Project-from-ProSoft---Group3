using System.Collections.Generic;
using System.Linq;
using EasySave.Services;
using EasyLogDLL;

namespace EasySave.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private const string HARDCODED_KEY = "EasySaveKey";

        public string Title => "Paramètres";

        public List<string> AvailableLanguages { get; } = new List<string> { "en", "fr" };
        public List<string> AvailableLogFormats { get; } = new List<string> { "json", "xml" };

        public string SelectedLanguage
        {
            get => SettingsManager.CurrentSettings.Language;
            set
            {
                if (SettingsManager.CurrentSettings.Language != value)
                {
                    SettingsManager.CurrentSettings.Language = value;
                    LanguageManager.Instance.CurrentLanguage = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedLogFormat
        {
            get => SettingsManager.CurrentSettings.LogFormat;
            set
            {
                if (SettingsManager.CurrentSettings.LogFormat != value)
                {
                    SettingsManager.CurrentSettings.LogFormat = value;
                    EasyLogger.LogFormat = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public string ExtensionsToEncrypt
        {
            get => string.Join(", ", SettingsManager.CurrentSettings.ExtensionsToEncrypt);
            set
            {
                var extensions = value.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                      .Select(e => e.Trim())
                                      .ToList();
                SettingsManager.CurrentSettings.ExtensionsToEncrypt = extensions;
                SettingsManager.SaveSettings();
                UpdateEncryptionService();
                OnPropertyChanged();
            }
        }

        public string BusinessSoftware
        {
            get => string.Join(", ", SettingsManager.CurrentSettings.BusinessSoftware);
            set
            {
                var softwares = value.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim())
                                       .ToList();
                SettingsManager.CurrentSettings.BusinessSoftware = softwares;
                SettingsManager.SaveSettings();
                OnPropertyChanged();
            }
        }

        public SettingViewModel()
        {
            UpdateEncryptionService();
        }

        private void UpdateEncryptionService()
        {
            string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            string cryptoPath = System.IO.Path.Combine(baseDir, "CryptoSoft.exe");

            // If not found in base dir, look in sibling project (dev mode)
            if (!System.IO.File.Exists(cryptoPath))
            {
                string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", "..", ".."));
                cryptoPath = System.IO.Path.Combine(projectRoot, "CryptoSoft", "bin", "Debug", "net8.0", "CryptoSoft.exe");
            }

            EncryptionService.Configure(cryptoPath, HARDCODED_KEY, SettingsManager.CurrentSettings.ExtensionsToEncrypt);
        }
    }
}




