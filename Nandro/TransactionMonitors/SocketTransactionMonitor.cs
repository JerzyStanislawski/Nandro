using Nandro.Nano;
using System;
using System.Numerics;
using System.Threading;

namespace Nandro.TransactionMonitors
{
    class SocketTransactionMonitor : IDisposable
    {
        private readonly INanoSocketClient _socket;
        private readonly Configuration _config;

        public SocketTransactionMonitor(INanoSocketClient socket, Configuration config)
        {
            _socket = socket;
            _config = config;
        }

        public bool Prepare(string nanoAccount, string uri)
        {
            return _socket.Subscribe(uri, nanoAccount, out _);
        }

        public bool Verify(string nanoAccount, BigInteger raw, CancellationTokenSource cancellationTokenSource, out string blockHash)
        {
            try
            {
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_config.TransactionTimeoutSec));

                if (_socket.Connected)
                {
                    NanoConfirmationResponse response;
                    do
                    {
                        response = _socket.Listen(cancellationTokenSource.Token);
                        if (VerifySocketResponse(response, raw, nanoAccount))
                        {
                            blockHash = response.Message.Hash;
                            return true;
                        }
                    }
                    while (response != null && !cancellationTokenSource.IsCancellationRequested);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_socket.Connected)
                    _socket.Close();
            }

            blockHash = String.Empty;
            return false;
        }

        public void Dispose()
        {
            _socket.Dispose();
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
