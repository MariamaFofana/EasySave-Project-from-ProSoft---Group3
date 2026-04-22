using System;
using System.IO;
using System.Text.Json;
using EasySave.Models;

namespace EasySave.Utils
{
    public static class StateManager
    {
        private static readonly string StateFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "state.json");

        private static readonly object _fileLock = new object();


        public static void UpdateState(BackupJob job)
        {
            lock (_fileLock)
            {
                try
                {
                    string? directory = Path.GetDirectoryName(StateFilePath);
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    var stateData = new
                    {
                        Name = job.Name,
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = job.Status.ToString(),
                        TotalFiles = job.TotalFiles,
                        TotalSize = job.TotalSize,
                        Progression = job.Progression,
                        FilesRemaining = job.FilesLeft,
                        SizeRemaining = job.SizeLeft,
                        SourcePath = job.CurrentSourceFile,
                        DestinationPath = job.CurrentTargetFile
                    };

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(stateData, options);

                    File.WriteAllText(StateFilePath, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CRITICAL] State update failed: {ex.Message}");
                }
            }
        }
    }
}