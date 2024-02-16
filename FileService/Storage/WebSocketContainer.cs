using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace Backend.Storage
{
    public class WebSocketContainer : IWebSocketContainer
    {
        private readonly List<WebSocket> _sockets;

        private readonly ILogger<WebSocketContainer> _logger;

        public WebSocketContainer(ILogger<WebSocketContainer> logger)
        {
            _sockets = [];
            _logger = logger;
        }

        public void AddWebSocket(WebSocket webSocket)
        {
            _logger.LogInformation($"Adding WS: {webSocket.GetHashCode()} to WebSocketContainer {webSocket.State}");
            _sockets.Add(webSocket);
        }

        public async Task RequestCheckinAsync()
        {
            await Process(ws =>
            {
                if (ws.State == WebSocketState.Open)
                {
                    _logger.LogInformation($"Sending checkin message to WS: {ws.GetHashCode()}");
                    return ws.SendAsync(new ArraySegment<byte>("Checkin"u8.ToArray()), WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None).AsTask();
                }

                return new Task(() => { });
            });
        }

        public async Task CloseWebSocketAsync()
        {
            await Process(ws =>
            {
                if (ws.State == WebSocketState.Open)
                {
                    _logger.LogInformation($"Closing WS: {ws.GetHashCode}");
                    return ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ContinueWith((_) => new Task(ws.Dispose));
                }

                return new Task(() => { });
            });
        }

        private async Task Process(Func<WebSocket, Task> excecuter)
        {
            foreach (var ws in _sockets)
            {
                await excecuter(ws);
            }

            //_sockets.RemoveAll(ws => ws.State != WebSocketState.Open);
        }
    }
}
