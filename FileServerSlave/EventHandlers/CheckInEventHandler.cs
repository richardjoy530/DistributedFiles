using FileServerMaster.Web.Controllers;
using FileServerSlave.Events;
using FileServerSlave.Files;
using FileServerSlave.Interceptor;

namespace FileServerSlave.EventHandlers
{
    public class CheckInEventHandler : IEventHandler
    {
        private readonly ILogger<CheckInEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ICheckInController _checkInController;
        private readonly ISlaveHostStringRetriver _slaveHostStringRetriver;
        private readonly IFileManager _fileManager;

        public CheckInEventHandler(ILogger<CheckInEventHandler> logger, IEventDispatcher eventDispatcher, IConfiguration configuration, ISlaveHostStringRetriver slaveHostStringRetriver, IFileManager fileManager)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _slaveHostStringRetriver = slaveHostStringRetriver ?? throw new ArgumentNullException(nameof(slaveHostStringRetriver));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

            HostString masterHostString;
            _ = bool.TryParse(configuration["UseHttps"], out var secure);
            if (secure)
            {
                masterHostString = new HostString(configuration["FileServerMasterHttps"]!);
            }
            else
            {
                masterHostString = new HostString(configuration["FileServerMasterHttp"]!);
            }

            _checkInController = ApiInterceptor.GetController<ICheckInController>(masterHostString, secure);
        }

        public void HandleEvent(EventBase e)
        {
            if (e is not CheckInEvent _)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
            }

            var req = new FileServerMaster.Web.Contracts.AvailableFiles
            {
                AvailableFileNames = _fileManager.GetAvailableFilesOnThisServer(),
                SlaveHostStrings = _slaveHostStringRetriver.GetLocalFileServerHosts().Select(h => h.ToString()).ToArray()
            };

            // api call to master server
            var resp = _checkInController.CheckIn(req);
            _logger.LogInformation("handled checkin event");

            if (resp.FileLinks.Count == 0)
            {
                return;
            }

            // fetch only the first one. this logic need's to be refined.
            var downLoadEvent = new DownloadEvent(resp.FileLinks.First());
            _eventDispatcher.FireEvent(downLoadEvent);
        }
    }
}
