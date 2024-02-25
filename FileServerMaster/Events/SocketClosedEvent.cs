using Common.Events;

namespace FileServerMaster.Events
{
    public class SocketClosedEvent : EventBase
    {
        public SocketClosedEvent(HostString[] slaveHostAddress)
        {
            SlaveHostAddress = slaveHostAddress;
        }

        public HostString[] SlaveHostAddress { get; }
    }
}
