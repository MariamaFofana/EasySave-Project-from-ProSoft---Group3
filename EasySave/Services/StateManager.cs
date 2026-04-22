using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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

                    // Retrieve existing states to prevent overwriting other jobs
                    List<StateRecord> allStates = new List<StateRecord>();
                    if (File.Exists(StateFilePath))
                    {
                        string existingJson = File.ReadAllText(StateFilePath);
                        if (!string.IsNullOrWhiteSpace(existingJson))
                        {
                            allStates = JsonSerializer.Deserialize<List<StateRecord>>(existingJson) ?? new List<StateRecord>();
                        }
                    }

                    // Prepare the updated state data for the current job
                    var newState = new StateRecord
                    {
                        Name = job.Name,
                        SourceFilePath = job.CurrentSourceFile ?? "",
                        TargetFilePath = job.CurrentTargetFile ?? "",
                        State = job.Status.ToString(),
                        TotalFilesToCopy = job.TotalFiles,
                        TotalFilesSize = job.TotalSize,
                        NbFilesLeftToDo = job.FilesLeft,
                        Progression = job.Progression
                    };

                    // Update the existing job entry or add it if not found
                    int index = allStates.FindIndex(s => s.Name == job.Name);
                    if (index != -1)
                    {
                        allStates[index] = newState;
                    }
                    else
                    {
                        allStates.Add(newState);
                    }

                    // Serialize and write the updated states list back to the JSON file
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(allStates, options);
                    File.WriteAllText(StateFilePath, jsonString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CRITICAL] State update failed: {ex.Message}");
                }
            }
        }

        // Internal DTO representing the expected JSON structure
        private class StateRecord
        {
            [JsonPropertyName("Name")]
            public string Name { get; set; }

            [JsonPropertyName("SourceFilePath")]
            public string SourceFilePath { get; set; }

            [JsonPropertyName("TargetFilePath")]
            public string TargetFilePath { get; set; }

            [JsonPropertyName("State")]
            public string State { get; set; }

            [JsonPropertyName("TotalFilesToCopy")]
            public int TotalFilesToCopy { get; set; }

            [JsonPropertyName("TotalFilesSize")]
            public long TotalFilesSize { get; set; }

            [JsonPropertyName("NbFilesLeftToDo")]
            public int NbFilesLeftToDo { get; set; }

            [JsonPropertyName("Progression")]
            public int Progression { get; set; }
        }
    }
}