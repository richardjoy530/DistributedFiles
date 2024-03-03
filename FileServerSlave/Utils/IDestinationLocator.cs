namespace FileServerSlave.Utils
{
    public interface IDestinationLocator
    {
        string GetDestinationFolderPath();

        void SetCustomLocation(string customPath);
    }
}