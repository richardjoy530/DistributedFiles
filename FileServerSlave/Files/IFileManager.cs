namespace FileServerSlave.Files
{
    public interface IFileManager
    {
        string[] GetAvailableFilesOnThisServer();

        byte[] GetFile(string filename);

        Task SaveFile(IFormFile formFile);
    }
}
