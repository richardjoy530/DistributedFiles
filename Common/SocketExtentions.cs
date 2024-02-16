using System.Net.WebSockets;
using System.Text;

namespace Common
{
    public static class SocketExtentions
    {
        public static async Task WriteAsync(this WebSocket ws, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, false, CancellationToken.None);
        }

        public static async Task<string> ReadAsync(this WebSocket ws)
        {
            var buffer = new byte[1024 * 4];
            await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
