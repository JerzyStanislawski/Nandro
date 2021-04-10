using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Nandro
{
    class NanoSocket : IDisposable
    {
        private readonly ClientWebSocket _socket;

        public NanoSocket()
        {
            _socket = new ClientWebSocket();
        }

        public bool Subscribe(string url, string nanoAddress)
        {
            try
            {
                using var tokenSource = new CancellationTokenSource();
                tokenSource.CancelAfter(TimeSpan.FromSeconds(10));

                _socket.ConnectAsync(new Uri(url), tokenSource.Token).Wait();

                var payload = CreateSubscriptionJsonPayload(nanoAddress);
                _socket.SendAsync(payload, WebSocketMessageType.Text, true, tokenSource.Token).Wait();

                var result = Receive<NanoAckResponse>();

                return result.Ack == "subscribe";
            }
            catch
            {
                return false;
            }
        }

        public NanoConfirmationResponse Listen()
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
                return null;

            return Receive<NanoConfirmationResponse>();
        }

        public void Close()
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                using var tokenSource = new CancellationTokenSource();
                tokenSource.CancelAfter(TimeSpan.FromSeconds(10));

                _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, tokenSource.Token);
            }
        }

        public bool Connected => _socket?.State == WebSocketState.Open;

        private T Receive<T>() where T : class
        {
            using var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(60));

            var bytes = new ArraySegment<byte>(new byte[4096]);
            var result = _socket.ReceiveAsync(bytes, tokenSource.Token).Result;

            if (result.MessageType != WebSocketMessageType.Close && result.Count > 0)
            {
                var span = new ReadOnlySpan<byte>(bytes.Array, 0, result.Count);
                return JsonSerializer.Deserialize<T>(span);
            }
            else
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
