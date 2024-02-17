

namespace Backend.Storage
{
    public interface IFileDistributorManager
    {
        string[] GetAllFileNames();

        HostString GetRetrivalHost(string fileName);

        void RemoveHost(HostString host);

        void UpdateFileAvailablity(HostString remoteHost, string[] availableFileNames);
    }
}
