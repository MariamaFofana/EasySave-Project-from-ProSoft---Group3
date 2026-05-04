using EasyLogDLL;
using EasySave.Factories;
using EasySave.Models;
using EasySave.Utils;
using System.Text.Json;
using EasySave.Services;

// The MainViewModel class receives, coordinates, and delegates
namespace EasySave.ViewModels
{
    public class MainViewModel
    {
        // The different attributes for the jobs
        private List<BackupJob> jobs;
        private string configPath;
        private readonly MonitoringService _monitoringService; // Added for system monitoring

        // Read-only list of jobs for other classes
        public IReadOnlyList<BackupJob> Jobs => jobs.AsReadOnly();

        // Constructor of the MainViewModel class
        public MainViewModel()
        {
            jobs = new List<BackupJob>();
            // Use AppData to properly centralize everything
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave");
            Directory.CreateDirectory(appData);

            configPath = Path.Combine(appData, "config.json");

            // INITIALIZE THE LOGGER HERE
            string logPath = Path.Combine(appData, "Logs");
            EasyLogger.Configure(logPath);

            // Apply Settings
            EasyLogger.LogFormat = SettingsManager.CurrentSettings.LogFormat;
            LanguageManager.GetInstance().CurrentLanguage = SettingsManager.CurrentSettings.Language;

            // Initialize and start the monitoring service
            string[] businessSoftware = SettingsManager.CurrentSettings.BusinessSoftware ?? Array.Empty<string>();
            _monitoringService = new MonitoringService(businessSoftware);
            _monitoringService.StartMonitoring();
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

            // MODIFICATION: Removed .Take(5) to allow unlimited jobs
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
            // MODIFICATION: Removed .Take(5) to save all jobs
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

            // MODIFICATION: Check for business software before starting
            if (_monitoringService.IsAnyBusinessSoftwareRunning())
            {
                Console.WriteLine("\n[Warning] Backup prevented: Business software detected.");
                return;
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
                // MODIFICATION: Re-check before each job in sequential execution
                if (_monitoringService.IsAnyBusinessSoftwareRunning())
                {
                    Console.WriteLine("\n[Warning] Sequential backup suspended: Business software detected.");
                    break;
                }
                ExecuteJob(i);
            }
        }

        // Create a new job
        public void CreateJob(string name, string sourceDirectory, string targetDirectory, BackupType type)
        {
            // MODIFICATION: Removed the limit check (jobs.Count >= 5)
            BackupJob job = BackupJobFactory.CreateJob(name, sourceDirectory, targetDirectory, type);
            jobs.Add(job);
            SaveJobs();
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
                    timeMs
                );
            }
        }
    }
}