using System;
using System.Collections.Generic;
using EasySave.Models;
using EasySave.Core;
using EasyLog_DLL;

namespace EasySave.ViewModels
{
    public class BackupViewModel
    {
        private List<BackupJob> _jobs;
        private ConfigManager _config;
        private StateManager _state;
        private IEasyLogger _logger;
        private int MAX_JOBS = 5;

        public event Action<JobState> OnJobProgressUpdated;

        public BackupViewModel(ConfigManager config, StateManager state, IEasyLogger logger)
        {
            _config = config;
            _state = state;
            _logger = logger;
            _jobs = new List<BackupJob>();
        }

        public void ExecuteJobs(List<int> jobIds)
        {
        }

        public void CreateJob(string name, string source, string target, BackupType type)
        {
        }

        private void HandleJobProgress(JobState state)
        {
        }

        private void HandleFileCopied(LogEntry log)
        {
        }
    }
}
