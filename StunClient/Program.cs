using Common.Stun;
using System.Net;
using System.Net.Sockets;

namespace StunClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());

            var localendpoint = new IPEndPoint(hostEntry.AddressList[1], 7000);
            //var senderRemote = new IPEndPoint(hostEntry.AddressList[1], 7777);
            var senderRemote = new IPEndPoint(IPAddress.Parse("20.244.39.178"), 7777);
            var se = (EndPoint)senderRemote;

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(localendpoint);

            var req = new StunMessageRequest(StunMessageFlags.None, null);
            sock.SendTo(req.GetBytes(), senderRemote);

            var buffer = new byte[1024];
            sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref se);
            var msg = StunMessageResponse.Parse(buffer);

            req = new StunMessageRequest(StunMessageFlags.ChangeAddress, null);
            sock.SendTo(req.GetBytes(), senderRemote);

            buffer = new byte[1024];
            sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref se);
            msg = StunMessageResponse.Parse(buffer);

        }
    }
}
