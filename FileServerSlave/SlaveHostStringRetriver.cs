
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;

namespace FileServerSlave
{
    public class SlaveHostStringRetriver : ISlaveHostStringRetriver
    {
        private readonly IServer _server;
        private readonly ILogger<SlaveHostStringRetriver> _logger;

        public SlaveHostStringRetriver(IServer server, ILogger<SlaveHostStringRetriver> logger)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            _logger.LogDebug("local file server addresses are \"{addresses}\"", string.Join(';', hoststrings));
            return hoststrings;
        }
    }
}
