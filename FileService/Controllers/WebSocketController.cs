using System.Net.WebSockets;
using System.Text;
using Backend.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
public class WebSocketController(ILogger<WebSocketController> logger) : ControllerBase
{
    [Route("/ws")]
    [HttpGet] // This is not required. kept to make swagger happy
    public async void Connect()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            logger.LogInformation($"Connected WS {webSocket.GetHashCode()}");
            SaveConnection(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async void SaveConnection(WebSocket webSocket)
    {
        Container.ConnectedSockets.Add(webSocket);
        
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            logger.LogInformation($"sending {Encoding.UTF8.GetString(buffer)}");
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
        
        webSocket.Dispose();
    }
}