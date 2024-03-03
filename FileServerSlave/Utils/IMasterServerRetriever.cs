
namespace FileServerSlave.Utils
{
    public interface IMasterServerRetriever
    {
        int RetryInSeconds { get; }

        bool Secure { get; }

        HostString GetMasterHostString();

        void SetCustomMasterHostString(HostString customHostString);
    }
}