using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using ZippedImageServer.Models;

namespace ZippedImageServer.Services;

public class KeyService(ServerContext context)
{
    public async Task<ApiKey[]> GetKeys(int? id)
    {
        if (id.HasValue)
        {
            var key = await context.ApiKeys
                .Include(k => k.Category)
                .AsNoTrackingWithIdentityResolution()
                .FirstOrDefaultAsync(k => k.Id == id.Value);
            if(key == null) 
            {
                throw new Exception("Key not found.");
            }

            key.Key = "";
            return [key];
        }
        else
        {
            //return all keys but remove the actual key value from the response
            return await context.ApiKeys
                .Include(k => k.Category)
                .AsNoTrackingWithIdentityResolution()
                .Select(k => new ApiKey
                {
                    Id = k.Id,
                    CategoryName = k.CategoryName,
                    Category = k.Category,
                    Description = k.Description,
                    CreatedAt = k.CreatedAt,
                    Key = "" // Hide the actual key value
                })
                .ToArrayAsync();
        }
    }

    public async Task<string> CreateKey(CreateKeyModel key)
    {
        Category? category = await context.Categories.FirstOrDefaultAsync(x => x.Name == key.Category);
        if (category == null)
        {
            throw new Exception("Category not found.");
        }
        
        var generatedKey = Guid.NewGuid().ToString(); // Generate a new key
        var newKey = new ApiKey
        {
            CategoryName = key.Category,
            Description = key.Description,
            CreatedAt = DateTime.UtcNow,
            Key =  BCrypt.Net.BCrypt.HashPassword(generatedKey),
        };
        
        await context.ApiKeys.AddAsync(newKey);
        
        await context.SaveChangesAsync();
        
        return generatedKey; // Return the generated key
    }
    
    public async Task DeleteKey(int id)
    {
        var key = await context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id);
        
        if (key == null)
        {
            throw new Exception("Key not found.");
        }
        
        context.ApiKeys.Remove(key);
        
        await context.SaveChangesAsync();
    }
}