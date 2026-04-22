using System;
using EasySave.Models;

namespace EasySave.Factories
{
    public static class BackupJobFactory
    {
        public static BackupJob CreateJob(string name, string source, string target, BackupType type)
        {
            BackupJob new_backup = type switch
            {
                BackupType.Full => new FullBackupJob(),
                BackupType.Differential => new DifferentialBackupJob(),
                _ => throw new ArgumentException("Invalid backup type")
            };

            new_backup.Name = name;
            new_backup.SourceDirectory = source;
            new_backup.TargetDirectory = target;
            new_backup.Type = type;

            return new_backup;
        }
    }
}
