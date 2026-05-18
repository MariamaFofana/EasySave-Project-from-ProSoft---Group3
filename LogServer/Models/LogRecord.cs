using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace LogServer.Models
{
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

        [JsonPropertyName("EncryptionTime")]
        [XmlElement("EncryptionTime")]
        public double EncryptionTime { get; set; }

        [JsonPropertyName("time")]
        [XmlElement("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("MachineName")]
        [XmlElement("MachineName")]
        public string MachineName { get; set; } = string.Empty;
    }
}

