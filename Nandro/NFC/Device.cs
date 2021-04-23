using NdefLibrary.Ndef;
using PCSC;
using PCSC.Iso7816;
using System;
using System.Text;

namespace Nandro.NFC
{
    class Device : IDisposable
    {
        private ICardReader _reader;
        public string Name => _reader.Name;

        public Device(ICardReader reader)
        {
            _reader = reader;
        }

        public void Transmit(string nanoUri)
        {
            var ndefRecord = new NdefUriRecord();
            ndefRecord.Uri = nanoUri;
            
            var ndefMessage = new NdefMessage { ndefRecord };
            //var apdu1 = new CommandApdu(IsoCase.Case2Short, _reader.Protocol)
            //{
            //    CLA = 0xFF,
            //    INS = new InstructionByte(InstructionCode.GetData),
            //    P1 = 0x00,
            //    P2 = 0x00,
            //    Le = 0
            //    //Data = ndefMessage.ToByteArray()
            //};
            //var apdu2 = new CommandApdu(IsoCase.Case4Short, _reader.Protocol)
            //{
            //    CLA = 0xFF,
            //    INS = new InstructionByte(InstructionCode.WriteRecord),
            //    P1 = 0x00,
            //    P2 = 0x00,
            //    Le = 0,
            //    Data = ndefMessage.ToByteArray()
            //};

           // using (_reader.Transaction(SCardReaderDisposition.Leave))
            {
                //var response = SendPseudoApdu(new byte[] { 0xD4, 0x06, 0x63, 0x05, 0x63, 0x0D, 0x63, 0x38 });

                //var param1 = response.FullApdu[2];
                //var param2 = response.FullApdu[3];
                //var param3 = response.FullApdu[4];

                //param1 = (byte)(param1 | 0x04);
                //param2 = (byte)(param2 & 0xEF);
                //param3 = (byte)(param3 & 0xF7);

                //response = SendPseudoApdu(new byte[] { 0xD4, 0x08, 0x63, 0x02, 0x80, 0x63, 0x03, 0x80, 0x63, 0x05, param1, 0x63, 0x0D, param2, 0x63, 0x38, param3 });
                //response = SendPseudoApdu(new byte[] { 0xD4, 0x12, 0x30 });
                var response = SendPseudoApdu(new byte[] { 0xD4, 0x8c, 0x05, 0x04, 0x00, 0x00, 0x00, 0x00, 0x20, 
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00});

                response = SendPseudoApdu(new byte[] { 0xD4, 0x86 });


            }
            //Send(apdu2);
        }

        private ResponseApdu SendPseudoApdu(byte[] payload)
        {
            var receiveBuffer = new byte[256];

                var apdu = new CommandApdu(IsoCase.Case4Short, _reader.Protocol)
                {
                    CLA = 0xFF,
                    INS = 0x00,
                    P1 = 0x00,
                    P2 = 0x00,
                    Data = payload
                }; 

             //var sendPci = SCardPCI.GetPci(SCardProtocol.T1);
                var bytesReceived = _reader.Transmit(apdu.ToArray(), receiveBuffer);

                if (bytesReceived > 0)
                {
                    Console.WriteLine(bytesReceived);
                    var responseApdu = new ResponseApdu(receiveBuffer, bytesReceived, apdu.Case, _reader.Protocol);
                    Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\ndata: {2}",
                        responseApdu.SW1,
                        responseApdu.SW2,
                        responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No data received");

                    return responseApdu;
                }
                else
            {
                var apdu2 = new CommandApdu(IsoCase.Case2Short, _reader.Protocol)
                {
                    CLA = 0xFF,
                    INS = 0xC0,
                    P1 = 0x00,
                    P2 = 0x00,
                    Le = 4
                };
                bytesReceived = _reader.Transmit(apdu.ToArray(), receiveBuffer);

                Console.WriteLine(bytesReceived);
                return null;
                }

           
        }

            private void Send(NdefMessage message)
        {
            var command = message.ToByteArray();
            var receiveBuffer = new byte[256];

            using (_reader.Transaction(SCardReaderDisposition.Leave))
            {
                var sendPci = SCardPCI.GetPci(_reader.Protocol);
                var receivePci = new SCardPCI();
                var bytesReceived = _reader.Transmit(command,receiveBuffer);

                if (bytesReceived > 0)
                {
                    Console.WriteLine(bytesReceived);
                    //Console.WriteLine(Encoding.UTF8.GetString(receiveBuffer));
                    //var responseApdu =
                    //    new ResponseApdu(receiveBuffer, bytesReceived, apdu.Case, _reader.Protocol);
                    //Console.WriteLine("SW1: {0:X2}, SW2: {1:X2}\ndata: {2}",
                    //    responseApdu.SW1,
                    //    responseApdu.SW2,
                    //    responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No data received");
                }
                else
                    Console.WriteLine("Sent");

            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
