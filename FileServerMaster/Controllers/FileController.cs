using Common.Proxy.Controllers;
using FileServerMaster.Storage;
using FileServerMaster.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FileServerMaster.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase, IMasterFileController, IFileController
{
    private readonly ILogger<FileController> _logger;
    private readonly IWebSocketContainer _webSocketContainer;
    private readonly IFileContainer _fileContainer;
    private readonly IFileDistributorManager _fileDistributorManager;

    public FileController(ILogger<FileController> logger, IWebSocketContainer webSocketContainer, IFileContainer fileQueueContainer, IFileDistributorManager fileDistributorManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        _fileContainer = fileQueueContainer ?? throw new ArgumentNullException(nameof(fileQueueContainer));
        _fileDistributorManager = fileDistributorManager ?? throw new ArgumentNullException(nameof(fileDistributorManager));
    }

    [HttpPost]
    public async Task UploadAsync(IFormFile file)
    {
        _logger.LogInformation($"Recived file {file.FileName}");
        _fileContainer.Add(file);

        _fileDistributorManager.UpdateFileAvailablity(Request.Host, [file.FileName]);
        await _webSocketContainer.RequestCheckinAsync();
    }

    [HttpGet("{filename}")]
    public FileData DownLoadFile(string filename)
    {
        var file = _fileContainer.Get(filename);
        if (file == null)
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
            Content = GetBytes(file.OpenReadStream())
        };
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