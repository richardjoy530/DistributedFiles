using System.Net.WebSockets;

namespace FileServerMaster.Storage
{
    public interface IWebSocketContainer
    {
        Task Listen(WebSocket webSocket);

        void Process(Action<(HostString Host, WebSocket Socket)> excecuter);
    }
}
