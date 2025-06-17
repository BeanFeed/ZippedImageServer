using Microsoft.AspNetCore.Mvc;
using ZippedImageServer.Models;
using ZippedImageServer.Services;

namespace ZippedImageServer.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        try
        {
            var token = await authService.Login(model);
            return Ok(token);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
}