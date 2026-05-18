using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace EasySave
{
    class Program
    {
        private const string SourceDirectory = @"C:\data";
        private const string TargetDirectory = @"C:\data_Competition";

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Accept "1" or no arguments, both will trigger the same optimized backup
            if (args.Length > 0 && args[0].Trim() != "1")
            {
                Console.WriteLine("[ERROR] Invalid argument. Use '1' or run without arguments to launch the backup.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine(" EasySave V3.0");
            Console.WriteLine();
            Console.WriteLine($"[START] Backup from '{SourceDirectory}' to '{TargetDirectory}' started.");

            if (!Directory.Exists(SourceDirectory))
            {
                Console.WriteLine($"[ERROR] Backup failed. Error: Source directory '{SourceDirectory}' does not exist.");
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Enumerate all files recursively (extremely memory-efficient streaming)
                var allFiles = Directory.EnumerateFiles(SourceDirectory, "*", SearchOption.AllDirectories).ToList();
                int totalFiles = allFiles.Count;
                long totalBytes = 0;

                foreach (var file in allFiles)
                {
                    totalBytes += new FileInfo(file).Length;
                }

                Console.WriteLine($"Found {totalFiles} files to copy ({FormatSize(totalBytes)}). Copying in parallel...");

                int copiedCount = 0;
                long copiedBytes = 0;
                object lockObj = new object();

                // Ultra high-performance parallel copy
                await Parallel.ForEachAsync(allFiles, new ParallelOptions 
                { 
                    MaxDegreeOfParallelism = Environment.ProcessorCount 
                }, async (file, cancellationToken) =>
                {
                    string relativePath = Path.GetRelativePath(SourceDirectory, file);
                    string targetPath = Path.Combine(TargetDirectory, relativePath);

                    string? targetFolder = Path.GetDirectoryName(targetPath);
                    if (targetFolder != null && !Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    // Highly optimized OS-level fast copy
                    File.Copy(file, targetPath, overwrite: true);

                    long fileLength = new FileInfo(file).Length;

                    lock (lockObj)
                    {
                        copiedCount++;
                        copiedBytes += fileLength;
                        
                        // Print progression in a single-line update
                        int progress = totalFiles > 0 ? (copiedCount * 100) / totalFiles : 100;
                        Console.Write($"\rProgress: {progress}% ({copiedCount}/{totalFiles} files copied)   ");
                    }
                });

                stopwatch.Stop();
                Console.WriteLine(); // New line after progress carriage return
                Console.WriteLine($"[SUCCESS] Backup completed successfully in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine($"[ERROR] Backup failed. Error: {ex.Message}");
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            double doubleBytes = bytes;
            int i = 0;
            while (doubleBytes >= 1024 && i < suffixes.Length - 1)
            {
                doubleBytes /= 1024;
                i++;
            }
            return $"{doubleBytes:F2} {suffixes[i]}";
        }
    }
}