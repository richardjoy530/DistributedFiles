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

            // updating the file availability table
            _fileDistributorManager.UpdateFileAvailablity(request.HostString.ToString(), request.AvailableFileNames);

            var filesToRetrive = _fileDistributorManager.GetAllFileNames().ToList();
            filesToRetrive.RemoveAll(f => intersection.Contains(f));
            var retrievalLinks = filesToRetrive.ToDictionary(f => f, _fileDistributorManager.GetRetrivalLink);

            return new ServerCheckinResponse { FileLinks = retrievalLinks };
        }
    }
}
