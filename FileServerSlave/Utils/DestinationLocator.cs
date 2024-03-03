namespace FileServerSlave.Utils
{
    public class DestinationLocator : IDestinationLocator
    {
        private readonly ILogger<DestinationLocator> _logger;
        private readonly string _defaultDestination;
        private static string? _customDestinationFolder;

        public DestinationLocator(ILogger<DestinationLocator> logger, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _defaultDestination = configuration["DistributedFolder"]!;
        }

        public void SetCustomLocation(string customPath)
        {
            if (!string.IsNullOrWhiteSpace(_customDestinationFolder))
            {
                return;
            }

            if (!Directory.Exists(customPath))
            {
                Directory.CreateDirectory(customPath);
            }

            _customDestinationFolder = customPath;
        }

        public string GetDestinationFolderPath()
        {
            return string.IsNullOrWhiteSpace(_customDestinationFolder) ? _defaultDestination : _customDestinationFolder;
        }
    }
}