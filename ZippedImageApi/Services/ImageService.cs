using System.Net.Http.Headers;
using System.Net.Http.Json;
using ZippedImageApi.Models;

namespace ZippedImageApi.Services;

public class ImageService
{
    private HttpClient _client;
    
    public ImageService()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(Connection.Origin);
        client.DefaultRequestHeaders.Authorization = Connection.JwtBearerToken == null ? null : new AuthenticationHeaderValue("Bearer", Connection.JwtBearerToken);
        client.DefaultRequestHeaders.Add("ApiKey", Connection.ApiKey ?? string.Empty);
        _client = client;
    }
    
    public async Task UserDownloadImage(string name, string category, string outputPath)
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrls.Image}/{category}/{name}");

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
        catch (Exception e)
        {
            if (e is HttpRequestException he)
            {
                throw new Exception(e.Message);
            }
            else throw;
        }
    }

    public async Task DownloadImage(string category, string name, string outputPath)
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrls.AdminImage}/download/{category}/{name}");

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
        catch (Exception e)
        {
            if (e is HttpRequestException he)
            {
                throw new Exception(e.Message);
            }
            else throw;
        }
    }
    
    public async Task<Image[]> GetImages(string? category)
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrls.AdminKey}/getimages?category={category}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve Images: " + await response.Content.ReadAsStringAsync());

            return (await response.Content.ReadFromJsonAsync<Image[]>())!;
        }
        catch (Exception e)
        {
            if (e is HttpRequestException he)
            {
                throw new Exception(e.Message);
            }
            else return [];
        }
    }
    
    public async Task<Image> GetImage(string name, string category)
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrls.AdminKey}/{category}/{name}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve Image: " + await response.Content.ReadAsStringAsync());

            return (await response.Content.ReadFromJsonAsync<Image>())!;
        }
        catch (Exception e)
        {
            if (e is HttpRequestException he)
            {
                throw new Exception(e.Message);
            }
            else throw;
        }
    }
    
    public async Task UploadImage(string category, FileStream file)
    {
        try
        {
            //create a multipart form data content that has a Category and a File
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(category), "category");
            content.Add(new StreamContent(file), "file", file.Name);
            
            var response = await _client.PostAsync($"{BaseUrls.AdminKey}", content);
            
            if(!response.IsSuccessStatusCode) throw new Exception("Failed to upload Image: " + await response.Content.ReadAsStringAsync());
        }
        catch (Exception e)
        {
            if (e is HttpRequestException he)
            {
                throw new Exception(e.Message);
            }
            else throw;
        }
    }

    public async Task DeleteImage(string name, string category)
    {
        try
        {
            var response = await _client.DeleteAsync($"{BaseUrls.AdminImage}?category={category}&name={name}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete Image: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (HttpRequestException e)
        {
            throw new Exception(e.Message);
        }
    }
}