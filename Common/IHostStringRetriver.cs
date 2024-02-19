using Microsoft.AspNetCore.Http;

namespace Common
{
    public interface IHostStringRetriver
    {
        HostString[] GetLocalFileServerHosts();
    }
}