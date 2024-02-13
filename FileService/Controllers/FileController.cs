using Backend.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly ILogger<FileController> _logger;

    public FileController(ILogger<FileController> logger)
    {
        _logger = logger;
    }

    [HttpPost()]
    public bool Upload(IFormFile file)
    {
        TempFileQueue.Files.Enqueue(file);
        return true;
    }
}