using System.Text;

namespace Common
{
    public static class StreamUtils
    {
        public static string GetBytes(this Stream stream)
        {
            var streamLength = (int)stream.Length; // total number of bytes read
            var numBytesReadPosition = 0; // actual number of bytes read
            var fileInBytes = new byte[streamLength];

            while (streamLength > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                var n = stream.Read(fileInBytes, numBytesReadPosition, streamLength);
                // Break when the end of the file is reached.
                if (n == 0)
                    break;
                numBytesReadPosition += n;
                streamLength -= n;
            }

            return fileInBytes.Base64Encode();
        }

        public static string Base64Encode(this byte[] bytes)
        {
            return Encoding.Unicode.GetString(bytes);
        }

        public static byte[] Base64Decode(this string base64string)
        {
            return Encoding.Unicode.GetBytes(base64string);
        }
    }
}
