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
        }

        public void Dispose()
        {
            _monitor.Dispose();
        }
    }
}
