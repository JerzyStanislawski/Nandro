using NdefLibrary.Ndef;
using PCSC;
using PCSC.Iso7816;
using System;
using System.Text;

namespace Nandro.NFC
{
    class Device : IDisposable
    {
        private IsoReader _reader;
        public string Name => _reader.ReaderName;

        public Device(ISCardReader reader)
        {
            _reader = new IsoReader(reader);
        }

        public void Transmit(string nanoUri)
        {
            var ndefRecord = new NdefUriRecord { Uri = nanoUri };
            var ndefMessage = new NdefMessage { ndefRecord };
            var apdu = new CommandApdu(IsoCase.Case3Short, _reader.ActiveProtocol)
            {
                CLA = 0xFF,
                INS = new InstructionByte(InstructionCode.WriteRecord),
                P1 = 0x00,
                P2 = 0x00,
                Data = ndefMessage.ToByteArray()
            };

            _reader.Transmit(apdu);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
