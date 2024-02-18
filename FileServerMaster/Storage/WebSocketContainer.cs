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

        public async Task Listen(WebSocket ws) // todo: improve
        {
            _sockets.Add(ws);

            var (ReciveResult, Message) = await ws.ReadAsync();
            while (!ws.CloseStatus.HasValue)
            {
                _logger.LogInformation("recived message: \"{Message}\"", Message);

                if (ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("closing connection (remote server initiated close message)");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, ReciveResult.CloseStatusDescription, CancellationToken.None);
                }
                (ReciveResult, Message) = await ws.ReadAsync(); // what happens if this crashes?.
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
