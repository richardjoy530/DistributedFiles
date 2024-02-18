using FileServerMaster.Storage;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace FileServerMaster.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly IWebSocketContainer _webSocketContainer;
    private readonly IFileDistributorManager _fileDistributorManager;

    public WebSocketController(ILogger<WebSocketController> logger, IWebSocketContainer webSocketContainer, IFileDistributorManager fileDistributorManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
    }

    [HttpGet] // This is not required. kept to make swagger happy
    public async Task ConnectAsync()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var (_, Message) = await webSocket.ReadAsync();
            _logger.LogInformation("recived message: \"{Message}\"", Message);
            await webSocket.WriteAsync("pong");

            try
            {
                await _webSocketContainer.Listen(webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                _logger.LogTrace(new EventId(0), ex, ex.Message);
            }
            finally
            {
                // need to come up with an idea to remove this server's file host from the availablity table.
                _logger.LogInformation("disposing connection");
                webSocket?.Dispose();
                _logger.LogInformation("disposed connection");
            }
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