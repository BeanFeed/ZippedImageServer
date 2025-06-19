using System.Net.Http.Headers;
using System.Net.Http.Json;
using ZippedImageApi.Models;

namespace ZippedImageApi.Services;

public class CategoryService
{
    private HttpClient _client;
    
    public CategoryService()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(Connection.Origin);
        client.DefaultRequestHeaders.Authorization = Connection.JwtBearerToken == null ? null : new AuthenticationHeaderValue("Bearer", Connection.JwtBearerToken);
        client.DefaultRequestHeaders.Add("ApiKey", Connection.ApiKey ?? string.Empty);
        _client = client;
    }
    
    public async Task<Category[]> Get()
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrls.AdminCategory}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to get Categories.");

            return (await response.Content.ReadFromJsonAsync<Category[]>())!;
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
    
    public async Task Create(CreateCategoryModel model)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"{BaseUrls.AdminCategory}", model);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create Category: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (HttpRequestException e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public async Task Delete(string category)
    {
        try
        {
            var response = await _client.DeleteAsync($"{BaseUrls.AdminCategory}?category={category}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete Category: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (HttpRequestException e)
        {
            throw new Exception(e.Message);
        }
    }
}