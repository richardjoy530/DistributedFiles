

namespace FileServerMaster.Storage
{
    public interface IFileDistributorManager
    {
        string[] GetAllFileNames();

        HostString GetRetrievalHost(string fileName);
    
        void RemoveHosting(HostString[] slaveHostAddress);
    
        void RemoveMaster(string fileName);

        void UpdateFileAvailability(HostString remoteHost, string[] availableFileNames);
    }
}