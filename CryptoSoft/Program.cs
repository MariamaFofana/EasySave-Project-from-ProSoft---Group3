using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CryptoSoft
{
    public class Program
    {
        private const string MutexName = @"Global\EasySave_CryptoSoft_SingleInstance";

        static void Main(string[] args)
        {
            bool createdNew;
            using Mutex mutex = new Mutex(false, MutexName, out createdNew);

            if (!createdNew)
            {
                Console.WriteLine("Another instance of CryptoSoft is already running.");
                Environment.ExitCode = 2;
                return;
            }

            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Invalid arguments. Usage: CryptoSoft.exe <filePath> <key>");
                    Environment.ExitCode = 3;
                    return;
                }

                string filePath = args[0];
                string key = args[1];

                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    Environment.ExitCode = 4;
                    return;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    Console.WriteLine("Encryption key cannot be empty.");
                    Environment.ExitCode = 5;
                    return;
                }

                byte[] data = File.ReadAllBytes(filePath);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] ^= keyBytes[i % keyBytes.Length];
                }
                //Thread.Sleep(5000); tester le mutex en lançant 2 instances de CryptoSoft.exe en même temps
                File.WriteAllBytes(filePath, data);
                Console.WriteLine($"{filePath} successfully encrypted");
                Environment.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected encryption error: {ex.Message}");
                Environment.ExitCode = 10;
            }
        }
    }
}