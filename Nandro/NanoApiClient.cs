using DotNano.RpcApi.Responses;
using DotNano.Shared.DataTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;

namespace Nandro
{
    class NanoApiClient : INanoClient, IDisposable
    {
        private readonly HttpClient _apiClient;
        private readonly string _url;
        private readonly JsonSerializerSettings _jsonSettings;

        public NanoApiClient(string url)
        {
            _apiClient = new HttpClient();
            _url = url;
            _jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new UnderscorePropertyNamesContractResolver()
            };
        }

        public AccountHistoryHistory GetLatestTransaction(string account)
        {
            var url = $"{_url}/?action=account_history&account={account}&count=1";
            var result = _apiClient.GetAsync(url).Result;

            if (result.IsSuccessStatusCode)
            {
                var history = JsonConvert.DeserializeObject<AccountHistoryResponse>(result.Content.ReadAsStringAsync().Result, _jsonSettings);
                if (!history.IsSuccessful)
                    throw new Exception($"Error from Node: {history.Error}");

                if (history.History.Any())
                    return history.History.First();
                else
                    return new AccountHistoryHistory();
            }
            else
                throw new Exception($"Error from API endpoint: {result.StatusCode}");
        }

        public string GetFrontier(string account)
        {
            var url = $"{_url}/?action=frontiers&account={account}&count=1";
            var result = _apiClient.GetAsync(url).Result;

            if (result.IsSuccessStatusCode)
            {
                var frontiers = JsonConvert.DeserializeObject<FrontiersResponse>(result.Content.ReadAsStringAsync().Result, _jsonSettings);

                if (!frontiers.IsSuccessful)
                    throw new Exception($"Error from Node: {frontiers.Error}");

                if (frontiers.Frontiers.Any())
                    return frontiers.Frontiers[new PublicAddress(account)].HexKeyString;
                else
                    return String.Empty;
            }
            else
                throw new Exception($"Error from API endpoint: {result.StatusCode}");
        }

        public IDictionary<string, BigInteger> GetPendingTxs(string account)
        {
            var url = $"{_url}/?action=pending&account={account}&source=true";
            var result = _apiClient.GetAsync(url).Result;

            if (result.IsSuccessStatusCode)
            {
                var pendingBlocks = JsonConvert.DeserializeObject<PendingResponse>(result.Content.ReadAsStringAsync().Result, _jsonSettings);

                if (!pendingBlocks.IsSuccessful)
                    throw new Exception($"Error from Node: {pendingBlocks.Error}");

                if (pendingBlocks.Blocks != null && pendingBlocks.Blocks.Any())
                    return pendingBlocks.Blocks.ToDictionary(x => x.Key.HexKeyString, x => x.Value.Amount);
                else
                    return new Dictionary<string, BigInteger>();
            }
            else
                throw new Exception($"Error from API endpoint: {result.StatusCode}");
        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }

    public class UnderscorePropertyNamesContractResolver : DefaultContractResolver
    {
        public UnderscorePropertyNamesContractResolver()
        {
            NamingStrategy = new SnakeCaseNamingStrategy();
        }
    }
}
