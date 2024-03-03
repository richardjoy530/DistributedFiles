using System.Net;

namespace Common.Stun
{
    public class StunMessageResponse
    {
        public readonly IPEndPoint ClientReflexiveEndpoint;

        public readonly Guid RefrenceId;

        public StunMessageResponse(IPEndPoint reflexiveAddress, Guid refrenceId)
        {
            ClientReflexiveEndpoint = reflexiveAddress;
            RefrenceId = refrenceId;
        }

        public static StunMessageResponse Parse(byte[] bytes) 
        {
            var guid = new Guid(bytes.AsSpan(8, 16));
            //Logger.Log($"id: \"{guid}\"");

            var port = BitConverter.ToInt32(bytes.AsSpan(4, 4));
            var ip_len = BitConverter.ToInt32(bytes.AsSpan(0, 4));

            var ipaddr = new IPAddress(bytes.AsSpan(24, ip_len));
            var endpoint = new IPEndPoint(ipaddr, port);
            Logger.Log($"response_endpoint: \"{ipaddr}\":\"{port}\"");

            return new StunMessageResponse(endpoint, guid);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[1024];

            ClientReflexiveEndpoint.Address.TryWriteBytes(bytes.AsSpan(24), out var ip_len);

            BitConverter.GetBytes(ip_len).CopyTo(bytes, 0);
            RefrenceId.ToByteArray().CopyTo(bytes.AsSpan(8, 16));
            BitConverter.GetBytes(ClientReflexiveEndpoint.Port).CopyTo(bytes, 4);

            var resp_len = 4 + 4 + 16 + ip_len;
            return bytes.AsSpan(0, resp_len).ToArray();
        }
    }
}
