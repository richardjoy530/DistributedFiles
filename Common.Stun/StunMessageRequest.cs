using System.Net;

namespace Common.Stun
{
    public class StunMessageRequest
    {
        public readonly StunMessageFlags ChangeAddressFlag;

        public readonly IPEndPoint? ResponseEndpoint;

        public readonly Guid ReferenceId;

        public StunMessageRequest(StunMessageFlags changeAddressFlag)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = null;
            ReferenceId = Guid.NewGuid();
        }

        private StunMessageRequest(StunMessageFlags changeAddressFlag, Guid referenceId)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = null;
            ReferenceId = referenceId;
        }

        public StunMessageRequest(StunMessageFlags changeAddressFlag, IPEndPoint? responseAddress, Guid referenceId)
        {
            ChangeAddressFlag = changeAddressFlag;
            ResponseEndpoint = responseAddress;
            ReferenceId = referenceId;
        }

        public static StunMessageRequest Parse(byte[] bytes)
        {
            var guid = new Guid(bytes.AsSpan(0, 16));
            Logger.Log($"id: \"{guid}\"");

            var changeAddrFlag = (StunMessageFlags)bytes[16];
            Logger.Log($"change_address: \"{changeAddrFlag}\"");

            var respAddrFlag = (StunMessageFlags)bytes[17];
            Logger.Log($"resp_addr_flag: \"{respAddrFlag}\"");

            if (respAddrFlag != StunMessageFlags.ResponseAddress)
            {
                return new StunMessageRequest(changeAddrFlag, guid);
            }

            var ipLen = BitConverter.ToInt32(bytes.AsSpan(18, 4));
            var port = BitConverter.ToInt32(bytes.AsSpan(22, 4));
            var ipaddr = new IPAddress(bytes.AsSpan(26, ipLen));
            var ipEndpoint = new IPEndPoint(ipaddr, port);
            Logger.Log($"response_endpoint: \"{ipaddr}\":\"{port}\"");

            return new StunMessageRequest(changeAddrFlag, ipEndpoint, guid);
        }

        public byte[] GetBytes()
        {
            var bytes = new byte[1024];
            int respLen;
            if (ResponseEndpoint == null)
            {
                respLen = 16 + 1 + 1;
                bytes[17] = (byte)StunMessageFlags.None; // setting response address flag = None
            }
            else
            {
                ResponseEndpoint.Address.TryWriteBytes(bytes.AsSpan(26), out var ipLen); // writing ip address in bytes
                BitConverter.GetBytes(ipLen).CopyTo(bytes, 18); // writing length of bytes for ip address in bytes
                BitConverter.GetBytes(ResponseEndpoint.Port).CopyTo(bytes, 22); // writing port in bytes
             
                respLen = 16 + 1 + 1 + 4 + 4 + ipLen;
                bytes[17] = (byte)StunMessageFlags.ResponseAddress; // setting response address flag = ResponseAddress
            }

            ReferenceId.ToByteArray().CopyTo(bytes.AsSpan(0, 16));

            bytes[16] = (byte)ChangeAddressFlag;

            return bytes.AsSpan(0, respLen).ToArray();
        }
    }
}