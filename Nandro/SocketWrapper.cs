using System;
using System.Net.WebSockets;
using System.Threading;

namespace Nandro
{
    public interface IWebSocket : IDisposable
    {
        void Connect(string url);
        WebSocketReceiveResult Receive(ArraySegment<byte> bytes, int timeoutSec, CancellationToken cancellationToken);
        void Send(ArraySegment<byte> bytes);
        void Close();
        WebSocketState State { get; }
    }

    public class SocketWrapper : IWebSocket
    {
        private readonly ClientWebSocket _websocket;

        public SocketWrapper()
        {
            _websocket = new ClientWebSocket();
        }

        public void Connect(string url)
        {
            using var tokenSource = GetTokenSource(10);

            _websocket.ConnectAsync(new Uri(url), tokenSource.Token).Wait();
        }

        public WebSocketReceiveResult Receive(ArraySegment<byte> bytes, int timeoutSec, CancellationToken cancellationToken)
        {
            using var timeoutTokenSource = GetTokenSource(timeoutSec);
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken);

            return _websocket.ReceiveAsync(bytes, tokenSource.Token).Result;
        }

        public void Send(ArraySegment<byte> bytes)
        {
            using var tokenSource = GetTokenSource(10);

            _websocket.SendAsync(bytes, WebSocketMessageType.Text, true, tokenSource.Token).Wait();
        }

        public void Close()
        {
            using var tokenSource = GetTokenSource(10);

            _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, tokenSource.Token).Wait();
        }

        private CancellationTokenSource GetTokenSource(int seconds)
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(seconds));

            return tokenSource;
        }

        public WebSocketState State => _websocket.State;

        public void Dispose()
        {
            _websocket.Dispose();
        }
    }
}