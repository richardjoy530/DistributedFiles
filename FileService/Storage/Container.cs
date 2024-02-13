using System.Net.WebSockets;

namespace Backend.Storage;

public static class Container
{
    public static Queue<IFormFile> Files { get; set; }

    public static List<WebSocket> ConnectedSockets { get; set; }
}
