using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Common.Events

{
    public class EventHandlerResolver : IEventHandlerResolver
    {
        private readonly ILogger<EventHandlerResolver> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IEventHandler> _eventHandlerMap;

        public EventHandlerResolver(ILogger<EventHandlerResolver> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _eventHandlerMap = new ConcurrentDictionary<string, IEventHandler>();
        }

        public IEventHandler ResolveHandlerFor(EventBase e)
        {
            if (_eventHandlerMap.TryGetValue(e.GetType().Name, out var handler))
            {
                return handler;
            }

            handler = _serviceProvider.GetKeyedService<IEventHandler>(e.GetType().Name);
            _eventHandlerMap.TryAdd(e.GetType().Name, handler!);

            return handler!;
        }
    }
}
