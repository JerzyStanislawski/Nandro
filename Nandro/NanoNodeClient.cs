using DotNano.RpcApi;
using DotNano.RpcApi.Responses;
using DotNano.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nandro
{
    class NanoNodeClient : INanoClient, IDisposable
    {
        private NanoRpcClient _rpcClient;

        public NanoNodeClient(string ipAddress, int port)
        {
            _rpcClient = new NanoRpcClient(ipAddress, port);
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
                    return (IDictionary<string, BigInteger>)response.Blocks.ToDictionary(x => x.Key, x => x.Value.Amount);
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
