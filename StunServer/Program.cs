using System.Net.Sockets;
using System.Net;
using Common.Stun;

namespace StunServer
{
    internal class Program
    {
        private const int PrimaryUdpPort = 6666;
        private const int SecondaryUdpPort = 7777;

        private const int PrimaryTcpPort = 8888;
        private const int SecondaryTcpPort = 9999;

        public static void Main(string[] args)
        {
            var server_ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                Logger.Log($"available ip: \"{ip}\"");
            }

            Logger.Log($"stun server running on: \"{server_ip}\"");
            var remote_server_ip = IPAddress.Parse(args[0]);
            Logger.Log($"remote stun server on: \"{remote_server_ip}\"");

            var primary_udp_server = new Thread(() => RunUdpTraversal(true, server_ip, remote_server_ip));
            var secondary_udp_server = new Thread(() => RunUdpTraversal(false, server_ip, remote_server_ip));

            var primary_tcp_server = new Thread(() => RunTcpTraversal(true, server_ip, remote_server_ip));
            var secondary_tcp_server = new Thread(() => RunTcpTraversal(false, server_ip, remote_server_ip));

            primary_udp_server.Start();
            secondary_udp_server.Start();

            primary_tcp_server.Start();
            secondary_tcp_server.Start();
        }

        private static void RunTcpTraversal(bool is_primary, IPAddress server_ip, IPAddress remote_server_ip)
        {
            // endpoint that host's stun server on this process
            var server_ip_endpoint_1 = new IPEndPoint(server_ip, is_primary ? PrimaryTcpPort : SecondaryTcpPort);
            var server_ip_endpoint_2 = new IPEndPoint(server_ip, is_primary ? SecondaryTcpPort : PrimaryTcpPort);

            // stun protocal needs 2 servers with different ips, these are the remote stun server
            var remote_server_ip_endpoint_1 = new IPEndPoint(remote_server_ip, is_primary ? PrimaryTcpPort : SecondaryTcpPort);
            var remote_server_ip_endpoint_2 = new IPEndPoint(remote_server_ip, is_primary ? SecondaryTcpPort : PrimaryTcpPort);

            // creating a TCP socket
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(server_ip_endpoint_1);

            sock.Listen(1); // for now only one connection can be accepted.
            while (true)
            {
                var client = sock.Accept();
                var client_endpoint = client.RemoteEndPoint;

                var buffer = new byte[1024];
                client.Receive(buffer);
                Logger.Log($"recived message from endpoint: \"{(client_endpoint as IPEndPoint)!.Address}\":\"{(client_endpoint as IPEndPoint)!.Port}\"");
                var req = StunMessageRequest.Parse(buffer);

                var resp = new StunMessageResponse((client_endpoint as IPEndPoint)!, req.RefrenceId);
                Logger.Log($"sending message to client endpoint: \"{(client_endpoint as IPEndPoint)!.Address}\":\"{(client_endpoint as IPEndPoint)!.Port}\"");
                client.Send(resp.GetBytes());
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        private static void RunUdpTraversal(bool is_primary, IPAddress server_ip, IPAddress remote_server_ip)
        {
            // endpoint that host's stun server on this process
            var server_ip_endpoint_1 = new IPEndPoint(server_ip, is_primary ? PrimaryUdpPort : SecondaryUdpPort);
            var server_ip_endpoint_2 = new IPEndPoint(server_ip, is_primary ? SecondaryUdpPort : PrimaryUdpPort);

            // stun protocal needs 2 servers with different ips, these are the remote stun server
            var remote_server_ip_endpoint_1 = new IPEndPoint(remote_server_ip, is_primary ? PrimaryUdpPort : SecondaryUdpPort);
            var remote_server_ip_endpoint_2 = new IPEndPoint(remote_server_ip, is_primary ? SecondaryUdpPort : PrimaryUdpPort);

            // creating a UPD socket
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(server_ip_endpoint_1);
            Logger.Log($"stun binding sucucssfull on: \"{server_ip_endpoint_1.Address}\":\"{server_ip_endpoint_1.Port}\"");

            while (true)
            {
                Logger.Log("-----------------------------------");
                var buffer = new byte[1024];

                // container variable for saving the stun client's endpoint after reciving message
                EndPoint client_endpoint = new IPEndPoint(IPAddress.Any, 0);

                // listening for a StunMessageRequest. this can come from a stun client or anoter remote stun server.
                sock.ReceiveFrom(new ArraySegment<byte>(buffer), ref client_endpoint);
                Logger.Log($"recived message from endpoint: \"{(client_endpoint as IPEndPoint)!.Address}\":\"{(client_endpoint as IPEndPoint)!.Port}\"");

                var msg = StunMessageRequest.Parse(buffer);

                switch (msg.ChangeAddressFlag)
                {
                    case StunMessageFlags.None:
                        {
                            if (msg.ResponseEndpoint != null )
                            {
                                var resp = new StunMessageResponse(msg.ResponseEndpoint, msg.RefrenceId);
                                var buff = resp.GetBytes();

                                Logger.Log($"sending message to client endpoint: \"{msg.ResponseEndpoint.Address}\":\"{msg.ResponseEndpoint.Port}\"");

                                sock.SendTo(buff, msg.ResponseEndpoint);
                            }
                            else
                            {
                                var resp = new StunMessageResponse((client_endpoint as IPEndPoint)!, msg.RefrenceId);
                                var buff = resp.GetBytes();

                                Logger.Log($"sending message to client endpoint: \"{(client_endpoint as IPEndPoint)!.Address}\":\"{(client_endpoint as IPEndPoint)!.Port}\"");

                                sock.SendTo(buff, (client_endpoint as IPEndPoint)!);
                            }

                            break;
                        }

                    case StunMessageFlags.ChangeIp:
                        {
                            var req = new StunMessageRequest(StunMessageFlags.None, (client_endpoint as IPEndPoint)!, msg.RefrenceId);
                            var buff = req.GetBytes();

                            Logger.Log($"sending message to stun server endpoint: \"{remote_server_ip_endpoint_1.Address}\":\"{remote_server_ip_endpoint_1.Port}\"");
                            sock.SendTo(buff, remote_server_ip_endpoint_1);
                            break;
                        }

                    case StunMessageFlags.ChangePort:
                        {
                            var req = new StunMessageRequest(StunMessageFlags.None, (client_endpoint as IPEndPoint)!, msg.RefrenceId);
                            var buff = req.GetBytes();

                            Logger.Log($"sending message to stun server endpoint: \"{server_ip_endpoint_2.Address}\":\"{server_ip_endpoint_2.Port}\"");
                            sock.SendTo(buff, server_ip_endpoint_2);
                            break;
                        }

                    case StunMessageFlags.ChangeBoth:
                        {
                            var req = new StunMessageRequest(StunMessageFlags.None, (client_endpoint as IPEndPoint)!, msg.RefrenceId);
                            var buff = req.GetBytes();

                            Logger.Log($"sending message to stun server endpoint: \"{remote_server_ip_endpoint_2.Address}\":\"{remote_server_ip_endpoint_2.Port}\"");
                            sock.SendTo(buff, remote_server_ip_endpoint_2);
                            break;
                        }
                }
            }
        }
    }
}