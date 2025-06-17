using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL.Entities;

public class Image
{
    [Key] public string Name { get; set; } = null!;
    [JsonIgnore]
    [ForeignKey(nameof(Category))]
    public string CategoryName { get; set; } = null!;
    public Category Category { get; set; } = null!;
}