using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace EasyLogDLL
{

    /// Static logging class that writes file-transfer actions to a daily JSON log.
    /// Each day gets its own file (YYYY-MM-DD.json) under the configured directory.
    /// JSON is indented with line breaks for Notepad readability (spec requirement).
    public static class EasyLogger
    {
        private static string _logDirectory = string.Empty;
        private static readonly object _fileLock = new object();

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static string LogFormat { get; set; } = "json";

    
        /// Sets the log directory. Must be called once at startup (in Program.Main).
        /// The directory is created automatically if it does not exist.
        
        public static void Configure(string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
                throw new ArgumentException("Log directory cannot be null or empty.", nameof(logDirectory));

            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
        }

        
        /// Writes a log entry to the current day's JSON file in real time.
        /// A negative transferTimeMs indicates an error during file transfer.
        
        public static void LogAction(string jobName, string source, string target, long size, int transferTimeMs)
        {
            if (string.IsNullOrWhiteSpace(_logDirectory))
                throw new InvalidOperationException(
                    "EasyLogger not configured. Call EasyLogger.Configure(directory) first.");

            var entry = new LogRecord
            {
                Time             = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                Name             = jobName,
                FileSource       = source,
                FileTarget       = target,
                FileSize         = size,
                FileTransferTime = transferTimeMs / 1000.0
            };

            bool isXml = string.Equals(LogFormat, "xml", StringComparison.OrdinalIgnoreCase);
            string extension = isXml ? ".xml" : ".json";

            string filePath = Path.Combine(
                _logDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + extension);

            lock (_fileLock)
            {
                if (isXml)
                {
                    List<LogRecord> entries = LoadExistingXml(filePath);
                    entries.Add(entry);
                    XmlSerializer serializer = new XmlSerializer(typeof(List<LogRecord>));
                    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        serializer.Serialize(writer, entries);
                    }
                }
                else
                {
                    List<LogRecord> entries = LoadExisting(filePath);
                    entries.Add(entry);
                    string json = JsonSerializer.Serialize(entries, JsonOptions);
                    File.WriteAllText(filePath, json, Encoding.UTF8);
                }
            }
        }

        private static List<LogRecord> LoadExistingXml(string path)
        {
            if (!File.Exists(path))
                return new List<LogRecord>();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<LogRecord>));
                using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
                {
                    var result = serializer.Deserialize(reader) as List<LogRecord>;
                    return result ?? new List<LogRecord>();
                }
            }
            catch
            {
                return new List<LogRecord>();
            }
        }

        /// Loads existing entries from a daily log file.
        private static List<LogRecord> LoadExisting(string path)
        {
            if (!File.Exists(path))
                return new List<LogRecord>();

            try
            {
                string content = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(content))
                    return new List<LogRecord>();

                return JsonSerializer.Deserialize<List<LogRecord>>(content)
                    ?? new List<LogRecord>();
            }
            catch (JsonException)
            {
                return new List<LogRecord>();
            }
        }

        /// Internal record for JSON serialization.
        public class LogRecord
        {
            [JsonPropertyName("Name")]
            [XmlElement("Name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("FileSource")]
            [XmlElement("FileSource")]
            public string FileSource { get; set; } = string.Empty;

            [JsonPropertyName("FileTarget")]
            [XmlElement("FileTarget")]
            public string FileTarget { get; set; } = string.Empty;

            [JsonPropertyName("FileSize")]
            [XmlElement("FileSize")]
            public long FileSize { get; set; }

            [JsonPropertyName("FileTransferTime")]
            [XmlElement("FileTransferTime")]
            public double FileTransferTime { get; set; }

            [JsonPropertyName("time")]
            [XmlElement("time")]
            public string Time { get; set; } = string.Empty;
        }
    }
}
