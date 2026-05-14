using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls.ApplicationLifetimes;
using EasyLogDLL;
using EasySave.Factories;
using EasySave.Models;
using EasySave.Services;
using EasySave.Utils;

namespace EasySave.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;
        private ObservableCollection<BackupJob> _jobs;
        private string _configPath;
        private readonly MonitoringService _monitoringService;

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<BackupJob> Jobs => _jobs;

        public MainViewModel()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            
            _jobs = new ObservableCollection<BackupJob>();
            _configPath = Path.Combine(projectDir, "Ressources", "config.json");

            // Centralized logging initialization
            string logPath = Path.Combine(projectDir, "Logs");
            EasyLogger.Configure(logPath);
            
            EasyLogger.LogFormat = SettingsManager.CurrentSettings.LogFormat;
            LanguageManager.GetInstance().CurrentLanguage = SettingsManager.CurrentSettings.Language;

            // Locate CryptoSoft binary
            string cryptoPath = Path.Combine(baseDir, "CryptoSoft.exe");
            if (!File.Exists(cryptoPath))
            {
                cryptoPath = Path.Combine(projectDir, "..", "CryptoSoft", "bin", "Debug", "net8.0", "CryptoSoft.exe");
            }
            
            EncryptionService.Configure(cryptoPath, "EasySaveKey", SettingsManager.CurrentSettings.ExtensionsToEncrypt);

            // Initialize the TransferOrchestrator with priority and bandwidth settings
            TransferOrchestrator.Configure(
                SettingsManager.CurrentSettings.PriorityExtensions,
                SettingsManager.CurrentSettings.LargeFileThresholdKB
            );

            // Restore saved state
            LoadJobs();

            _currentPage = new BackupListViewModel(this);

            _monitoringService = new MonitoringService();
            _monitoringService.StartMonitoring();
        }

        private void SubscribeToJobEvents(BackupJob job)
        {
            job.OnStateChanged += HandleJobStateChanged;
            job.OnFileCopied += HandleJobLog;
        }

        public void NavigateToBackupList() => CurrentPage = new BackupListViewModel(this);
        public void NavigateToEditJob() => CurrentPage = new EditJobViewModel(this);
        public void NavigateToSettings() => CurrentPage = new SettingViewModel();

        public void Quit()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

        private class BackupJobConfig
        {
            public string Name { get; set; } = string.Empty;
            public string SourceDirectory { get; set; } = string.Empty;
            public string TargetDirectory { get; set; } = string.Empty;
            public BackupType Type { get; set; }
        }

        public void LoadJobs()
        {
            if (!File.Exists(_configPath)) return;

            try
            {
                string json = File.ReadAllText(_configPath);
                if (string.IsNullOrWhiteSpace(json)) return;

                List<BackupJobConfig>? savedJobs = JsonSerializer.Deserialize<List<BackupJobConfig>>(json);
                if (savedJobs == null) return;

                _jobs.Clear();
                foreach (BackupJobConfig savedJob in savedJobs)
                {
                    if (savedJob == null) continue;

                    BackupJob job = BackupJobFactory.CreateJob(
                        savedJob.Name ?? string.Empty,
                        savedJob.SourceDirectory ?? string.Empty,
                        savedJob.TargetDirectory ?? string.Empty,
                        savedJob.Type
                    );

                    SubscribeToJobEvents(job);
                    _jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading jobs: {ex.Message}");
            }
        }

        public void SaveJobs()
        {
            List<BackupJobConfig> savedJobs = _jobs
                .Select(job => new BackupJobConfig
                {
                    Name = job.Name,
                    SourceDirectory = job.SourceDirectory,
                    TargetDirectory = job.TargetDirectory,
                    Type = job.Type
                })
                .ToList();

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(savedJobs, options);
            File.WriteAllText(_configPath, json);
        }

        public void ExecuteJob(int index)
        {
            if (index < 0 || index >= _jobs.Count) return;

            if (_monitoringService.IsAnyBusinessSoftwareRunning())
            {
                Console.WriteLine("\n[Warning] Backup prevented: Business software detected.");
                return;
            }

            BackupJob job = _jobs[index];
            job.Execute();
        }

        public void ExecuteAllJobs()
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_monitoringService.IsAnyBusinessSoftwareRunning())
                {
                    Console.WriteLine("\n[Warning] Sequential backup suspended: Business software detected.");
                    break;
                }
                ExecuteJob(i);
            }
        }

        public void ExecuteSelectedJobs()
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (!_jobs[i].IsSelected) continue;

                if (_monitoringService.IsAnyBusinessSoftwareRunning())
                {
                    Console.WriteLine("\n[Warning] Sequential backup suspended: Business software detected.");
                    break;
                }
                ExecuteJob(i);
            }
        }

        public void CreateJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        {
            BackupJob job = BackupJobFactory.CreateJob(name, sourceDirectory, targetDirectory, type);
            SubscribeToJobEvents(job);
            _jobs.Add(job);
            SaveJobs();
        }

        public void DeleteJob(BackupJob job)
        {
            if (job != null && _jobs.Contains(job))
            {
                _jobs.Remove(job);
                SaveJobs();
            }
        }

        private void HandleJobStateChanged(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
            {
                StateManager.UpdateState(job);
            }
        }

        private void HandleJobLog(BackupJob job, int timeMs, int encryptionTimeMs)
        {
            if (job == null) return;

            long currentFileSize = 0;
            if (!string.IsNullOrWhiteSpace(job.CurrentSourceFile) && File.Exists(job.CurrentSourceFile))
            {
                currentFileSize = new FileInfo(job.CurrentSourceFile).Length;
            }

            EasyLogger.LogAction(
                job.Name,
                job.CurrentSourceFile,
                job.CurrentTargetFile,
                currentFileSize,
                timeMs,
                encryptionTimeMs
            );
        }
    }
}
