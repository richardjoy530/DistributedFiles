
namespace Backend.Storage
{
    public interface IFileDistributorManager
    {
        string[] FilesToSync(string[] availableFileNames);

        string[] GetRetrivalLinks(string f);

        void UpdateFileAvailablity(string remoteHost, string[] availableFileNames);
    }
}
