using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.Models
{
    /// <summary>
    /// Represents the base structure of a backup task.
    /// Implements ObservableObject to enable real-time UI updates via data binding.
    /// </summary>
    public abstract class BackupJob : ObservableObject
    {
        // Core configuration properties
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public BackupType Type { get; set; }

        private JobStatus _status;
        public JobStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // Statistics for progress tracking
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int FilesLeft { get; set; }
        public long SizeLeft { get; set; }

        private int _progression;
        public int Progression
        {
            get => _progression;
            set => SetProperty(ref _progression, value);
        }

        // Tracking properties for active transfer
        public string CurrentSourceFile { get; set; }
        public string CurrentTargetFile { get; set; }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }


        /// <summary>
        /// Triggered whenever the job's state or progression changes.
        /// Primarily used by the StateManager to update real-time log files.
        /// </summary>
        public event EventHandler OnStateChanged;

        /// <summary>
        /// Triggered after a file has been successfully copied.
        /// Provides transfer performance data (duration and encryption time).
        /// </summary>
        public event Action<BackupJob, int, int> OnFileCopied;

        // Life-cycle control methods to be implemented by specific backup types
        public abstract void Execute();
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();

        /// <summary>
        /// Safely raises the OnStateChanged event.
        /// </summary>
        protected void TriggerStateChanged()
        {
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Safely raises the OnFileCopied event with performance metrics.
        /// </summary>
        protected void TriggerFileCopied(int transferTimeMs, int encryptionTimeMs)
        {
            OnFileCopied?.Invoke(this, transferTimeMs, encryptionTimeMs);
        }
    }
}
