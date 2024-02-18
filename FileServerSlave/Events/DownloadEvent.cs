namespace FileServerSlave.Events
{
    public class DownloadEvent : EventBase
    {
        public readonly KeyValuePair<string, HostString> FileLink;

        public DownloadEvent(KeyValuePair<string, HostString> filelink)
        {
            FileLink = filelink;
        }
    }
}
