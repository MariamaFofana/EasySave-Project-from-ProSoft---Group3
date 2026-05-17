using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia.Threading;
using EasySave.Services;

namespace EasySave.Models
{
    public class DifferentialBackupJob : BackupJob
    {
        private CancellationTokenSource _cts;
        private ManualResetEventSlim _pauseEvent = new(true);

        public override void Execute()
        {
            _cts = new CancellationTokenSource();

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
                var allSourceFiles = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);
                var filesToCopy = new List<string>();

                TotalSize = 0;

                foreach (var sourceFile in allSourceFiles)
                {
                    var relativePath = Path.GetRelativePath(SourceDirectory, sourceFile);
                    var targetFile = Path.Combine(TargetDirectory, relativePath);

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

                if (TotalFiles == 0)
                {
                    Status = JobStatus.Completed;
                    TriggerStateChanged();
                    return;
                }

                TriggerStateChanged();

                foreach (var file in filesToCopy)
                {
                    if (_cts.Token.IsCancellationRequested)
                    {
                        Status = JobStatus.Stopped;
                        TriggerStateChanged();
                        return;
                    }

                    _pauseEvent.Wait(_cts.Token);

                    if (MonitoringService.Instance != null && MonitoringService.Instance.IsAnyBusinessSoftwareRunning())
                    {
                        Status = JobStatus.Paused;
                        ErrorMessage = "Paused: business software detected";
                        TriggerStateChanged();

                        while (MonitoringService.Instance.IsAnyBusinessSoftwareRunning())
                        {
                            Thread.Sleep(1000);
                            if (_cts.Token.IsCancellationRequested)
                            {
                                Status = JobStatus.Stopped;
                                TriggerStateChanged();
                                return;
                            }
                        }

                        Status = JobStatus.Active;
                        ErrorMessage = string.Empty;
                        TriggerStateChanged();
                    }

                    FileInfo fileInfo = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(SourceDirectory, file);
                    var targetPath = Path.Combine(TargetDirectory, relativePath);

                    CurrentSourceFile = file;
                    CurrentTargetFile = targetPath;

                    string? targetDir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetDir))
                        Directory.CreateDirectory(targetDir);

                    TransferToken? token = null;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    int transferTimeMs;
                    int encryptionTimeMs = 0;

                    try
                    {
                        token = TransferOrchestrator.RequestTransfer(file, fileInfo.Length, _cts.Token);

                        File.Copy(file, targetPath, true);
                        stopwatch.Stop();
                        transferTimeMs = (int)stopwatch.ElapsedMilliseconds;

                        encryptionTimeMs = EncryptionService.EncryptIfNeeded(targetPath);
                    }
                    catch (OperationCanceledException)
                    {
                        stopwatch.Stop();
                        Status = JobStatus.Stopped;
                        TriggerStateChanged();
                        return;
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        transferTimeMs = -1;
                        ErrorMessage = ex.Message;
                    }
                    finally
                    {
                        if (token != null)
                            TransferOrchestrator.ReleaseTransfer(token);
                    }

                    TriggerFileCopied(transferTimeMs, encryptionTimeMs);

                    // Update progress on UI thread
                    int filesLeft = FilesLeft - 1;
                    long sizeLeft = SizeLeft - fileInfo.Length;
                    int progression = TotalFiles > 0 ? ((TotalFiles - filesLeft) * 100) / TotalFiles : 100;

                    Dispatcher.UIThread.Post(() =>
                    {
                        FilesLeft = filesLeft;
                        SizeLeft = sizeLeft;
                        Progression = progression;
                        TriggerStateChanged();
                    });
                }

                if (Status != JobStatus.Error && Status != JobStatus.Stopped)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        Status = JobStatus.Completed;
                        TriggerStateChanged();
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Status = JobStatus.Error;
                    ErrorMessage = ex.Message;
                    TriggerStateChanged();
                });
            }
        }

        public override void Play()
        {
            if (Status == JobStatus.Paused)
            {
                Status = JobStatus.Active;
                _pauseEvent.Set();
                TriggerStateChanged();
            }
            else
            {
                Execute();
            }
        }

        public override void Pause()
        {
            if (Status == JobStatus.Active)
            {
                Status = JobStatus.Paused;
                _pauseEvent.Reset();
                TriggerStateChanged();
            }
        }

        public override void Stop()
        {
            _cts?.Cancel();
            _pauseEvent.Set();
        }
    }
}
