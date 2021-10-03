namespace Nandro
{
    public class Configuration
    {
        public int Id { get; set; }
        public string NanoAccount { get; set; }
        public string NodeUri { get; set; }
        public string NodeSocketUri { get; set; }
        public string PublicNanoApiUri { get; set; }
        public string PublicNanoSocketUri { get; set; }
        public bool OwnNode { get; set; }
        public string CurrencyCode { get; set; }
        public int TransactionTimeoutSec { get; set; }
    }
}
