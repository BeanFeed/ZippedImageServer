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
    
    public async Task<byte[]> DownloadImage(string name, string category, string? apiKey = null)
    {
        Image? image = await context.Images
            .FirstOrDefaultAsync(i => i.Name == name && i.CategoryName == category);
        
        if (image == null)
        {
            throw new KeyNotFoundException("Image not found");
        }

        if (apiKey != null)
        {
            ApiKey[] keys = await context.ApiKeys.Where(i => i.Category.Name == category).ToArrayAsync();
            if (keys.Length == 0)
            {
                throw new KeyNotFoundException("API key not found for this category");
            }
            
            bool keyExists = false;
            
            foreach (var key in keys)
            {
                if (BCrypt.Net.BCrypt.Verify(apiKey, key.Key))
                {
                    keyExists = true;
                    break;
                }
            }
            
            if (!keyExists)
            {
                throw new UnauthorizedAccessException("Invalid API key");
            }
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
    
    public async Task<Image[]> GetImages(string? category)
    {
        if (string.IsNullOrEmpty(category))
        {
            return await context.Images
                .Include(i => i.Category)
                .ToArrayAsync();
        }
        else
        {
            Category? cat = await context.Categories.FirstOrDefaultAsync(c => c.Name == category);
            if (cat == null)
            {
                throw new KeyNotFoundException("Category not found");
            }
            
            return await context.Images
                .Where(i => i.CategoryName == category)
                .Include(i => i.Category)
                .ToArrayAsync();
        }
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
        Image? image = await context.Images.FirstOrDefaultAsync(i => i.Name == name && i.CategoryName == category);

        if (image == null)
        {
            throw new KeyNotFoundException("Image not found");
        }
        
        var filePath = Path.Combine(_executablePath, image.Category.Folder, image.Name);

        if (File.Exists(filePath)) File.Delete(filePath);
            else throw new FileNotFoundException("Failed to find image file");
        
        context.Images.Remove(image);
        
        await context.SaveChangesAsync();
    }
    
    public async Task<Category[]> GetCategories()
    {
        return await context.Categories.ToArrayAsync();
    }
    
    public async Task CreateCategory(CreateCategoryModel category)
    {
        category.Folder = category.Folder.Replace("..", ".");
        Category? existingFolder = await context.Categories.FirstOrDefaultAsync(c => c.Folder == category.Folder);
        
        if (existingFolder != null)
            throw new KeyNotFoundException("Folder already in use");
        
        Directory.CreateDirectory(Path.Join(_executablePath, category.Folder));
        
        Category newCategory = new()
        {
            Name = category.Name,
            Folder = category.Folder
        };
        
        await context.Categories.AddAsync(newCategory);
    }
    
    public async Task DeleteCategory(string category)
    {
        Category? existingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Folder == category);
        
        if (existingCategory == null)
            throw new KeyNotFoundException("Category not found");
        
        Image[] images = await context.Images.Where(i => i.CategoryName == category).ToArrayAsync();

        foreach (var image in images)
        {
            await DeleteImage(image.Name, category);
        }
        
        Directory.Delete(Path.Join(_executablePath, existingCategory.Folder));
        
        context.Categories.Remove(existingCategory);
        
        await context.SaveChangesAsync();
    }
    
}