using Common;
using Common.Proxy.Controllers;
using FileServerMaster.Storage;
using FileServerMaster.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Web;

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
        _logger.LogInformation("recived file \"{filename}\"", file.FileName);
        _fileContainer.Add(file);

        _fileDistributorManager.UpdateFileAvailablity(Request.Host, [file.FileName]);
        await _webSocketContainer.RequestCheckInAllAsync();
    }

    [HttpGet("{filename}")]
    public FileData? DownLoadFile(string filename)
    {
        filename = HttpUtility.UrlDecode(filename);
        var file = _fileContainer.Get(filename);
        if (file == null)
        {
            _logger.LogInformation("\"{}\" was not present in the file container", filename);
            return null;
        }

        _logger.LogInformation("\"{}\" was downloaded", filename);
        return file;
    }
}