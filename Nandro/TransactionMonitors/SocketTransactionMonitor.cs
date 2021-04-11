using Nandro.Nano;
using System;
using System.Numerics;

namespace Nandro.TransactionMonitors
{
    class SocketTransactionMonitor
    {
        private readonly INanoSocketClient _socket;
        private readonly Configuration _config;

        public SocketTransactionMonitor(INanoSocketClient socket, Configuration config)
        {
            _socket = socket;
            _config = config;
        }

        public bool Prepare(string nanoAccount)
        {
            return _socket.Subscribe(_config.NanoSocketUri, nanoAccount);
        }

        public bool Verify(string nanoAccount, BigInteger raw)
        {
            try
            {
                if (_socket.Connected)
                {
                    NanoConfirmationResponse response;
                    do
                    {
                        response = _socket.Listen();
                        if (VerifySocketResponse(response, raw, nanoAccount))
                            return true;
                    }
                    while (response != null);
                }
            }
            catch (OperationCanceledException)
            {
            }
            return false;
        }

        private bool VerifySocketResponse(NanoConfirmationResponse response, BigInteger raw, string nanoReceiveAddress)
        {
            if (response == null || response.Message == null)
                return false;

            var amount = BigInteger.Parse(response.Message.Amount);

            return response.Message.Block.Subtype == "send" && response.Message.Block.LinkAsAccount == nanoReceiveAddress && amount == raw;
        }
    }
}
