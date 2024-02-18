using FileDistributor.Events;

namespace FileDistributor.EventHandlers
{
    public interface IEventHandler
    {
        void HandleEvent(EventBase e);
    }
}
