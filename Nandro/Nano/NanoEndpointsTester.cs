namespace Nandro.Nano
{
    class NanoEndpointsTester
    {
        const string _dummyAccount = "nano_1111111111111111111111111111111111111111111111111111hifc8npp";

        public bool TestSocket(string uri)
        {
            using var socketClient = new NanoSocketClient(new SocketWrapper(), new Configuration { TransactionTimeoutSec = 30 });
            return socketClient.Subscribe(uri, _dummyAccount);
        }

        public bool TestNode(string uri)
        {
            var nodeClient = new NanoNodeClient(uri);
            try
            {
                var result = nodeClient.GetPendingTxs(_dummyAccount);
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        public State TestState(Configuration config)
        {
            var state = new State();
            if (config.OwnNode)
            {
                if (!string.IsNullOrEmpty(config.NodeUri))
                    state.OwnNodeApi = TestNode(config.NodeUri);
                if (!string.IsNullOrEmpty(config.NodeSocketUri))
                    state.OwnNodeSocket = TestSocket(config.NodeSocketUri);
            }
            state.PublicNodeSocket = TestSocket(config.PublicNanoSocketUri);
            state.PublicNodeApi = TestNode(config.PublicNanoApiUri);

            return state;
        }
    }
}
