using System;
using System.IO;
using System.Text;

namespace CryptoSoft
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguments");
                return;
            }

            string filePath = args[0];
            string key = args[1];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"file not found : {filePath}");
                return;
            }

            byte[] data = File.ReadAllBytes(filePath);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            for (int i = 0; i < data.Length; i++)
                data[i] ^= keyBytes[i % keyBytes.Length];

            File.WriteAllBytes(filePath, data);
            Console.WriteLine($"{filePath} successfully encrypted");
        }
    }
}
