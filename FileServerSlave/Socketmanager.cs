using Common;
using Common.Events;
using FileServerSlave.Events;
using FileServerSlave.Utils;
using System.Net.WebSockets;

namespace FileServerSlave;

public class SocketManager : ISocketManager
{
    private readonly ILogger<SocketManager> _logger;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IHostStringRetriver _hostStringRetriver;
    private readonly IMasterServerRetriver _masterServerRetriver;
    private readonly HostString _hostString;
    private readonly bool _secure;
    private readonly int _retryInSeconds;

    public SocketManager(ILogger<SocketManager> logger,
                         IEventDispatcher eventQueueManager,
                         IHostStringRetriver slaveHostStringRetriver,
                         IMasterServerRetriver masterServerRetriver)
    {

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventQueueManager ?? throw new ArgumentNullException(nameof(eventQueueManager));
        _hostStringRetriver = slaveHostStringRetriver ?? throw new ArgumentNullException(nameof(slaveHostStringRetriver));
        _masterServerRetriver = masterServerRetriver ?? throw new ArgumentNullException(nameof(masterServerRetriver));

        _retryInSeconds = _masterServerRetriver.RetryInSeconds;
        _secure = _masterServerRetriver.Secure;
        _hostString = _masterServerRetriver.GetMasterHostString();

        _logger.LogDebug("configured master host address is \"{host}\"", _hostString);
    }

    public async void EstablishConnection(CancellationToken ct)
    {
        _logger.LogInformation("[Connection] starting socket listener");
        while (!ct.IsCancellationRequested)
        {
            await Listen(ct);
            _logger.LogInformation("[Connection] trying to reconnect after \"{}\" seconds", _retryInSeconds);
            await Task.Delay(1000 * _retryInSeconds);
        }
    }

    private async Task Listen(CancellationToken ct)
    {
        ClientWebSocket? ws = null;
        try
        {
            var hoststrings = string.Join(';', _hostStringRetriver.GetLocalFileServerHosts());
            ws = new ClientWebSocket();

            var wsscheme = _secure ? "wss" : "ws";
            var wsuri = new Uri($"{wsscheme}://{_hostString}/ws");

            _logger.LogInformation("[Connection] trying to connect \"{wsuri}\"", wsuri);
            await ws.ConnectAsync(wsuri, ct);
            _logger.LogInformation("[Connection] connection established to \"{wsuri}\"", wsuri);

            await ws.WriteAsync("ping", ct);
            await ws.WriteAsync($"slave server hosted at: \"{hoststrings}\"", ct);

            var (ReciveResult, Message) = await ws.ReadAsync(ct);
            while (!ReciveResult.CloseStatus.HasValue)
            {
                _logger.LogInformation("[Message] received message: \"{Message}\"", Message);
                HandleMessage(Message);

                if (ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("[Connection] closing connection (remote server initiated close message)");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, ReciveResult.CloseStatusDescription, ct);
                    break;
                }
                (ReciveResult, Message) = await ws.ReadAsync(ct); // what happens if this crashes?.
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