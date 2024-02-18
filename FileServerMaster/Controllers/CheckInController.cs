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

        public CheckInController(IFileDistributorManager fileDistributorManager, IFileContainer fileContainer)
        {
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
            _fileContainer = fileContainer ?? throw new ArgumentNullException(nameof(fileContainer));
        }

        [HttpPost]
        public ServerCheckinResponse CheckIn(AvailableFiles request)
        {
            // discarding files saved in the container when its already in the file-server
            var containerFiles = _fileContainer.GetTempFileNames().ToArray();
            var intersection = containerFiles.Intersect(request.AvailableFileNames).ToArray();
            _fileContainer.DiscardFiles(intersection);

            foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
            {
                _fileDistributorManager.RemoveHost(new HostString(hostString));
            }

            foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
            { 
                // updating the file availability table
                _fileDistributorManager.UpdateFileAvailablity(new HostString(hostString), request.AvailableFileNames);
            }

            var filesToRetrive = _fileDistributorManager.GetAllFileNames().ToList();
            filesToRetrive.RemoveAll(f => intersection.Contains(f));
            var retrievalLinks = filesToRetrive.ToDictionary(f => f, _fileDistributorManager.GetRetrivalHost);

            return new ServerCheckinResponse { FileLinks = retrievalLinks };
        }
    }
}
