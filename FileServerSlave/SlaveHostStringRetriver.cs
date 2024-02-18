
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;

namespace FileServerSlave
{
    public class SlaveHostStringRetriver : ISlaveHostStringRetriver
    {
        private readonly IServer _server;

        public SlaveHostStringRetriver(IServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public HostString[] GetLocalFileServerHosts()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName()).HostName;
            var ports = _server.Features.Get<IServerAddressesFeature>()!.Addresses.Select(a => new Uri(a).Port).ToArray();

            var hoststrings = new HostString[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                hoststrings[i] = new HostString(host, ports[i]);
            }

            return hoststrings;
        }
    }
}
