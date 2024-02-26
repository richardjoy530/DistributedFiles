using Common.Events;
using Common.Proxy.Controllers;
using FileServerSlave.Events;
using FileServerSlave.Files;
using FileServerSlave.Interceptor;

namespace FileServerSlave.EventHandlers
{
    public class DownLoadEventHandler : IEventHandler
    {
        private readonly ILogger<DownLoadEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IFileManager _fileManager;
        private readonly bool _secure;

        public DownLoadEventHandler(ILogger<DownLoadEventHandler> logger, IEventDispatcher eventDispatcher, IConfiguration configuration, IFileManager fileManager)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

            _ = bool.TryParse(configuration["UseHttps"], out _secure);
        }

        public void HandleEvent(EventBase e)
        {
            if (e is not DownloadEvent de)
            {
                _logger.LogError("cannot handle event of type \"{type}\"", e.GetType().Name);
                return;
            }

            _logger.LogInformation("[DownloadEvent] host for \"{filename}\" is \"{hoststring}\"", de.FileName, de.HostString);
            if (string.IsNullOrWhiteSpace(de.HostString.ToString()))
            {
                _logger.LogError("download host is invalid");
                return;
            }

            var fileController = ApiInterceptor.GetController<IFileController>(de.HostString, _secure);

            try
            {
                var resp = fileController.DownloadFileStream(de.FileName);

                if (resp is null)
                {
                    _logger.LogWarning("download \"{}\" from \"{HostString}\" returned null", de.FileName, de.HostString);
                }
                else
                {
                    _ = _fileManager.SaveFileAsync(resp.Content.ReadAsStream(), de.FileName);
                }

                var ce = new CheckInEvent();
                _eventDispatcher.FireEvent(ce);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogTrace(new EventId(0), ex, ex.Message);
            }
        }
    }
}
