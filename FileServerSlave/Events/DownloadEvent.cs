namespace FileServerSlave.Events
{
    public class DownloadEvent : EventBase
    {
        public readonly HostString HostString;
        public readonly string FileName;

        public DownloadEvent(KeyValuePair<string, string> filelink)
        {
            FileName = filelink.Key;
            HostString = new HostString(filelink.Value);
        }
    }
}
