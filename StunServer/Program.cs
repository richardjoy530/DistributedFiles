using System.Net.Sockets;
using System.Net;
using Common.Stun;

namespace StunServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host2 = args[0];
            var port1 = args[1]; // 6666
            var port2 = args[2]; // 7777

            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var endPoint11 = new IPEndPoint(hostEntry.AddressList[1], int.Parse(port1));
            var endPoint12 = new IPEndPoint(hostEntry.AddressList[1], int.Parse(port2));
            var endPoint21 = new IPEndPoint(IPAddress.Parse(host2), int.Parse(port1));
            var endPoint22 = new IPEndPoint(IPAddress.Parse(host2), int.Parse(port2));

            var sender = new IPEndPoint(IPAddress.Any, 0);
            var senderRemote = (EndPoint)sender;

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(endPoint11);
            while (true)
            {
                var buffer = new byte[1024];
                sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref senderRemote);
                var msg = StunMessageRequest.Parse(buffer);

                if (msg.ChangeAddressFlag == StunMessageFlags.None)
                {
                    var resp = new StunMessageResponse((senderRemote as IPEndPoint)!.Address);
                    var buff = resp.GetBytes();
                    _ = sock.SendTo(buff, senderRemote);
                }
                else if (msg.ChangeAddressFlag == StunMessageFlags.ChangeAddress)
                {
                    var req = new StunMessageRequest(StunMessageFlags.None, (senderRemote as IPEndPoint)!.Address);
                    var buff = req.GetBytes();
                    _ = sock.SendTo(buff, endPoint21);
                }
                else if (msg.ChangeAddressFlag == StunMessageFlags.ChangePort)
                {
                    var req = new StunMessageRequest(StunMessageFlags.None, (senderRemote as IPEndPoint)!.Address);
                    var buff = req.GetBytes();
                    _ = sock.SendTo(buff, endPoint12);
                }
                else if (msg.ChangeAddressFlag == StunMessageFlags.ChangeBoth)
                {
                    var req = new StunMessageRequest(StunMessageFlags.None, (senderRemote as IPEndPoint)!.Address);
                    var buff = req.GetBytes();
                    _ = sock.SendTo(buff, endPoint22);
                }
            }
        }
    }
}