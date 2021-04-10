using DotNano.RpcApi.Responses;
using System.Collections.Generic;
using System.Numerics;

namespace Nandro
{
    internal interface INanoClient
    {
        AccountHistoryHistory GetLatestTransaction(string account);
        string GetFrontier(string account);
        IDictionary<string, BigInteger> GetPendingTxs(string nanoAccount);
    }
}