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
    /// <summary>
    /// The core orchestrator of the application.
    /// Manages the job collection, cross-view navigation, and global services initialization.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;
        private ObservableCollection<BackupJob> _jobs;
        private string _configPath;
        private readonly MonitoringService _monitoringService; // Added for system monitoring

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

        // Constructor of the MainViewModel class
        public MainViewModel()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // The project structure requires us to go up 3 levels to reach the source folders from the build output
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            
            _jobs = new ObservableCollection<BackupJob>();
            _configPath = Path.Combine(projectDir, "Ressources", "config.json");

            // Centralized logging initialization
            string logPath = Path.Combine(projectDir, "Logs");
            EasyLogger.Configure(logPath);
            
            // Synchronize logger with current user preferences
            EasyLogger.LogFormat = SettingsManager.CurrentSettings.LogFormat;
            LanguageManager.GetInstance().CurrentLanguage = SettingsManager.CurrentSettings.Language;

            // Locate the CryptoSoft binary based on environment (development vs production)
            string cryptoPath = Path.Combine(baseDir, "CryptoSoft.exe");
            if (!File.Exists(cryptoPath))
            {
                cryptoPath = Path.Combine(projectDir, "..", "CryptoSoft", "bin", "Debug", "net8.0", "CryptoSoft.exe");
            }
            
            EncryptionService.Configure(cryptoPath, "EasySaveKey", SettingsManager.CurrentSettings.ExtensionsToEncrypt);

            // Restore saved state
            LoadJobs();

            // Initial view state
            _currentPage = new BackupListViewModel(this);

            // Initialize and start the monitoring service
            _monitoringService = new MonitoringService();
            _monitoringService.StartMonitoring();
        }


        // Call subscription and unsubscription methods for job events to track their progress and logs
        private void SubscribeToJobEvents(BackupJob job)
        {
            job.OnStateChanged += HandleJobStateChanged;
            job.OnFileCopied += HandleJobLog;
        }

        /// <summary>
        /// Navigation methods to switch between different application screens.
        /// </summary>
        public void NavigateToBackupList() => CurrentPage = new BackupListViewModel(this);
        public void NavigateToEditJob() => CurrentPage = new EditJobViewModel(this);
        public void NavigateToSettings() => CurrentPage = new SettingViewModel();

        /// <summary>
        /// Gracefully closes the application across different platforms.
        /// </summary>
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

        /// <summary>
        /// Data Transfer Object used for JSON persistence.
        /// Keeps the persistence logic decoupled from the rich Model classes.
        /// </summary>
        private class BackupJobConfig
        {
            public string Name { get; set; } = string.Empty;
            public string SourceDirectory { get; set; } = string.Empty;
            public string TargetDirectory { get; set; } = string.Empty;
            public BackupType Type { get; set; }
        }

        /// <summary>
        /// Restores the backup jobs from the local JSON configuration file.
        /// Reconstructs the Model objects and attaches necessary event handlers.
        /// </summary>
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

                    // Hook into job events for real-time state tracking and logging
                    SubscribeToJobEvents(job);
                    _jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading jobs: {ex.Message}");
            }
        }

        /// <summary>
        /// Synchronizes the current job list to the local persistence storage.
        /// </summary>
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

        /// <summary>
        /// Triggers the background execution of a specific backup task.
        /// </summary>
        public void ExecuteJob(int index)
        {
            if (index < 0 || index >= _jobs.Count) return;

            // Check for business software before starting
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
                // Re-check before each job in sequential execution
                if (_monitoringService.IsAnyBusinessSoftwareRunning())
                {
                    Console.WriteLine("\n[Warning] Sequential backup suspended: Business software detected.");
                    break;
                }
                ExecuteJob(i);
            }
        }

        /// <summary>
        /// Sequentially runs only the jobs that are currently selected.
        /// </summary>
        public void ExecuteSelectedJobs()
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (!_jobs[i].IsSelected) continue;

                // Re-check before each job in sequential execution
                if (_monitoringService.IsAnyBusinessSoftwareRunning())
                {
                    Console.WriteLine("\n[Warning] Sequential backup suspended: Business software detected.");
                    break;
                }
                ExecuteJob(i);
            }
        }

        /// <summary>
        /// Adds a new job to the system and persists the change.
        /// </summary>
        public void CreateJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        {
            // MODIFICATION: Removed the limit check (jobs.Count >= 5)
            BackupJob job = BackupJobFactory.CreateJob(name, sourceDirectory, targetDirectory, type);
            SubscribeToJobEvents(job);
            _jobs.Add(job);
            SaveJobs();
        }

        /// <summary>
        /// Removes a job from the system and updates persistence.
        /// </summary>
        public void DeleteJob(BackupJob job)
        {
            if (job != null && _jobs.Contains(job))
            {
                _jobs.Remove(job);
                SaveJobs();
            }
        }

        /// <summary>
        /// Updates the real-time state file whenever a job's internal state changes.
        /// </summary>
        private void HandleJobStateChanged(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
            {
                StateManager.UpdateState(job);
            }
        }

        /// <summary>
        /// Processes file transfer completion events to generate historical log entries.
        /// </summary>
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