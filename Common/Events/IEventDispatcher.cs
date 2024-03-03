namespace Common.Events
{
    public interface IEventDispatcher
    {
        void FireEvent(EventBase e);
    }
}