
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public interface IFileContainer
    {
        Task<bool> AddToStream(IFormFile formFile);
        
        Task DiscardFilesAsync(string[] filesToRemoveFromContainer);

        Stream? GetStream(string filename);

        IEnumerable<string> GetTempFileNames();
    }
}
