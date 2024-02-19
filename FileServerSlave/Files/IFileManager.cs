using Common.Proxy.Controllers;

namespace FileServerSlave.Files
{
    public interface IFileManager
    {
        string[] GetAvailableFilesOnThisServer();

        FileData? GetFile(string filename);

        Task SaveFile(FileData file);
    }
}
