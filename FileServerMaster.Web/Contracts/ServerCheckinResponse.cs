using Microsoft.AspNetCore.Http;

namespace FileServerMaster.Web.Contracts
{
    public class ServerCheckinResponse
    {
        public required IDictionary<string, HostString> FileLinks { get; set; }
    }
}
