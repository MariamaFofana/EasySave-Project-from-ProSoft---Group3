using EasySave.Models;
using EasySave.Services;
using EasySave.UnitTests.Fixtures;
using EasySave.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace EasySave.UnitTests.ViewModels
{
    public class MainViewModelTests : IClassFixture<TemporaryDirectoryFixture>, IDisposable
    {
        private readonly TemporaryDirectoryFixture _fixture;
        private readonly string _configPath;
        private readonly List<string> _originalBusinessSoftware;

        public MainViewModelTests(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string resourcesDir = Path.Combine(projectDir, "Ressources");
            Directory.CreateDirectory(resourcesDir);

            _configPath = Path.Combine(resourcesDir, "config.json");

            if (File.Exists(_configPath))
                File.Delete(_configPath);

            _originalBusinessSoftware = SettingsManager.CurrentSettings.BusinessSoftware != null
                ? new List<string>(SettingsManager.CurrentSettings.BusinessSoftware)
                : new List<string>();

            SettingsManager.CurrentSettings.BusinessSoftware = new List<string>();
            SettingsManager.SaveSettings();
        }

        public void Dispose()
        {
            SettingsManager.CurrentSettings.BusinessSoftware = _originalBusinessSoftware;
            SettingsManager.SaveSettings();

            if (File.Exists(_configPath))
                File.Delete(_configPath);
        }

        [Fact]
        public void CreateJob_Should_Add_Job_To_Collection()
        {
            // Arrange
            var viewModel = new MainViewModel();

            // Act
            viewModel.CreateJob("Job1", @"C:\Source", @"C:\Target", BackupType.Full);

            // Assert
            Assert.Single(viewModel.Jobs);
            Assert.Equal("Job1", viewModel.Jobs[0].Name);
        }

        [Fact]
        public void DeleteJob_Should_Remove_Job_From_Collection()
        {
            // Arrange
            var viewModel = new MainViewModel();
            viewModel.CreateJob("JobToDelete", @"C:\Source", @"C:\Target", BackupType.Full);

            BackupJob job = viewModel.Jobs[0];

            // Act
            viewModel.DeleteJob(job);

            // Assert
            Assert.Empty(viewModel.Jobs);
        }

        [Fact]
        public void ExecuteJob_Should_Run_Job_When_No_BusinessSoftware_Is_Detected()
        {
            // Arrange
            SettingsManager.CurrentSettings.BusinessSoftware = new List<string>();
            SettingsManager.SaveSettings();

            var viewModel = new MainViewModel();
            var job = new TestBackupJob
            {
                Name = "ExecutableJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full
            };

            viewModel.Jobs.Add(job);

            // Act
            viewModel.ExecuteJob(0);

            // Assert
            Assert.True(job.ExecuteWasCalled);
        }

        [Fact]
        public void ExecuteJob_Should_Not_Run_Job_When_BusinessSoftware_Is_Detected()
        {
            // Arrange
            string currentProcessName = Process.GetCurrentProcess().ProcessName;

            SettingsManager.CurrentSettings.BusinessSoftware = new List<string>
            {
                currentProcessName
            };
            SettingsManager.SaveSettings();

            var viewModel = new MainViewModel();
            var job = new TestBackupJob
            {
                Name = "BlockedJob",
                SourceDirectory = @"C:\Source",
                TargetDirectory = @"C:\Target",
                Type = BackupType.Full
            };

            viewModel.Jobs.Add(job);

            // Act
            viewModel.ExecuteJob(0);

            // Assert
            Assert.False(job.ExecuteWasCalled);
        }

        private sealed class TestBackupJob : BackupJob
        {
            public bool ExecuteWasCalled { get; private set; }

            public override void Execute()
            {
                ExecuteWasCalled = true;
            }

            public override void Play() { }

            public override void Pause() { }

            public override void Stop() { }
        }
    }
}