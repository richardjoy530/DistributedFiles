using Common;
using FileServerSlave.Events;
using System.Net.WebSockets;

namespace FileServerSlave;

public class SocketManager : ISocketManager
{
    private readonly ILogger<SocketManager> _logger;
    private readonly IEventDispatcher _eventQueueManager;
    private readonly ISlaveHostStringRetriver _slaveHostStringRetriver;
    private readonly HostString _hostString;
    private readonly bool _secure;

    public SocketManager(ILogger<SocketManager> logger, IEventDispatcher eventQueueManager, IConfiguration configuration, ISlaveHostStringRetriver slaveHostStringRetriver)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventQueueManager = eventQueueManager ?? throw new ArgumentNullException(nameof(eventQueueManager));
        _slaveHostStringRetriver = slaveHostStringRetriver ?? throw new ArgumentNullException(nameof(slaveHostStringRetriver));

        _ = bool.TryParse(configuration["UseHttps"], out _secure);
        if (_secure)
        {
            _hostString = new HostString(configuration["FileServerMasterHttps"]!);
        }
        else
        {
            _hostString = new HostString(configuration["FileServerMasterHttp"]!);
        }
    }

    public async void EstablishConnection(CancellationToken token)
    {
        do
        {
            await Listen(token);
            _logger.LogInformation("trying to reconnect after 30 seconds");
            Thread.Sleep(1000 * 30); // wait for 30 seconds
        } while (true);
    }

    private async Task Listen(CancellationToken ct)
    {
        ClientWebSocket? ws = null;
        try
        {
            ws = new ClientWebSocket();

            var wsscheme = _secure ? "wss" : "ws";
            var wsuri = new Uri($"{wsscheme}://{_hostString}/ws");

            _logger.LogInformation("trying to connect \"{wsuri}\"", wsuri);
            await ws.ConnectAsync(wsuri, ct);
            _logger.LogInformation("connection established to \"{wsuri}\"", wsuri);

            await ws.WriteAsync("ping", ct);
            var hoststrings = string.Join(';', _slaveHostStringRetriver.GetLocalFileServerHosts()); // this is an edge case.. need to handle it later.
            await ws.WriteAsync($"file server hosted at: {hoststrings}", ct);

            var (ReciveResult, Message) = await ws.ReadAsync(ct);
            while (!ReciveResult.CloseStatus.HasValue)
            {
                _logger.LogInformation("recived message: \"{Message}\"", Message);
                HandleMessage(Message);

                if (ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("closing connection (remote server initiated close message)");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, ReciveResult.CloseStatusDescription, ct);
                    break;
                }
                (ReciveResult, Message) = await ws.ReadAsync(ct); // what happens if this crashes?.
            }

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing connection (remote server initiated close message)", ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex.Message);
            _logger.LogTrace(new EventId(1), ex, ex.Message);
        }
        finally
        {
            _logger.LogInformation("disposing connection");
            ws?.Dispose();
            _logger.LogInformation("disposed connection");
        }
    }

    private void HandleMessage(string message)
    {
        if (message.Contains("checkin"))
        {
            var checkinEvent = new CheckInEvent();
            _eventQueueManager.FireEvent(checkinEvent);
        }
    }
}