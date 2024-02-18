using FileServerSlave.Events;

namespace FileServerSlave.EventHandlers
{
    public interface IEventHandler
    {
        void HandleEvent(EventBase e);
    }
}
