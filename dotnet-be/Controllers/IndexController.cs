using Microsoft.AspNetCore.Mvc;

namespace auth_dotnet_api.Controllers;

[ApiController]
[Route("api/")]
public class IndexController : ControllerBase
{
    private readonly ILogger<IndexController> _logger;

    public IndexController(ILogger<IndexController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "Welcome to the API!" });
    }
}