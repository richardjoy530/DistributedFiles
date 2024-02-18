using Microsoft.AspNetCore.Http;

namespace Backend.Web.Contracts
{
    public class ServerCheckinResponse
    {
        public required IDictionary<string, HostString> FileLinks { get; set; }
    }
}
