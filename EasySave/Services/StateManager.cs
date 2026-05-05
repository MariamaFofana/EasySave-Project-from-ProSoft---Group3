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
    /// <summary>
    /// Manages the "Active Log" or real-time state of all backup jobs.
    /// This file is updated during execution to allow external monitoring of progress.
    /// </summary>
    public static class StateManager
    {
        // Compute path relative to project root
        private static readonly string _logDir = Path.Combine(
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")), 
            "Logs");

        private static readonly string _stateFilePathJson = Path.Combine(_logDir, "state.json");
        private static readonly string _stateFilePathXml = Path.Combine(_logDir, "state.xml");

        // Prevents file corruption when multiple jobs try to update their state simultaneously
        private static readonly object _fileLock = new object();

        /// <summary>
        /// Updates or adds the state of a specific job in the global state file.
        /// Handles both JSON and XML formats based on user settings.
        /// </summary>
        public static void UpdateState(BackupJob job)
        {
            lock (_fileLock)
            {
                try
                {
                    bool isXml = string.Equals(SettingsManager.CurrentSettings.LogFormat, "xml", StringComparison.OrdinalIgnoreCase);
                    string stateFilePath = isXml ? _stateFilePathXml : _stateFilePathJson;
                    
                    // Ensure the Logs directory exists
                    string? directory = Path.GetDirectoryName(stateFilePath);
                    if (directory != null && !Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    List<StateRecord> allStates = LoadCurrentStates(stateFilePath, isXml);

                    // Map internal JobStatus to the business-required string values (ACTIVE, END, etc.)
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

                    // Upsert logic: update if exists, otherwise add new
                    int index = allStates.FindIndex(s => s.Name == job.Name);
                    if (index != -1)
                        allStates[index] = newState;
                    else
                        allStates.Add(newState);

                    SaveStates(stateFilePath, allStates, isXml);
                }
                catch (Exception ex)
                {
                    // Fail-safe to prevent backup interruption due to logging issues
                    Console.WriteLine($"[CRITICAL] State update failed: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads existing states from the disk to preserve other jobs' information.
        /// </summary>
        private static List<StateRecord> LoadCurrentStates(string path, bool isXml)
        {
            if (!File.Exists(path)) return new List<StateRecord>();

            try
            {
                if (isXml)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<StateRecord>));
                    using (StreamReader reader = new StreamReader(path, System.Text.Encoding.UTF8))
                    {
                        return serializer.Deserialize(reader) as List<StateRecord> ?? new List<StateRecord>();
                    }
                }
                else
                {
                    string json = File.ReadAllText(path);
                    return string.IsNullOrWhiteSpace(json) ? new List<StateRecord>() : JsonSerializer.Deserialize<List<StateRecord>>(json) ?? new List<StateRecord>();
                }
            }
            catch
            {
                return new List<StateRecord>();
            }
        }

        /// <summary>
        /// Writes the updated list of states back to the persistent storage.
        /// </summary>
        private static void SaveStates(string path, List<StateRecord> states, bool isXml)
        {
            if (isXml)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<StateRecord>));
                using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
                {
                    serializer.Serialize(writer, states);
                }
            }
            else
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(states, options);
                File.WriteAllText(path, jsonString);
            }
        }

        /// <summary>
        /// Internal Data Transfer Object representing the state file structure.
        /// Decouples the persistent JSON/XML structure from the runtime Models.
        /// </summary>
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