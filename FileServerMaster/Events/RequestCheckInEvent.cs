using Common.Events;

namespace FileServerMaster.Events
{
    public class RequestCheckInEvent : EventBase
    {
        public readonly bool RequestCheckInAll;

        public readonly bool IsExclude;

        public HostString[]? Slaves { get; }

        public RequestCheckInEvent()
        {
            RequestCheckInAll = true;
        }

        public RequestCheckInEvent(HostString[] slaves, bool isExclude = false)
        {
            Slaves = slaves;
            IsExclude = isExclude;
        }
    }
}