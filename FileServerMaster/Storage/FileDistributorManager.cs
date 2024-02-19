using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace FileServerMaster.Storage
{
    public class FileDistributorManager : IFileDistributorManager
    {
        private readonly ConcurrentDictionary<string, Queue<HostString>> _availablityTable;
        private readonly ILogger<FileDistributorManager> _logger;
        private readonly IWebHostEnvironment _environment;

        public FileDistributorManager(ILogger<FileDistributorManager> logger, IWebHostEnvironment environment)
        {
            _availablityTable = new ConcurrentDictionary<string, Queue<HostString>>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public string[] GetAllFileNames()
        {
            return _availablityTable.Keys.ToArray();
        }

        public HostString GetRetrivalHost(string fileName)
        {
            if (_availablityTable.TryGetValue(fileName, out var hosts))
            {
                var host = hosts.Dequeue();
                hosts.Enqueue(host);
                _logger.LogDebug("retrival host for \"{fileName}\" is \"{host}\"", fileName, host);
                return host;
            }

            _logger.LogError("\"{}\" does not exist in the file availablity table", fileName);
            throw new InvalidOperationException($"\"{fileName}\" does not exist in the file availablity table");
        }

        public void RemoveHost(HostString host)
        {
            _logger.LogDebug("removing \"{host}\" from availablity table", host);
            foreach (var hosts in _availablityTable.Values)
            {
                if (hosts.Contains(host))
                {
                    while (true) // assuming there is only few hosts per file. todo need to come up with another great idea. 
                    {
                        var temp = hosts.Dequeue();
                        if (temp == host)
                        {
                            SerializeAvailablityTableInDevelopment();
                            return;
                        }

                        hosts.Enqueue(temp);
                    }
                }
            }

            SerializeAvailablityTableInDevelopment();
        }

        public void UpdateFileAvailablity(HostString host, string[] availableFileNames)
        {
            _logger.LogDebug("updating file availablity table with \"{host}\" - \"{filenames}\"", host, string.Join(',', availableFileNames));
            foreach (var fileName in availableFileNames)
            {
                if (_availablityTable.TryGetValue(fileName, out var hosts))
                {
                    if (!hosts.Contains(host))
                    {
                        hosts.Enqueue(host);
                    }
                }
                else
                {
                    hosts = new Queue<HostString>();
                    hosts.Enqueue(host);

                    _availablityTable.TryAdd(fileName, hosts);
                }
            }

            SerializeAvailablityTableInDevelopment();
        }

        private void SerializeAvailablityTableInDevelopment()
        {
            if (_environment.IsDevelopment())
            {
                var serialised = JsonConvert.SerializeObject(_availablityTable);
                _logger.LogDebug("awailablity table: \n {serialised}", serialised);
            }
        }
    }
}
