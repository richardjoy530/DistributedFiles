using Common;
using Common.Events;
using FileServerSlave.Events;
using FileServerSlave.Utils;
using System.Net.WebSockets;

namespace FileServerSlave
{
    public class SocketManager : ISocketManager
    {
        private readonly ILogger<SocketManager> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IHostStringRetriever _hostStringRetriever;
        private readonly HostString _hostString;
        private readonly bool _secure;
        private readonly int _retryInSeconds;

        public SocketManager(ILogger<SocketManager> logger,
            IEventDispatcher eventQueueManager,
            IHostStringRetriever slaveHostStringRetriever,
            IMasterServerRetriever masterServerRetriever)
        {
            ArgumentNullException.ThrowIfNull(masterServerRetriever);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventQueueManager ?? throw new ArgumentNullException(nameof(eventQueueManager));
            _hostStringRetriever = slaveHostStringRetriever ?? throw new ArgumentNullException(nameof(slaveHostStringRetriever));

            _retryInSeconds = masterServerRetriever.RetryInSeconds;
            _secure = masterServerRetriever.Secure;
            _hostString = masterServerRetriever.GetMasterHostString();

            _logger.LogDebug("configured master host address is \"{host}\"", _hostString);
        }

        public async void EstablishConnection(CancellationToken ct)
        {
            _logger.LogInformation("[Connection] starting socket listener");
            while (!ct.IsCancellationRequested)
            {
                await Listen(ct);
                _logger.LogInformation("[Connection] trying to reconnect after \"{}\" seconds", _retryInSeconds);
                await Task.Delay(1000 * _retryInSeconds, ct);
            }
        }

        private async Task Listen(CancellationToken ct)
        {
            ClientWebSocket? ws = null;
            try
            {
                var hostStrings = string.Join(';', _hostStringRetriever.GetLocalFileServerHosts());
                ws = new ClientWebSocket();

                var wssScheme = _secure ? "wss" : "ws";
                var wsUri = new Uri($"{wssScheme}://{_hostString}/ws");

                _logger.LogInformation("[Connection] trying to connect \"{wsUri}\"", wsUri);
                await ws.ConnectAsync(wsUri, ct);
                _logger.LogInformation("[Connection] connection established to \"{wsUri}\"", wsUri);

                await ws.WriteAsync("ping", ct);
                await ws.WriteAsync($"slave server hosted at: \"{hostStrings}\"", ct);

                var (receiveResult, message) = await ws.ReadAsync(ct);
                while (!receiveResult.CloseStatus.HasValue)
                {
                    _logger.LogInformation("[Message] received message: \"{message}\"", message);
                    HandleMessage(message);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("[Connection] closing connection (remote server initiated close message)");
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, receiveResult.CloseStatusDescription, ct);
                        break;
                    }
                    (receiveResult, message) = await ws.ReadAsync(ct); // what happens if this crashes?.
                }

                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing connection (remote server initiated close message)", ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogTrace(new EventId(1), ex, ex.Message);
            }
            finally
            {
                ws?.Dispose();
                _logger.LogInformation("[Connection] disposed connection");
            }
        }

        private void HandleMessage(string message)
        {
            if (message.Contains("checkin") || message.Contains("pong"))
            {
                var checkinEvent = new CheckInEvent();
                _eventDispatcher.FireEvent(checkinEvent);
            }
        }
    }
}