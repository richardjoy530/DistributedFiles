using FileServerMaster.Web.Contracts;
using FileServerMaster.Storage;
using Microsoft.AspNetCore.Mvc;
using FileServerMaster.Web.Controllers;
using Common.Events;
using FileServerMaster.Events;
using Microsoft.AspNetCore.Http;

namespace FileServerMaster.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CheckInController : ControllerBase, ICheckInController
    {
        private readonly IFileDistributorManager _fileDistributorManager;
        private readonly IWebSocketContainer _webSocketContainer;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IFileContainer _fileContainer;
        private readonly ILogger<CheckInController> _logger;

        public CheckInController(IFileDistributorManager fileDistributorManager,
                                 IFileContainer fileContainer,
                                 ILogger<CheckInController> logger,
                                 IWebSocketContainer webSocketContainer,
                                 IEventDispatcher eventDispatcher)
        {
            _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
            _fileContainer = fileContainer ?? throw new ArgumentNullException(nameof(fileContainer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        [HttpPost]
        public ServerCheckinResponse CheckIn(AvailableFiles request)
        {
            /*
             * Check-In Sequence
             * 
             * 1. Remove the file from the file-container if you find it in any slave-servers.
             *      container files - slave files
             *
             * 2. Request check-in if there are any file new files
             * 
             * 3. Update the availablity table with file-remotehost map.
             * 
             * 4. Get files that are not in the slave files.
             *      all files from availablity table - slave files => files-to-be-retrived
             *      
             * 5. Get the file-host map for the files-to-be-retrived
             *      
             */

            // 1
            _fileContainer.DiscardFiles(request.AvailableFileNames);
            

            _logger.LogDebug("checked-in slave server addresse(s) are \"{addresses}\"", string.Join(';', request.SlaveHostStrings));

            // 2
            if (request.AvailableFileNames.Any(sf => _fileDistributorManager.GetAllFileNames().Contains(sf)))
            {
                // requesting checking to all slaves except this one
                _eventDispatcher.FireEvent(new RequestCheckInEvent([new HostString(request.SlaveHostStrings.First())], true));
            }

            // 3
            if (request.AvailableFileNames.Length != 0)
            {
                foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
                {
                    // updating the file availability table
                    _fileDistributorManager.UpdateFileAvailablity(new HostString(hostString), request.AvailableFileNames);
                }
            }

            // 4
            var filesToRetrive = _fileDistributorManager.GetAllFileNames().ToList();
            filesToRetrive.RemoveAll(f => request.AvailableFileNames.Contains(f));

            // 5
            var retrievalLinks = filesToRetrive.ToDictionary(f => f, f => _fileDistributorManager.GetRetrivalHost(f).ToString());

            return new ServerCheckinResponse { FileLinks = retrievalLinks };
        }
    }
}
