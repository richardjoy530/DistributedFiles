
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common
{
    public class HostStringRetriver : IHostStringRetriver
    {
        private readonly IServer _server;
        private readonly ILogger<HostStringRetriver> _logger;

        public HostStringRetriver(IServer server, ILogger<HostStringRetriver> logger)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public HostString[] GetLocalFileServerHosts()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].MapToIPv4().ToString();
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
