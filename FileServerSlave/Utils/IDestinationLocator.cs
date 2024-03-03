namespace FileServerSlave.Utils
{
    public interface IDestinationLocator
    {
        string GetDestinationFolderPath();

        bool SetCustomLocation(string customPath);
    }
}
