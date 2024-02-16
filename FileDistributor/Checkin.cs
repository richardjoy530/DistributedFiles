using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace FileDistributor;

public class Checkin
{
    public async void Ping()
    {
        var ws = new ClientWebSocket();
        var ct = new CancellationToken(false);
        await ws.ConnectAsync(new Uri("wss://localhost:7180/ws"), ct);


        var buffer = new byte[1024 * 4];
        var receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var msg = Encoding.UTF8.GetString(buffer);
            await Console.Out.WriteLineAsync($"Recived message: {msg}");
            if (msg == "checkin")
            {
                await Console.Out.WriteLineAsync("Check-In");
            }
        }

        await Console.Out.WriteLineAsync("Closing connection ... ");

        await ws.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);

        ws.Dispose();
    }
}