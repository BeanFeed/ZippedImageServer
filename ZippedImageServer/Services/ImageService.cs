using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using ZippedImageServer.Models;
using ZippedImageServer.Models.Image;

namespace ZippedImageServer.Services;

public class ImageService(ServerContext context, KeyService keyService, IHttpContextAccessor httpContextAccessor)
{
    private HttpContext _httpContext = httpContextAccessor.HttpContext;

    private string _executablePath = AppContext.BaseDirectory;
    
    public async Task<byte[]> DownloadImage(string name, string category)
    {
        Image? image = await context.Images
            .FirstOrDefaultAsync(i => i.Name == name && i.CategoryName == category);

        if (image == null)
        {
            throw new KeyNotFoundException("Image not found");
        }
        
        
        
        string imagePath = Path.Combine(_executablePath, image.Category.Folder, image.Name);
        
        if (!File.Exists(imagePath)) throw new FileNotFoundException("Failed to find image file");
        
        var bytes = await File.ReadAllBytesAsync(imagePath);

        return bytes;
    }

    public async Task<Image> GetImage(string name, string category)
    {
        Image? image = await context.Images.FirstOrDefaultAsync(i => i.Name == name && i.CategoryName == category);
        
        if (image == null)
        {
            throw new KeyNotFoundException("Image not found");
        }
        
        return image;
    }
    
    public async Task<Image[]> GetImages(string category)
    {
        return await context.Images
            .Where(i => i.CategoryName == category)
            .ToArrayAsync();
    }

    public async Task UploadImage(UploadImageModel image)
    {
        Image newImage = new()
        {
            Name = image.File.FileName,
            CategoryName = image.Category,
        };
        
        Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Name == image.Category);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }
        
        FileStream newFileStream = new(
            Path.Combine(_executablePath, category.Folder, image.File.FileName),
            FileMode.Create);
        
        await image.File.CopyToAsync(newFileStream);
        
        await context.Images.AddAsync(newImage);
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteImage(string name, string category)
    {
        
    }
    
    public async Task<Category[]> GetCategories()
    {
        throw new NotImplementedException();
    }
    
    public async Task CreateCategory(CreateCategoryModel category)
    {
        throw new NotImplementedException();
    }
    
    public async Task DeleteCategory(string category)
    {
        throw new NotImplementedException();
    }
    
}