using Common.Stun;
using System;
using System.Net;
using System.Net.Sockets;

namespace StunClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestUdpHolePunching();
        }

        private static void TestUdpHolePunching()
        {
            var client_ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            var client_ip_endpoint = new IPEndPoint(client_ip, 7000);

            EndPoint server_ip_endpoint_11 = new IPEndPoint(IPAddress.Parse("20.244.39.178"), 6666);
            EndPoint server_ip_endpoint_12 = new IPEndPoint(IPAddress.Parse("20.244.39.178"), 7777);
            EndPoint server_ip_endpoint_21 = new IPEndPoint(IPAddress.Parse("20.235.241.67"), 6666);
            EndPoint server_ip_endpoint_22 = new IPEndPoint(IPAddress.Parse("20.235.241.67"), 7777);

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(client_ip_endpoint);

            SendDatagram(sock, server_ip_endpoint_11, StunMessageFlags.None);
            SendDatagram(sock, server_ip_endpoint_12, StunMessageFlags.None);
            SendDatagram(sock, server_ip_endpoint_21, StunMessageFlags.None);
            SendDatagram(sock, server_ip_endpoint_22, StunMessageFlags.None);
        }

        private static void SendDatagram(Socket sock, EndPoint server_ip_endpoint, StunMessageFlags flags)
        {
            var req = new StunMessageRequest(flags);
            //Logger.Log($"sending message to: \"{(server_ip_endpoint as IPEndPoint)!.Address}\":\"{(server_ip_endpoint as IPEndPoint)!.Port}\"");
            sock.SendTo(req.GetBytes(), server_ip_endpoint);

            var buffer = new byte[1024];
            sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref server_ip_endpoint);
            //Logger.Log($"recived message from endpoint: \"{(server_ip_endpoint as IPEndPoint)!.Address}\":\"{(server_ip_endpoint as IPEndPoint)!.Port}\"");
            _ = StunMessageResponse.Parse(buffer);
        }
    }
}
