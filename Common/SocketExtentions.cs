using System.Net.WebSockets;
using System.Text;

namespace Common
{
    public static class SocketExtentions
    {
        public static async Task WriteAsync(this WebSocket ws, string message)
        {
            await ws.WriteAsync(message, CancellationToken.None);
        }

        public static async Task WriteAsync(this WebSocket ws, string message, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, false, cancellationToken);
        }

        public static async Task<(WebSocketReceiveResult ReciveResult, string Message)> ReadAsync(this WebSocket ws)
        {
            return await ws.ReadAsync(CancellationToken.None);
        }

        public static async Task<(WebSocketReceiveResult ReciveResult, string Message)> ReadAsync(this WebSocket ws, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            var wsr = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            return (wsr, Encoding.UTF8.GetString(buffer));
        }
    }
}
