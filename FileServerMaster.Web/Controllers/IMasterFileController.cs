using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileServerMaster.Web.Controllers
{
    public interface IMasterFileController
    {
        [HttpPost]
        [Route("file")]
        void UploadAsync(IFormFile file);
    }
}