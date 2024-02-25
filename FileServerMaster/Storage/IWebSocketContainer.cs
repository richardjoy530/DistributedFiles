using System.Net.WebSockets;

namespace FileServerMaster.Storage
{
    public interface IWebSocketContainer
    {
        Task Listen(WebSocket webSocket, ICollection<HostString> slaveHosts);

        void CloseWebSocketAsync();

        void Process(Action<(HostString Host, WebSocket Socket)> excecuter);

        void DisposeAndRemove(HostString[] hostString);
    }
}
