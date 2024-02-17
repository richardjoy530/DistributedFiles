using System.Collections.Concurrent;

namespace Backend.Storage
{
    public class FileDistributorManager : IFileDistributorManager
    {
        private readonly ConcurrentDictionary<string, Queue<string>> _availablityTable;

        public FileDistributorManager()
        {
            _availablityTable = new ConcurrentDictionary<string, Queue<string>>();
        }

        public string[] GetAllFileNames()
        {
            return _availablityTable.Keys.ToArray();
        }

        public string GetRetrivalLink(string fileName)
        {
            if (_availablityTable.TryGetValue(fileName, out var hosts))
            {
                var host = hosts.Dequeue();
                hosts.Enqueue(host);
                return host;
            }

            throw new InvalidOperationException("The requested file does not exists in the file availablity table");
        }

        public void UpdateFileAvailablity(string remoteHost, string[] availableFileNames)
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
                    hosts = new Queue<string>();
                    hosts.Enqueue(remoteHost);

                    _availablityTable.TryAdd(fileName, hosts);
                }
            }
        }
    }
}
