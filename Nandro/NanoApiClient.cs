using DotNano.RpcApi;
using System;
using System.Net.Http;

namespace Nandro
{
    class NanoApiClient : IDisposable
    {
        private HttpClient _apiClient;
        private string _url;

        public NanoApiClient(string url)
        {
            _apiClient = new HttpClient();
            _url = url;
        }

        public string? AccountHistory(string account)
        {
            try
            {
                var url = $"{_url}/?action=account_history&account={account}&count=10";
                var result = _apiClient.GetAsync(url).Result;

                if (result.IsSuccessStatusCode)
                    return result.Content.ReadAsStringAsync().Result;
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }
}
