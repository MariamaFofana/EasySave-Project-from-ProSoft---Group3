using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EasySave.Services
{
    public class MonitoringService
    {
        private readonly int _checkInterval;
        public Action<bool>? OnBusinessSoftwareStateChanged;
        private bool _wasRunning = false;

        public static MonitoringService? Instance { get; private set; }

        public MonitoringService(int interval = 2000)
        {
            _checkInterval = interval;
            Instance = this;
        }

        public void StartMonitoring()
        {
            Thread monitorThread = new Thread(() =>
            {
                while (true)
                {
                    bool isRunning = IsAnyBusinessSoftwareRunning();
                    if (isRunning != _wasRunning)
                    {
                        _wasRunning = isRunning;
                        OnBusinessSoftwareStateChanged?.Invoke(isRunning);
                    }
                    Thread.Sleep(_checkInterval);
                }
            });

            monitorThread.IsBackground = true;
            monitorThread.Start();
        }

        public bool IsAnyBusinessSoftwareRunning()
        {
            var softwareList = SettingsManager.CurrentSettings.BusinessSoftware;
            if (softwareList == null || softwareList.Count == 0) return false;
            return softwareList.Any(name => Process.GetProcessesByName(name).Length > 0);
        }
    }
}