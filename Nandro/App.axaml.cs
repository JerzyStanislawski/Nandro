using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DotNano.RpcApi.Responses;
using DotNano.Shared.DataTypes;
using Nandro.ViewModels;
using Nandro.Views;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nandro
{
    public class App : Application
    {
        private MainWindowViewModel _mainWindowVM;

        public override void Initialize()
        {
            _mainWindowVM = new MainWindowViewModel();

            AvaloniaXamlLoader.Load(this);

            _mainWindowVM.DisplayQR();

            var settings = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            settings.Converters.Add(new PublicAddressConverter());
            settings.Converters.Add(new BigIntegerConverter());
            settings.Converters.Add(new HexKey64Converter());

            using var apiClient = new NanoApiClient("https://proxy.nanos.cc/proxy");
            var response = apiClient.AccountHistory("nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk");
            var history = JsonSerializer.Deserialize<AccountHistoryResponse>(response, settings);

            //using var socket = new NanoSocket();
            //if (socket.Subscribe("wss://socket.nanos.cc", "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk"))
            //{
            //    var response = socket.Listen();
            //    _mainWindowVM.Block = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true});
            //}
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _mainWindowVM,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        
    }

    class PublicAddressConverter : JsonConverter<PublicAddress>
    {
        public override PublicAddress? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            return new PublicAddress(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, PublicAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    class HexKey64Converter : JsonConverter<HexKey64>
    {
        public override HexKey64? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            return new HexKey64(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, HexKey64 value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    class BigIntegerConverter : JsonConverter<BigInteger>
    {
        public override BigInteger Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            return BigInteger.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
