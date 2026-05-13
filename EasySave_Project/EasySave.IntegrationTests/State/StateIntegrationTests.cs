using EasySave.Models;
using EasySave.Services;
using EasySave.ViewModels;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace EasySave.IntegrationTests.State
{
    public class StateIntegrationTests : IDisposable
    {
        private readonly string _logsDir;
        private readonly string _stateJsonPath;

        public StateIntegrationTests()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

            _logsDir = Path.Combine(projectDir, "Logs");
            Directory.CreateDirectory(_logsDir);

            _stateJsonPath = Path.Combine(_logsDir, "state.json");

            if (File.Exists(_stateJsonPath))
                File.Delete(_stateJsonPath);

            SettingsManager.CurrentSettings.LogFormat = "json";
            SettingsManager.SaveSettings();
        }

        [Fact]
        public void ExecuteJob_Should_Update_State_File_When_Job_Raises_StateChanged()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var job = new TestBackupJob
            {
                Name = "StateIntegrationJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full
            };

            viewModel.Jobs.Add(job);

            MethodInfo? subscribeMethod = typeof(MainViewModel).GetMethod(
                "SubscribeToJobEvents",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(subscribeMethod);
            subscribeMethod!.Invoke(viewModel, new object[] { job });

            // Act
            viewModel.ExecuteJob(0);

            // Assert
            Assert.True(File.Exists(_stateJsonPath));

            string content = File.ReadAllText(_stateJsonPath);
            Assert.Contains("StateIntegrationJob", content);
            Assert.Contains("ACTIVE", content);
        }
        [Fact]
        public void ExecuteJob_Should_Write_End_In_State_File_When_Job_Is_Completed()
        {
            // Arrange
            var viewModel = new MainViewModel();
            var job = new CompletedTestBackupJob
            {
                Name = "CompletedStateJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full
            };

            viewModel.Jobs.Add(job);

            MethodInfo? subscribeMethod = typeof(MainViewModel).GetMethod(
                "SubscribeToJobEvents",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(subscribeMethod);
            subscribeMethod!.Invoke(viewModel, new object[] { job });

            // Act
            viewModel.ExecuteJob(0);

            // Assert
            Assert.True(File.Exists(_stateJsonPath));

            string content = File.ReadAllText(_stateJsonPath);
            Assert.Contains("CompletedStateJob", content);
            Assert.Contains("END", content);
        }

        private sealed class CompletedTestBackupJob : BackupJob
        {
            public override void Execute()
            {
                Status = JobStatus.Completed;
                TotalFiles = 4;
                TotalSize = 400;
                FilesLeft = 0;
                SizeLeft = 0;
                Progression = 100;
                CurrentSourceFile = @"C:\Source\done.txt";
                CurrentTargetFile = @"C:\Target\done.txt";

                TriggerStateChanged();
            }

            public override void Play() { }

            public override void Pause() { }

            public override void Stop() { }
        }
        public void Dispose()
        {
            if (File.Exists(_stateJsonPath))
                File.Delete(_stateJsonPath);
        }

        private sealed class TestBackupJob : BackupJob
        {
            public override void Execute()
            {
                Status = JobStatus.Active;
                TotalFiles = 10;
                TotalSize = 1000;
                FilesLeft = 5;
                SizeLeft = 500;
                Progression = 50;
                CurrentSourceFile = @"C:\Source\file.txt";
                CurrentTargetFile = @"C:\Target\file.txt";

                TriggerStateChanged();
            }

            public override void Play() { }

            public override void Pause() { }

            public override void Stop() { }
        }
    }
}