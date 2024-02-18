using Common;
using System.Net.WebSockets;

namespace FileDistributor;

public class SocketManager : ISocketmanager
{
    public async void EstablishConnection(ILogger<Program> logger, CancellationToken token)
    {
        do
        {
            var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri("ws://192.168.18.87:7180/ws"), CancellationToken.None); // for testing
            await ws.WriteAsync("ping");

            await Listen(logger, ws, token);
            Thread.Sleep(1000 * 60); // wait for one min
        } while (true);
    }

    private async Task Listen(ILogger<Program> logger, ClientWebSocket ws, CancellationToken token)
    {
        try
        {

            while (!ws.CloseStatus.HasValue && ws.State == WebSocketState.Open)
            {
                var rslt = await ws.ReadAsync(token);
                logger.LogInformation($"Recived message: {rslt.Message}");
                if (rslt.ReciveResult.MessageType == WebSocketMessageType.Close)
                {
                    logger.LogInformation("Closing connection ... ");
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, rslt.ReciveResult.CloseStatusDescription, CancellationToken.None);
                    break;
                }
            }

            logger.LogInformation("Closing connection ... ");
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket was closed", CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            logger.LogTrace(new EventId(1), ex, ex.Message);
        }
        finally
        {
            ws.Dispose();
        }
    }
}