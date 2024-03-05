using Dapper;
using DapperBug;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;

async Task SeedDb()
{
    Console.WriteLine("Initializing DB");

    await using (var dbContext = new BloggingContext())
    {
        if (dbContext.Posts.Any())
        {
            Console.WriteLine("Purge existing data");

            dbContext.RemoveRange(dbContext.Posts);

            await dbContext.SaveChangesAsync();
        }
    }

    await using (var dbContext = new BloggingContext())
    {
        Console.WriteLine("Insert fake data");

        for (var i = 1; i <= 100; i++)
        {
            await dbContext.AddRangeAsync(new Post
            {
                PostId = i,
                Content = new PostContent(i.ToString(), i),
                Title = $"some-title-{i + 1}"
            });
        }

        await dbContext.SaveChangesAsync();

        Console.WriteLine("DB Seed done");
    }
}
        ConfigureJson();

        var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new BloggingContext());
var app = builder.Build();

// await SeedDb();

app.MapGet("/", () => "use /posts");
app.MapGet("/posts", (BloggingContext dbContext) => dbContext.Posts.ToListAsync());
app.MapGet("/test", async () =>
{
    await using var conn = new NpgsqlConnection("Host=localhost;Database=test;Username=demo;Password=demo;");

    var content = await conn.QueryFirstOrDefaultAsync<string>("select \"Content\" from public.\"Posts\"");

    return content;
});

app.Run();

void ConfigureJson()
{
#pragma warning disable CS0618 // Type or member is obsolete
    // NpgsqlConnection.GlobalTypeMapper.UseJsonNet();
#pragma warning restore CS0618 // Type or member is obsolete
}
