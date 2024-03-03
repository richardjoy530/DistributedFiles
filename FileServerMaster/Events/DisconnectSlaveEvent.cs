using Common.Events;

namespace FileServerMaster.Events
{
    public class DisconnectSlaveEvent : EventBase
    {
        public readonly bool DisconnectAll;

        public readonly bool IsExclude;

        public HostString[]? Slaves { get; }

        public DisconnectSlaveEvent()
        {
            DisconnectAll = true;
        }   

        public DisconnectSlaveEvent(HostString[] slaves, bool isExclude = false)
        {
            Slaves = slaves;
            IsExclude = isExclude;
        }
    }
}