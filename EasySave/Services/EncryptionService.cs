using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EasySave.Services
{
    
    /// Handles file encryption by calling the external CryptoSoft.exe process.
    /// Used by FullBackupJob and DifferentialBackupJob after copying a file.
    
    public static class EncryptionService
    {
        private static string _cryptoSoftPath = string.Empty;
        private static string _encryptionKey = string.Empty;
        private static HashSet<string> _extensionsToEncrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        
        /// Configures the encryption service. Called once at startup from appSettings.json values.
        
        public static void Configure(string cryptoSoftPath, string encryptionKey, IEnumerable<string> extensionsToEncrypt)
        {
            _cryptoSoftPath = cryptoSoftPath ?? string.Empty;
            _encryptionKey = encryptionKey ?? string.Empty;

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

        /// Checks if a file should be encrypted based on its extension.
        public static bool ShouldEncrypt(string filePath)
        {
            if (_extensionsToEncrypt.Count == 0)
                return false;
            string extension = Path.GetExtension(filePath);
            return _extensionsToEncrypt.Contains(extension);
        }

        /// Checks if file needs encryption, and if so, encrypts it.
        /// Returns:
        ///   0  = no encryption needed (extension not in list)
        ///   >0 = encryption time in ms (success)
        ///   less than 0 = error
        
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
                    proc.WaitForExit();
                    sw.Stop();

                    if (proc.ExitCode != 0)
                        return -proc.ExitCode;

                    return (int)sw.ElapsedMilliseconds;
                }
            }
            catch
            {
                return -1;
            }
        }

        public static bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_cryptoSoftPath) && _extensionsToEncrypt.Count > 0;
    }
}
