using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZippedImageServer.Models;
using ZippedImageServer.Models.Image;
using ZippedImageServer.Services;

namespace ZippedImageServer.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("~/admin/image")]
public class AdminImageController(ImageService imageService) : ControllerBase
{
    [HttpGet]
    [Route("getimages")]
    public async Task<IActionResult> GetImages(string? category)
    {
        try
        {
            return Ok(await imageService.GetImages(category));
        } catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetImage(string name, string category)
    {
        try
        {
            return Ok(await imageService.GetImage(name, category));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageModel model)
    {
        try
        {
            await imageService.UploadImage(model);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete]
    public async Task<IActionResult> DeleteImage(string name, string category)
    {
        try
        {
            await imageService.DeleteImage(name, category);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("~/admin/categories")]
public class AdminCategoryController(ImageService imageService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await imageService.GetCategories());
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryModel category)
    {
        try
        {
            await imageService.CreateCategory(category);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete]
    public async Task<IActionResult> DeleteCategory(string category)
    {
        try
        {
            await imageService.DeleteCategory(category);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("~/admin/keys")]
public class AdminKeyController(KeyService keyService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetKeys()
    {
        return Ok(await keyService.GetKeys(null));
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