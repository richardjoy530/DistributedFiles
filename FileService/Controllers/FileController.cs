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
    private readonly IFileQueueContainer _fileQueueContainer;

    public FileController(ILogger<FileController> logger, IWebSocketContainer webSocketContainer, IFileQueueContainer fileQueueContainer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webSocketContainer = webSocketContainer ?? throw new ArgumentNullException(nameof(webSocketContainer));
        _fileQueueContainer = fileQueueContainer ?? throw new ArgumentNullException(nameof(fileQueueContainer));
    }

    [HttpPost]
    public async Task UploadAsync(IFormFile file)
    {
        _logger.LogInformation($"Recived file {file.FileName}");
        _fileQueueContainer.EnQueue(file);
        await _webSocketContainer.RequestCheckinAsync();
    }

    [HttpGet]
    public IFormFile? DownloadFile()
    {
        var file = _fileQueueContainer.DeQueue();
        if (file == null)
        {
            Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        return file;
    }
}