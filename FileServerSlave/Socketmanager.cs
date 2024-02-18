using Common;
using FileServerSlave.Events;
using System.Net.WebSockets;

namespace FileServerSlave;

public class SocketManager : ISocketManager
{
    private readonly ILogger<SocketManager> _logger;
    private readonly IEventDispatcher _eventQueueManager;

    public SocketManager(ILogger<SocketManager> logger, IEventDispatcher eventQueueManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventQueueManager = eventQueueManager ?? throw new ArgumentNullException(nameof(eventQueueManager));
    }

    public async void EstablishConnection(CancellationToken token)
    {
        do
        {
            await Listen(token, new HostString(), false);
            Thread.Sleep(1000 * 60); // wait for one min
        } while (true);
    }

    private async Task Listen(CancellationToken token, HostString backendHost, bool secure)
    {
        ClientWebSocket? ws = null;
        try
        {
            ws = new ClientWebSocket();
            var wsscheme = secure ? "wss" : "ws";
            await ws.ConnectAsync(new Uri($"{wsscheme}://{backendHost}/ws"), CancellationToken.None);
            await ws.WriteAsync("ping");

            while (!ws.CloseStatus.HasValue && ws.State == WebSocketState.Open)
            {
                var rslt = await ws.ReadAsync(token);
                _logger.LogInformation($"Recived message: {rslt.Message}");
                HandleMessage(rslt.Message);

                if (rslt.ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("Closing connection ... ");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, rslt.ReciveResult.CloseStatusDescription, CancellationToken.None);
                    break;
                }
            }

            _logger.LogInformation("Closing connection ... ");
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket was closed", CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            _logger.LogTrace(new EventId(1), ex, ex.Message);
        }
        finally
        {
            ws?.Dispose();
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