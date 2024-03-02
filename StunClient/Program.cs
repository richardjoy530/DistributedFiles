using Common.Stun;
using System.Net;
using System.Net.Sockets;

namespace StunClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client_ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            var client_ip_endpoint = new IPEndPoint(client_ip, 7000);

            var server_ip_endpoint = new IPEndPoint(client_ip, 6666); // change
            var server_endpoint = (EndPoint)server_ip_endpoint;

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(client_ip_endpoint);

            /////////// None
            var req = new StunMessageRequest(StunMessageFlags.None);
            sock.SendTo(req.GetBytes(), server_ip_endpoint);

            var buffer = new byte[1024];
            sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref server_endpoint);
            Console.WriteLine($"recived message from endpoint: \"{server_ip_endpoint!.Address}\":\"{server_ip_endpoint.Port}\"");
            var msg = StunMessageResponse.Parse(buffer);

            ///////////// ChangeIp
            //req = new StunMessageRequest(StunMessageFlags.ChangeIp);
            //sock.SendTo(req.GetBytes(), server_ip_endpoint);

            //buffer = new byte[1024];
            //sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref server_endpoint);
            //Console.WriteLine($"recived message from endpoint: \"{server_ip_endpoint!.Address}\":\"{server_ip_endpoint.Port}\"");
            //msg = StunMessageResponse.Parse(buffer);

            /////////// ChangePort
            req = new StunMessageRequest(StunMessageFlags.ChangePort);
            sock.SendTo(req.GetBytes(), server_ip_endpoint);

            buffer = new byte[1024];
            sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref server_endpoint);
            Console.WriteLine($"recived message from endpoint: \"{server_ip_endpoint!.Address}\":\"{server_ip_endpoint.Port}\"");
            msg = StunMessageResponse.Parse(buffer);

            ///////////// ChangeBoth
            //req = new StunMessageRequest(StunMessageFlags.ChangeBoth);
            //sock.SendTo(req.GetBytes(), server_ip_endpoint);

            //buffer = new byte[1024];
            //sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref server_endpoint);
            //Console.WriteLine($"recived message from endpoint: \"{server_ip_endpoint!.Address}\":\"{server_ip_endpoint.Port}\"");
            //msg = StunMessageResponse.Parse(buffer);

        }
    }
}
