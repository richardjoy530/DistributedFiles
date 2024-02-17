namespace Backend.Contracts
{
    public class AvailableFiles
    {
        public HostString HostString { get; set; }

        public required string[] AvailableFileNames { get; set; }
    }
}
