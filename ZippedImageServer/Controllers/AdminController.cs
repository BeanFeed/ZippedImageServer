using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZippedImageServer.Models;

namespace ZippedImageServer.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("~/admin/image")]
public class AdminImageController : ControllerBase
{
    [HttpGet]
    [Route("getimages")]
    public async Task<IActionResult> GetImages(string? category)
    {
        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetImage(string name, string category)
    {
        // Implementation for getting images
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        // Implementation for uploading an image
        return Ok();
    }
    
    [HttpDelete]
    public async Task<IActionResult> DeleteImage(string name, string category)
    {
        // Implementation for deleting an image
        return Ok();
    }
}

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("~/admin/categories")]
public class AdminCategoryController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        // Implementation for getting categories
        return Ok();
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryModel category)
    {
        // Implementation for creating a category
        return Ok();
    }
    
    [HttpDelete("{category}")]
    public async Task<IActionResult> DeleteCategory(string category)
    {
        // Implementation for deleting a category
        return Ok();
    }
}

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("~/admin/keys")]
public class AdminKeyController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetKeys()
    {
        // Implementation for getting keys
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateKey(CreateKeyModel key)
    {
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteKey(int keyId)
    {
        return Ok();
    }
}