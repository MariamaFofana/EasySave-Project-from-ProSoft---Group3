using EasySave.Services;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace EasySave.UnitTests.Services
{
    public class MonitoringServiceTests
    {
        [Fact]
        public void IsAnyBusinessSoftwareRunning_Should_ReturnFalse_When_ListIsEmpty()
        {
            List<string> originalList = SaveCurrentBusinessSoftwareList();

            try
            {
                SettingsManager.CurrentSettings.BusinessSoftware = new List<string>();
                SettingsManager.SaveSettings();

                var monitoringService = new MonitoringService();

                bool result = monitoringService.IsAnyBusinessSoftwareRunning();

                Assert.False(result);
            }
            finally
            {
                RestoreBusinessSoftwareList(originalList);
            }
        }

        [Fact]
        public void IsAnyBusinessSoftwareRunning_Should_ReturnFalse_When_ConfiguredSoftware_Is_Not_Running()
        {
            List<string> originalList = SaveCurrentBusinessSoftwareList();

            try
            {
                SettingsManager.CurrentSettings.BusinessSoftware = new List<string>
                {
                    "definitely_not_a_real_running_process_12345"
                };
                SettingsManager.SaveSettings();

                var monitoringService = new MonitoringService();

                bool result = monitoringService.IsAnyBusinessSoftwareRunning();

                Assert.False(result);
            }
            finally
            {
                RestoreBusinessSoftwareList(originalList);
            }
        }

        [Fact]
        public void IsAnyBusinessSoftwareRunning_Should_ReturnTrue_When_CurrentProcess_Is_Configured()
        {
            List<string> originalList = SaveCurrentBusinessSoftwareList();

            try
            {
                string currentProcessName = Process.GetCurrentProcess().ProcessName;

                SettingsManager.CurrentSettings.BusinessSoftware = new List<string>
                {
                    currentProcessName
                };
                SettingsManager.SaveSettings();

                var monitoringService = new MonitoringService();

                bool result = monitoringService.IsAnyBusinessSoftwareRunning();

                Assert.True(result);
            }
            finally
            {
                RestoreBusinessSoftwareList(originalList);
            }
        }

        private static List<string> SaveCurrentBusinessSoftwareList()
        {
            return SettingsManager.CurrentSettings.BusinessSoftware != null
                ? new List<string>(SettingsManager.CurrentSettings.BusinessSoftware)
                : new List<string>();
        }

        private static void RestoreBusinessSoftwareList(List<string> originalList)
        {
            SettingsManager.CurrentSettings.BusinessSoftware = originalList;
            SettingsManager.SaveSettings();
        }
    }
}