using EasyLogDLL;
using EasySave.UnitTests.Fixtures;
using EasySave.UnitTests.Helpers;
using System;
using System.IO;
using Xunit;

namespace EasySave.UnitTests.Logs
{
    public class EasyLoggerTests : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public EasyLoggerTests(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Configure_Should_Create_Log_Directory()
        {
            // Arrange
            if (Directory.Exists(TestPaths.LogsDirectory))
            {
                Directory.Delete(TestPaths.LogsDirectory, true);
            }

            // Act
            EasyLogger.Configure(TestPaths.LogsDirectory);

            // Assert
            Assert.True(Directory.Exists(TestPaths.LogsDirectory));
        }

        [Fact]
        public void LogAction_Should_Create_Json_Log_File_When_Format_Is_Json()
        {
            // Arrange
            EasyLogger.Configure(TestPaths.LogsDirectory);
            EasyLogger.LogFormat = "json";

            string logFilePath = Path.Combine(
                TestPaths.LogsDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "JobJson",
                @"C:\Source\file.txt",
                @"C:\Target\file.txt",
                100,
                1500,
                500
            );

            // Assert
            Assert.True(File.Exists(logFilePath));
        }

        [Fact]
        public void LogAction_Should_Write_Job_Name_In_Json_Log_File()
        {
            // Arrange
            EasyLogger.Configure(TestPaths.LogsDirectory);
            EasyLogger.LogFormat = "json";

            string logFilePath = Path.Combine(
                TestPaths.LogsDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "MyLoggerJob",
                @"C:\Source\file.txt",
                @"C:\Target\file.txt",
                100,
                1500,
                500
            );

            // Assert
            Assert.True(File.Exists(logFilePath));

            string content = File.ReadAllText(logFilePath);
            Assert.Contains("MyLoggerJob", content);
        }

        [Fact]
        public void LogAction_Should_Create_Xml_Log_File_When_Format_Is_Xml()
        {
            // Arrange
            EasyLogger.Configure(TestPaths.LogsDirectory);
            EasyLogger.LogFormat = "xml";

            string logFilePath = Path.Combine(
                TestPaths.LogsDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".xml");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "JobXml",
                @"C:\Source\file.txt",
                @"C:\Target\file.txt",
                100,
                1500,
                500
            );

            // Assert
            Assert.True(File.Exists(logFilePath));
        }

        [Fact]
        public void LogAction_Should_Write_Encryption_Time_In_Log_File()
        {
            // Arrange
            EasyLogger.Configure(TestPaths.LogsDirectory);
            EasyLogger.LogFormat = "json";

            string logFilePath = Path.Combine(
                TestPaths.LogsDirectory,
                DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            // Act
            EasyLogger.LogAction(
                "EncryptionJob",
                @"C:\Source\file.txt",
                @"C:\Target\file.txt",
                100,
                2000,
                1000
            );

            // Assert
            Assert.True(File.Exists(logFilePath));

            string content = File.ReadAllText(logFilePath);

            // 1000 ms => 1 second in the serialized log
            Assert.Contains("EncryptionTime", content);
            Assert.Contains("1", content);
        }
    }
}