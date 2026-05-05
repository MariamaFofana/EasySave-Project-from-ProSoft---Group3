using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EasySave.Services
{
    public class MonitoringService
    {
        private readonly string[] _businessSoftware;
        private readonly int _checkInterval;
        public Action<bool>? OnBusinessSoftwareStateChanged;
        private bool _wasRunning = false;

        public MonitoringService(string[] softwareList, int interval = 2000)
        {
            _businessSoftware = softwareList;
            _checkInterval = interval;
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
            return _businessSoftware.Any(name => Process.GetProcessesByName(name).Length > 0);
        }
    }
}