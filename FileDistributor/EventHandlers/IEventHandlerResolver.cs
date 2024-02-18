using FileDistributor.Events;

namespace FileDistributor.EventHandlers
{
    public interface IEventHandlerResolver
    {
        IEventHandler ResolveHandlerFor(EventBase e);
    }
}
