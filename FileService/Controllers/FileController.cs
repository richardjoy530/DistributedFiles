using System.Net.WebSockets;
using System.Text;
using Backend.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController(ILogger<FileController> logger) : ControllerBase
{
    [HttpPost]
    public bool Upload(IFormFile file)
    {
        Storage.Container.Files.Enqueue(file);
        return true;
    }
}