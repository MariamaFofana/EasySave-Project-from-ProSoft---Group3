using System;
using System.Collections.Generic;
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
        private List<BackupJob> jobs;
        private string configPath;

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<BackupJob> Jobs => jobs.AsReadOnly();

        public MainViewModel()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            jobs = new List<BackupJob>();

            configPath = Path.Combine(baseDir, "Ressources", "config.json");

            // INITIALIZE THE LOGGER HERE
            string logPath = Path.Combine(baseDir, "logs");
            EasyLogger.Configure(logPath);

            
            // Apply Settings
            EasyLogger.LogFormat = SettingsManager.CurrentSettings.LogFormat;
            LanguageManager.GetInstance().CurrentLanguage = SettingsManager.CurrentSettings.Language;

            // Initialize Encryption Service

            string cryptoPath = Path.Combine(baseDir, "CryptoSoft.exe");
            
            if (!File.Exists(cryptoPath))
            {
                string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
                cryptoPath = Path.Combine(projectRoot, "CryptoSoft", "bin", "Debug", "net8.0", "CryptoSoft.exe");
            }
            
            EncryptionService.Configure(cryptoPath, "EasySaveKey", SettingsManager.CurrentSettings.ExtensionsToEncrypt);


            // Load Jobs
            LoadJobs();

            // Page par défaut au démarrage
            _currentPage = new BackupListViewModel(this);
        }

        // Méthodes appelées par les boutons de la MainWindow
        public void NavigateToBackupList() => CurrentPage = new BackupListViewModel(this);
        public void NavigateToEditJob() => CurrentPage = new EditJobViewModel(this);
        public void NavigateToSettings() => CurrentPage = new SettingViewModel();




        public void Quit()
        {
            // On écrit "Avalonia.Application" explicitement
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

        // Internal class used only for JSON configuration backup
        private class BackupJobConfig
        {
            public string Name { get; set; } = string.Empty;
            public string SourceDirectory { get; set; } = string.Empty;
            public string TargetDirectory { get; set; } = string.Empty;
            public BackupType Type { get; set; }
        }

        // Call subscription and unsubscription methods for job events to track their progress and logs
        private void SubscribeToJobEvents(BackupJob job)
        {
            job.OnStateChanged += HandleJobStateChanged;
            job.OnFileCopied += HandleJobLog;
        }
        private void UnsubscribeFromJobEvents(BackupJob job)
        {
            job.OnStateChanged -= HandleJobStateChanged;
            job.OnFileCopied -= HandleJobLog;
        }
        private void HandleJobStateChanged(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
            {
                StateManager.UpdateState(job);
                Console.Write($"\rProgression : {job.Progression}%");
            }
        }

        // Methods required for job management
        // Load jobs from configuration
        public void LoadJobs()
        {
            jobs = new List<BackupJob>();

            if (!File.Exists(configPath))
            {
                return;
            }

            string json = File.ReadAllText(configPath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            List<BackupJobConfig>? savedJobs = JsonSerializer.Deserialize<List<BackupJobConfig>>(json);

            if (savedJobs == null)
            {
                return;
            }

            foreach (BackupJobConfig savedJob in savedJobs)
            {
                if (savedJob == null) continue;

                BackupJob job = BackupJobFactory.CreateJob(
                    savedJob.Name ?? string.Empty,
                    savedJob.SourceDirectory ?? string.Empty,
                    savedJob.TargetDirectory ?? string.Empty,
                    savedJob.Type
                );

                jobs.Add(job);
            }
        }
        // Save the list of jobs to the configuration
        public void SaveJobs()
        {
            List<BackupJobConfig> savedJobs = jobs
                .Select(job => new BackupJobConfig
                {
                    Name = job.Name,
                    SourceDirectory = job.SourceDirectory,
                    TargetDirectory = job.TargetDirectory,
                    Type = job.Type
                })
                .ToList();

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(savedJobs, options);
            File.WriteAllText(configPath, json);

        }

        // Run a single job from its index 
        public void ExecuteJob(int index)
        {
            if (index < 0 || index >= jobs.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid job index.");
            }

            BackupJob job = jobs[index];

            UnsubscribeFromJobEvents(job);
            SubscribeToJobEvents(job);

            job.Execute();

            UnsubscribeFromJobEvents(job);
        }
        // Execute all jobs in the list in order
        public void ExecuteAllJobs()
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                ExecuteJob(i);
            }
        }

        // Create a new job
        public void CreateJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        {
            BackupJob job = BackupJobFactory.CreateJob(name, sourceDirectory, targetDirectory, type);
            jobs.Add(job);
            SaveJobs();
        }

        // Delete a job
        public void DeleteJob(BackupJob job)
        {
            if (job != null && jobs.Contains(job))
            {
                jobs.Remove(job);
                SaveJobs();
            }
        }


        // Get the progress of a job during its execution
        private void HandleJobProgress(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
            {
                StateManager.UpdateState(job);
            }
        }
        // Receive log info from the job and forward it to the logger
        private void HandleJobLog(BackupJob job, int timeMs, int encryptionTimeMs)
        {
            if (job != null)
            {
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
}
