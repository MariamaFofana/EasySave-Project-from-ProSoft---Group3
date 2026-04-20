using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.Core
{
    public class ConfigManager
    {
        private string _configPath;

        public ConfigManager(string path)
        {
            _configPath = path;
        }

        public List<BackupJob> LoadJobs()
        {
            return new List<BackupJob>();
        }

        public void SaveJobs(List<BackupJob> jobs)
        {
        }
    }
}
