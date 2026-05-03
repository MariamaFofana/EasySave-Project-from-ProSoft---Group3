using System;

namespace EasySave.Models
{
    public abstract class BackupJob
    {
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupType Type { get; set; }
        public JobStatus Status { get; set; }
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesLeft { get; set; }
        public long SizeLeft { get; set; }
        public int Progression { get; set; }
        public string CurrentSourceFile { get; set; }
        public string CurrentTargetFile { get; set; }

        public event EventHandler OnStateChanged;

        // v2.0: Added encryptionTimeMs parameter (0 = no encryption, >0 = time, <0 = error)
        public event Action<BackupJob, int, int> OnFileCopied;

        public abstract void Execute();

        protected void TriggerStateChanged()
        {
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void TriggerFileCopied(int transferTimeMs, int encryptionTimeMs)
        {
            OnFileCopied?.Invoke(this, transferTimeMs, encryptionTimeMs);
        }
    }
}
