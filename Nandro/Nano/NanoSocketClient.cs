using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;

namespace Nandro.Nano
{
    class NanoSocketClient : INanoSocketClient, IDisposable
    {
        private readonly IWebSocket _socket;
        private readonly Configuration _config;

        public NanoSocketClient(IWebSocket webSocket, Configuration config)
        {
            _socket = webSocket;
            _config = config;
        }

        public bool Subscribe(string url, string nanoAddress, out string error)
        {
            try
            {
                _socket.Connect(url);

                var payload = CreateSubscriptionJsonPayload(nanoAddress);
                _socket.Send(payload);

                var result = Receive<NanoAckResponse>(10, CancellationToken.None);

                error = String.Empty;
                return result?.Ack == "subscribe";
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public NanoConfirmationResponse Listen(CancellationToken cancellationToken)
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
                return null;

            return Receive<NanoConfirmationResponse>(_config.TransactionTimeoutSec, cancellationToken);
        }

        public void Close()
        {
            try
            {
                if (_socket != null && _socket.State == WebSocketState.Open)
                {
                    _socket.Close();
                }
            }
            catch
            {
            }
        }

        public bool Connected => _socket?.State == WebSocketState.Open;

        private T Receive<T>(int timeoutSec, CancellationToken cancellationToken) where T : class
        {
            try
            {
                var bytes = new ArraySegment<byte>(new byte[4096]);
                var result = _socket.Receive(bytes, timeoutSec, cancellationToken);

                if (result.MessageType != WebSocketMessageType.Close && result.Count > 0)
                {
                    var span = new ReadOnlySpan<byte>(bytes.Array, 0, result.Count);
                    return JsonSerializer.Deserialize<T>(span);
                }
            }
            catch (AggregateException e) when (e.InnerException is OperationCanceledException)
            {
            }
            return null;
        }

        private ArraySegment<byte> CreateSubscriptionJsonPayload(string nanoAddress)
        {           
            var subscription = new NanoSocketAction("subscribe", "confirmation", nanoAddress);
            var json = JsonSerializer.SerializeToUtf8Bytes(subscription);
            return new ArraySegment<byte>(json);
        }

        public void Dispose()
        {
            Close();
            _socket?.Dispose();
        }
    }
}
