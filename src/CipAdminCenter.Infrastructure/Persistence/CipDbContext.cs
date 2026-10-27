using System.Linq.Expressions;
using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;
using CipAdminCenter.Domain.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CipAdminCenter.Infrastructure.Persistence;

public sealed class CipDbContext(DbContextOptions<CipDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Solution> Solutions => Set<Solution>();
    public DbSet<Workload> Workloads => Set<Workload>();
    public DbSet<Agreement> Agreements => Set<Agreement>();
    public DbSet<SolutionPackage> SolutionPackages => Set<SolutionPackage>();
    public DbSet<BillingProfile> BillingProfiles => Set<BillingProfile>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Entitlement> Entitlements => Set<Entitlement>();
    public DbSet<SolutionAvailability> SolutionAvailabilities => Set<SolutionAvailability>();
    public DbSet<ContextConfiguration> ContextConfigurations => Set<ContextConfiguration>();
    public DbSet<GatewayDefinition> GatewayDefinitions => Set<GatewayDefinition>();

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        foreach (var idType in typeof(IGuidId).Assembly.GetTypes().Where(t => t is { IsValueType: true } && typeof(IGuidId).IsAssignableFrom(t)))
        {
            configurationBuilder.Properties(idType).HaveConversion(CreateIdConverter(idType));
        }
    }

    private static Type CreateIdConverter(Type idType) => typeof(GuidIdConverter<>).MakeGenericType(idType);

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CipDbContext).Assembly);
}

/// <summary>Maps strongly typed Guid identifiers (record structs with a Guid constructor) to plain uuid columns.</summary>
public sealed class GuidIdConverter<T>() : ValueConverter<T, Guid>(ToProvider, FromProvider)
    where T : struct, IGuidId
{
    private static readonly Expression<Func<T, Guid>> ToProvider = id => id.Value;

    private static readonly Expression<Func<Guid, T>> FromProvider = BuildFromProvider();

    private static Expression<Func<Guid, T>> BuildFromProvider()
    {
        var parameter = Expression.Parameter(typeof(Guid), "value");
        var constructor = typeof(T).GetConstructor([typeof(Guid)])!;
        return Expression.Lambda<Func<Guid, T>>(Expression.New(constructor, parameter), parameter);
    }
}
