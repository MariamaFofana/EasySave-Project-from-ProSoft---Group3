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
    }
}


