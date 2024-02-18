using FileDistributor.Events;

namespace FileDistributor.EventHandlers
{
    public class CheckInEventHandler : IEventHandler
    {
        private readonly ILogger<CheckInEventHandler> _logger;

        public CheckInEventHandler(ILogger<CheckInEventHandler> logger)
        {
            _logger = logger;
        }

        public void HandleEvent(EventBase e)
        {
            _logger.LogInformation("Handled checkin event");
        }
    }
}
