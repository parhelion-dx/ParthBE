using Microsoft.AspNetCore.Mvc;

namespace ParthBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    // GET /api/Hello
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Our backend is running");
    }
}
