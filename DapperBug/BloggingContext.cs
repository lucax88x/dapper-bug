using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DapperBug;

public class BloggingContext : DbContext
{
    public DbSet<Post> Posts { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql("Host=localhost;Database=test;Username=demo;Password=demo;");
        options.LogTo(Console.WriteLine);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.Content)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PostContent>(v));
        });
    }
}