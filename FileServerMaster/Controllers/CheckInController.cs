using FileServerMaster.Web.Contracts;
using FileServerMaster.Storage;
using Microsoft.AspNetCore.Mvc;
using FileServerMaster.Web.Controllers;

namespace FileServerMaster.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase, ICheckInController
    {
        private readonly IFileDistributorManager _fileDistributorManager;
        private readonly IFileContainer _fileContainer;
        private readonly ILogger<CheckInController> _logger;

        public CheckInController(IFileDistributorManager fileDistributorManager, IFileContainer fileContainer, ILogger<CheckInController> logger)
        {
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
            _fileContainer = fileContainer ?? throw new ArgumentNullException(nameof(fileContainer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public ServerCheckinResponse CheckIn(AvailableFiles request)
        {
            // discarding files saved in the container when its already in the file-server
            var containerFiles = _fileContainer.GetTempFileNames().ToArray();
            var intersection = containerFiles.Intersect(request.AvailableFileNames).ToArray();

            if (intersection.Length != 0)
            {
                _fileContainer.DiscardFiles(intersection);
            }

            _logger.LogDebug("local file server addresses are \"{addresses}\"", string.Join(';', request.SlaveHostStrings));
            foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
            {
                _fileDistributorManager.RemoveHost(new HostString(hostString)); // todo .. there is some problem here.
            }

            if (request.AvailableFileNames.Length != 0)
            {
                foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
                {
                    // updating the file availability table
                    _fileDistributorManager.UpdateFileAvailablity(new HostString(hostString), request.AvailableFileNames);
                }
            }

            var filesToRetrive = _fileDistributorManager.GetAllFileNames().ToList();
            filesToRetrive.RemoveAll(f => intersection.Contains(f));
            var retrievalLinks = filesToRetrive.ToDictionary(f => f, f => _fileDistributorManager.GetRetrivalHost(f).ToString());

            return new ServerCheckinResponse { FileLinks = retrievalLinks };
        }
    }
}
