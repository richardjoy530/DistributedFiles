using Microsoft.AspNetCore.Mvc;

namespace Common.Proxy.Controllers
{
    public interface IFileController
    {
        [HttpGet]
        [Route("file/{filename}")]
        FileData? DownLoadFile(string filename);
    }
}