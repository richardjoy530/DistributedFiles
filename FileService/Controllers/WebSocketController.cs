using Backend.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace Backend.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly IWebSocketContainer _webSocketContainer;


    public WebSocketController(ILogger<WebSocketController> logger, IWebSocketContainer webSocketContainer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
    }

    [HttpGet] // This is not required. kept to make swagger happy
    public async Task ConnectAsync()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation($"Connected WS {webSocket.GetHashCode()}");
            _webSocketContainer.AddWebSocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

#if DEBUG
    [HttpGet("checkin")]
    public async Task CheckinAsync()
    {
        _logger.LogInformation("Requesting check-in");
        await _webSocketContainer.RequestCheckinAsync();
    }
#endif

    [HttpDelete]
    public async Task CloseAllAsync()
    {
        _logger.LogInformation("Closing all WebSockets");
        await _webSocketContainer.CloseWebSocketAsync();
    }
}