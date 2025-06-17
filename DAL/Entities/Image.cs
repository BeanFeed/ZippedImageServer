using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Entities;

public class Image
{
    [Key] public string Name { get; set; } = null!;
    [ForeignKey(nameof(Category))]
    public required string CategoryName { get; set; } = null!;
    public Category Category { get; set; } = null!;
}