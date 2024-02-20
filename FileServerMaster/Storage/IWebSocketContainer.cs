using System.Net.WebSockets;

namespace FileServerMaster.Storage
{
    public interface IWebSocketContainer
    {
        Task Listen(WebSocket webSocket);

        Task Process(Func<(HostString Host, WebSocket Socket), Task> excecuter);
    }
}
