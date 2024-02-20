using System.Net.WebSockets;
using Common;
using Common.Events;
using FileServerMaster.Events;
using FileServerMaster.Storage;

namespace FileServerMaster.EventHandlers
{
    public class RequestCheckInEventHandler : IEventHandler
    {
        private readonly ILogger<RequestCheckInEventHandler> _logger;
        private readonly IWebSocketContainer _webSocketContainer;

        public RequestCheckInEventHandler(ILogger<RequestCheckInEventHandler> logger, IWebSocketContainer webSocketContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        }

        public async Task HandleEvent(EventBase e)
        {
            if (e is not RequestCheckInEvent rcie)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
                return;
            }

            if (rcie.RequestCheckInAll)
            {
                _logger.LogInformation("[RequestCheckInEvent] RequestCheckInAll");

                await _webSocketContainer.Process(SendCheckInMessage);
            }
            else if (rcie.IsExclude == false)
            {
                _logger.LogInformation("[RequestCheckInEvent] IsExclude == false");

                await _webSocketContainer.Process(hws =>
                {
                    if (rcie.Slaves!.Contains(hws.Host))
                    {
                        return SendCheckInMessage(hws);
                    }

                    return new Task(() => { });
                });
            }
            else
            {
                await _webSocketContainer.Process(hws =>
                {

                    if (!rcie.Slaves!.Contains(hws.Host))
                    {
                        return SendCheckInMessage(hws);
                    }

                    _logger.LogInformation($"[RequestCheckInEvent] IsExclude == true");
                    return new Task(() => { });
                });
            }
        }

        private Task SendCheckInMessage((HostString Host, WebSocket Socket) hws)
        {
            _logger.LogInformation("[Message] send \"checkin\" to \"{host}\"", hws.Host);
            return hws.Socket.WriteAsync("checkin");
        }
    }
}
