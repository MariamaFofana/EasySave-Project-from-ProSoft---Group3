using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using LogServer.Models;

namespace LogServer
{
    class Program
    {
        private static readonly object _fileLock = new object();
        private const string LogDirectory = "Logs";
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };
        private const int Port = 5000;

        static async Task Main(string[] args)
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            TcpListener listener = new TcpListener(IPAddress.Any, Port); // Listen on all available network interfaces on the specified port (5000).
            listener.Start();
            Console.WriteLine($"Socket Log Server started on port {Port}...");

            while (true)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync(); // Accepts an incoming connection attempt.
                    _ = HandleClientAsync(client); // Handles the client asynchronously without blocking the main loop.
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream()) // Gets the underlying stream of the TCP connection. 
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))// Reads the stream of the TCP connection.
            {
                try
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null) return;

                    string[] parts = line.Split('|', 2);
                    if (parts.Length < 2) return;

                    string format = parts[0].ToLower();
                    string jsonContent = parts[1];

                    LogRecord? log = JsonSerializer.Deserialize<LogRecord>(jsonContent);
                    if (log == null) return;

                    SaveLog(log, format);
                    Console.WriteLine($"[{DateTime.Now}] Received log from {log.MachineName} (Format: {format})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                }
            }
        }

        private static void SaveLog(LogRecord log, string format)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            bool isXml = format == "xml";
            string extension = isXml ? ".xml" : ".json";
            string filePath = Path.Combine(LogDirectory, currentDate + extension);

            lock (_fileLock)
            {
                if (isXml)
                {
                    List<LogRecord> entries = new List<LogRecord>();
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(List<LogRecord>));// Serializer for LogRecord objects.
                            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                            {
                                var result = serializer.Deserialize(reader) as List<LogRecord>;
                                if (result != null) entries = result;
                            }
                        }
                        catch { /* Ignore corrupt file */ }
                    }

                    entries.Add(log);
                    XmlSerializer outSerializer = new XmlSerializer(typeof(List<LogRecord>));
                    using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                    {
                        outSerializer.Serialize(writer, entries);
                    }
                }
                else
                {
                    List<LogRecord> entries = new List<LogRecord>();
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            string content = File.ReadAllText(filePath);
                            entries = JsonSerializer.Deserialize<List<LogRecord>>(content) ?? new List<LogRecord>();
                        }
                        catch { /* Ignore corrupt file */ }
                    }

                    entries.Add(log);
                    string json = JsonSerializer.Serialize(entries, JsonOptions);
                    File.WriteAllText(filePath, json);
                }
            }
        }
    }
}

