using System.Net.WebSockets;

namespace FileServerMaster.Storage
{
    public interface IWebSocketContainer
    {
        Task Listen(WebSocket webSocket);

        Task RequestCheckinAsync();

        Task CloseWebSocketAsync();
    }
}
