using Microsoft.AspNetCore.Mvc;

namespace Common.Proxy.Controllers
{
    public interface IFileController
    {
        [Route("file/{filename}")]
        HttpResponseMessage? DownloadFileStream(string filename);
    }
}