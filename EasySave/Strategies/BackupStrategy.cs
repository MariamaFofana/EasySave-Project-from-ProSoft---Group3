using System;
using EasySave.Models;
using EasyLog_DLL;

namespace EasySave.Strategies
{
    public abstract class BackupStrategy
    {
        protected ITransferEngine _transferEngine;

        public event Action<JobState> OnStrategyProgress;
        public event Action<LogEntry> OnFileCopied;

        public BackupStrategy(ITransferEngine engine)
        {
            _transferEngine = engine;
        }

        public abstract void Execute(BackupJob job);
    }
}
