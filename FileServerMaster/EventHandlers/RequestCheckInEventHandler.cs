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

        public void HandleEvent(EventBase e)
        {
            if (e is not RequestCheckInEvent rcie)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
                return;
            }

            if (rcie.RequestCheckInAll)
            {
                _logger.LogInformation("[RequestCheckInEvent] requesting checkin from all slaves");

                _webSocketContainer.Process(SendCheckInMessage);
            }
            else if (rcie.IsExclude == false)
            {
                _logger.LogInformation("[RequestCheckInEvent] requesting checkin from slaves except \"{slaves}\"", string.Join(" - ", rcie.Slaves!));

                _webSocketContainer.Process(hws =>
                {
                    if (rcie.Slaves!.Contains(hws.Host))
                    {
                        SendCheckInMessage(hws);
                    }
                });
            }
            else
            {
                _logger.LogInformation("[RequestCheckInEvent] requesting checkin from slaves \"{slaves}\"", string.Join(" - ", rcie.Slaves!));

                _webSocketContainer.Process(hws =>
                {
                    if (!rcie.Slaves!.Contains(hws.Host))
                    {
                        SendCheckInMessage(hws);
                    }
                });
            }
        }

        private void SendCheckInMessage((HostString Host, WebSocket Socket) hws)
        {
            _logger.LogInformation("[Message] send \"checkin\" to \"{host}\"", hws.Host);

            try
            {
                _ = hws.Socket.WriteAsync("checkin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogTrace(new EventId(0), ex, ex.Message);
            }
        }
    }
}