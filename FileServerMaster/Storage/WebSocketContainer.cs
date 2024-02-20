using Common;
using System.Net.WebSockets;

namespace FileServerMaster.Storage
{
    public class WebSocketContainer : IWebSocketContainer
    {
        private readonly IDictionary<HostString, WebSocket> _sockets;

        private readonly ILogger<WebSocketContainer> _logger;

        public WebSocketContainer(ILogger<WebSocketContainer> logger)
        {
            _sockets = new Dictionary<HostString, WebSocket>();
            _logger = logger;
        }

        public async Task Listen(WebSocket ws)
        {
            var (ReciveResult, Message) = await ws.ReadAsync();
            while (!ws.CloseStatus.HasValue)
            {
                _logger.LogInformation("recived message: \"{Message}\"", Message);
                HandleMessage(Message, ws);

                if (ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("closing connection (remote server initiated close message)");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, ReciveResult.CloseStatusDescription, CancellationToken.None);
                }
                (ReciveResult, Message) = await ws.ReadAsync();
            }
        }

        public async Task CloseWebSocketAsync()
        {
            await Process(ws =>
            {
                _logger.LogInformation($"Closing WS: {ws.GetHashCode}");
                return ws.Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ContinueWith((_) => new Task(ws.Socket.Dispose));
            });
        }

        public async Task Process(Func<(HostString Host, WebSocket Socket), Task> excecuter)
        {

            var closedhosts = _sockets.Where(kv => kv.Value.State != WebSocketState.Open).Select(kv => kv.Key);

            foreach (var host in closedhosts)
            {
                _logger.LogDebug("removing \"{host}\" from socket container since its closed", host);
                _sockets.Remove(host);
            }

            foreach (var ws in _sockets)
            {
                await excecuter((ws.Key, ws.Value));
            }
        }

        private void HandleMessage(string message, WebSocket ws)
        {
            if (message.Contains("slave server hosted at: "))
            {
                var section = message.Split('"')[1];
                var hosts = section.Split(';').Select(s => new HostString(s)).ToArray();
                foreach (var host in hosts)
                {
                    _logger.LogInformation("adding \"{host}\" to socket container", host);
                    _sockets.Add(host, ws);
                }
            }
        }
    }
}
