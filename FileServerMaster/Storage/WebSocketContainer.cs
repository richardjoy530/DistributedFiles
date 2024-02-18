using Common;
using System.Net.WebSockets;

namespace FileServerMaster.Storage
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

        public async Task Listen(WebSocket webSocket) // todo: improve
        {
            _logger.LogInformation($"Adding WS: {webSocket.GetHashCode()} to WebSocketContainer");
            _sockets.Add(webSocket);

            while (webSocket.State == WebSocketState.Open && !webSocket.CloseStatus.HasValue)
            {
                var rslt = await webSocket.ReadAsync();
                if (rslt.ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation($"Closing WS: {webSocket.GetHashCode}");
                    await webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.NormalClosure, webSocket.CloseStatusDescription, CancellationToken.None);
                    webSocket.Dispose();
                }
            }
        }

        public async Task RequestCheckinAsync()
        {
            await Process(ws =>
            {
                if (ws.State == WebSocketState.Open)
                {
                    return ws.WriteAsync("checkin");
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
            _sockets.RemoveAll(ws => ws.State != WebSocketState.Open);

            foreach (var ws in _sockets)
            {
                await excecuter(ws);
            }
        }
    }
}
