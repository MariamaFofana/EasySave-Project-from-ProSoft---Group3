using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using EasySave.Services;

namespace EasySave.Models
{
    public class FullBackupJob : BackupJob
    {
        public override void Execute()
        {
            // Pre-execution validation
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
                // List all files and calculate totals
                var allFiles = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                TotalFiles = allFiles.Length;
                TotalSize = allFiles.Sum(file => new FileInfo(file).Length);

                // Initialize remaining counters
                FilesLeft = TotalFiles;
                SizeLeft = TotalSize;
                Progression = 0;

                // Trigger state change after initial calculation
                TriggerStateChanged();

                // Copy loop
                foreach (var file in allFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(SourceDirectory, file);
                    var targetPath = Path.Combine(TargetDirectory, relativePath);

                    // Update current state files
                    CurrentSourceFile = file;
                    CurrentTargetFile = targetPath;

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    // Start stopwatch for logging transfer time
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    int transferTimeMs;
                    int encryptionTimeMs = 0;

                    try
                    {
                        File.Copy(file, targetPath, true);
                        stopwatch.Stop();
                        transferTimeMs = (int)stopwatch.ElapsedMilliseconds;

                        // Encrypt if needed (v2.0 addition)
                        encryptionTimeMs = EncryptionService.EncryptIfNeeded(targetPath);
                    }
                    catch (Exception)
                    {
                        stopwatch.Stop();
                        transferTimeMs = -1;
                    }

                    TriggerFileCopied(transferTimeMs, encryptionTimeMs);

                    // Update progress state
                    FilesLeft--;
                    SizeLeft -= fileInfo.Length;
                    Progression = TotalFiles > 0 ? ((TotalFiles - FilesLeft) * 100) / TotalFiles : 100;

                    // Trigger state change event
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
        public override void Play() => Execute();
        public override void Pause() { /* TODO */ }
        public override void Stop() { /* TODO */ }
    }
}

