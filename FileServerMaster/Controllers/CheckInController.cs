using FileServerMaster.Web.Contracts;
using FileServerMaster.Storage;
using Microsoft.AspNetCore.Mvc;
using FileServerMaster.Web.Controllers;
using Common.Events;
using FileServerMaster.Events;

namespace FileServerMaster.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase, ICheckInController
    {
        private readonly IFileDistributorManager _fileDistributorManager;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IFileContainer _fileContainer;
        private readonly ILogger<CheckInController> _logger;

        public CheckInController(IFileDistributorManager fileDistributorManager,
            IFileContainer fileContainer,
            ILogger<CheckInController> logger,
            IEventDispatcher eventDispatcher)
        {
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
            _fileContainer = fileContainer ?? throw new ArgumentNullException(nameof(fileContainer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        [HttpPost]
        public ServerCheckinResponse CheckIn(AvailableFiles request)
        {
            /*
             * Check-In Sequence
             *
             * 1. DisposeAndRemove the file from the file-container if you find it in any slave-servers.
             *      container files - slave files
             *
             * 2. Update the availability table with file-remote-host map.
             *
             * 3. Request check-in if there are any file new files
             *      if any in slave files that are not in availability table
             *
             * 4. Get files that are not in the slave files.
             *      all files from availability table - slave files => files-to-be-retrieved
             *
             * 5. Get the file-host map for the files-to-be-retrieved
             *
             */
            _logger.LogInformation("[CheckIn] checked-in slave server address(es) are \"{addresses}\"", string.Join(';', request.SlaveHostStrings));

            // 1
            _fileContainer.DiscardFiles(request.AvailableFileNames);

            // 2
            var tableFilesOld = _fileDistributorManager.GetAllFileNames(); // if we update the file table in #2, then #4 will never run
            if (request.AvailableFileNames.Length != 0)
            {
                foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
                {
                    // updating the file availability table
                    _fileDistributorManager.UpdateFileAvailability(new HostString(hostString), request.AvailableFileNames);
                }
            }

            // 3
            if (request.AvailableFileNames.Any(sf => !tableFilesOld.Contains(sf)))
            {
                // requesting checking to all slaves except this one
                // todo only send checkin to slaves that doesnt have this file
                _eventDispatcher.FireEvent(new RequestCheckInEvent([new HostString(request.SlaveHostStrings.First())], true));
            }

            // 4
            var filesToRetrieve = _fileDistributorManager.GetAllFileNames().ToList();
            filesToRetrieve.RemoveAll(f => request.AvailableFileNames.Contains(f));

            // 5
            var retrievalLinks = filesToRetrieve.ToDictionary(f => f, f => _fileDistributorManager.GetRetrievalHost(f).ToString());

            return new ServerCheckinResponse { FileLinks = retrievalLinks };
        }
    }
}