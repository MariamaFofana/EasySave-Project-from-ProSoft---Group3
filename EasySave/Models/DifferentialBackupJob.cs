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
                // Analyze differences to determine files to copy
                var allSourceFiles = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                var filesToCopy = new List<string>();

                TotalSize = 0; // Reset total size

                foreach (var sourceFile in allSourceFiles)
                {
                    var relativePath = Path.GetRelativePath(SourceDirectory, sourceFile);
                    var targetFile = Path.Combine(TargetDirectory, relativePath);

                    // Differential condition: target doesn't exist OR source is newer
                    if (!File.Exists(targetFile) || File.GetLastWriteTime(sourceFile) > File.GetLastWriteTime(targetFile))
                    {
                        filesToCopy.Add(sourceFile);
                        TotalSize += new FileInfo(sourceFile).Length;
                    }
                }

                // Initialize state with the affected files
                TotalFiles = filesToCopy.Count;
                FilesLeft = TotalFiles;
                SizeLeft = TotalSize;
                Progression = 0;

                // Terminate early if no files were modified
                if (TotalFiles == 0)
                {
                    Status = JobStatus.Completed;
                    TriggerStateChanged();
                    return;
                }

                // Ready to copy
                TriggerStateChanged();

                // Copy loop
                foreach (var file in filesToCopy)
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
                    Progression = ((TotalFiles - FilesLeft) * 100) / TotalFiles;

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
    }
}