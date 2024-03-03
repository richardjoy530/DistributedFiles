using System.Net;

namespace Common.Stun
{
    public class StunMessageResponse
    {
        public readonly IPEndPoint ClientReflexiveEndpoint;

        public readonly Guid ReferenceId;

        public StunMessageResponse(IPEndPoint reflexiveAddress, Guid referenceId)
        {
            ClientReflexiveEndpoint = reflexiveAddress;
            ReferenceId = referenceId;
        }

        public static StunMessageResponse Parse(byte[] bytes) 
        {
            var guid = new Guid(bytes.AsSpan(8, 16));
            //Logger.Log($"id: \"{guid}\"");

            var port = BitConverter.ToInt32(bytes.AsSpan(4, 4));
            var ipLen = BitConverter.ToInt32(bytes.AsSpan(0, 4));

            var ipaddr = new IPAddress(bytes.AsSpan(24, ipLen));
            var endpoint = new IPEndPoint(ipaddr, port);
            Logger.Log($"response_endpoint: \"{ipaddr}\":\"{port}\"");

            return new StunMessageResponse(endpoint, guid);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[1024];

            ClientReflexiveEndpoint.Address.TryWriteBytes(bytes.AsSpan(24), out var ipLen);

            BitConverter.GetBytes(ipLen).CopyTo(bytes, 0);
            ReferenceId.ToByteArray().CopyTo(bytes.AsSpan(8, 16));
            BitConverter.GetBytes(ClientReflexiveEndpoint.Port).CopyTo(bytes, 4);

            var respLen = 4 + 4 + 16 + ipLen;
            return bytes.AsSpan(0, respLen).ToArray();
        }
    }
}