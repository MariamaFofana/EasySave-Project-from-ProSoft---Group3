using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace EasySave.Models
{
    public class DifferentialBackupJob : BackupJob
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
                // 2. Analyser les différences pour calculer ce qu'il faut vraiment copier
                var allSourceFiles = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                var filesToCopy = new List<string>();

                TotalSize = 0; // On réinitialise pour être sûr

                foreach (var sourceFile in allSourceFiles)
                {
                    var relativePath = Path.GetRelativePath(SourceDirectory, sourceFile);
                    var targetFile = Path.Combine(TargetDirectory, relativePath);

                    // Condition Différentielle : n'existe pas cible OU source plus récente
                    if (!File.Exists(targetFile) || File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(targetFile))
                    {
                        filesToCopy.Add(sourceFile);
                        TotalSize += new FileInfo(sourceFile).Length;
                    }
                }

                // Initialisation de l'état avec seulement les fichiers concernés
                TotalFiles = filesToCopy.Count;
                FilesLeft = TotalFiles;
                SizeLeft = TotalSize;
                Progression = 0;

                // Si aucun fichier n'a été modifié, on termine directement
                if (TotalFiles == 0)
                {
                    Status = JobStatus.Completed;
                    TriggerStateChanged();
                    return;
                }

                // Prêt à copier
                TriggerStateChanged();

                // 3. Boucle de copie
                foreach (var file in filesToCopy)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(SourceDirectory, file);
                    var targetPath = Path.Combine(TargetDirectory, relativePath);

                    // Mise à jour de l'état
                    CurrentSourceFile = file;
                    CurrentTargetFile = targetPath;

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    // Chronomètre toujours prêt pour le futur Logger
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    File.Copy(file, targetPath, true);
                    stopwatch.Stop();

                    // 4. Mettre à jour l'avancement
                    FilesLeft--;
                    SizeLeft -= fileInfo.Length;
                    Progression = ((TotalFiles - FilesLeft) * 100) / TotalFiles;

                    // On alerte l'interface / StateManager
                    TriggerStateChanged();
                }

                Status = JobStatus.Completed;
                TriggerStateChanged();
            }
            catch (Exception)
            {
                Status = JobStatus.Error;
                TriggerStateChanged();
            }
        }
    }
}