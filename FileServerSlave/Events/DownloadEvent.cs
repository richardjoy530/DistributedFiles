namespace FileServerSlave.Events
{
    public class DownloadEvent : EventBase
    {
        public readonly HostString HostString;
        public readonly string FileName;

        public DownloadEvent(KeyValuePair<string, HostString> filelink)
        {
            FileName = filelink.Key;
            HostString = filelink.Value;
        }
    }
}
