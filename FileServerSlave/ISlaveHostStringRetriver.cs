using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;

namespace FileServerSlave
{
    public interface ISlaveHostStringRetriver
    {
        HostString[] GetLocalFileServerHosts();
    }
}
`