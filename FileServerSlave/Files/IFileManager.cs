using Common.Proxy.Controllers;

namespace FileServerSlave.Files
{
    public interface IFileManager
    {
        string[] GetAvailableFilesOnThisServer();

        (FileStream? FileStream, string ContentType) GetFile(string filename);

        Task SaveFile(Stream stream, string fileName);
    }
}
