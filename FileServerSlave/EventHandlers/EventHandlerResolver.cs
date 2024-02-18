using FileServerSlave.Events;

namespace FileServerSlave.EventHandlers
{
    public class EventHandlerResolver : IEventHandlerResolver
    {
        private readonly ILogger<EventHandlerResolver> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDictionary<string, IEventHandler> _eventHandlerMap;

        public EventHandlerResolver(ILogger<EventHandlerResolver> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _eventHandlerMap = new Dictionary<string, IEventHandler>();
        }

        public IEventHandler ResolveHandlerFor(EventBase e)
        {
            if (_eventHandlerMap.TryGetValue(e.GetType().Name, out var handler))
            {
                return handler;
            }

            handler = _serviceProvider.GetKeyedService<IEventHandler>(e.GetType().Name);
            _eventHandlerMap.Add(e.GetType().Name, handler!);

            return handler!;
        }
    }
}
