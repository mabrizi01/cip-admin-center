using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Infrastructure.Access;
using CipAdminCenter.Infrastructure.Persistence;
using CipAdminCenter.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CipAdminCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<CipDbContext>((sp, options) =>
            options.UseNpgsql(sp.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CipDbContext>());
        services.AddScoped<IOrganisationRepository, OrganisationRepository>();
        services.AddScoped<ISolutionRepository, SolutionRepository>();
        services.AddScoped<IWorkloadRepository, WorkloadRepository>();
        services.AddScoped<IAgreementRepository, AgreementRepository>();
        services.AddScoped<IEntitlementRepository, EntitlementRepository>();
        services.AddScoped<ISolutionAvailabilityRepository, SolutionAvailabilityRepository>();
        services.AddScoped<IContextConfigurationRepository, ContextConfigurationRepository>();
        services.AddSingleton<IPlatformAccessGuard, DeferredPlatformAccessGuard>();

        services.AddHealthChecks().AddDbContextCheck<CipDbContext>("postgresql");
        return services;
    }

    /// <summary>Applies migrations at startup when Database:ApplyMigrationsOnStartup is enabled.</summary>
    public static async Task ApplyMigrationsIfConfiguredAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        if (scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ApplyMigrationsOnStartup)
        {
            await scope.ServiceProvider.GetRequiredService<CipDbContext>().Database.MigrateAsync(ct);
        }
    }
}
