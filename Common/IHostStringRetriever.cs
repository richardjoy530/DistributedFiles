using Microsoft.AspNetCore.Http;

namespace Common
{
    public interface IHostStringRetriever
    {
        IEnumerable<HostString> GetLocalFileServerHosts();
    }
}