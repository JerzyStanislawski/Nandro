namespace Nandro
{
    class Configuration
    {
        public string NanoNodeIp { get; set; }
        public int NanoNodePort { get; set; }
        public string NanoPublicApiUri { get; set; }
        public string NanoSocketUri { get; set; }
        public int TransactionTimeoutSec { get; set; }
    }
}
