namespace FileServerSlave.Events
{
    public interface IEventDispatcher
    {
        void FireEvent(EventBase e);
    }
}
