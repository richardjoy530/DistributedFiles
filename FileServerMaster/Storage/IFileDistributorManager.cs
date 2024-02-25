

namespace FileServerMaster.Storage
{
    public interface IFileDistributorManager
    {
        string[] GetAllFileNames();

        HostString GetRetrivalHost(string fileName);
        void RemoveHosting(HostString[] slaveHostAddress);
        void RemoveMaster(string fileName);

        void UpdateFileAvailablity(HostString remoteHost, string[] availableFileNames);
    }
}
