namespace FileServerMaster.Web.Contracts
{
    public class AvailableFiles
    {
        public required string[] SlaveHostStrings { get; init; }

        public required string[] AvailableFileNames { get; init; }
    }
}