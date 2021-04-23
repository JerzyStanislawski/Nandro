using System.IO;
using System.Text.Json;

namespace Nandro
{
    public class Configuration
    {
        public string NanoAccount { get; set; }
        public string NodeUri { get; set; }
        public string NodeSocketUri { get; set; }
        public string PublicNanoApiUri { get; set; }
        public string PublicNanoSocketUri { get; set; }
        public bool OwnNode { get; set; }
        public int TransactionTimeoutSec { get; set; }

        const string _fileName = "config.json";
        public void Persist()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_fileName, json);
        }

        public static Configuration Load()
        {
            try
            {
                var json = File.ReadAllText(_fileName);
                return JsonSerializer.Deserialize<Configuration>(json);
            }
            catch
            {
                return new Configuration
                {
                    PublicNanoApiUri = "https://proxy.nanos.cc/proxy",
                    PublicNanoSocketUri = "wss://socket.nanos.cc",
                    TransactionTimeoutSec = 60
                };
            }
        }
    }
}
