using System.Net.Sockets;
using System.Net;
using System.Text;

namespace StunServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var localendpoint = new IPEndPoint(hostEntry.AddressList[1], 6666);

            var sender = new IPEndPoint(IPAddress.Any, 0);
            var senderRemote = (EndPoint)sender;

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(localendpoint);
            while (true)
            {
                var buffer = new byte[1024 * 4];
                var count = sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref senderRemote);

                Console.WriteLine(Encoding.UTF8.GetString(buffer).Trim());
                Console.WriteLine(count);
            }

            sock.Close();
            sock.Dispose();
        }
    }
}