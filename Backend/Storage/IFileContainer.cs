namespace Backend.Storage
{
    public interface IFileContainer
    {
        List<IFormFile> Files { get; }

        void Add(IFormFile formFile);

        IFormFile? Get(string filename);
    }
}
