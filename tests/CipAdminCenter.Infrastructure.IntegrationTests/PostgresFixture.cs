using CipAdminCenter.Application;
using CipAdminCenter.Infrastructure;
using CipAdminCenter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CipAdminCenter.Infrastructure.IntegrationTests;

/// <summary>
/// Uses an existing PostgreSQL server (e.g. the compose "db" service) given by the CIP_TEST_POSTGRES environment variable
/// (a connection string without a database name). Each fixture creates and drops its own database. Tests are skipped when the variable is absent.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    public const string EnvironmentVariable = "CIP_TEST_POSTGRES";

    private readonly string? _server = Environment.GetEnvironmentVariable(EnvironmentVariable);
    private readonly string _database = $"cip_test_{Guid.NewGuid():N}";

    public ServiceProvider? Services { get; private set; }

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(_server))
        {
            return;
        }

        var builder = new NpgsqlConnectionStringBuilder(_server) { Database = _database };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Database:ConnectionString"] = builder.ConnectionString })
            .Build();
        Services = new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CipDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Services is null)
        {
            return;
        }

        using (var scope = Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<CipDbContext>().Database.EnsureDeletedAsync();
        }

        await Services.DisposeAsync();
    }
}

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(PostgresFixture.EnvironmentVariable)))
        {
            Skip = $"Set {PostgresFixture.EnvironmentVariable} to a PostgreSQL server connection string to run.";
        }
    }
}
