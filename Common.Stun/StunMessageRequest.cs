using System.Net;

namespace Common.Stun
{
    public class StunMessageRequest
    {
        public readonly StunMessageFlags ChangeAddressFlag;

        public readonly IPAddress? ResponseAddress;

        public readonly Guid RefrenceId;

        public StunMessageRequest(StunMessageFlags changeAddressFlag, IPAddress? responseAddress)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseAddress = responseAddress;
            RefrenceId = Guid.NewGuid();
        }

        public StunMessageRequest(StunMessageFlags changeAddressFlag, IPAddress? responseAddress, Guid refrenceId)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseAddress = responseAddress;
            RefrenceId = refrenceId;
        }

        public static StunMessageRequest Parse(byte[] bytes)
        {
            var guid = new Guid(bytes.AsSpan(0, 16));
            Console.WriteLine(guid);

            var change_addr_flag = (StunMessageFlags)bytes[16];
            Console.WriteLine(change_addr_flag);
            
            var resp_addr_flag = (StunMessageFlags)bytes[17];
            Console.WriteLine(resp_addr_flag);

            IPAddress? ipaddr = null;
            if (resp_addr_flag == StunMessageFlags.ResponseAddress)
            {
                var ip_len = BitConverter.ToInt32(bytes.AsSpan(18, 4));
                ipaddr = new IPAddress(bytes.AsSpan(22, ip_len));
            }

            return new StunMessageRequest(change_addr_flag, ipaddr);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[1024];
            int len;
            if (ResponseAddress == null)
            {
                len = 16 + 1 + 1 + 4;
                bytes[17] = (byte)StunMessageFlags.None;
                bytes[18] = bytes[19] = bytes[20] = bytes[21] = 0;
            }
            else
            {
                ResponseAddress.TryWriteBytes(bytes.AsSpan(22), out var count);
                len = 16 + 1 + 1 + 4 + count;
                bytes[17] = (byte)StunMessageFlags.ResponseAddress;

                BitConverter.GetBytes(count).CopyTo(bytes, 18);
            }

            RefrenceId.ToByteArray().CopyTo(bytes.AsSpan(0, 16));

            bytes[16] = (byte)ChangeAddressFlag;

            return bytes.AsSpan(0, len).ToArray();
        }
    }
}
