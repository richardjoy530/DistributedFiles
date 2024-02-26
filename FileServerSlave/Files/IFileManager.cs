using Common.Proxy.Controllers;

namespace FileServerSlave.Files
{
    public interface IFileManager
    {
        string[] GetAvailableFilesOnThisServer();
        
        FileStream? GetStream(string filename);
        
        Task SaveFileAsync(Stream stream, string fileName);
    }
}
