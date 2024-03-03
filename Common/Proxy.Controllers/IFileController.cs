using Microsoft.AspNetCore.Mvc;

namespace Common.Proxy.Controllers
{
    public interface IFileController
    {
        [HttpGet]
        [Route("file/{filename}")]
        ActionResult DownLoadFile(string filename);
    }
}