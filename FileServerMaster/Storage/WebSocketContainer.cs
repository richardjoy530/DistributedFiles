using Common;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace FileServerMaster.Storage
{
    public class WebSocketContainer : IWebSocketContainer
    {
        private readonly ConcurrentDictionary<HostString, WebSocket> _sockets;
        private readonly ILogger<WebSocketContainer> _logger;

        public WebSocketContainer(ILogger<WebSocketContainer> logger)
        {
            _sockets = new ConcurrentDictionary<HostString, WebSocket>();
            _logger = logger;
        }

        public async Task Listen(WebSocket ws, ICollection<HostString> hostStrings)
        {
            var (ReciveResult, Message) = await ws.ReadAsync();
            while (!ws.CloseStatus.HasValue)
            {
                _logger.LogInformation("[Message] received message: \"{Message}\"", Message);
                HandleMessage(Message, ws, hostStrings);

                if (ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("[Connection] closing connection (remote server initiated close message)");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, ReciveResult.CloseStatusDescription, CancellationToken.None);
                }
                (ReciveResult, Message) = await ws.ReadAsync();
            }
        }

        public void Process(Action<(HostString Host, WebSocket Socket)> excecuter)
        {
            foreach (var ws in _sockets)
            {
                excecuter((ws.Key, ws.Value));
            }
        }

        private void HandleMessage(string message, WebSocket ws, ICollection<HostString> hostStrings)
        {
            if (message.Contains("slave server hosted at: "))
            {
                var section = message.Split('"')[1];
                var hosts = section.Split(';').Select(s => new HostString(s)).ToArray();
                foreach (var host in hosts)
                {
                    _logger.LogInformation("[SocketContainer] adding \"{host}\" to socket container", host);
                    _sockets[host] = ws;
                    hostStrings.Add(host);
                }
            }
        }

        public void DisposeAndRemove(HostString[] hostString)
        {
            foreach (var host in hostString)
            {
                if (_sockets.TryGetValue(host, out var ws))
                {
                    ws?.Dispose();
                    _sockets.Remove(host, out _);
                    _logger.LogInformation("[SocketContainer] removed \"{host}\" from socket container", host);
                }
            }
        }
    }
}