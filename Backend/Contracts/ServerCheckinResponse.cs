namespace Backend.Contracts
{
    public class ServerCheckinResponse
    {
        public required IDictionary<string, HostString> FileLinks { get; set; }
    }
}
