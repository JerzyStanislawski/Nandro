using PCSC;
using PCSC.Monitoring;
using System;
using System.Linq;

namespace Nandro.NFC
{
    class Monitor : IDisposable
    {
        private IDeviceMonitor _monitor;

        internal event EventHandler<DeviceAttachedEventArgs> DeviceAttached;
        internal event EventHandler<DeviceDetachedEventArgs> DeviceDetached;

        public void Start()
        {
            _monitor = DeviceMonitorFactory.Instance.Create(SCardScope.System);

            _monitor.StatusChanged += StatusChanged;

            _monitor.Start();
        }

        private void StatusChanged(object sender, DeviceChangeEventArgs e)
        {
            if (e.AttachedReaders.Any())
                DeviceAttached.Invoke(sender, new DeviceAttachedEventArgs { DeviceName = e.AttachedReaders.First() });

            if (e.DetachedReaders.Any())
                DeviceDetached.Invoke(sender, new DeviceDetachedEventArgs { DeviceName = e.AttachedReaders.First() });
        }

        public void Dispose()
        {
            _monitor.StatusChanged -= StatusChanged;
            _monitor.Dispose();
        }
    }

    class DeviceAttachedEventArgs : EventArgs
    {
        public string DeviceName { get; set; }
    }

    class DeviceDetachedEventArgs : EventArgs
    {
        public string DeviceName { get; set; }
    }
}
