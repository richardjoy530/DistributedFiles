namespace Common.Events
{
    public interface IEventHandlerResolver
    {
        IEventHandler ResolveHandlerFor(EventBase e);
    }
}
