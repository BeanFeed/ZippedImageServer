using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ZippedImageApi.Services;

public class AuthService
{
    private HttpClient _client;
    
    public AuthService()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(Connection.Origin);
        client.DefaultRequestHeaders.Authorization = Connection.JwtBearerToken == null ? null : new AuthenticationHeaderValue("Bearer", Connection.JwtBearerToken);
        client.DefaultRequestHeaders.Add("ApiKey", Connection.ApiKey ?? string.Empty);
        _client = client;
    }
    
    public async Task<string> Login(string username, string password)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"{BaseUrls.Authentication}/login", new { Username = username, Password = password });
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }
            else
            {
                throw new Exception($"Login failed: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (HttpRequestException e)
        {
            throw new Exception(e.Message);
        }
    }
}