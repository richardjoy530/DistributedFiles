
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public interface IFileContainer
    {
        void Add(IFormFile formFile);

        void DiscardFiles(string[] filesToRemoveFromContainer);

        FileData? Get(string filename);

        IEnumerable<string> GetTempFileNames();
    }
}
