using System;
using System.Threading;

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
        public event Action<BackupJob, int, int> OnFileCopied;

        protected ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);
        protected CancellationTokenSource _cancelSource = new CancellationTokenSource();

        public abstract void Execute();

        public void Pause() => _pauseEvent.Reset();
        public void Resume() => _pauseEvent.Set();
        public void Stop() => _cancelSource.Cancel();

        protected void CheckControl()
        {
            _pauseEvent.Wait();
            if (_cancelSource.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

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