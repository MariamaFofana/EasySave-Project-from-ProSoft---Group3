using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EasySave.Services
{
    /// <summary>
    /// Service dedicated to securing files through encryption.
    /// Acts as a wrapper around the external CryptoSoft binary.
    /// </summary>
    public static class EncryptionService
    {
        private static string _cryptoSoftPath = string.Empty;
        private static string _encryptionKey = string.Empty;
        private static HashSet<string> _extensionsToEncrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Global initialization of the encryption engine.
        /// Consumes configuration from the SettingsManager.
        /// </summary>
        public static void Configure(string cryptoSoftPath, string encryptionKey, IEnumerable<string> extensionsToEncrypt)
        {
            _cryptoSoftPath = cryptoSoftPath ?? string.Empty;
            _encryptionKey = encryptionKey ?? string.Empty;

            // Ensure extensions are normalized for consistent lookups (e.g., always starts with '.')
            _extensionsToEncrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string ext in extensionsToEncrypt)
            {
                string normalized = ext.Trim();
                if (!string.IsNullOrEmpty(normalized))
                {
                    if (!normalized.StartsWith("."))
                        normalized = "." + normalized;
                    _extensionsToEncrypt.Add(normalized);
                }
            }
        }

        /// <summary>
        /// Business logic to determine if a specific file qualifies for encryption based on user preferences.
        /// </summary>
        public static bool ShouldEncrypt(string filePath)
        {
            if (_extensionsToEncrypt.Count == 0)
                return false;
                
            string extension = Path.GetExtension(filePath);
            return _extensionsToEncrypt.Contains(extension);
        }

        /// <summary>
        /// Orchestrates the encryption process for a given file.
        /// Invokes the external CryptoSoft process and measures its performance.
        /// </summary>
        /// <returns>
        /// 0 if skipped, 
        /// positive milliseconds if successful, 
        /// negative code if an error occurred.
        /// </returns>
        public static int EncryptIfNeeded(string filePath)
        {
            if (!ShouldEncrypt(filePath))
                return 0;

            if (string.IsNullOrWhiteSpace(_cryptoSoftPath) || !File.Exists(_cryptoSoftPath))
                return -1;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _cryptoSoftPath,
                    Arguments = $"\"{filePath}\" \"{_encryptionKey}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Stopwatch sw = Stopwatch.StartNew();
                using (Process proc = Process.Start(psi))
                {
                    if (proc == null) return -1;
                    
                    proc.WaitForExit();
                    sw.Stop();

                    // If CryptoSoft returns a non-zero exit code, we treat it as an encryption failure
                    if (proc.ExitCode != 0)
                        return -proc.ExitCode;

                    return (int)sw.ElapsedMilliseconds;
                }
            }
            catch
            {
                // Silent catch to prevent backup disruption, but returns error code for logging
                return -1;
            }
        }

        /// <summary>
        /// Verification property to check if the encryption sub-system is ready for use.
        /// </summary>
        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_cryptoSoftPath) && _extensionsToEncrypt.Count > 0;
    }
}
