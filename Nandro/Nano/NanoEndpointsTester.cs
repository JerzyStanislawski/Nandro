using System;

namespace Nandro.Nano
{
    class NanoEndpointsTester
    {
        const string _dummyAccount = "nano_1natrium1o3z5519ifou7xii8crpxpk8y65qmkih8e8bpsjri651oza8imdd";

        public bool TestSocket(string uri, out string error)
        {
            using var socketClient = new NanoSocketClient(new SocketWrapper(), new Configuration { TransactionTimeoutSec = 30 });
            return socketClient.Subscribe(uri, _dummyAccount, out error);
        }

        public bool TestNode(string uri, out string error)
        {
            try
            {
                var nodeClient = new NanoNodeClient(uri);
                var result = nodeClient.GetFrontier(_dummyAccount);
                
                error = String.Empty;
                return result != null;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        //public State TestState(Configuration config)
        //{
        //    var state = new State();
        //    if (config.OwnNode)
        //    {
        //        if (!string.IsNullOrEmpty(config.NodeUri))
        //            state.OwnNodeApi = TestNode(config.NodeUri);
        //        if (!string.IsNullOrEmpty(config.NodeSocketUri))
        //            state.OwnNodeSocket = TestSocket(config.NodeSocketUri);
        //    }
        //    state.PublicNodeSocket = TestSocket(config.PublicNanoSocketUri);
        //    state.PublicNodeApi = TestNode(config.PublicNanoApiUri);

        //    return state;
        //}
    }

    public enum EndpointTestResult
    {
        Success,
        Fail,
        Pending
    }
}
