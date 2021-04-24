namespace Nandro.NFC
{
    class DeviceEventArgs
    {
        public DeviceEventArgs(Device device)
        {
            Device = device;
        }

        public Device Device { get; }
    }
}