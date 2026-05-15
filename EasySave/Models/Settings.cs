using System;
using System.Collections.Generic;

namespace EasySave.Models
{
    public class Settings
    {
        public string Language { get; set; } = "en";
        public string LogFormat { get; set; } = "json";

        public List<string> ExtensionsToEncrypt { get; set; } = new List<string>();
        public List<string> BusinessSoftware { get; set; } = new List<string>();

        public List<string> PriorityExtensions { get; set; } = new List<string>();
        public int LargeFileThresholdKb { get; set; } = 1024;
        public string LogMode { get; set; } = "local";
        //en attente de mise en place du mode de log centralisé
        public string CentralLogServerUrl { get; set; } = "http://localhost:5000/api/logs";
    }
}


