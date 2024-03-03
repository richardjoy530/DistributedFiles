using System.Net.WebSockets;
using Common.Events;
using FileServerMaster.Events;
using FileServerMaster.Storage;

namespace FileServerMaster.EventHandlers
{
    public class DisconnectSlaveEventHandler : IEventHandler
    {
        private readonly ILogger<RequestCheckInEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IWebSocketContainer _webSocketContainer;

        public DisconnectSlaveEventHandler(ILogger<RequestCheckInEventHandler> logger, IEventDispatcher eventDispatcher, IWebSocketContainer webSocketContainer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        }

        public void HandleEvent(EventBase e)
        {
            if (e is not DisconnectSlaveEvent dse)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
                return;
            }

            if (dse.DisconnectAll)
            {
                _logger.LogInformation("[DisconnectSlaveEvent] closing all slave connections");

                _webSocketContainer.Process(CloseSocket);
            }
            else if (dse.IsExclude == false)
            {
                _logger.LogInformation("[DisconnectSlaveEvent] closing slave connections except \"{slaves}\"", string.Join(" - ", dse.Slaves!));

                _webSocketContainer.Process(hws =>
                {
                    if (dse.Slaves!.Contains(hws.Host))
                    {
                        CloseSocket(hws);
                    }
                });
            }
            else
            {
                _logger.LogInformation("[DisconnectSlaveEvent] closing slave connections \"{slaves}\"", string.Join(" - ", dse.Slaves!));

                _webSocketContainer.Process(hws =>
                {
                    if (!dse.Slaves!.Contains(hws.Host))
                    {
                        CloseSocket(hws);
                    }
                });
            }

            _eventDispatcher.FireEvent(new SocketClosedEvent(dse.Slaves!));
        }

        private void CloseSocket((HostString Host, WebSocket Socket) hws)
        {
            try
            {
                _ = hws.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closure requested by master", CancellationToken.None);
                _logger.LogInformation("[DisconnectSlaveEvent] closed \"{host}\"", hws.Host);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogTrace(new EventId(0), ex, ex.Message);
            }
        }
    }
}