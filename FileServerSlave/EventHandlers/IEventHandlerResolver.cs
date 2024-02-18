using FileServerSlave.Events;

namespace FileServerSlave.EventHandlers
{
    public interface IEventHandlerResolver
    {
        IEventHandler ResolveHandlerFor(EventBase e);
    }
}
