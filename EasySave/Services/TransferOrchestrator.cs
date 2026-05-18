using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EasySave.Services
{
    /// Global coordinator for file transfers across all parallel backup jobs.
    /// Enforces two v3.0 rules:
    /// 
    /// 1. PRIORITY FILES: If any job has a priority-extension file waiting,
    ///    all non-priority transfers across ALL jobs must wait.
    /// 
    /// 2. LARGE FILES: Only ONE file larger than the configured threshold
    ///    can transfer at a time (bandwidth protection). Smaller files
    ///    can still transfer freely in parallel.
    /// 
    /// Usage in backup jobs:
    ///   var token = TransferOrchestrator.RequestTransfer(filePath, fileSize);
    ///   try { File.Copy(...); EncryptionService.EncryptIfNeeded(...); }
    ///   finally { TransferOrchestrator.ReleaseTransfer(token); }
   
    public static class TransferOrchestrator
    {
        // Configuration
        private static HashSet<string> _priorityExtensions = new(StringComparer.OrdinalIgnoreCase);
        private static long _largeFileThresholdBytes = long.MaxValue;

        // Large file semaphore: only 1 large file at a time
        private static readonly SemaphoreSlim _largeSemaphore = new(1, 1);

        // Priority tracking
        private static int _pendingPriorityCount = 0;
        private static readonly ManualResetEventSlim _noPriorityPending = new(true);
        private static readonly object _priorityLock = new();

        /// Configures the orchestrator. Called once at startup.
        /// <param name="priorityExtensions">File extensions that get priority (e.g. ".docx", ".pdf")</param>
        /// <param name="largeFileThresholdKB">Size limit in KB. Files above this transfer one at a time.</param>
        public static void Configure(IEnumerable<string> priorityExtensions, long largeFileThresholdKB)
        {
            _priorityExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string ext in priorityExtensions)
            {
                string normalized = ext.Trim();
                if (!string.IsNullOrEmpty(normalized))
                {
                    if (!normalized.StartsWith("."))
                        normalized = "." + normalized;
                    _priorityExtensions.Add(normalized);
                }
            }

            if (largeFileThresholdKB <= 0)
            {
                _largeFileThresholdBytes = long.MaxValue;
            }
            else
            {
                _largeFileThresholdBytes = largeFileThresholdKB * 1024;
            }
        }

        /// Called BEFORE copying a file. Blocks if needed (priority or large file rules).
        /// Returns a TransferToken that MUST be disposed after the transfer completes.
        public static TransferToken RequestTransfer(string filePath, long fileSize, CancellationToken cancel = default)
        {
            string extension = Path.GetExtension(filePath);
            bool isPriority = _priorityExtensions.Contains(extension);
            bool isLarge = fileSize > _largeFileThresholdBytes;

            // RULE 1: Priority file management
            if (!isPriority)
            {
                // This is NOT a priority file: wait until all priority files are done
                _noPriorityPending.Wait(cancel);
            }

            // RULE 2: Large file bandwidth limitation
            if (isLarge)
            {
                _largeSemaphore.Wait(cancel);
            }

            return new TransferToken(isPriority, isLarge);
        }

        /// Called AFTER copying (and encrypting) a file. Releases any held resources.
        public static void ReleaseTransfer(TransferToken token)
        {
            if (token == null) return;

            // Release large file semaphore
            if (token.IsLarge)
            {
                _largeSemaphore.Release();
            }

            // Update priority counter
            if (token.IsPriority)
            {
                DeregisterPendingPriority(1);
            }
        }

        /// Registers pending priority files. Blocks non-priority transfers.
        public static void RegisterPendingPriority(int count)
        {
            if (count <= 0) return;
            lock (_priorityLock)
            {
                _pendingPriorityCount += count;
                _noPriorityPending.Reset(); // Block non-priority transfers
            }
        }

        /// Deregisters pending priority files. Unblocks non-priority transfers if none left.
        public static void DeregisterPendingPriority(int count)
        {
            if (count <= 0) return;
            lock (_priorityLock)
            {
                _pendingPriorityCount -= count;
                if (_pendingPriorityCount <= 0)
                {
                    _pendingPriorityCount = 0;
                    _noPriorityPending.Set(); // Unblock non-priority transfers
                }
            }
        }

        /// Checks if a file extension is considered priority.
        public static bool IsPriorityFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            return _priorityExtensions.Contains(extension);
        }

        /// Checks if a file exceeds the large file threshold.
        public static bool IsLargeFile(long fileSize)
        {
            return fileSize > _largeFileThresholdBytes;
        }

        ///Whether any priority files are currently being processed.
        public static bool HasPendingPriority => _pendingPriorityCount > 0;

        /// Whether the orchestrator has been configured.
        public static bool IsConfigured => _priorityExtensions.Count > 0;
    }

    /// Token returned by RequestTransfer. Must be passed to ReleaseTransfer after the copy.
    public class TransferToken
    {
        public bool IsPriority { get; }
        public bool IsLarge { get; }

        internal TransferToken(bool isPriority, bool isLarge)
        {
            IsPriority = isPriority;
            IsLarge = isLarge;
        }
    }
}
