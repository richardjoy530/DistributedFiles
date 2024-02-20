using Common;
using Common.Events;
using FileServerSlave.Events;
using System.Net.WebSockets;

namespace FileServerSlave;

public class SocketManager : ISocketManager
{
    private readonly ILogger<SocketManager> _logger;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IHostStringRetriver _hostStringRetriver;
    private readonly HostString _hostString;
    private readonly bool _secure;

    public SocketManager(ILogger<SocketManager> logger, IEventDispatcher eventQueueManager, IConfiguration configuration, IHostStringRetriver slaveHostStringRetriver)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventDispatcher = eventQueueManager ?? throw new ArgumentNullException(nameof(eventQueueManager));
        _hostStringRetriver = slaveHostStringRetriver ?? throw new ArgumentNullException(nameof(slaveHostStringRetriver));

        _ = bool.TryParse(configuration["UseHttps"], out _secure);
        if (_secure)
        {
            _hostString = new HostString(configuration["FileServerMasterHttps"]!);
        }
        else
        {
            _hostString = new HostString(configuration["FileServerMasterHttp"]!);
        }

        _logger.LogDebug("configured master host address is \"{host}\"", _hostString);
    }

    public async void EstablishConnection(CancellationToken token)
    {
        await Task.Delay(1000 * 5);
        do
        {
            await Listen(token);
            _logger.LogInformation("trying to reconnect after 30 seconds");
            Thread.Sleep(1000 * 5); // wait for 5 seconds
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
            var hoststrings = string.Join(';', _hostStringRetriver.GetLocalFileServerHosts()); // this is an edge case.. need to handle it later.
            await ws.WriteAsync($"slave server hosted at: \"{hoststrings}\"", ct);

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
            _logger.LogError(ex.Message);
            _logger.LogTrace(new EventId(1), ex, ex.Message);
        }
        finally
        {
            _logger.LogInformation("disposing connection");
            ws?.Dispose();
            _logger.LogInformation("disposed connection");
        }
    }

    private async void HandleMessage(string message)
    {
        if (message.Contains("checkin") || message.Contains("pong"))
        {
            var checkinEvent = new CheckInEvent();
            await _eventDispatcher.FireEvent(checkinEvent);
        }
    }
}