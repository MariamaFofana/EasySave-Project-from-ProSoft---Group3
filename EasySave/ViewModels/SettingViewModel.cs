using System.Collections.Generic;
using System.Linq;
using EasySave.Services;
using EasyLogDLL;

namespace EasySave.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private const string HARDCODED_KEY = "EasySaveKey";

        private string _largeFileThresholdKbText;

        public string Title => "Settings";

        public List<string> AvailableLanguages { get; } = new List<string> { "en", "fr" };
        public List<string> AvailableLogFormats { get; } = new List<string> { "json", "xml" };
        public List<string> AvailableLogModes { get; } = new List<string> { "local", "central", "mixed" };

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

        public string SelectedLogMode
        {
            get => SettingsManager.CurrentSettings.LogMode;
            set
            {
                if (SettingsManager.CurrentSettings.LogMode != value)
                {
                    SettingsManager.CurrentSettings.LogMode = value;
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

        public string PriorityExtensions
        {
            get => string.Join(", ", SettingsManager.CurrentSettings.PriorityExtensions);
            set
            {
                var extensions = value.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                      .Select(e => e.Trim())
                                      .ToList();

                SettingsManager.CurrentSettings.PriorityExtensions = extensions;
                SettingsManager.SaveSettings();
                OnPropertyChanged();
            }
        }

        public string LargeFileThresholdKbText
        {
            get => _largeFileThresholdKbText;
            set
            {
                _largeFileThresholdKbText = value;

                if (int.TryParse(value, out int threshold) && threshold > 0)
                {
                    SettingsManager.CurrentSettings.LargeFileThresholdKb = threshold;
                    SettingsManager.SaveSettings();
                }

                OnPropertyChanged();
            }
        }

        public string CentralLogServerUrl
        {
            get => SettingsManager.CurrentSettings.CentralLogServerUrl;
            set
            {
                if (SettingsManager.CurrentSettings.CentralLogServerUrl != value)
                {
                    SettingsManager.CurrentSettings.CentralLogServerUrl = value;
                    SettingsManager.SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public SettingViewModel()
        {
            _largeFileThresholdKbText = SettingsManager.CurrentSettings.LargeFileThresholdKb.ToString();
            UpdateEncryptionService();
        }

        private void UpdateEncryptionService()
        {
            string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            string cryptoPath = System.IO.Path.Combine(baseDir, "CryptoSoft.exe");

            if (!System.IO.File.Exists(cryptoPath))
            {
                string projectRoot = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(baseDir, "..", "..", "..", ".."));
                cryptoPath = System.IO.Path.Combine(
                    projectRoot, "CryptoSoft", "bin", "Debug", "net8.0", "CryptoSoft.exe");
            }

            EncryptionService.Configure(
                cryptoPath,
                HARDCODED_KEY,
                SettingsManager.CurrentSettings.ExtensionsToEncrypt);
        }
    }
}