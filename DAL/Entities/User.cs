using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

public class User
{
    [Key] public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}