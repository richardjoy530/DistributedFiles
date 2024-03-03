using Common.Stun;
using System.Net;
using System.Net.Sockets;

namespace StunClient
{
    internal abstract class Program
    {
        public static void Main()
        {
            //Logger.Log("--tcp--");
            //await TestTcpHolePunching();
            Logger.Log("--udp--");
            TestUdpHolePunching();
        }

        private static async Task TestTcpHolePunching()
        {
            var client_ip = (await Dns.GetHostEntryAsync(Dns.GetHostName())).AddressList[1];
            var client_ip_endpoint = new IPEndPoint(client_ip, 7000);

            //EndPoint server_ip_endpoint_11 = new IPEndPoint(IPAddress.Parse("192.168.18.87"), 8888);
            //EndPoint server_ip_endpoint_12 = new IPEndPoint(IPAddress.Parse("192.168.18.87"), 9999);

            EndPoint server_ip_endpoint_11 = new IPEndPoint(IPAddress.Parse("20.244.39.178"), 8888);
            EndPoint server_ip_endpoint_12 = new IPEndPoint(IPAddress.Parse("20.244.39.178"), 9999);
            EndPoint server_ip_endpoint_21 = new IPEndPoint(IPAddress.Parse("20.235.241.67"), 8888);
            EndPoint server_ip_endpoint_22 = new IPEndPoint(IPAddress.Parse("20.235.241.67"), 9999);

            await SendTcpMessage(client_ip_endpoint, server_ip_endpoint_11, StunMessageFlags.None);
            await SendTcpMessage(client_ip_endpoint, server_ip_endpoint_12, StunMessageFlags.None);
            await SendTcpMessage(client_ip_endpoint, server_ip_endpoint_21, StunMessageFlags.None);
            await SendTcpMessage(client_ip_endpoint, server_ip_endpoint_22, StunMessageFlags.None);
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

            var tasks = new List<Task>
            {
                Task.Run(() => SendUdpMessage(sock, server_ip_endpoint_11, StunMessageFlags.None)),
                Task.Run(() => SendUdpMessage(sock, server_ip_endpoint_12, StunMessageFlags.ChangePort)),
                Task.Run(() => SendUdpMessage(sock, server_ip_endpoint_21, StunMessageFlags.None)),
                Task.Run(() => SendUdpMessage(sock, server_ip_endpoint_22, StunMessageFlags.ChangePort))
            };

            Task.WaitAll([..tasks]);
        }

        private static async Task SendTcpMessage(IPEndPoint client_ip_endpoint, EndPoint server_ip_endpoint, StunMessageFlags flags)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(client_ip_endpoint);

            await sock.ConnectAsync(server_ip_endpoint);
            var req = new StunMessageRequest(flags);
            await sock.SendAsync(req.GetBytes());

            var buffer = new byte[1024];
            await sock.ReceiveAsync(new ArraySegment<byte>(buffer));
            _ = StunMessageResponse.Parse(buffer);

            //await sock.DisconnectAsync(true);
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }

        private static void SendUdpMessage(Socket sock, EndPoint server_ip_endpoint, StunMessageFlags flags)
        {
            var req = new StunMessageRequest(flags);
            //Logger.Log($"sending message to: \"{(server_ip_endpoint as IPEndPoint)!.Address}\":\"{(server_ip_endpoint as IPEndPoint)!.Port}\"");
            sock.SendTo(req.GetBytes(), server_ip_endpoint);

            var buffer = new byte[1024];
            sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref server_ip_endpoint);
            //Logger.Log($"received message from endpoint: \"{(server_ip_endpoint as IPEndPoint)!.Address}\":\"{(server_ip_endpoint as IPEndPoint)!.Port}\"");
            _ = StunMessageResponse.Parse(buffer);
        }
    }
}