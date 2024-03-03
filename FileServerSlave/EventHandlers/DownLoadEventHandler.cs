using Common.Events;
using FileServerSlave.Events;
using FileServerSlave.Files;
using FileServerSlave.Utils;

namespace FileServerSlave.EventHandlers
{
    public class DownLoadEventHandler : IEventHandler
    {
        private readonly ILogger<DownLoadEventHandler> _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IFileManager _fileManager;
        private readonly bool _secure;

        public DownLoadEventHandler(ILogger<DownLoadEventHandler> logger,
                                    IEventDispatcher eventDispatcher,
                                    IMasterServerRetriver masterServerRetriver,
                                    IFileManager fileManager)
        {
            ArgumentNullException.ThrowIfNull(masterServerRetriver);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));

            _secure = masterServerRetriver.Secure;
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

            var url = new UriBuilder()
            {
                Scheme = _secure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp,
                Host = de.HostString.Host,
                Port = de.HostString.Port ?? default,
                Path = $"file\\{de.FileName}"
            }.Uri;

            _ = Task.Run(async () =>
            {
                try
                {
                    var client = new HttpClient();
                    var stream = await client.GetStreamAsync(url);

                    await _fileManager.SaveFile(stream!, de.FileName);

                    stream.Close();
                    stream.Dispose();
                    client.Dispose();

                    var ce = new CheckInEvent();
                    _eventDispatcher.FireEvent(ce);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    _logger.LogTrace(new EventId(0), ex, ex.Message);
                }
            });
        }
    }
}
