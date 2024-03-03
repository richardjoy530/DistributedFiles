
namespace FileServerSlave.Utils
{
    public interface IMasterServerRetriver
    {
        int RetryInSeconds { get; }

        bool Secure { get; }

        HostString GetMasterHostString();

        bool SetCustomMasterHostString(HostString customHostString);
    }
}