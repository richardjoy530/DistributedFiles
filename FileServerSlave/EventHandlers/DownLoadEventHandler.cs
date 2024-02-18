using FileServerSlave.Events;

namespace FileServerSlave.EventHandlers
{
    public class DownLoadEventHandler : IEventHandler
    {
        private readonly ILogger<DownLoadEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;

        public DownLoadEventHandler(ILogger<DownLoadEventHandler> logger, IEventDispatcher eventDispatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }


        public void HandleEvent(EventBase e)
        {
            if (e is not DownloadEvent de)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
            }
        }
    }
}
