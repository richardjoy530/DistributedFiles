using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Web.Controllers
{
    public interface IFileController
    {
        [HttpGet]
        [Route("file/{filename}")]
        IFormFile? DownloadFile(string filename);

        [HttpPost]
        [Route("file")]
        Task UploadAsync(IFormFile file);
    }
}