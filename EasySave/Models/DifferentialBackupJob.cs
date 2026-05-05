using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using EasySave.Services;

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
                // Analyze differences
                var allSourceFiles = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                var filesToCopy = new List<string>();

                TotalSize = 0;

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

                TotalFiles = filesToCopy.Count;
                FilesLeft = TotalFiles;
                SizeLeft = TotalSize;
                Progression = 0;

                // No files modified: done immediately
                if (TotalFiles == 0)
                {
                    Status = JobStatus.Completed;
                    TriggerStateChanged();
                    return;
                }

                TriggerStateChanged();

                // Copy loop
                foreach (var file in filesToCopy)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(SourceDirectory, file);
                    var targetPath = Path.Combine(TargetDirectory, relativePath);

                    CurrentSourceFile = file;
                    CurrentTargetFile = targetPath;

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

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
                        if (encryptionTimeMs < 0) throw new Exception("Encryption failed.");
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        transferTimeMs = -1;
                        Status = JobStatus.Error;
                        ErrorMessage = ex.Message;
                        TriggerFileCopied(transferTimeMs, encryptionTimeMs);
                        TriggerStateChanged();
                    }

                    TriggerFileCopied(transferTimeMs, encryptionTimeMs);


                    // Update progress
                    FilesLeft--;
                    SizeLeft -= fileInfo.Length;
                    Progression = TotalFiles > 0 ? ((TotalFiles - FilesLeft) * 100) / TotalFiles : 100;

                    TriggerStateChanged();
                }
                if(Status != JobStatus.Error)
                    Status = JobStatus.Completed;
                TriggerStateChanged();
            }
            catch (Exception ex)
            {
                Status = JobStatus.Error;
                ErrorMessage = ex.Message;
                TriggerStateChanged();
            }

        }
        public override void Play() => Execute();
        public override void Pause() { /* TODO */ }
        public override void Stop() { /* TODO */ }
    }
}

