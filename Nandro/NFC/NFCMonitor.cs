using PCSC;
using System;
using System.Linq;

namespace Nandro.NFC
{
    class NFCMonitor : IDisposable
    {
        private ISCardContext _context;

        private DeviceMonitor _deviceMonitor;
        private CardMonitor _cardMonitor;
        private Device _device;
        private bool _initialized;

        public event EventHandler<DeviceEventArgs> DeviceStatusChanged;

        public void Start()
        {
            try
            {
                var contextFactory = ContextFactory.Instance;
                _context = contextFactory.Establish(SCardScope.System);

                _cardMonitor = new CardMonitor(_context);

                _deviceMonitor = new DeviceMonitor();
                _deviceMonitor.DeviceAttached += DeviceAttached;
                _deviceMonitor.DeviceDetached += DeviceDetached;
                _deviceMonitor.Start();

                _initialized = true;
            }
            catch
            {
            }
        }

        public Device DetectDevice()
        {
            if (!_initialized)
                return null;

            var readerNames = _context.GetReaders();

            if (readerNames.Any())
            {
                _device = new Device(_cardMonitor, readerNames[0]);
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

            _device = new Device(_cardMonitor, e.DeviceName);
            DeviceStatusChanged.Invoke(this, new DeviceEventArgs(_device));
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
