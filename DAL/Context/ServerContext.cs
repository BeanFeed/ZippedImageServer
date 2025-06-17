using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context;

public class ServerContext : DbContext
{
    public ServerContext() {}
    public ServerContext(DbContextOptions<ServerContext> options) : base(options) {}
    
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlite($"Data Source={Path.Join(Directory.GetCurrentDirectory(), "data", "zirdata.sqlite")}");
    }
}