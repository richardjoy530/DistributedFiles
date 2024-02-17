namespace Backend.Contracts
{
    public class AvailableFiles
    {
        public required string HostString { get; set; }

        public required string[] AvailableFileNames { get; set; }
    }
}
