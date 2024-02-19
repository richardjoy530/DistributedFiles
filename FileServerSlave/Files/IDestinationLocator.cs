namespace FileServerSlave.Files
{
    public interface IDestinationLocator
    {
        string GetDestinationFolderPath();

        bool SetCustomLocation(string customPath);
    }
}
