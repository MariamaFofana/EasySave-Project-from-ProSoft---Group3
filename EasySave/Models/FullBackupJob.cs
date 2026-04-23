using System;
using System.IO;
using System.Linq;
using System.Diagnostics; // Required for Stopwatch

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
                    try
                    {
                        File.Copy(file, targetPath, true);
                        stopwatch.Stop();
                        TriggerFileCopied((int)stopwatch.ElapsedMilliseconds);
                    }
                    catch (Exception)
                    {
                        stopwatch.Stop();
                        TriggerFileCopied(-1);
                    }

                    // Update progress state
                    FilesLeft--;
                    SizeLeft -= fileInfo.Length;

                    // Prevent division by zero if directory is empty
                    Progression = TotalFiles > 0 ? ((TotalFiles - FilesLeft) * 100) / TotalFiles : 100;

                    // Trigger state change event
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