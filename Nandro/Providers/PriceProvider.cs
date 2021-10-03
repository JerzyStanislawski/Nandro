using CoinGecko.Clients;
using Splat;
using System;
using System.Net.Http;

namespace Nandro.Providers
{
    class PriceProvider : IDisposable
    {
        public const string UsdCode = "USD";
        public const string EurCode = "EUR";

        private MinuteTimer _timer;
        private string _currencyCode;
        private HttpClient _httpClient;
        private SimpleClient _coinGeckoClient;
        private static readonly object _lockObject = new object();

        public decimal CurrencyPrice { get; private set; }
        public decimal UsdPrice { get; private set; }
        public decimal EurPrice { get; private set; }
        public bool UpToDate { get; private set; }
        public bool Initialized => UsdPrice != 0 && EurPrice != 0 && CurrencyPrice != 0;

        public PriceProvider()
        {
            _httpClient = new HttpClient();
            _coinGeckoClient = new SimpleClient(_httpClient);
            _timer = new MinuteTimer(GetNanoPrice, false);
        }

        public void Initialize()
        {
            _currencyCode = Locator.Current.GetService<Configuration>().CurrencyCode.ToLower();

            GetNanoPrice();
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

        public decimal CurrencyToNano(decimal amount)
        {
            lock (_lockObject)
            {
                if (CurrencyPrice == 0)
                    throw new InvalidOperationException($"{_currencyCode} price not initialized");

                return amount / CurrencyPrice;
            }
        }

        private void GetNanoPrice()
        {
            try
            {
                var result = _coinGeckoClient.GetSimplePrice(new[] { "nano" }, new[] { UsdCode.ToLower(), EurCode.ToLower(), _currencyCode }).Result;

                lock (_lockObject)
                {
                    UsdPrice = result["nano"][UsdCode.ToLower()].Value;
                    EurPrice = result["nano"][EurCode.ToLower()].Value;
                    CurrencyPrice = result["nano"][_currencyCode].Value;
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
