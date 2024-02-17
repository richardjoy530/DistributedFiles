using Backend.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
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
    public IFormFile? DownloadFile(string filename)
    {
        var file = _fileContainer.Get(filename);
        if (file == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        return file;
    }
}