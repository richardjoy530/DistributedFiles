using Backend.Contracts;
using Backend.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase
    {
        private readonly IFileDistributorManager _fileDistributorManager;

        public CheckInController(IFileDistributorManager fileDistributorManager)
        {
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
        }

        [HttpPost]
        public ServerCheckinResponse CheckIn(FileServerInfo request)
        {
            var remoteIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? throw new ArgumentException("RemoteIpAddress");
            var remoteHost = new HostString(remoteIp, Request.HttpContext.Connection.RemotePort);
            
            //todo: remove from file contaner.

            _fileDistributorManager.UpdateFileAvailablity(remoteHost.ToString(), request.AvailableFileNames);
            var fileNames = _fileDistributorManager.FilesToSync(request.AvailableFileNames);

            var retrievalList = fileNames.ToDictionary(f => f, _fileDistributorManager.GetRetrivalLinks);

            return new ServerCheckinResponse { FileLinks = retrievalList };
        }
    }
}
