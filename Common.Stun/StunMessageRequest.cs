using System.Net;

namespace Common.Stun
{
    public class StunMessageRequest
    {
        public readonly StunMessageFlags ChangeAddressFlag;

        public readonly IPEndPoint? ResponseEndpoint;

        public readonly Guid RefrenceId;

        public StunMessageRequest(StunMessageFlags changeAddressFlag)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = null;
            RefrenceId = Guid.NewGuid();
        }

        public StunMessageRequest(StunMessageFlags changeAddressFlag, Guid refrenceId)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = null;
            RefrenceId = refrenceId;
        }

        public StunMessageRequest(StunMessageFlags changeAddressFlag, IPEndPoint? responseAddress)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = responseAddress;
            RefrenceId = Guid.NewGuid();
        }

        public StunMessageRequest(StunMessageFlags changeAddressFlag, IPEndPoint? responseAddress, Guid refrenceId)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = responseAddress;
            RefrenceId = refrenceId;
        }

        public static StunMessageRequest Parse(byte[] bytes)
        {
            var guid = new Guid(bytes.AsSpan(0, 16));
            Logger.Log($"id: \"{guid}\"");

            var change_addr_flag = (StunMessageFlags)bytes[16];
            Logger.Log($"change_address: \"{change_addr_flag}\"");

            var resp_addr_flag = (StunMessageFlags)bytes[17];
            Logger.Log($"resp_addr_flag: \"{resp_addr_flag}\"");

            if (resp_addr_flag == StunMessageFlags.ResponseAddress)
            {
                var ip_len = BitConverter.ToInt32(bytes.AsSpan(18, 4));
                var port = BitConverter.ToInt32(bytes.AsSpan(22, 4));
                var ipaddr = new IPAddress(bytes.AsSpan(26, ip_len));
                var ip_endpoint = new IPEndPoint(ipaddr, port);
                Logger.Log($"response_endpoint: \"{ipaddr}\":\"{port}\"");

                return new StunMessageRequest(change_addr_flag, ip_endpoint, guid);
            }

            return new StunMessageRequest(change_addr_flag, guid);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[1024];
            int resp_len;
            if (ResponseEndpoint == null)
            {
                resp_len = 16 + 1 + 1;
                bytes[17] = (byte)StunMessageFlags.None; // setting response address flag = None
            }
            else
            {
                ResponseEndpoint.Address.TryWriteBytes(bytes.AsSpan(26), out var ip_len); // writing ip address in bytes
                BitConverter.GetBytes(ip_len).CopyTo(bytes, 18); // writing length of bytes for ip address in bytes
                BitConverter.GetBytes(ResponseEndpoint.Port).CopyTo(bytes, 22); // writing port in bytes
             
                resp_len = 16 + 1 + 1 + 4 + 4 + ip_len;
                bytes[17] = (byte)StunMessageFlags.ResponseAddress; // setting response address flag = ResponseAddress
            }

            RefrenceId.ToByteArray().CopyTo(bytes.AsSpan(0, 16));

            bytes[16] = (byte)ChangeAddressFlag;

            return bytes.AsSpan(0, resp_len).ToArray();
        }
    }
}
