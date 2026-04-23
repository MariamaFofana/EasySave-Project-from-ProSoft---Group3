using EasyLogDLL;
using EasySave.Factories;
using EasySave.Models;
using EasySave.Services;
using System.Text.Json;
//la classe mainviewmodel reçoit, coordonne, délègue, voir 
namespace EasySave.ViewModels
{
    public class MainViewModel
    {
        //les diferentes attributs pour les jobs
        private List<BackupJob> jobs;
        private string configPath;
        //lecture seule de la liste des jobs pour les autres classes
        public IReadOnlyList<BackupJob> Jobs => jobs.AsReadOnly();
        //constructeur de la classe mainviewmodel
        public MainViewModel()
        {
            jobs = new List<BackupJob>();
            //configPath = "config.json"; 
            configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        }
        // Classe interne utilisée uniquement pour la sauvegarde JSON de la configuration
        private class BackupJobConfig
        {
            public string Name { get; set; } = string.Empty;
            public string SourceDirectory { get; set; } = string.Empty;
            public string TargetDirectory { get; set; } = string.Empty;
            public BackupType Type { get; set; }
        }

        //appel aux méthodes d'abonnement et de désabonnement aux événements des jobs pour suivre leur progression et les logs
        private void SubscribeToJobEvents(BackupJob job)
        {
            job.OnStateChanged += HandleJobStateChanged;
        }
        private void UnsubscribeFromJobEvents(BackupJob job)
        {
            job.OnStateChanged -= HandleJobStateChanged;
        }
        private void HandleJobStateChanged(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
            {
                StateManager.UpdateState(job);

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
                    0
                );
            }
        }

        //les methodes necessaires pour la gestion des jobs
        //Charger les jobs de la configuration
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

            foreach (BackupJobConfig savedJob in savedJobs.Take(5))
            {
                BackupJob job = BackupJobFactory.CreateJob(
                    savedJob.Name,
                    savedJob.SourceDirectory,
                    savedJob.TargetDirectory,
                    savedJob.Type
                );

                jobs.Add(job);
            }
        }
        //Sauvegarder la liste des jobs dans la configuration
        public void SaveJobs()
        {
            List<BackupJobConfig> savedJobs = jobs
                .Take(5)
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
        
        //Lancer un seul job depuis son index 
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
        //excuter tous les jobs de la liste par ordre
        public void ExecuteAllJobs()
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                ExecuteJob(i);
            }
        }
        //avoir la progression d’un job pendant son exécution
        private void HandleJobProgress(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
            {
                StateManager.UpdateState(job);
            }
        }
        //Recevoir une info de log depuis le job, transmettre au logger
        private void HandleJobLog(object? sender, EventArgs args)
        {
            if (sender is BackupJob job)
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
                    0
                );
            }
        }
        

    }
}
