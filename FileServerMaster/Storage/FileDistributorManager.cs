﻿using System.Collections.Concurrent;
using Common;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace FileServerMaster.Storage
{
    public class FileDistributorManager : IFileDistributorManager
    {
        private readonly ConcurrentDictionary<string, Queue<HostString>> _availablityTable;
        private readonly ILogger<FileDistributorManager> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHostStringRetriver _hostStringRetriver;

        public FileDistributorManager(ILogger<FileDistributorManager> logger, IWebHostEnvironment environment, IHostStringRetriver hostStringRetriver)
        {
            _availablityTable = new ConcurrentDictionary<string, Queue<HostString>>();
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
                var host = hosts.Dequeue();
                hosts.Enqueue(host);
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
            _logger.LogInformation("[AvailablityTable] \"{file}\" is no longer hosted by master \"{host}\"", fileName, masterHost);

            if (_availablityTable.TryGetValue(fileName, out var hostStrings))
            {
                for (int i = 0; i < hostStrings.Count; i++)
                {
                    var temp = hostStrings.Dequeue();
                    if (temp == masterHost)
                    {
                        LogAvailablityTable();
                        return;
                    }

                    hostStrings.Enqueue(temp);
                }
            }
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
                    hosts = new Queue<HostString>();
                    hosts.Enqueue(host);
                    _logger.LogInformation("[AvailablityTable] \"{file}\" hosted by \"{host}\"", fileName, host);
                    _availablityTable.TryAdd(fileName, hosts);
                }
            }

            LogAvailablityTable();
        }

        private void LogAvailablityTable()
        {
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
