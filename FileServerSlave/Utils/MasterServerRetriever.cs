namespace FileServerSlave.Utils
{
    public class MasterServerRetriever : IMasterServerRetriever
    {
        public bool Secure { get; }
        public int RetryInSeconds { get; }

        private const int DefaultRetryInSeconds = 5;

        private readonly ILogger<MasterServerRetriever> _logger;
        private readonly HostString _hostString;
        private HostString? _customHostString;

        public MasterServerRetriever(ILogger<MasterServerRetriever> logger, IConfiguration configuration)
        {
            bool secure;
            int retryInSeconds;
            ArgumentNullException.ThrowIfNull(configuration);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            secure = bool.TryParse(configuration["UseHttps"], out secure) && secure;
            retryInSeconds = int.TryParse(configuration["RetryInSeconds"], out retryInSeconds) ? retryInSeconds : DefaultRetryInSeconds;

            Secure = secure;
            RetryInSeconds = retryInSeconds;

            _hostString = Secure ? new HostString(configuration["FileServerMasterHttps"]!) : new HostString(configuration["FileServerMasterHttp"]!);
        }

        public void SetCustomMasterHostString(HostString customHostString)
        {
            _customHostString ??= customHostString;
        }

        public HostString GetMasterHostString()
        {

            if (_customHostString == null)
            {
                return _hostString;
            }

            return (HostString)_customHostString;
        }
    }
}