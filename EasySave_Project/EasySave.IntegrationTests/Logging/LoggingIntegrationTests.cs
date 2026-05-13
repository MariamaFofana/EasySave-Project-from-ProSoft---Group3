using EasyLogDLL;
using System;
using System.IO;
using Xunit;

namespace EasySave.IntegrationTests.Logging
{
    public class LoggingIntegrationTests : IDisposable
    {
        private readonly string _baseTestDirectory;

        public LoggingIntegrationTests()
        {
            _baseTestDirectory = Path.Combine(
                Path.GetTempPath(),
                "EasySaveLoggingIntegrationTests",
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(_baseTestDirectory);
        }

        [Fact]
        public void LogAction_Should_Create_Json_Log_File_With_Expected_Content()
        {
            // Arrange
            EasyLogger.Configure(_baseTestDirectory);
            EasyLogger.LogFormat = "json";

            string logFilePath = Path.Combine(
                _baseTestDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "IntegrationJob",
                @"C:\Source\file.txt",
                @"C:\Target\file.txt",
                120,
                2000,
                1000
            );

            // Assert
            Assert.True(File.Exists(logFilePath));

            string content = File.ReadAllText(logFilePath);

            Assert.Contains("IntegrationJob", content);
            Assert.Contains("FileSource", content);
            Assert.Contains("FileTarget", content);
            Assert.Contains("EncryptionTime", content);
        }
        [Fact]
        public void LogAction_Should_Create_Xml_Log_File_With_Expected_Content()
        {
            // Arrange
            EasyLogger.Configure(_baseTestDirectory);
            EasyLogger.LogFormat = "xml";

            string logFilePath = Path.Combine(
                _baseTestDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".xml");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "IntegrationXmlJob",
                @"C:\Source\file.txt",
                @"C:\Target\file.txt",
                120,
                2000,
                1000
            );

            // Assert
            Assert.True(File.Exists(logFilePath));

            string content = File.ReadAllText(logFilePath);

            Assert.Contains("IntegrationXmlJob", content);
            Assert.Contains("FileSource", content);
            Assert.Contains("FileTarget", content);
            Assert.Contains("EncryptionTime", content);
        }
        [Fact]
        public void LogAction_Should_Append_Multiple_Entries_In_The_Same_Daily_Log_File()
        {
            // Arrange
            EasyLogger.Configure(_baseTestDirectory);
            EasyLogger.LogFormat = "json";

            string logFilePath = Path.Combine(
                _baseTestDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "JobOne",
                @"C:\Source\file1.txt",
                @"C:\Target\file1.txt",
                100,
                1000,
                500
            );

            EasyLogger.LogAction(
                "JobTwo",
                @"C:\Source\file2.txt",
                @"C:\Target\file2.txt",
                200,
                1500,
                700
            );

            // Assert
            Assert.True(File.Exists(logFilePath));

            string content = File.ReadAllText(logFilePath);

            Assert.Contains("JobOne", content);
            Assert.Contains("JobTwo", content);
        }
        public void Dispose()
        {
            if (Directory.Exists(_baseTestDirectory))
            {
                Directory.Delete(_baseTestDirectory, true);
            }
        }
    }
}