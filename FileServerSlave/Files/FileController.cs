using Common.Proxy.Controllers;
using FileServerSlave.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FileServerSlave.Files
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase, IFileController
    {
        private readonly IFileManager _fileManager;

        public FileController(IFileManager fileManager)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        }

        // need to optimize this endpoints. use stream.
        [HttpGet("{filename}")]
        public byte[] DownLoadFile(string filename)
        {
            var file = _fileManager.GetFile(filename);
            if (file.Length == 0)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return [];
            }

            Response.ContentType = "image/jpg";
            return file;
        }
    }
}
