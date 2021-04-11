using DotNano.RpcApi.Responses;
using Nandro.Nano;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro.TransactionMonitors
{
    class RpcTransactionMonitor
    {
        private readonly INanoClient _nanoClient;

        public (string, IDictionary<string, BigInteger>) Prepare(string nanoAccount)
        {
                var frontier = _nanoClient.GetFrontier(nanoAccount);
                var pendingTxs = _nanoClient.GetPendingTxs(nanoAccount);
                return (frontier, pendingTxs);
        }

        public bool Verify(string nanoAccount, BigInteger raw, string previousHash, IEnumerable<string> pendingHashes)
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
