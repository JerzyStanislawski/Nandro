using DotNano.RpcApi.Responses;
using System.Collections.Generic;
using System.Numerics;

namespace Nandro.Nano
{
    public interface INanoClient
    {
        AccountHistoryHistory GetLatestTransaction(string account);
        AccountHistoryResponse GetLatestTransactions(string account, int count);
        string GetFrontier(string account);
        IDictionary<string, BigInteger> GetPendingTxs(string nanoAccount);
        AccountBalanceResponse GetBalance(string account);
        string GetRepresentative(string account);
    }
}