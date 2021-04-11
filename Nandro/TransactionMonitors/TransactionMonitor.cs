using System.Numerics;

namespace Nandro.TransactionMonitors
{
    class TransactionMonitor
    {
        private readonly SocketTransactionMonitor _socketTransactionMonitor;
        private readonly RpcTransactionMonitor _rpcTransactionMonitor;

        public TransactionMonitor(SocketTransactionMonitor socketTransactionMonitor, RpcTransactionMonitor rpcTransactionMonitor)
        {
            _socketTransactionMonitor = socketTransactionMonitor;
            _rpcTransactionMonitor = rpcTransactionMonitor;
        }

       public bool Verify(string nanoAccount, BigInteger raw)
        {
            var connected = _socketTransactionMonitor.Prepare(nanoAccount);
            if (connected)
                return _socketTransactionMonitor.Verify(nanoAccount, raw);
            else
            {
                var (frontier, pendingTxs) = _rpcTransactionMonitor.Prepare(nanoAccount);
                return _rpcTransactionMonitor.Verify(nanoAccount, raw, frontier, pendingTxs?.Keys);
            }
        }
    }
}
