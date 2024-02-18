using System.Collections.Concurrent;

namespace FileServerMaster.Storage
{
    public class FileDistributorManager : IFileDistributorManager
    {
        private readonly ConcurrentDictionary<string, Queue<HostString>> _availablityTable;

        public FileDistributorManager()
        {
            _availablityTable = new ConcurrentDictionary<string, Queue<HostString>>();
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
                return host;
            }

            throw new InvalidOperationException("The requested file does not exists in the file availablity table");
        }

        public void RemoveHost(HostString host)
        {
            foreach (var hosts in _availablityTable.Values)
            {
                if (hosts.Contains(host))
                {
                    while (true) // assuming there is only few hosts per file. todo need to come up with another great idea. 
                    {
                        var temp = hosts.Dequeue();
                        if (temp == host)
                        {
                            return;
                        }

                        hosts.Enqueue(temp);
                    }
                }
            }
        }

        public void UpdateFileAvailablity(HostString remoteHost, string[] availableFileNames)
        {
            foreach (var fileName in availableFileNames)
            {
                if (_availablityTable.TryGetValue(fileName, out var hosts))
                {
                    if (!hosts.Contains(remoteHost))
                    {
                        hosts.Enqueue(remoteHost);
                    }
                }
                else
                {
                    hosts = new Queue<HostString>();
                    hosts.Enqueue(remoteHost);

                    _availablityTable.TryAdd(fileName, hosts);
                }
            }
        }
    }
}
