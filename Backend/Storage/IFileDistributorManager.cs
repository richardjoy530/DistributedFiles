
namespace Backend.Storage
{
    public interface IFileDistributorManager
    {
        string[] GetAllFileNames();

        string GetRetrivalLink(string fileName);

        void UpdateFileAvailablity(string remoteHost, string[] availableFileNames);
    }
}
