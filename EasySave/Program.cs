using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace EasySave
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("====================================================");
            Console.WriteLine("                       EasySave                     ");
            Console.WriteLine("====================================================");
            Console.WriteLine();

            string sourceDirectory = @"C:\data";
            string targetDirectory = @"C:\data_Competition";

            if (args.Length == 1 && args[0].Trim() == "1")
            {
                Console.WriteLine($"[CLI] Running backup using default paths:");
                Console.WriteLine($"      Source: {sourceDirectory}");
                Console.WriteLine($"      Target: {targetDirectory}");
            }
            else if (args.Length >= 2)
            {
                sourceDirectory = args[0];
                targetDirectory = args[1];
                Console.WriteLine($"[CLI] Source directory: {sourceDirectory}");
                Console.WriteLine($"[CLI] Target directory: {targetDirectory}");
            }
            else
            {
                // Interactive Mode
                while (true)
                {
                    Console.Write($"Enter source directory path (leave empty for '{sourceDirectory}'): ");
                    string? input = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(input))
                    {
                        sourceDirectory = input;
                    }
                    if (!Directory.Exists(sourceDirectory))
                    {
                        Console.WriteLine($"[ERROR] Source directory '{sourceDirectory}' does not exist. Please try again.");
                        continue;
                    }
                    break;
                }

                while (true)
                {
                    Console.Write($"Enter target directory path (leave empty for '{targetDirectory}'): ");
                    string? input = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrEmpty(input))
                    {
                        targetDirectory = input;
                    }
                    break;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"[START] Backup from '{sourceDirectory}' to '{targetDirectory}' started.");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Enumerate all files recursively
                var allFiles = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories).ToList();
                int totalFiles = allFiles.Count;
                long totalBytes = 0;

                var fileInfos = new List<(string Path, long Length)>();
                foreach (var file in allFiles)
                {
                    long length = new FileInfo(file).Length;
                    fileInfos.Add((file, length));
                    totalBytes += length;
                }

                Console.WriteLine($"Found {totalFiles} files to copy ({FormatSize(totalBytes)}). Copying in parallel...");
                Console.WriteLine();

                int copiedCount = 0;
                long copiedBytes = 0;
                object lockObj = new object();

                // Initialize progress display
                UpdateProgress(0, totalFiles, 0, totalBytes);

                // Parallel copying using custom stream copier
                await Parallel.ForEachAsync(fileInfos, new ParallelOptions 
                { 
                    MaxDegreeOfParallelism = Environment.ProcessorCount 
                }, async (fileInfo, cancellationToken) =>
                {
                    string relativePath = Path.GetRelativePath(sourceDirectory, fileInfo.Path);
                    string targetPath = Path.Combine(targetDirectory, relativePath);

                    await CustomCopyFileAsync(fileInfo.Path, targetPath, (bytesWritten) =>
                    {
                        lock (lockObj)
                        {
                            copiedBytes += bytesWritten;
                            UpdateProgress(copiedCount, totalFiles, copiedBytes, totalBytes);
                        }
                    });

                    lock (lockObj)
                    {
                        copiedCount++;
                        UpdateProgress(copiedCount, totalFiles, copiedBytes, totalBytes);
                    }
                });

                stopwatch.Stop();
                Console.WriteLine(); 
                Console.WriteLine();
                Console.WriteLine("====================================================");
                Console.WriteLine($"[SUCCESS] Backup completed successfully!");
                Console.WriteLine($"Total files copied: {copiedCount} / {totalFiles}");
                Console.WriteLine($"Total size: {FormatSize(copiedBytes)}");
                Console.WriteLine($"Time elapsed: {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
                Console.WriteLine("====================================================");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("====================================================");
                Console.WriteLine($"[ERROR] Backup failed. Error: {ex.Message}");
                Console.WriteLine("====================================================");
            }

            if (args.Length < 2)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static async Task CustomCopyFileAsync(string sourcePath, string targetPath, Action<long> onBytesWritten)
        {
            string? directoryPath = Path.GetDirectoryName(targetPath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            const int bufferSize = 64 * 1024; // 64 KB buffer
            byte[] buffer = new byte[bufferSize];

            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
            using (var targetStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
            {
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await targetStream.WriteAsync(buffer, 0, bytesRead);
                    onBytesWritten(bytesRead);
                }
            }
        }

        private static void UpdateProgress(int copiedCount, int totalFiles, long copiedBytes, long totalBytes)
        {
            int progressPercent = totalBytes > 0 ? (int)((copiedBytes * 100) / totalBytes) : 100;
            if (progressPercent > 100) progressPercent = 100;
            
            int barWidth = 30;
            int filledWidth = (progressPercent * barWidth) / 100;
            string progressBar = new string('■', filledWidth) + new string(' ', barWidth - filledWidth);
            
            Console.Write($"\rProgress: [{progressBar}] {progressPercent}% | {copiedCount}/{totalFiles} files | {FormatSize(copiedBytes)}/{FormatSize(totalBytes)}    ");
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