using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { environment = env.EnvironmentName });
    }
}
