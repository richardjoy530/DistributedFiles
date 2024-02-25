using Common.Events;
using FileServerMaster.Events;
using FileServerMaster.Storage;

namespace FileServerMaster.EventHandlers
{
    public class SocketClosedEventHandler : IEventHandler
    {
        private readonly ILogger<SocketClosedEventHandler> _logger;
        private readonly IWebSocketContainer _webSocketContainer;
        private readonly IFileDistributorManager _fileDistributorManager;

        public SocketClosedEventHandler(ILogger<SocketClosedEventHandler> logger, IWebSocketContainer webSocketContainer, IFileDistributorManager fileDistributorManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        }

        public void HandleEvent(EventBase e)
        {
            if (e is not SocketClosedEvent sce)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
                return;
            }

            _webSocketContainer.DisposeAndRemove(sce.SlaveHostAddress);
            _fileDistributorManager.RemoveHosting(sce.SlaveHostAddress);

            var hoststrings = string.Join(';', sce.SlaveHostAddress);
            _logger.LogInformation("[SocketClosedEvent] disposed and removed \"{}\"", hoststrings);
        }
    }
}
