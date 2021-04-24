using PCSC;
using System;
using System.Linq;

namespace Nandro.NFC
{
    class NFCMonitor : IDisposable
    {
        private ISCardContext _context;

        private DeviceMonitor _deviceMonitor;
        private Device _device;

        public event EventHandler<DeviceEventArgs> DeviceStatusChanged;

        public NFCMonitor(DeviceMonitor deviceMonitor)
        {
            _deviceMonitor = deviceMonitor;

            _deviceMonitor.DeviceAttached += DeviceAttached;
            _deviceMonitor.DeviceDetached += DeviceDetached;
        }

        public void Start()
        {
            _deviceMonitor.Start();
        }

        public Device DetectDevice()
        {
            var contextFactory = ContextFactory.Instance;
            _context = contextFactory.Establish(SCardScope.System);
            var readerNames = _context.GetReaders();

            if (readerNames.Any())
            {
                Connect(readerNames[0]);
                return _device;
            }

            return null;
        }

        private void DeviceDetached(object sender, DeviceDetachedEventArgs e)
        {
            if (e.DeviceName == _device.Name)
            {
                _device.Dispose();
                _device = null;
                DeviceStatusChanged.Invoke(this, new DeviceEventArgs(_device));
            }
        }

        private void DeviceAttached(object sender, DeviceAttachedEventArgs e)
        {
            if (_device != null)
                _device.Dispose();

            Connect(e.DeviceName);
            DeviceStatusChanged.Invoke(this, new DeviceEventArgs(_device));
        }

        private void Connect(string name)
        {
            var reader = _context.ConnectReader(name, SCardShareMode.Shared, SCardProtocol.Any);
            _device = new Device(reader);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
