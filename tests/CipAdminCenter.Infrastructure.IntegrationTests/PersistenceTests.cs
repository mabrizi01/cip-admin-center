using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Application.Configuration;
using CipAdminCenter.Application.Deployment;
using CipAdminCenter.Application.Entitlements;
using CipAdminCenter.Application.Organisations;
using CipAdminCenter.Application.Routing;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;
using Microsoft.Extensions.DependencyInjection;

namespace CipAdminCenter.Infrastructure.IntegrationTests;

public class PersistenceTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [PostgresFact]
    public async Task Use_cases_persist_and_resolve_a_configured_workload_on_postgresql()
    {
        // Each use case gets its own scope (its own DbContext), so every read goes back to the database.
        async Task<T> Run<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = postgres.Services!.CreateScope();
            return await action(scope.ServiceProvider);
        }

        var customer = await Run(s => s.GetRequiredService<CreateCustomer>().ExecuteAsync(new CreateCustomerCommand("C", new ExternalReferenceDto("register", "123")), Ct));
        var tenant = await Run(s => s.GetRequiredService<CreateTenant>().ExecuteAsync(new CreateTenantCommand(customer.Id, "T"), Ct));
        var facility = await Run(s => s.GetRequiredService<CreateFacility>().ExecuteAsync(new CreateFacilityCommand(tenant.Id, "F"), Ct));
        var unit = await Run(s => s.GetRequiredService<CreateUnit>().ExecuteAsync(new CreateUnitCommand(facility.Id, "U"), Ct));

        var solution = await Run(s => s.GetRequiredService<RegisterSolution>().ExecuteAsync(new RegisterSolutionCommand("PG", "Solution"), Ct));
        solution = await Run(s => s.GetRequiredService<AddSolutionItem>().ExecuteAsync(new AddSolutionItemCommand(solution.Id, SolutionItemKind.Application, "App"), Ct));
        solution = await Run(s => s.GetRequiredService<RegisterApplicationVersion>()
            .ExecuteAsync(new RegisterApplicationVersionCommand(solution.Id, solution.Items.Single().Id, "1.0", "legacy"), Ct));
        var versionId = solution.Versions.Single().Id;
        var workload = await Run(s => s.GetRequiredService<RegisterWorkload>().ExecuteAsync(
            new RegisterWorkloadCommand(solution.Id, "w", WorkloadType.RestApi, ImplementationGeneration.Legacy, new Uri("https://legacy.example.org"), [versionId]), Ct));

        await Run(s => s.GetRequiredService<RecordEntitlement>().ExecuteAsync(
            new RecordEntitlementCommand(customer.Id, EntitlementScopeKind.Solution, solution.Id, null, null, new DateOnly(2020, 1, 1), null), Ct));
        await Run(s => s.GetRequiredService<MakeSolutionAvailable>().ExecuteAsync(new MakeSolutionAvailableCommand(solution.Id, ContextLevel.Tenant, tenant.Id), Ct));

        await Run(async s =>
        {
            await s.GetRequiredService<DefineContextConfiguration>().ExecuteAsync(new DefineContextConfigurationCommand(
                solution.Id, ContextLevel.Tenant, tenant.Id, [new ConfigurationEntryDto("theme", "blue", false)], versionId), Ct);
            return 0;
        });
        await Run(async s =>
        {
            await s.GetRequiredService<DefineContextConfiguration>().ExecuteAsync(new DefineContextConfigurationCommand(
                solution.Id, ContextLevel.Unit, unit.Id, [new ConfigurationEntryDto("size", "large")]), Ct);
            return 0;
        });

        var effective = await Run(s => s.GetRequiredService<ResolveEffectiveConfiguration>().ExecuteAsync(solution.Id, ContextLevel.Unit, unit.Id, Ct));
        var target = await Run(s => s.GetRequiredService<ResolveTarget>().ExecuteAsync(solution.Id, ContextLevel.Unit, unit.Id, Ct));
        var reloaded = await Run(s => s.GetRequiredService<GetCustomer>().ExecuteAsync(customer.Id, Ct));

        Assert.Equal("blue", effective.Values["theme"].Value);
        Assert.Equal("large", effective.Values["size"].Value);
        Assert.Equal(workload.Id, target.WorkloadId);
        Assert.Equal("123", reloaded.ExternalReference!.Identifier);
    }
}
