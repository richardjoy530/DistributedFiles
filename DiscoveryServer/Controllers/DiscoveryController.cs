using Microsoft.AspNetCore.Mvc;

namespace DiscoveryServer.Controllers
{
    [ApiController]
    [Route("discover")]
    public class DiscoveryController : ControllerBase
    {
        private readonly ILogger<DiscoveryController> _logger;

        public DiscoveryController(ILogger<DiscoveryController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public HostString Get()
        {
            var ip = HttpContext.Connection.RemoteIpAddress!.MapToIPv4().ToString();
            var remotehost = new HostString(ip, HttpContext.Connection.RemotePort);
            _logger.LogInformation("remote host address is: \"{host}\"", remotehost);
            return remotehost;
        }
    }
}
