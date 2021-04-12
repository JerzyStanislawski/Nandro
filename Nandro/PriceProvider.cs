using CoinGecko.Clients;
using System;
using System.Net.Http;
using System.Threading;

namespace Nandro
{
    class PriceProvider : IDisposable
    {
        private Timer _timer;
        private HttpClient _httpClient;
        private SimpleClient _client;

        private decimal _usdPrice;
        private decimal _eurPrice;

        public PriceProvider()
        {
            _timer = new Timer(new TimerCallback(GetNanoPrice), null, 0, 60 * 1000);
            _httpClient = new HttpClient();
            _client = new SimpleClient(_httpClient);
        }

        public decimal UsdToNano(decimal usdAmount)
        {
            if (_usdPrice == 0)
                throw new InvalidOperationException("USD price not initialized");

            return usdAmount / _usdPrice;
        }

        public decimal EurToNano(decimal eurAmount)
        {
            if (_eurPrice == 0)
                throw new InvalidOperationException("USD price not initialized");

            return eurAmount / _eurPrice;
        }

        private void GetNanoPrice(object _)
        {
            try
            {
                var result = _client.GetSimplePrice(new[] { "nano" }, new[] { "usd", "eur" }).Result;

                _usdPrice = result["nano"]["usd"].Value;
                _eurPrice = result["nano"]["eur"].Value;
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
