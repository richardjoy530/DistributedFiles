namespace FileDistributor.Events
{
    public interface IEventDispatcher
    {
        void FireEvent(EventBase e);
    }
}
