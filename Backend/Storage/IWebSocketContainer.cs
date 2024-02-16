using System.Net.WebSockets;

namespace Backend.Storage
{
    public interface IWebSocketContainer
    {
        Task Listen(WebSocket webSocket);

        Task RequestCheckinAsync();

        Task CloseWebSocketAsync();
    }
}
