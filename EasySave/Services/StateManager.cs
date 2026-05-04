using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.Utils
{
    public static class StateManager
    {
        private static readonly string StateFilePathJson = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "state.json");

        private static readonly string StateFilePathXml = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave", "state.xml");

        private static readonly object _fileLock = new object();


        public static void UpdateState(BackupJob job)
        {
            lock (_fileLock)
            {
                try
                {
                    bool isXml = string.Equals(SettingsManager.CurrentSettings.LogFormat, "xml", StringComparison.OrdinalIgnoreCase);
                    string stateFilePath = isXml ? StateFilePathXml : StateFilePathJson;
                    
                    string? directory = Path.GetDirectoryName(stateFilePath);
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // Retrieve existing states to prevent overwriting other jobs
                    List<StateRecord> allStates = new List<StateRecord>();
                    if (File.Exists(stateFilePath))
                    {
                        if (isXml)
                        {
                            try
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(List<StateRecord>));
                                using (StreamReader reader = new StreamReader(stateFilePath, System.Text.Encoding.UTF8))
                                {
                                    var result = serializer.Deserialize(reader) as List<StateRecord>;
                                    if (result != null) allStates = result;
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            string existingJson = File.ReadAllText(stateFilePath);
                            if (!string.IsNullOrWhiteSpace(existingJson))
                            {
                                allStates = JsonSerializer.Deserialize<List<StateRecord>>(existingJson) ?? new List<StateRecord>();
                            }
                        }
                    }

                    // Prepare the updated state data for the current job
                    bool isEnd = job.Status == JobStatus.Completed;
                    string stateStr = job.Status switch
                    {
                        JobStatus.Inactive => "INACTIVE",
                        JobStatus.Active => "ACTIVE",
                        JobStatus.Completed => "END",
                        JobStatus.Error => "ERROR",
                        _ => job.Status.ToString().ToUpper()
                    };

                    var newState = new StateRecord
                    {
                        Name = job.Name,
                        SourceFilePath = isEnd ? "" : (job.CurrentSourceFile ?? ""),
                        TargetFilePath = isEnd ? "" : (job.CurrentTargetFile ?? ""),
                        State = stateStr,
                        TotalFilesToCopy = isEnd ? 0 : job.TotalFiles,
                        TotalFilesSize = isEnd ? 0 : job.TotalSize,
                        NbFilesLeftToDo = isEnd ? 0 : job.FilesLeft,
                        Progression = isEnd ? 0 : job.Progression
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

                    // Serialize and write the updated states list back
                    if (isXml)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<StateRecord>));
                        using (StreamWriter writer = new StreamWriter(stateFilePath, false, System.Text.Encoding.UTF8))
                        {
                            serializer.Serialize(writer, allStates);
                        }
                    }
                    else
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(allStates, options);
                        File.WriteAllText(stateFilePath, jsonString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CRITICAL] State update failed: {ex.Message}");
                }
            }
        }

        // Internal DTO representing the expected JSON structure
        public class StateRecord
        {
            [JsonPropertyName("Name")]
            [XmlElement("Name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("SourceFilePath")]
            [XmlElement("SourceFilePath")]
            public string SourceFilePath { get; set; } = string.Empty;

            [JsonPropertyName("TargetFilePath")]
            [XmlElement("TargetFilePath")]
            public string TargetFilePath { get; set; } = string.Empty;

            [JsonPropertyName("State")]
            [XmlElement("State")]
            public string State { get; set; } = string.Empty;

            [JsonPropertyName("TotalFilesToCopy")]
            [XmlElement("TotalFilesToCopy")]
            public int TotalFilesToCopy { get; set; }

            [JsonPropertyName("TotalFilesSize")]
            [XmlElement("TotalFilesSize")]
            public long TotalFilesSize { get; set; }

            [JsonPropertyName("NbFilesLeftToDo")]
            [XmlElement("NbFilesLeftToDo")]
            public int NbFilesLeftToDo { get; set; }

            [JsonPropertyName("Progression")]
            [XmlElement("Progression")]
            public int Progression { get; set; }
        }
    }
}