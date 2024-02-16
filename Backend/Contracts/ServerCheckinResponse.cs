namespace Backend.Contracts
{
    public class ServerCheckinResponse
    {
        public required IDictionary<string, string[]> FileLinks { get; set; }
    }
}
