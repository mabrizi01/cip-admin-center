using CipAdminCenter.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace CipAdminCenter.Api.IntegrationTests;

/// <summary>Hosts the real API on an in-memory SQLite database so endpoint behaviour is tested without a database server.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Database:ConnectionString", "Host=placeholder");
        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services
                .Where(d => d.ServiceType == typeof(DbContextOptions<CipDbContext>)
                    || d.ServiceType.IsGenericType && d.ServiceType.GetGenericArguments().Contains(typeof(CipDbContext)) && d.ServiceType != typeof(DbContextOptions<CipDbContext>) && d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration"))
                .ToList())
            {
                services.Remove(descriptor);
            }

            _connection.Open();
            services.AddDbContext<CipDbContext>(o => o.UseSqlite(_connection).ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<CipDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
