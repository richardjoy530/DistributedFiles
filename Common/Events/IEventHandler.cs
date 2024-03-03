namespace Common.Events
{
    public interface IEventHandler
    {
        void HandleEvent(EventBase e);
    }
}