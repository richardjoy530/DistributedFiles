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
            /*
             * Check-In Sequence
             * 
             * 1. Remove the file from the file-container if you find it in any slave-servers.
             * 
             * 2. Update the availablity table with file-remotehost map.
             * 
             * 3. Get the list of files that are not in the availability table.
             *      all files from availablity table - slave files => files-to-be-retrived
             *      
             * 4. Get the file-host map for the files-to-be-retrived
             */

            // 1
            var containerfiles = _fileContainer.GetTempFileNames();
            var intersection = containerfiles.Intersect(request.AvailableFileNames).ToArray();

            if (intersection.Length != 0)
            {
                _fileContainer.DiscardFiles(intersection);
            }

            _logger.LogDebug("local file server addresses are \"{addresses}\"", string.Join(';', request.SlaveHostStrings));
            
            // this was writen by me. i forgot why even i made this removal. can't reason out why.
            //foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
            //{
            //    _fileDistributorManager.RemoveHost(new HostString(hostString)); // todo .. there is some problem here.
            //}

            // 2.
            if (request.AvailableFileNames.Length != 0)
            {
                foreach (var hostString in request.SlaveHostStrings) // usually this will be just 1 or 2 loop
                {
                    // updating the file availability table
                    _fileDistributorManager.UpdateFileAvailablity(new HostString(hostString), request.AvailableFileNames);
                }
            }

            // 3
            var filesToRetrive = _fileDistributorManager.GetAllFileNames().ToList();
            filesToRetrive.RemoveAll(f => request.AvailableFileNames.Contains(f));

            // 4
            var retrievalLinks = filesToRetrive.ToDictionary(f => f, f => _fileDistributorManager.GetRetrivalHost(f).ToString());

            return new ServerCheckinResponse { FileLinks = retrievalLinks };
        }
    }
}
