using FileServerMaster.Storage;
using Common;
using Microsoft.AspNetCore.Mvc;
using Common.Events;
using FileServerMaster.Events;

namespace FileServerMaster.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
{
    private readonly ILogger<WebSocketController> _logger;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IWebSocketContainer _webSocketContainer;
    private readonly IFileDistributorManager _fileDistributorManager;

    public WebSocketController(ILogger<WebSocketController> logger,
                               IWebSocketContainer webSocketContainer,
                               IFileDistributorManager fileDistributorManager,
                               IEventDispatcher eventDispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
    }

    [HttpGet] // This is not required. kept to make swagger happy
    public async Task ConnectAsync()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var ip = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
            var socketHost = new HostString(ip, HttpContext.Connection.RemotePort);
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("[Connection] accepted connection from \"{host}\"", socketHost); // this is not the slave file-server's host address

            var (_, Message) = await webSocket.ReadAsync();
            _logger.LogInformation("[Message] received message: \"{Message}\"", Message);
            await webSocket.WriteAsync("pong");

            try
            {
                await _webSocketContainer.Listen(webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogTrace(new EventId(0), ex, ex.Message);
            }
            finally
            {
                // need to come up with an idea to remove this server's file host from the availablity table.
                webSocket?.Dispose();
                _logger.LogInformation("[Connection] disposed connection");
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
        await _eventDispatcher.FireEvent(new RequestCheckInEvent());
    }
#endif
}