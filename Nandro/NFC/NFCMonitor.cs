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
        private bool _initialized;

        public event EventHandler<DeviceEventArgs> DeviceStatusChanged;
        public Device Device => _device;

        public void Start()
        {
            try
            {
                _context = ContextFactory.Instance.Establish(SCardScope.System);

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
                try
                {
                    _device = new Device(readerNames[0], _context.ConnectReader(readerNames[0], SCardShareMode.Direct, SCardProtocol.Unset));
                    return _device;
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        private void DeviceDetached(object sender, DeviceDetachedEventArgs e)
        {
            try
            {
                if (e.DeviceName == _device.Name)
                {
                    _device.Dispose();
                    _device = null;
                    DeviceStatusChanged.Invoke(this, new DeviceEventArgs(_device));
                }
            }
            catch (Exception)
            {
            }
        }

        private void DeviceAttached(object sender, DeviceAttachedEventArgs e)
        {
            try
            {
                _device?.Dispose();

                _device = new Device(e.DeviceName, _context.ConnectReader(e.DeviceName, SCardShareMode.Direct, SCardProtocol.Unset));
                DeviceStatusChanged.Invoke(this, new DeviceEventArgs(_device));
            }
            catch (Exception ex)
            {
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
