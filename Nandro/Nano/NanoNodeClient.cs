using DotNano.RpcApi;
using DotNano.RpcApi.Responses;
using DotNano.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nandro.Nano
{
    class NanoNodeClient : INanoClient, IDisposable
    {
        private NanoRpcClient _rpcClient;

        public NanoNodeClient(string uri)
        {
            _rpcClient = new NanoRpcClient(uri);
        }

        public AccountHistoryHistory GetLatestTransaction(string account)
        {
            var response = _rpcClient.AccountHistory(account, 1).Result;
            if (response.IsSuccessful)
            {
                if (response.History.Any())
                    return response.History.First();
                else
                    return null;
            }
            else
                throw new Exception($"Error from Node: {response.Error}");
        }

        public AccountHistoryResponse GetLatestTransactions(string account, int count)
        {
            var response = _rpcClient.AccountHistory(account, count).Result;
            if (response.IsSuccessful)
                return response;
            else
                throw new Exception($"Error from Node: {response.Error}");
        }

        public string GetFrontier(string account)
        {
            var response = _rpcClient.Frontiers(account, 1).Result;
            if (response.IsSuccessful)
            {
                if (response.Frontiers.Any())
                    return response.Frontiers[new PublicAddress(account)].HexKeyString;
                else
                    return null;
            }
            else
                throw new Exception($"Error from Node: {response.Error}");
        }

        public IDictionary<string, BigInteger> GetPendingTxs(string account)
        {
            var response = (PendingResponse)_rpcClient.Pending(new PublicAddress(account), source: true).Result;
            if (response.IsSuccessful)
            {
                if (response.Blocks.Any())
                    return response.Blocks.ToDictionary(x => x.Key.HexKeyString, x => x.Value.Amount);
                else
                    return null;
            }
            else
                throw new Exception($"Error from Node: {response.Error}");
        }

        public void Dispose()
        {
            _rpcClient.Dispose();
        }
    }
}
