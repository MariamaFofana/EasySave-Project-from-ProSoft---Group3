using System;
using System.IO;
using System.Linq;
using System.Diagnostics; // Requis pour utiliser Stopwatch (Chronomètre)

namespace EasySave.Models
{
    public class FullBackupJob : BackupJob
    {
        public override void Execute()
        {
            // 1. Validation avant exécution
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(SourceDirectory) || string.IsNullOrEmpty(TargetDirectory))
            {
                Status = JobStatus.Error;
                TriggerStateChanged();
                return;
            }

            Status = JobStatus.Active;
            TriggerStateChanged();

            try
            {
                // 2. Lister tous les fichiers et calculer les totaux
                var allFiles = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                TotalFiles = allFiles.Length;
                TotalSize = allFiles.Sum(file => new FileInfo(file).Length);

                // Initialisation des compteurs "Restants" (Utile pour le StateManager)
                FilesLeft = TotalFiles;
                SizeLeft = TotalSize;
                Progression = 0;

                // On prévient l'interface que le calcul initial est terminé et que la copie va commencer
                TriggerStateChanged();

                // 3. Boucle de copie
                foreach (var file in allFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(SourceDirectory, file);
                    var targetPath = Path.Combine(TargetDirectory, relativePath);

                    // Mise à jour des fichiers en cours pour le StateManager
                    CurrentSourceFile = file;
                    CurrentTargetFile = targetPath;

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    // Chronométrer la copie spécifiquement pour le EasyLogger (transferTimeMs)
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    File.Copy(file, targetPath, true);
                    stopwatch.Stop();

                    // 4. Mettre à jour l'état d'avancement interne
                    FilesLeft--;
                    SizeLeft -= fileInfo.Length;
                    // Protection contre la division par zéro si le dossier est vide
                    Progression = TotalFiles > 0 ? ((TotalFiles - FilesLeft) * 100) / TotalFiles : 100;

                    // 5. Déclencher les événements pour le MainViewModel

                    // Alerte le StateManager de mettre à jour le fichier d'état en temps réel
                    TriggerStateChanged();
                }

                Status = JobStatus.Completed;
                TriggerStateChanged();
            }
            catch (Exception ex)
            {
                Status = JobStatus.Error;
                TriggerStateChanged();
            }
        }
    }
}