using System.Net.WebSockets;

namespace Backend.Storage
{
    public interface IWebSocketContainer
    {
        void AddWebSocket(WebSocket webSocket);

        Task RequestCheckinAsync();

        Task CloseWebSocketAsync();
    }
}
