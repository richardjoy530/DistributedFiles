using System.Net.WebSockets;

namespace FileDistributor;

public class Checkin
{
    public async void Ping()
    {
        var ws = new ClientWebSocket();
        var ct = new CancellationToken(false);
        await ws.ConnectAsync(new Uri("wss://localhost:7180/ws"), ct);
        int i = 0;
        while (!ws.CloseStatus.HasValue)
        {
            if (i++ == 5)
            {
                break;
            }
            await ws.SendAsync(new ArraySegment<byte>("this is a test from client"u8.ToArray()), WebSocketMessageType.Text, WebSocketMessageFlags.None, ct);
        }

        await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
        ws.Dispose();
    }
}