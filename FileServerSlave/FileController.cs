using Microsoft.AspNetCore.Mvc;

namespace FileServerSlave
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        // need to optimize this endpoints. use stream.
        [HttpGet("{filename}")]
        public IActionResult DownLoadFile(string filename)
        {
            var filePath = Path.Combine("C:\\DistributedFiles", filename);
            if (System.IO.File.Exists(filePath))
            {

                var stream = System.IO.File.OpenRead(filePath);
                return File(GetBytes(stream), "image/jpg");
            }

            return NotFound();
        }

        private static byte[] GetBytes(Stream stream)
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

            return fileInBytes;
        }
    }
}
