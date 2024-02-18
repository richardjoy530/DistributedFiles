namespace FileServerMaster.Web.Contracts
{
    public class AvailableFiles
    {
        public required string[] SlaveHostStrings { get; set; }

        public required string[] AvailableFileNames { get; set; }
    }
}
