using System;
using System.Numerics;
using System.Threading;

namespace Nandro.TransactionMonitors
{
    class TransactionMonitor : IDisposable
    {
        private readonly SocketTransactionMonitor _socketTransactionMonitor;
        private readonly RpcTransactionMonitor _rpcPublicTransactionMonitor;
        private readonly RpcTransactionMonitor _rpcLocalTransactionMonitor;
        private readonly Configuration _configuration;

        public TransactionMonitor(SocketTransactionMonitor socketTransactionMonitor, RpcTransactionMonitor rpcPublicTransactionMonitor, RpcTransactionMonitor rpcLocalTransactionMonitor, Configuration configuration)
        {
            _socketTransactionMonitor = socketTransactionMonitor;
            _rpcPublicTransactionMonitor = rpcPublicTransactionMonitor;
            _rpcLocalTransactionMonitor = rpcLocalTransactionMonitor;
            _configuration = configuration;
        }

        public bool Verify(string nanoAccount, BigInteger raw, out string blockHash, CancellationTokenSource externalCancellationTokenSource = null)
        {
            using var cancellationTokenSource = externalCancellationTokenSource ?? new CancellationTokenSource();

            if (!String.IsNullOrEmpty(_configuration.NodeSocketUri))
            {
                var connectedLocal = _socketTransactionMonitor.Prepare(nanoAccount, _configuration.NodeSocketUri);
                if (connectedLocal)
                    return _socketTransactionMonitor.Verify(nanoAccount, raw, cancellationTokenSource, out blockHash);                
            }

            if (_rpcLocalTransactionMonitor != null)
            {
                try
                {
                    var (frontier, pendingTxs) = _rpcLocalTransactionMonitor.Prepare(nanoAccount);
                    return _rpcLocalTransactionMonitor.Verify(nanoAccount, raw, frontier, pendingTxs?.Keys, cancellationTokenSource, out blockHash);
                }
                catch
                {
                }
            }

            var connectedPublic = _socketTransactionMonitor.Prepare(nanoAccount, _configuration.PublicNanoSocketUri);
            if (connectedPublic)
                return _socketTransactionMonitor.Verify(nanoAccount, raw, cancellationTokenSource, out blockHash);
            else
            {
                try
                {
                    var (frontier, pendingTxs) = _rpcPublicTransactionMonitor.Prepare(nanoAccount);
                    return _rpcPublicTransactionMonitor.Verify(nanoAccount, raw, frontier, pendingTxs?.Keys, cancellationTokenSource, out blockHash);
                }
                catch
                {
                }
            }

            blockHash = String.Empty;
            return false;
        }

        public void Dispose()
        {
            _socketTransactionMonitor.Dispose();
        }
    }
}
