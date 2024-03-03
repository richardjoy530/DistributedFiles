using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common
{
    public class HostStringRetriever : IHostStringRetriever
    {
        private readonly IServer _server;
        private readonly ILogger<HostStringRetriever> _logger;

        public HostStringRetriever(IServer server, ILogger<HostStringRetriever> logger)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<HostString> GetLocalFileServerHosts()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].MapToIPv4().ToString();
            var addresses = _server.Features.Get<IServerAddressesFeature>()!.Addresses;
            if (addresses.Count == 0)
            {
                _logger.LogWarning("attempted to retrieve file-server address before it was hosted");
                throw new InvalidOperationException();
            }

            var ports = addresses.Select(a => new Uri(a).Port).ToArray();

            var hostStrings = new HostString[ports.Length];

            for (var i = 0; i < ports.Length; i++)
            {
                hostStrings[i] = new HostString(host, ports[i]);
            }

            _logger.LogDebug("local file server addresses are \"{addresses}\"", string.Join(';', hostStrings));
            return hostStrings;
        }
    }
}