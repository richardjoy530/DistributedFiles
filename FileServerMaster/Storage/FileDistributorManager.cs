﻿using System.Collections.Concurrent;
using Common;

namespace FileServerMaster.Storage
{
    public class FileDistributorManager : IFileDistributorManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<HostString>> _availablityTable;
        private readonly ILogger<FileDistributorManager> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHostStringRetriver _hostStringRetriver;

        public FileDistributorManager(ILogger<FileDistributorManager> logger, IWebHostEnvironment environment, IHostStringRetriver hostStringRetriver)
        {
            _availablityTable = new ConcurrentDictionary<string, ConcurrentQueue<HostString>>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _hostStringRetriver = hostStringRetriver ?? throw new ArgumentNullException(nameof(hostStringRetriver));
        }

        public string[] GetAllFileNames()
        {
            return _availablityTable.Keys.ToArray();
        }

        public HostString GetRetrivalHost(string fileName)
        {
            if (_availablityTable.TryGetValue(fileName, out var hosts))
            {
                HostString host;
                lock (hosts) // locking the ccq since its being manipulated here.
                {
                    hosts.TryDequeue(out host);
                    hosts.Enqueue(host);
                }

                _logger.LogInformation("[AvailablityTable] host for \"{fileName}\" is \"{host}\"", fileName, host);
                LogAvailablityTable();
                return host;
            }

            _logger.LogError("\"{}\" does not exist in the file availablity table", fileName);
            throw new InvalidOperationException($"\"{fileName}\" does not exist in the file availablity table");
        }

        public void RemoveMaster(string fileName)
        {
            var masterHost = _hostStringRetriver.GetLocalFileServerHosts().First();

            if (_availablityTable.TryGetValue(fileName, out var hostStrings))
            {
                for (int i = 0; i < hostStrings.Count; i++)
                {
                    lock (hostStrings)
                    {
                        hostStrings.TryDequeue(out var temp);
                        if (temp == masterHost)
                        {
                            LogAvailablityTable();
                            _logger.LogInformation("[AvailablityTable] \"{file}\" is no longer hosted by master \"{host}\"", fileName, masterHost);
                            return;
                        }

                        hostStrings.Enqueue(temp);
                    }
                }
            }
        }

        public void RemoveHosting(HostString[] slaveHostAddress)
        {
            foreach (var fileName in _availablityTable.Keys)
            {
                lock (_availablityTable[fileName]) // locking the ccq since its being manipulated here.
                {
                    var hsl = slaveHostAddress.ToList();
                    for (int i = 0; i < _availablityTable[fileName].Count; i++)
                    {
                        if (hsl.Count == 0)
                        {
                            break;
                        }
                        
                        _availablityTable[fileName].TryDequeue(out var temp);
                        
                        if (!hsl.Remove(temp)) // true when temp is not present in hsl, i.e. continue the loop by enqueuing the dequeued element. 
                        {
                            _availablityTable[fileName].Enqueue(temp);
                        }
                        else
                        {
                            _logger.LogInformation("[AvailablityTable] \"{file}\" is no longer hosted by \"{host}\"", fileName, temp);
                        }
                    }
                }
            }
            LogAvailablityTable();
        }

        public void UpdateFileAvailablity(HostString host, string[] availableFileNames)
        {
            foreach (var fileName in availableFileNames)
            {
                if (_availablityTable.TryGetValue(fileName, out var hosts))
                {
                    _logger.LogInformation("[AvailablityTable] \"{file}\" is already present", fileName);
                    if (!hosts.Contains(host))
                    {
                        _logger.LogInformation("[AvailablityTable] \"{file}\" hosted by \"{host}\"", fileName, host);
                        hosts.Enqueue(host);
                    }
                }
                else
                {
                    hosts = new ConcurrentQueue<HostString>();
                    hosts.Enqueue(host);
                    _logger.LogInformation("[AvailablityTable] \"{file}\" hosted by \"{host}\"", fileName, host);
                    _availablityTable.TryAdd(fileName, hosts);
                }
            }

            LogAvailablityTable();
        }

        private void LogAvailablityTable()
        {
            return;
            if (_environment.IsDevelopment())
            {
                foreach (var item in _availablityTable)
                {
                    _logger.LogInformation("[AvailablityTable] \"{file}\" hosted by \"{host}\"", item.Key, string.Join(" - ", item.Value));
                }
            }
        }
    }
}
