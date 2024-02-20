namespace Common.Events
{
    public interface IEventHandler
    {
        Task HandleEvent(EventBase e);
    }
}
