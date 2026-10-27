using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Application.Commercial;
using CipAdminCenter.Application.Configuration;
using CipAdminCenter.Application.Deployment;
using CipAdminCenter.Application.Entitlements;
using CipAdminCenter.Application.Organisations;
using CipAdminCenter.Application.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CipAdminCenter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<CreateCustomer>();
        services.AddScoped<GetCustomer>();
        services.AddScoped<CreateTenant>();
        services.AddScoped<CreateFacility>();
        services.AddScoped<CreateUnit>();
        services.AddScoped<RegisterSolution>();
        services.AddScoped<GetSolution>();
        services.AddScoped<AddSolutionItem>();
        services.AddScoped<RegisterApplicationVersion>();
        services.AddScoped<RegisterWorkload>();
        services.AddScoped<RecordAgreement>();
        services.AddScoped<RecordEntitlement>();
        services.AddScoped<MakeSolutionAvailable>();
        services.AddScoped<DefineContextConfiguration>();
        services.AddScoped<ResolveEffectiveConfiguration>();
        services.AddScoped<ResolveTarget>();
        services.AddScoped<ForwardRequest>();
        return services;
    }
}
