using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileServerMaster.Web.Controllers
{
    public interface IMasterFileController
    {
        [HttpPost]
        [Route("file")]
        Task UploadAsync(IFormFile file);
    }
}