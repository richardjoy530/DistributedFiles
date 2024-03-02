using System.Net.Sockets;
using System.Net;
using Common.Stun;

namespace StunServer
{
    internal class Program
    {
        private const int PrimaryPort = 6666;
        private const int SecondaryPort = 7777;

        public static void Main(string[] args)
        {
            var server_ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            Console.WriteLine($"stun server running on: \"{server_ip}\"");
            var remote_server_ip = IPAddress.Parse(args[0]);
            Console.WriteLine($"remote stun server on: \"{server_ip}\"");

            var primary = Run(true, server_ip, remote_server_ip);
            var secondary = Run(false, server_ip, remote_server_ip);

            Task.WaitAll(primary, secondary);
        }

        private static async Task Run(bool is_primary, IPAddress server_ip, IPAddress remote_server_ip)
        {
            // container variable for saving the stun client's endpoint after reciving message
            var client_ip_endpoint = new IPEndPoint(IPAddress.Any, 0);

            // endpoint that host's stun server on this process
            var server_ip_endpoint_1 = new IPEndPoint(server_ip, is_primary ? PrimaryPort : SecondaryPort);
            var server_ip_endpoint_2 = new IPEndPoint(server_ip, is_primary ? SecondaryPort : PrimaryPort);

            // stun protocal needs 2 servers with different ips, these are the remote stun server
            var remote_server_ip_endpoint_1 = new IPEndPoint(remote_server_ip, is_primary ? PrimaryPort : SecondaryPort);
            var remote_server_ip_endpoint_2 = new IPEndPoint(remote_server_ip, is_primary ? SecondaryPort : PrimaryPort);

            // creating a UPD socket
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(server_ip_endpoint_1);
            Console.WriteLine($"stun binding sucucssfull on: \"{server_ip_endpoint_1.Address}\":\"{server_ip_endpoint_1.Port}\"");

            while (true)
            {
                var buffer = new byte[1024];

                // listening for a StunMessageRequest. this can come from a stun client or anoter remote stun server.
                //var client_endpoint = (EndPoint)client_ip_endpoint;
                await sock.ReceiveFromAsync(new ArraySegment<byte>(buffer), client_ip_endpoint); // NOT WORKING .. not able to retrive client endpoint
                Console.WriteLine($"recived message from endpoint: \"{client_ip_endpoint!.Address}\":\"{client_ip_endpoint.Port}\"");

                var msg = StunMessageRequest.Parse(buffer);

                switch (msg.ChangeAddressFlag)
                {
                    case StunMessageFlags.None:
                        {
                            var resp = new StunMessageResponse(client_ip_endpoint!, msg.RefrenceId);
                            var buff = resp.GetBytes();

                            Console.WriteLine($"sending message to client endpoint: \"{client_ip_endpoint!.Address}\":\"{client_ip_endpoint.Port}\"");
                            await sock.SendToAsync(buff, client_ip_endpoint);
                            break;
                        }

                    case StunMessageFlags.ChangeIp:
                        {
                            var req = new StunMessageRequest(StunMessageFlags.None, client_ip_endpoint, msg.RefrenceId);
                            var buff = req.GetBytes();

                            Console.WriteLine($"sending message to stun server endpoint: \"{remote_server_ip_endpoint_1.Address}\":\"{remote_server_ip_endpoint_1.Port}\"");
                            await sock.SendToAsync(buff, remote_server_ip_endpoint_1);
                            break;
                        }

                    case StunMessageFlags.ChangePort:
                        {
                            var req = new StunMessageRequest(StunMessageFlags.None, client_ip_endpoint, msg.RefrenceId);
                            var buff = req.GetBytes();

                            Console.WriteLine($"sending message to stun server endpoint: \"{server_ip_endpoint_2.Address}\":\"{server_ip_endpoint_2.Port}\"");
                            await sock.SendToAsync(buff, server_ip_endpoint_2);
                            break;
                        }

                    case StunMessageFlags.ChangeBoth:
                        {
                            var req = new StunMessageRequest(StunMessageFlags.None, client_ip_endpoint, msg.RefrenceId);
                            var buff = req.GetBytes();

                            Console.WriteLine($"sending message to stun server endpoint: \"{remote_server_ip_endpoint_2.Address}\":\"{remote_server_ip_endpoint_2.Port}\"");
                            await sock.SendToAsync(buff, remote_server_ip_endpoint_2);
                            break;
                        }
                }
            }
        }
    }
}