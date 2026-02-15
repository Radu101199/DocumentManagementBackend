using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Test endpoint called");
        _logger.LogWarning("This is a warning");
        _logger.LogError("This is an error");
        
        return Ok(new { message = "Logging is working!" });
    }
}