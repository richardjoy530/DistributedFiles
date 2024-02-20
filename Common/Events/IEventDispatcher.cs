namespace Common.Events
{
    public interface IEventDispatcher
    {
        Task FireEvent(EventBase e);
    }
}
