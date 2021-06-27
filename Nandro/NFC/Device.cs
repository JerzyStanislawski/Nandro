using Nandro.Nano;
using NdefLibrary.Ndef;
using PCSC;
using PCSC.Iso7816;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace Nandro.NFC
{
    class Device : IDisposable
    {
        private ICardReader _reader;
        public string Name { get; }

        public Device(string name, ICardReader cardHandle)
        {
            Name = name;
            _reader = cardHandle;
        }

        public void Transmit(string nanoAccount, BigInteger amount, CancellationToken cancellationToken)
        {
            var ndefRecord = new NdefUriRecord
            {
                Uri = Tools.GenerateNanoUri(nanoAccount, amount)
            };

            var ndefMessage = new NdefMessage { ndefRecord };
            var ndefBytes = ndefMessage.ToByteArray();

            while (!EmulateNanoUriTag(ndefBytes, cancellationToken) && !cancellationToken.IsCancellationRequested);
        }

        private bool EmulateNanoUriTag(byte[] nanoUriNdef, CancellationToken cancellationToken)
        {
            try
            {
                SetParameters();

                InitAsTarget(cancellationToken);
                var response = GetDataCommand();
                if (response == null || response.GetData()[2] != 0x00)
                {
                    InitAsTarget(cancellationToken);
                    response = GetDataCommand();
                }
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x87, 0x00, 0x00, 0xA4, 0x04, 0x00 }); // D5-87-00 - correct TgGetData response; 00-A4-04-00 - Select application command

                if (cancellationToken.IsCancellationRequested)
                    return false;

                // Pure NDEF length, without SetData bytes (0xD4, 0x8E) and R-APDU SW1, SW2 bytes (0x90, 0x00)
                var length = nanoUriNdef.Length;

                AcknowledgeCommand();
                response = GetDataCommand();
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x87, 0x00, 0x00, 0xA4, 0x00, 0x0C }); // D5-87-00 - correct TgGetData response; 00-A4-00-0C - Select capability container

                AcknowledgeCommand();
                response = GetDataCommand();
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x87, 0x00, 0x00, 0xB0, 0x00, 0x00 }); // D5-87-00 - correct TgGetData response; 00-B0-00-00 - Read capability container file

                SendPseudoApdu(new byte[] { 0xD4, 0x8E, 
                    0x00, 0x0F, //Length of CC file
                    0x20, // Version 2.0
                    0x00, 0xFF, // Max R-APDU size
                    0x00, 0xFF, // Max C-APDU size
                    0x04, // Control File type
                    0x06, // Control Type length
                    0xE1, 0x04, // NDEF file identifier
                    0x00, (byte)(length + 2), // Max NDEF length,
                    0x00, // Read access (no security)
                    0x00, // Write access (no security)
                    0x90, 0x00 });
                response = GetDataCommand();
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x87, 0x00, 0x00, 0xA4, 0x00, 0x0C, 0x02, 0xE1, 0x04 }); // D5-87-00 - correct TgGetData response; 00-A4-00-0C-02-E1-04 - Select NDEF file at E1-04

                AcknowledgeCommand();
                response = GetDataCommand();
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x87, 0x00, 0x00, 0xB0, 0x00, 0x00, 0x02 }); // D5-87-00 - correct TgGetData response; 00-B0-00-00-02 - Read NDEF file length (2 bytes)

                response = SendPseudoApdu(new byte[] { 0xD4, 0x8E, 0x00, (byte)(length), 0x90, 0x00 }); // Send length of pure NDEF file
                response = GetDataCommand();
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x87, 0x00, 0x00, 0xB0, 0x00 }); // D5-87-00 - correct TgGetData response; 00-B0-00 - Read NDEF file

                // The NFC Forum Type 4 Tag specification is not clear if the file is read at the offset of 0 (NDEF length + NDEF content) or 2 (just NDEF content). 
                // NDEF file must be prepared for both cases.
                var offset = response.GetData()[6];
                if (offset != 0x00 && offset != 0x02)
                    return false;

                var ndefSendCommand = BuildNdefCommand(nanoUriNdef, offset);
                response = SendPseudoApdu(ndefSendCommand);

                Beep();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Beep()
        {
            var receiveBuffer = new byte[2];
            var apdu = new CommandApdu(IsoCase.Case3Short, _reader.Protocol)
            {
                CLA = 0xFF,
                INS = 0x00,
                P1 = 0x40,
                P2 = 0xA0,
                Data = new byte[] { 0x05, 0x00, 0x01, 0x01 }
            };
            var arr = apdu.ToArray();

            try
            {
                var bytesReceived = _reader.Control(new IntPtr(3225264), arr, arr.Length, receiveBuffer, 2);
            }
            catch (Exception ex)
            {
            }
        }

        private byte[] BuildNdefCommand(byte[] nanoUriNdef, byte offset)
        {
            var ndefSendCommand = new byte[offset == 0x00 ? nanoUriNdef.Length + 6 : nanoUriNdef.Length + 4];
            ndefSendCommand[0] = 0xD4;
            ndefSendCommand[1] = 0x8E;

            if (offset == 0x00)
            {
                ndefSendCommand[2] = 0x00;
                ndefSendCommand[3] = (byte)nanoUriNdef.Length;
            }

            nanoUriNdef.CopyTo(ndefSendCommand, offset == 0x00 ? 4 : 2);
            ndefSendCommand[nanoUriNdef.Length + (offset == 0x00 ? 4 : 2)] = 0x90;
            ndefSendCommand[nanoUriNdef.Length + (offset == 0x00 ? 5 : 3)] = 0x00;

            return ndefSendCommand;
        }

        private ResponseApdu AcknowledgeCommand()
        {
            return SendPseudoApdu(new byte[] { 0xD4, 0x8E, 0x90, 0x00 });
        }

        private ResponseApdu GetDataCommand()
        {
            return SendPseudoApdu(new byte[] { 0xD4, 0x86 });
        }

        private void SetParameters()
        {
            var response = SendPseudoApdu(new byte[] { 0xD4, 0x06, 0x63, 0x05, 0x63, 0x0D, 0x63, 0x38 });

            var param1 = response.FullApdu[2];
            var param2 = response.FullApdu[3];
            var param3 = response.FullApdu[4];

            param1 = (byte)(param1 | 0x04);
            param2 = (byte)(param2 & 0xEF);
            param3 = (byte)(param3 & 0xF7);

            SendPseudoApdu(new byte[] { 0xD4, 0x08, 0x63, 0x02, 0x80, 0x63, 0x03, 0x80, 0x63, 0x05, param1, 0x63, 0x0D, param2, 0x63, 0x38, param3 });
            SendPseudoApdu(new byte[] { 0xD4, 0x12, 0x30 });
        }

        private ResponseApdu SendPseudoApdu(byte[] payload)
        {
            var receiveBuffer = new byte[256];

            var apdu = new CommandApdu(IsoCase.Case3Short, _reader.Protocol)
            {
                CLA = 0xFF,
                INS = 0x00,
                P1 = 0x00,
                P2 = 0x00,
                Data = payload
            };
            var arr = apdu.ToArray();

            try
            {
                var bytesReceived = _reader.Control(new IntPtr(3225264), arr, arr.Length, receiveBuffer, 256);

                return (bytesReceived > 0)
                    ? new ResponseApdu(receiveBuffer, bytesReceived, apdu.Case, _reader.Protocol)
                    : null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void InitAsTarget(CancellationToken cancellationToken)
        {
            ResponseApdu response = null;
            do
            {
                response = SendPseudoApdu(new byte[] { 0xD4, 0x8c, 0x05, 0x04, 0x00, 0x12, 0x34, 0x56, 0x20,
                      0x01, 0xFE,         // NFCID2t (8 bytes) https://github.com/adafruit/Adafruit-PN532/blob/master/Adafruit_PN532.cpp FeliCa NEEDS TO BEGIN WITH 0x01 0xFE!
                      0x05, 0x01, 0x86,
                      0x04, 0x02, 0x02,
                      0x03, 0x00,         // PAD (8 bytes)
                      0x4B, 0x02, 0x4F,
                      0x49, 0x8A, 0x00,
                      0xFF, 0xFF,         // System code (2 bytes)
      
                      0x01, 0x01, 0x66,   // NFCID3t (10 bytes)
                      0x6D, 0x01, 0x01, 0x10,
                      0x02, 0x00, 0x00,
                      0x00, 0x00});
            }
            while (response is null && !cancellationToken.IsCancellationRequested);

            if (response != null)
            {
                EnsureResponseExpected(response, new byte[] { 0xD5, 0x8D, 0x08 });
            }
        }

        private void EnsureResponseExpected(ResponseApdu response, byte[] expectedPrefix)
        {
            if (!response.IsValid || response.SW1 != 0x90)
                throw new InvalidApduException("Response invalid");

            if (!expectedPrefix.SequenceEqual(response.GetData().Take(expectedPrefix.Length)))
                throw new InvalidApduException("Unexpected response");
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
