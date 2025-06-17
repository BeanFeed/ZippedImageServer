using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DAL.Entities;

[PrimaryKey(nameof(Id))]
public class ApiKey
{
    public int Id { get; set; }
    public required string Key { get; set; } = null!;
    [JsonIgnore]
    [ForeignKey(nameof(Category))]
    public required string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Category Category { get; set; } = null!;
}