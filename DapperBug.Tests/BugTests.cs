using Dapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        await MigrateDb();

#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.UseJsonNet();
#pragma warning restore CS0618 // Type or member is obsolete

        var task = async () =>
        {
            await using var conn = new NpgsqlConnection(
                "Host=localhost;Database=test;Username=demo;Password=demo;"
            );

            return await conn.QueryFirstOrDefaultAsync<string>(
                "select \"Content\" from public.\"Posts\""
            );
        };

        testOutputHelper.WriteLine("And...");
        await task.Should().NotThrowAsync();
    }

    private async Task MigrateDb()
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<BloggingContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            })
            .BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BloggingContext>();

        // Apply migrations
        testOutputHelper.WriteLine("Migrating");
        await dbContext.Database.MigrateAsync();
        testOutputHelper.WriteLine("Migrated");
    }
}
