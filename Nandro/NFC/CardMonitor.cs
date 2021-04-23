using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using System;
using System.Numerics;

namespace Nandro.NFC
{
    class CardMonitor : IDisposable
    {
        private ISCardMonitor _monitor;
        private ISCardContext _context;

        public CardMonitor(ISCardContext context)
        {
            _context = context;
        }

        public void Start(string readerName)
        {
            _monitor = MonitorFactory.Instance.Create(SCardScope.System);

            _monitor.CardInserted += (sender, args) => CardInsertedEvent(args);
            //_monitor.CardRemoved += (sender, args) => DisplayEvent("CardRemoved", args);
            //_monitor.Initialized += (sender, args) => DisplayEvent("Initialized", args);
            _monitor.StatusChanged += StatusChanged;
            _monitor.MonitorException += MonitorException;

            _monitor.Start(readerName);

           // Connect(readerName);
        }

        private void MonitorException(object sender, PCSCException exception)
        {
        }

        private void StatusChanged(object sender, StatusChangeEventArgs e)
        {
            Console.WriteLine(e.NewState.ToString());
        }

        private void CardInsertedEvent(CardStatusEventArgs args)
        {
            Connect(args.ReaderName);
        }

        private void Connect(string readerName)
        {
            var nanoAccount = "nano_3wm37qz19zhei7nzscjcopbrbnnachs4p1gnwo5oroi3qonw6inwgoeuufdp";
            var amount = BigInteger.Parse("100000000000000000000000000");

            var reader = _context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any);
            using var device = new Device(reader);
            device.Transmit($"nano:{nanoAccount}?amount={amount}");
        }

        public void Dispose()
        {
            _monitor.Dispose();
        }
    }
}
