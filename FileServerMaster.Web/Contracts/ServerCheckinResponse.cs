using Microsoft.AspNetCore.Http;

namespace FileServerMaster.Web.Contracts
{
    public class ServerCheckinResponse
    {
        public required IDictionary<string, string> FileLinks { get; set; }
    }
}
