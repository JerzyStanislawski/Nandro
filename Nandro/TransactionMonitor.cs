using DotNano.RpcApi.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro
{
    class TransactionMonitor
    {
        private readonly NanoSocket _socket;
        private readonly INanoClient _nanoClient;
        private readonly Configuration _config;

        public TransactionMonitor(NanoSocket socket, INanoClient nanoClient, Configuration config)
        {
            _socket = socket;
            _nanoClient = nanoClient;
            _config = config;
        }

        public (string, IDictionary<string, BigInteger>, bool) Prepare(string nanoAccount)
        {
            var connected = _socket.Subscribe(_config.NanoSocketUri, nanoAccount);

            if (!connected)
            {
                var frontier = _nanoClient.GetFrontier(nanoAccount);
                var pendingTxs = _nanoClient.GetPendingTxs(nanoAccount);
                return (frontier, pendingTxs, connected);
            }
            else
                return (null, null, true);
        }

        public bool VerifyWithSocket(string nanoAccount, BigInteger raw)
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

        public bool VerifyWithNanoClient(string nanoAccount, BigInteger raw, string previousHash, IEnumerable<string> pendingHashes)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));

            var task = Task.Run(() =>
            {
                do
                {
                    var latestBlock = _nanoClient.GetLatestTransaction(nanoAccount);
                    if (VerifyLatestBlock(latestBlock, raw, previousHash))
                        return true;
                    var currentPendingTxs = _nanoClient.GetPendingTxs(nanoAccount);
                    if (VerifyPendingTxs(currentPendingTxs, pendingHashes, raw))
                        return true;

                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                }
                while (!cancellationTokenSource.IsCancellationRequested);

                return false;
            });

            return task.Result;
        }

        private bool VerifySocketResponse(NanoConfirmationResponse response, BigInteger raw, string nanoReceiveAddress)
        {
            if (response == null || response.Message == null)
                return false;

            var amount = BigInteger.Parse(response.Message.Amount);

            return response.Message.Block.Subtype == "send" && response.Message.Block.LinkAsAccount == nanoReceiveAddress && amount == raw;
        }


        private bool VerifyLatestBlock(AccountHistoryHistory latestBlock, BigInteger raw, string previousHash)
        {
            if (latestBlock == null)
                return false;

            return latestBlock.Type == "receive" && latestBlock.Amount == raw && String.Compare(latestBlock.Hash.HexKeyString, previousHash) != 0;
        }


        private bool VerifyPendingTxs(IDictionary<string, BigInteger> currentPendingTxs, IEnumerable<string> pendingHashes, BigInteger raw)
        {
            var currentPendingHashes = currentPendingTxs.Keys;
            var diff = currentPendingHashes.Except(pendingHashes);
            if (diff.Any())
            {
                foreach (var newPendingHash in diff)
                {
                    if (currentPendingTxs[newPendingHash] == raw)
                        return true;
                }
            }
            return false;
        }
    }
}
