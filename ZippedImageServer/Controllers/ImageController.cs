using Microsoft.AspNetCore.Mvc;
using ZippedImageServer.Services;

namespace ZippedImageServer.Controllers;

[ApiController]
[Route("~/[controller]")]
public class ImageController(ImageService imageService) : ControllerBase
{
    [HttpGet]
    [Route("{category}/{name}")]
    public async Task<ActionResult> Get([FromHeader] string apikey, string name, string category)
    {
        try
        {
            FileStream stream = await imageService.DownloadImage(name, category, apikey, true);
            return File(stream, "application/octet-stream", name);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}