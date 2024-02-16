using Common;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace FileDistributor;

public static class Connector
{
    public static async void EstablishConnection(ILogger<Program> logger)
    {
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri("wss://localhost:7180/ws"), CancellationToken.None);

        await ws.WriteAsync("ping");

        while (!ws.CloseStatus.HasValue && ws.State == WebSocketState.Open)
        {
            var msg = await ws.ReadAsync();
            logger.LogInformation($"Recived message: {msg}");
        }

        logger.LogInformation("Closing connection ... ");

        await ws.CloseAsync(ws.CloseStatus?? WebSocketCloseStatus.NormalClosure, ws.CloseStatusDescription, CancellationToken.None);
        ws.Dispose();
    }
}