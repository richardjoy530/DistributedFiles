using System.Runtime.InteropServices;

namespace Common.Proxy.Controllers
{
    public interface IFileController
    {
        [HttpGet]
        [Route("file/{filename}")]
        byte[] DownLoadFile(string filename);
    }
}