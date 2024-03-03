using Common.Events;

namespace FileServerSlave.Events
{
    public class DownloadEvent : EventBase
    {
        public readonly HostString HostString;
        public readonly string FileName;

        public DownloadEvent(KeyValuePair<string, string> fileLink)
        {
            FileName = fileLink.Key;
            HostString = new HostString(fileLink.Value);
        }
    }
}