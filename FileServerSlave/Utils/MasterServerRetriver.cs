namespace FileServerSlave.Utils
{
    public class MasterServerRetriver : IMasterServerRetriver
    {
        public bool Secure { get; }
        public int RetryInSeconds { get; }

        private const int DefautRetryInSeconds = 5;

        private int _retryInSeconds;
        private bool _secure;
        private readonly ILogger<MasterServerRetriver> _logger;
        private readonly HostString _hostString;
        private HostString? _customHostString;

        public MasterServerRetriver(ILogger<MasterServerRetriver> logger, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _secure = bool.TryParse(configuration["UseHttps"], out _secure) && _secure;
            _retryInSeconds = int.TryParse(configuration["RetryInSeconds"], out _retryInSeconds) ? _retryInSeconds : DefautRetryInSeconds;

            Secure = _secure;
            RetryInSeconds = _retryInSeconds;

            if (Secure)
            {
                _hostString = new HostString(configuration["FileServerMasterHttps"]!);
            }
            else
            {
                _hostString = new HostString(configuration["FileServerMasterHttp"]!);
            }
        }

        public bool SetCustomMasterHostString(HostString customHostString)
        {
            if (_customHostString == null)
            {
                _customHostString = customHostString;
                return true;
            }

            return false;
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
