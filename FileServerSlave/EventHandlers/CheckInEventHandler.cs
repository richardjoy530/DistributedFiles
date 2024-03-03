using Common;
using Common.Events;
using FileServerMaster.Web.Controllers;
using FileServerSlave.Events;
using FileServerSlave.Files;
using FileServerSlave.Interceptor;
using FileServerSlave.Utils;

namespace FileServerSlave.EventHandlers
{
    public class CheckInEventHandler : IEventHandler
    {
        private readonly ILogger<CheckInEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICheckInController _checkInController;
        private readonly IHostStringRetriever _hostStringRetriever;
        private readonly IFileManager _fileManager;

        public CheckInEventHandler(ILogger<CheckInEventHandler> logger,
            IEventDispatcher eventDispatcher,
            IHostStringRetriever slaveHostStringRetriever,
            IFileManager fileManager,
            IMasterServerRetriever masterServerRetriever)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _hostStringRetriever = slaveHostStringRetriever ?? throw new ArgumentNullException(nameof(slaveHostStringRetriever));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            ArgumentNullException.ThrowIfNull(nameof(masterServerRetriever));
            
            var secure = masterServerRetriever.Secure;
            var masterHostString = masterServerRetriever.GetMasterHostString();

            _logger.LogDebug("configured master host address is \"{host}\"", masterHostString);
            _checkInController = ApiInterceptor.GetController<ICheckInController>(masterHostString, secure);
        }

        public void HandleEvent(EventBase e)
        {
            if (e is not CheckInEvent _)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
                return;
            }

            var req = new FileServerMaster.Web.Contracts.AvailableFiles
            {
                AvailableFileNames = _fileManager.GetAvailableFilesOnThisServer(),
                SlaveHostStrings = _hostStringRetriever.GetLocalFileServerHosts().Select(h => h.ToString()).ToArray()
            };

            // api call to master server
            var resp = _checkInController.CheckIn(req);

            if (resp.FileLinks.Count == 0)
            {
                _logger.LogInformation("[CheckIn] no files to retrieve");
                return;
            }

            // fetch only the first one. this logic need's to be refined.
            var downLoadEvent = new DownloadEvent(resp.FileLinks.First());
            _eventDispatcher.FireEvent(downLoadEvent);
        }
    }
}