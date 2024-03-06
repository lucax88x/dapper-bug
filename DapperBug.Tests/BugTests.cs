using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace DapperBug.Tests;

public class BugTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string? _connectionString;

    public async Task InitializeAsync()
    {
        testOutputHelper.WriteLine("Starting.. (takes time to download docker stuff etc");

        _container = new PostgreSqlBuilder().Build();

        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        testOutputHelper.WriteLine("DB up");
    }

    public Task DisposeAsync()
    {
        return _container!.StopAsync();
    }

    [Fact]
    public async Task TestMigration()
    {
        var sp = BuildIoc();
        await MigrateDb(sp);
        await SeedDb(sp);

#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.UseJsonNet();
#pragma warning restore CS0618 // Type or member is obsolete

        var task = async () =>
        {
            // await using var conn = new NpgsqlConnection(_connectionString);
            await using var conn = new NpgsqlConnection("Host=localhost;Database=test;Username=demo;Password=demo;");

            var items = await conn.QueryFirstOrDefaultAsync<string>(
                "select \"Content\" from public.\"Posts\""
            );
            
            testOutputHelper.WriteLine(JsonConvert.SerializeObject(items));
        };

        testOutputHelper.WriteLine("And...");
        await task.Should().NotThrowAsync();
    }

    private static async Task SeedDb(ServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<BloggingContext>();

        dbContext.Posts.Add(
            new Post
            {
                Content = new PostContent("1", 2),
                PostId = new Random().Next(100000000),
                Title = "title"
            }
        );

        await dbContext.SaveChangesAsync();
    }

    private async Task MigrateDb(ServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<BloggingContext>();

        // Apply migrations
        testOutputHelper.WriteLine("Migrating");
        await dbContext.Database.MigrateAsync();
        testOutputHelper.WriteLine("Migrated");
    }

    private ServiceProvider BuildIoc()
    {
        return new ServiceCollection()
            .AddDbContext<BloggingContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            })
            .BuildServiceProvider();
    }
}
