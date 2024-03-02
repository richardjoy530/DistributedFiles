using System.Net;
using System.Net.Sockets;
using System.Text;

namespace StunClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            var localendpoint = new IPEndPoint(hostEntry.AddressList[1], 7777);
            var senderRemote = new IPEndPoint(hostEntry.AddressList[1], 6666);

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(localendpoint);

            var buffer = Encoding.UTF8.GetBytes("hello world");
            var count = sock.SendTo(buffer, senderRemote);

            Console.WriteLine(count);
        }
    }
}
