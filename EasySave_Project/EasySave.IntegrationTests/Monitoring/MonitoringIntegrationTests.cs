using EasySave.Models;
using EasySave.Services;
using EasySave.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace EasySave.IntegrationTests.Monitoring
{
    public class MonitoringIntegrationTests : IDisposable
    {
        private readonly string _configPath;
        private readonly List<string> _originalBusinessSoftware;

        public MonitoringIntegrationTests()
        {
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
        }

        [Fact]
        public void ExecuteJob_Should_Be_Blocked_When_BusinessSoftware_Is_Running()
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
                Name = "BlockedIntegrationJob",
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

        [Fact]
        public void ExecuteJob_Should_Run_When_No_BusinessSoftware_Is_Detected()
        {
            // Arrange
            SettingsManager.CurrentSettings.BusinessSoftware = new List<string>();
            SettingsManager.SaveSettings();

            var viewModel = new MainViewModel();
            var job = new TestBackupJob
            {
                Name = "AllowedIntegrationJob",
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

        public void Dispose()
        {
            SettingsManager.CurrentSettings.BusinessSoftware = _originalBusinessSoftware;
            SettingsManager.SaveSettings();

            if (File.Exists(_configPath))
                File.Delete(_configPath);
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