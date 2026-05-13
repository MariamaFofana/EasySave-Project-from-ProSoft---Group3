using EasySave.Models;
using EasySave.Services;
using EasySave.UnitTests.Fixtures;
using EasySave.Utils;
using System;
using System.IO;
using Xunit;

namespace EasySave.UnitTests.Services
{
    public class StateManagerTests : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public StateManagerTests(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void UpdateState_Should_Create_State_File_In_Json_Format()
        {
            // Arrange
            SettingsManager.CurrentSettings.LogFormat = "json";
            SettingsManager.SaveSettings();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string logsDir = Path.Combine(projectDir, "Logs");
            string stateFilePath = Path.Combine(logsDir, "state.json");

            if (File.Exists(stateFilePath))
                File.Delete(stateFilePath);

            var job = new TestBackupJob
            {
                Name = "TestJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full,
                Status = JobStatus.Active,
                TotalFiles = 10,
                TotalSize = 1000,
                FilesLeft = 4,
                SizeLeft = 400,
                Progression = 60,
                CurrentSourceFile = @"C:\Source\file.txt",
                CurrentTargetFile = @"C:\Target\file.txt"
            };

            // Act
            StateManager.UpdateState(job);

            // Assert
            Assert.True(File.Exists(stateFilePath));
        }

        [Fact]
        public void UpdateState_Should_Write_Job_Name_In_Json_File()
        {
            // Arrange
            SettingsManager.CurrentSettings.LogFormat = "json";
            SettingsManager.SaveSettings();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string logsDir = Path.Combine(projectDir, "Logs");
            string stateFilePath = Path.Combine(logsDir, "state.json");

            if (File.Exists(stateFilePath))
                File.Delete(stateFilePath);

            var job = new TestBackupJob
            {
                Name = "MyTestJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full,
                Status = JobStatus.Active,
                TotalFiles = 5,
                TotalSize = 500,
                FilesLeft = 2,
                SizeLeft = 200,
                Progression = 60,
                CurrentSourceFile = @"C:\Source\file.txt",
                CurrentTargetFile = @"C:\Target\file.txt"
            };

            // Act
            StateManager.UpdateState(job);

            // Assert
            Assert.True(File.Exists(stateFilePath));

            string content = File.ReadAllText(stateFilePath);
            Assert.Contains("MyTestJob", content);
        }
        [Fact]
        public void UpdateState_Should_Create_State_File_In_Xml_Format()
        {
            // Arrange
            SettingsManager.CurrentSettings.LogFormat = "xml";
            SettingsManager.SaveSettings();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string logsDir = Path.Combine(projectDir, "Logs");
            string stateFilePath = Path.Combine(logsDir, "state.xml");

            if (File.Exists(stateFilePath))
                File.Delete(stateFilePath);

            var job = new TestBackupJob
            {
                Name = "XmlTestJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full,
                Status = JobStatus.Active,
                TotalFiles = 3,
                TotalSize = 300,
                FilesLeft = 1,
                SizeLeft = 100,
                Progression = 66,
                CurrentSourceFile = @"C:\Source\file.txt",
                CurrentTargetFile = @"C:\Target\file.txt"
            };

            // Act
            StateManager.UpdateState(job);

            // Assert
            Assert.True(File.Exists(stateFilePath));

            string content = File.ReadAllText(stateFilePath);
            Assert.Contains("XmlTestJob", content);
        }
        [Fact]
        public void UpdateState_Should_Write_End_When_Job_Is_Completed()
        {
            // Arrange
            SettingsManager.CurrentSettings.LogFormat = "json";
            SettingsManager.SaveSettings();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string logsDir = Path.Combine(projectDir, "Logs");
            string stateFilePath = Path.Combine(logsDir, "state.json");

            if (File.Exists(stateFilePath))
                File.Delete(stateFilePath);

            var job = new TestBackupJob
            {
                Name = "CompletedJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full,
                Status = JobStatus.Completed,
                TotalFiles = 5,
                TotalSize = 500,
                FilesLeft = 0,
                SizeLeft = 0,
                Progression = 100,
                CurrentSourceFile = @"C:\Source\file.txt",
                CurrentTargetFile = @"C:\Target\file.txt"
            };

            // Act
            StateManager.UpdateState(job);

            // Assert
            Assert.True(File.Exists(stateFilePath));

            string content = File.ReadAllText(stateFilePath);
            Assert.Contains("END", content);
        }
        [Fact]
        public void UpdateState_Should_Clear_Runtime_Fields_When_Job_Is_Completed()
        {
            // Arrange
            SettingsManager.CurrentSettings.LogFormat = "json";
            SettingsManager.SaveSettings();

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string logsDir = Path.Combine(projectDir, "Logs");
            string stateFilePath = Path.Combine(logsDir, "state.json");

            if (File.Exists(stateFilePath))
                File.Delete(stateFilePath);

            var job = new TestBackupJob
            {
                Name = "FinishedJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full,
                Status = JobStatus.Completed,
                TotalFiles = 8,
                TotalSize = 800,
                FilesLeft = 0,
                SizeLeft = 0,
                Progression = 100,
                CurrentSourceFile = @"C:\Source\file.txt",
                CurrentTargetFile = @"C:\Target\file.txt"
            };

            // Act
            StateManager.UpdateState(job);

            // Assert
            Assert.True(File.Exists(stateFilePath));

            string content = File.ReadAllText(stateFilePath);

            Assert.Contains("\"SourceFilePath\": \"\"", content);
            Assert.Contains("\"TargetFilePath\": \"\"", content);
            Assert.Contains("\"TotalFilesToCopy\": 0", content);
            Assert.Contains("\"TotalFilesSize\": 0", content);
            Assert.Contains("\"NbFilesLeftToDo\": 0", content);
            Assert.Contains("\"Progression\": 0", content);
        }
        private sealed class TestBackupJob : BackupJob
        {
            public override void Execute() { }
            public override void Play() { }
            public override void Pause() { }
            public override void Stop() { }
        }
    }
}