using System;
using System.Collections.Generic;

namespace EasySave.Models
{
    public class Settings
    {
        public string Language { get; set; } = "en";
        public string LogFormat { get; set; } = "json";
        public string[] BusinessSoftware { get; set; } = Array.Empty<string>();
        public string[] EncryptedExtensions { get; set; } = Array.Empty<string>();
        public List<string> ExtensionsToEncrypt { get; set; } = new List<string>();
    }
}


