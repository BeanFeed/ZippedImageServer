using System.Text.Json.Serialization;

namespace ZippedImageApi.Models;

public class ApiKey
{
    public int Id { get; set; }
    public string Key { get; set; } = null!;
    [JsonIgnore]
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Category Category { get; set; } = null!;
}