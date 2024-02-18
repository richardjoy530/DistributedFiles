namespace FileServerSlave
{
    public interface ISlaveHostStringRetriver
    {
        HostString[] GetLocalFileServerHosts();
    }
}