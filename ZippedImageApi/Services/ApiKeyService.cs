using System.Net.Http.Headers;
using System.Net.Http.Json;
using ZippedImageApi.Models;

namespace ZippedImageApi.Services;

public class ApiKeyService
{
    private HttpClient _client;
    
    public ApiKeyService()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(Connection.Origin);
        client.DefaultRequestHeaders.Authorization = Connection.JwtBearerToken == null ? null : new AuthenticationHeaderValue("Bearer", Connection.JwtBearerToken);
        client.DefaultRequestHeaders.Add("ApiKey", Connection.ApiKey ?? string.Empty);
        _client = client;
    }
    
    public async Task<ApiKey[]> Get()
    {
        try
        {
            var response = await _client.GetAsync($"{BaseUrls.AdminKey}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve API keys.");

            return (await response.Content.ReadFromJsonAsync<ApiKey[]>())!;
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

    public async Task<string> Create(CreateKeyModel model)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"{BaseUrls.AdminKey}", model);
            if (response.IsSuccessStatusCode)
            {
                var key = await response.Content.ReadAsStringAsync();
                return key;
            }
            else
            {
                throw new Exception($"Failed to create API key: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (HttpRequestException e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public async Task Delete(int id)
    {
        try
        {
            var response = await _client.DeleteAsync($"{BaseUrls.AdminKey}?keyId={id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete API key: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (HttpRequestException e)
        {
            throw new Exception(e.Message);
        }
    }
}