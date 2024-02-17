
namespace Backend.Storage
{
    public interface IFileContainer
    {
        void Add(IFormFile formFile);

        void DiscardFiles(string[] filesToRemoveFromContainer);

        IFormFile? Get(string filename);

        IEnumerable<string> GetTempFileNames();
    }
}
