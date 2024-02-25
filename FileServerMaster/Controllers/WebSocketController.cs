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
            var socketHost = new HostString(ip, HttpContext.Connection.RemotePort); // this is not the slave file-server's host address
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("[Connection] accepted connection from \"{host}\"", socketHost);

            var (_, Message) = await webSocket.ReadAsync();
            _logger.LogInformation("[Message] received message: \"{Message}\"", Message);
            await webSocket.WriteAsync("pong");

            var slaveHosts = new List<HostString>();

            try
            {
                await _webSocketContainer.Listen(webSocket, slaveHosts);
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
                _eventDispatcher.FireEvent(new SocketClosedEvent(slaveHosts.ToArray()));
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

#if DEBUG
    [HttpGet("checkin")]
    public void CheckinAsync()
    {
        _eventDispatcher.FireEvent(new RequestCheckInEvent());
    }

    [HttpDelete]
    public void CloseAllAsync()
    {
        _logger.LogInformation("Closing all WebSockets");
        _webSocketContainer.CloseWebSocketAsync(); // todo use event dispatcher
    }
#endif
}