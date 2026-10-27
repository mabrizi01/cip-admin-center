using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CipAdminCenter.Infrastructure.Persistence;

/// <summary>Design-time factory for `dotnet ef`. Reads the connection string from the Database__ConnectionString environment variable.</summary>
internal sealed class CipDbContextFactory : IDesignTimeDbContextFactory<CipDbContext>
{
    public CipDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("Database__ConnectionString")
            ?? "Host=localhost;Database=design_time_only";
        return new CipDbContext(new DbContextOptionsBuilder<CipDbContext>().UseNpgsql(connectionString).Options);
    }
}
