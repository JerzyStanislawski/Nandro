using PCSC;
using System;
using System.Linq;

namespace Nandro.NFC
{
    class Initializer : IDisposable
    {
        private ISCardContext _context;

        public Device DetectDevice()
        {
            var contextFactory = ContextFactory.Instance;
            _context = contextFactory.Establish(SCardScope.System);
            var readerNames = _context.GetReaders();

            if (readerNames.Any())
            {
                var a = _context.Connect(readerNames[0], SCardShareMode.Shared, SCardProtocol.Any);
                var reader = (ICardReader)_context.ConnectReader(readerNames[0], SCardShareMode.Shared, SCardProtocol.Any);
                return new Device(reader);
            }

            return null;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
