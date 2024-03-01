
using Common.Proxy.Controllers;

namespace FileServerMaster.Storage
{
    public interface IFileContainer
    {
        Task<bool> AddFileAsync(Stream stream, string fileName, string contentType);

        void DiscardFiles(string[] filesToRemoveFromContainer);

        (FileStream? FileStream, string ContentType) GetFile(string fileName);

        IEnumerable<string> GetTempFileNames();
    }
}
