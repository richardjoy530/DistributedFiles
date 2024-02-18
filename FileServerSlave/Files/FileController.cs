using Common.Proxy.Controllers;
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
        public FileData DownLoadFile(string filename)
        {
            var file = _fileManager.GetFile(filename);
            if (file.Length == 0)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return new FileData
                {
                    FileName = string.Empty,
                    Content = []
                };
            }

            Response.ContentType = "image/jpg";
            return new FileData
            {
                FileName = filename,
                Content = file
            };
        }
    }
}
