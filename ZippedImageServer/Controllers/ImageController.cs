using Microsoft.AspNetCore.Mvc;

namespace ZippedImageServer.Controllers;

[ApiController]
[Route("~/[controller]")]
public class ImageController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get(string name, string category)
    {
        
        return Ok();
    }
}