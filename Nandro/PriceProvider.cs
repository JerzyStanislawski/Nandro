using CoinGecko.Clients;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro
{
    class PriceProvider : IDisposable
    {
        private Timer _timer;
        private HttpClient _httpClient;
        private SimpleClient _client;
        private static readonly object _lockObject = new object();

        public decimal UsdPrice { get; private set; }
        public decimal EurPrice { get; private set; }
        public bool UpToDate { get; private set; }
        public bool Initialized => UsdPrice != 0 && EurPrice != 0;

        public PriceProvider()
        {
            _httpClient = new HttpClient();
            _client = new SimpleClient(_httpClient);
            _timer = new Timer(new TimerCallback(state => GetNanoPrice(state)), null, 60 * 1000, 60 * 1000);
        }

        public void Initialize()
        {
            GetNanoPrice(null);
        }

        public decimal UsdToNano(decimal usdAmount)
        {
            lock (_lockObject)
            {
                if (UsdPrice == 0)
                    throw new InvalidOperationException("USD price not initialized");

                return usdAmount / UsdPrice;
            }
        }

        public decimal EurToNano(decimal eurAmount)
        {
            lock (_lockObject)
            {
                if (EurPrice == 0)
                    throw new InvalidOperationException("EUR price not initialized");

                return eurAmount / EurPrice;
            }
        }

        private void GetNanoPrice(object _)
        {
            try
            {
                var result = _client.GetSimplePrice(new[] { "nano" }, new[] { "usd", "eur" }).Result;

                lock (_lockObject)
                {
                    UsdPrice = result["nano"]["usd"].Value;
                    EurPrice = result["nano"]["eur"].Value;
                }
                UpToDate = true;
            }
            catch
            {
                UpToDate = false;
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
