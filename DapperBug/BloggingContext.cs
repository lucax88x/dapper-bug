using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DapperBug;

public class BloggingContext : DbContext
{
    public DbSet<Post> Posts { get; set; }

    public BloggingContext(DbContextOptions<BloggingContext> opts)
        : base(opts) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity
                .Property(e => e.Content)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<PostContent>(v)
                );
        });
    }
}
