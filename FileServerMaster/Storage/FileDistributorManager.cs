using System.Collections.Concurrent;
using Common;

namespace FileServerMaster.Storage
{
    public class FileDistributorManager : IFileDistributorManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<HostString>> _availabilityTable;
        private readonly ILogger<FileDistributorManager> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHostStringRetriever _hostStringRetriever;

        public FileDistributorManager(ILogger<FileDistributorManager> logger, IWebHostEnvironment environment, IHostStringRetriever hostStringRetriever)
        {
            _availabilityTable = new ConcurrentDictionary<string, ConcurrentQueue<HostString>>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _hostStringRetriever = hostStringRetriever ?? throw new ArgumentNullException(nameof(hostStringRetriever));
        }

        public string[] GetAllFileNames()
        {
            return _availabilityTable.Keys.ToArray();
        }

        public HostString GetRetrievalHost(string fileName)
        {
            if (_availabilityTable.TryGetValue(fileName, out var hosts))
            {
                HostString host;
                lock (hosts) // locking the ccq since its being manipulated here.
                {
                    hosts.TryDequeue(out host);
                    hosts.Enqueue(host);
                }

                _logger.LogInformation("[AvailabilityTable] host for \"{fileName}\" is \"{host}\"", fileName, host);
                LogAvailabilityTable();
                return host;
            }

            _logger.LogError("\"{}\" does not exist in the file availability table", fileName);
            throw new InvalidOperationException($"\"{fileName}\" does not exist in the file availability table");
        }

        public void RemoveMaster(string fileName)
        {
            var masterHost = _hostStringRetriever.GetLocalFileServerHosts().First();

            if (!_availabilityTable.TryGetValue(fileName, out var hostStrings))
            {
                return;
            }

            for (var i = 0; i < hostStrings.Count; i++)
            {
                lock (hostStrings)
                {
                    hostStrings.TryDequeue(out var temp);
                    if (temp == masterHost)
                    {
                        LogAvailabilityTable();
                        _logger.LogInformation("[AvailabilityTable] \"{file}\" is no longer hosted by master \"{host}\"", fileName, masterHost);
                        return;
                    }

                    hostStrings.Enqueue(temp);
                }
            }
        }

        public void RemoveHosting(HostString[] slaveHostAddress)
        {
            foreach (var fileName in _availabilityTable.Keys)
            {
                lock (_availabilityTable[fileName]) // locking the ccq since its being manipulated here.
                {
                    var hsl = slaveHostAddress.ToList();
                    for (var i = 0; i < _availabilityTable[fileName].Count; i++)
                    {
                        if (hsl.Count == 0)
                        {
                            break;
                        }
                        
                        _availabilityTable[fileName].TryDequeue(out var temp);
                        
                        if (!hsl.Remove(temp)) // true when temp is not present in hsl, i.e. continue the loop by enqueuing the dequeued element. 
                        {
                            _availabilityTable[fileName].Enqueue(temp);
                        }
                        else
                        {
                            _logger.LogInformation("[AvailabilityTable] \"{file}\" is no longer hosted by \"{host}\"", fileName, temp);
                        }
                    }
                }
            }
            LogAvailabilityTable();
        }

        public void UpdateFileAvailability(HostString host, string[] availableFileNames)
        {
            foreach (var fileName in availableFileNames)
            {
                if (_availabilityTable.TryGetValue(fileName, out var hosts))
                {
                    _logger.LogInformation("[AvailabilityTable] \"{file}\" is already present", fileName);
                    if (hosts.Contains(host))
                    {
                        continue;
                    }

                    _logger.LogInformation("[AvailabilityTable] \"{file}\" hosted by \"{host}\"", fileName, host);
                    hosts.Enqueue(host);
                }
                else
                {
                    hosts = new ConcurrentQueue<HostString>();
                    hosts.Enqueue(host);
                    _logger.LogInformation("[AvailabilityTable] \"{file}\" hosted by \"{host}\"", fileName, host);
                    _availabilityTable.TryAdd(fileName, hosts);
                }
            }

            LogAvailabilityTable();
        }

        private void LogAvailabilityTable()
        {
            if (!_environment.IsDevelopment())
            {
                return;
            }
        
            foreach (var item in _availabilityTable)
            {
                _logger.LogInformation("[AvailabilityTable] \"{file}\" hosted by \"{host}\"", item.Key, string.Join(" - ", item.Value));
            }
        }
    }
}