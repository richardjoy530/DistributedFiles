using System.Net;

namespace Common.Stun
{
    public class StunMessageResponse // todo pass in the guid from request
    {
        public readonly IPAddress MappedAddress;

        public StunMessageResponse(IPAddress mappedAddress)
        {
            MappedAddress = mappedAddress;
        }

        public static StunMessageResponse Parse(byte[] bytes) 
        {
            var len = BitConverter.ToInt32(bytes.AsSpan(0, 4));

            var ip = new IPAddress(bytes.AsSpan(4, len));

            return new StunMessageResponse(ip);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[1024];

            MappedAddress.TryWriteBytes(bytes.AsSpan(4), out var len);

            BitConverter.GetBytes(len).CopyTo(bytes, 0);
            return bytes.AsSpan(0, 4 + len).ToArray();
        }
    }
}
