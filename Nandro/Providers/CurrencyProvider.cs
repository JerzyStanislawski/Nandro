using CoinGecko.Clients;
using Nandro.Models;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Nandro.Providers
{
    public class CurrencyProvider
    {
        private HttpClient _httpClient;
        private SimpleClient _coinGeckoClient;
        private Configuration _config;
        private Currency[] _allCurrencies;

        public CurrencyProvider()
        {
            _httpClient = new HttpClient();
            _coinGeckoClient = new SimpleClient(_httpClient);

            _config = Locator.Current.GetService<Configuration>();

            Initialize();
        }

        private void Initialize()
        {
            var allCurrenciesJson = File.ReadAllText("currencies.json");
            _allCurrencies = JsonSerializer.Deserialize<Currency[]>(allCurrenciesJson);
        }

        public Currency DefaultCurrency => _allCurrencies.SingleOrDefault(x => x.Code == _config.CurrencyCode);
        public bool IsDefaultCurrencyStandard => DefaultCurrency.Code == PriceProvider.UsdCode || DefaultCurrency.Code == PriceProvider.EurCode;

        public IEnumerable<Currency> GetSupportedCurrencies()
        {
            try
            {
                var exchangeCurrencies = _coinGeckoClient.GetSupportedVsCurrencies().Result;
                var exchangeCurrenciesUpper = exchangeCurrencies.Select(x => x.ToUpper());

                return _allCurrencies.Where(x => exchangeCurrenciesUpper.Contains(x.Code));
            }
            catch
            {
                return Array.Empty<Currency>();
            }
        }
    }
}
