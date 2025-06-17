using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

public class Category
{
    [Key] public string Name { get; set; } = null!;
    public string Folder { get; set; } = null!;
}