using Microsoft.Extensions.Logging;

namespace Common.Events
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly IEventHandlerResolver _eventHandlerResolver;

        public EventDispatcher(ILogger<EventDispatcher> logger, IEventHandlerResolver eventHandlerResolver)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventHandlerResolver = eventHandlerResolver ?? throw new ArgumentNullException(nameof(eventHandlerResolver));
        }

        public void FireEvent(EventBase e)
        {
            _logger.LogInformation("[Event] \"{event}\" dispatched", e.GetType().Name);
            _eventHandlerResolver
                .ResolveHandlerFor(e)
                .HandleEvent(e);
        }
    }
}