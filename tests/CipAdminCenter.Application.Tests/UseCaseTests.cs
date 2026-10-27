using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Application.Commercial;
using CipAdminCenter.Application.Configuration;
using CipAdminCenter.Application.Deployment;
using CipAdminCenter.Application.Entitlements;
using CipAdminCenter.Application.Organisations;
using CipAdminCenter.Application.Routing;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Tests;

public class UseCaseTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private static readonly TimeProvider Clock = new FakeClock(new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero));

    private readonly InMemoryStore _store = new();

    private sealed record World(CustomerDto Customer, TenantDto Tenant, FacilityDto Facility, UnitDto Unit, SolutionDto Solution, Guid VersionId, WorkloadDto Workload);

    private async Task<World> BuildWorldAsync()
    {
        var customer = await new CreateCustomer(_store, _store).ExecuteAsync(new CreateCustomerCommand("Customer"), Ct);
        var tenant = await new CreateTenant(_store, _store).ExecuteAsync(new CreateTenantCommand(customer.Id, "Tenant"), Ct);
        var facility = await new CreateFacility(_store, _store).ExecuteAsync(new CreateFacilityCommand(tenant.Id, "Facility"), Ct);
        var unit = await new CreateUnit(_store, _store).ExecuteAsync(new CreateUnitCommand(facility.Id, "Unit"), Ct);
        var solution = await new RegisterSolution(_store, _store).ExecuteAsync(new RegisterSolutionCommand("SOL", "Solution"), Ct);
        solution = await new AddSolutionItem(_store, _store).ExecuteAsync(new AddSolutionItemCommand(solution.Id, SolutionItemKind.Application, "App"), Ct);
        solution = await new RegisterApplicationVersion(_store, _store)
            .ExecuteAsync(new RegisterApplicationVersionCommand(solution.Id, solution.Items.Single().Id, "1.0", "legacy"), Ct);
        var versionId = solution.Versions.Single().Id;
        var workload = await new RegisterWorkload(_store, _store, _store).ExecuteAsync(
            new RegisterWorkloadCommand(solution.Id, "legacy-api", WorkloadType.RestApi, ImplementationGeneration.Legacy, new Uri("https://legacy.example.org/base"), [versionId]), Ct);
        return new World(customer, tenant, facility, unit, solution, versionId, workload);
    }

    private Task EntitleAsync(World w) => new RecordEntitlement(_store, _store, _store, _store, _store).ExecuteAsync(
        new RecordEntitlementCommand(w.Customer.Id, EntitlementScopeKind.Solution, w.Solution.Id, null, null, new DateOnly(2025, 1, 1), null), Ct);

    private MakeSolutionAvailable MakeAvailable() => new(_store, _store, _store, _store, _store, _store, Clock);

    private DefineContextConfiguration Define() => new(_store, _store, _store, _store, _store, _store);

    private ResolveTarget Target() => new(_store, _store, _store, _store, _store);

    [Fact]
    public async Task Hierarchy_is_created_top_down_and_rejects_unknown_parents()
    {
        var w = await BuildWorldAsync();

        Assert.Equal(w.Customer.Id, (await new GetCustomer(_store).ExecuteAsync(w.Customer.Id, Ct)).Id);
        Assert.Equal(w.Tenant.Id, w.Facility.TenantId);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CreateTenant(_store, _store).ExecuteAsync(new CreateTenantCommand(Guid.NewGuid(), "T"), Ct));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            new CreateUnit(_store, _store).ExecuteAsync(new CreateUnitCommand(Guid.NewGuid(), "U"), Ct));
    }

    [Fact]
    public async Task Duplicate_solution_code_is_rejected()
    {
        await BuildWorldAsync();

        await Assert.ThrowsAsync<DomainException>(() =>
            new RegisterSolution(_store, _store).ExecuteAsync(new RegisterSolutionCommand("sol", "Again"), Ct));
    }

    [Fact]
    public async Task Workload_cannot_serve_an_unregistered_version()
    {
        var w = await BuildWorldAsync();

        await Assert.ThrowsAsync<DomainException>(() => new RegisterWorkload(_store, _store, _store).ExecuteAsync(
            new RegisterWorkloadCommand(w.Solution.Id, "x", WorkloadType.Container, ImplementationGeneration.Current, new Uri("https://x.example.org"), [Guid.NewGuid()]), Ct));
    }

    [Fact]
    public async Task Agreement_is_recorded_for_an_existing_customer()
    {
        var w = await BuildWorldAsync();

        var agreement = await new RecordAgreement(_store, _store, _store).ExecuteAsync(
            new RecordAgreementCommand(w.Customer.Id, "AG-1", "Framework", new DateOnly(2025, 1, 1), null, [new AgreementConditionDto("C1", "text")]), Ct);

        Assert.Single(agreement.Conditions);
        await Assert.ThrowsAsync<NotFoundException>(() => new RecordAgreement(_store, _store, _store).ExecuteAsync(
            new RecordAgreementCommand(Guid.NewGuid(), "AG-2", "x", new DateOnly(2025, 1, 1), null), Ct));
    }

    [Fact]
    public async Task Entitlement_does_not_imply_availability_and_availability_requires_entitlement()
    {
        var w = await BuildWorldAsync();
        var command = new MakeSolutionAvailableCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id);

        await Assert.ThrowsAsync<DomainException>(() => MakeAvailable().ExecuteAsync(command, Ct));

        await EntitleAsync(w);
        Assert.Empty(_store.Availabilities);

        var result = await MakeAvailable().ExecuteAsync(command, Ct);
        Assert.True(result.IsAvailable);
        Assert.Single(_store.Availabilities);
    }

    [Fact]
    public async Task Effective_configuration_inherits_and_applies_permitted_overrides()
    {
        var w = await BuildWorldAsync();
        await Define().ExecuteAsync(new DefineContextConfigurationCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id,
            [new ConfigurationEntryDto("theme", "blue"), new ConfigurationEntryDto("mode", "strict", false)]), Ct);
        await Define().ExecuteAsync(new DefineContextConfigurationCommand(w.Solution.Id, ContextLevel.Unit, w.Unit.Id,
            [new ConfigurationEntryDto("theme", "green")]), Ct);

        var effective = await new ResolveEffectiveConfiguration(_store, _store, _store)
            .ExecuteAsync(w.Solution.Id, ContextLevel.Unit, w.Unit.Id, Ct);

        Assert.Equal("green", effective.Values["theme"].Value);
        Assert.True(effective.Values["theme"].IsOverride);
        Assert.Equal("strict", effective.Values["mode"].Value);
        Assert.Equal(ContextLevel.Tenant, effective.Values["mode"].SourceLevel);
    }

    [Fact]
    public async Task Locked_values_cannot_be_overridden_locally()
    {
        var w = await BuildWorldAsync();
        await Define().ExecuteAsync(new DefineContextConfigurationCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id,
            [new ConfigurationEntryDto("mode", "strict", false)]), Ct);

        await Assert.ThrowsAsync<DomainException>(() => Define().ExecuteAsync(new DefineContextConfigurationCommand(
            w.Solution.Id, ContextLevel.Facility, w.Facility.Id, [new ConfigurationEntryDto("mode", "lax")]), Ct));
    }

    [Fact]
    public async Task Configured_version_resolves_to_a_workload_only_where_the_solution_is_available()
    {
        var w = await BuildWorldAsync();
        await EntitleAsync(w);
        await Define().ExecuteAsync(new DefineContextConfigurationCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id, [], w.VersionId), Ct);

        await Assert.ThrowsAsync<DomainException>(() => Target().ExecuteAsync(w.Solution.Id, ContextLevel.Unit, w.Unit.Id, Ct));

        await MakeAvailable().ExecuteAsync(new MakeSolutionAvailableCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id), Ct);
        var target = await Target().ExecuteAsync(w.Solution.Id, ContextLevel.Unit, w.Unit.Id, Ct);

        Assert.Equal(w.Workload.Id, target.WorkloadId);
        Assert.Contains(_store.AccessRequests, r => r.Operation == "routing.resolve");
    }

    [Fact]
    public async Task Selection_of_an_unserved_version_is_rejected()
    {
        var w = await BuildWorldAsync();
        var solution = await new RegisterApplicationVersion(_store, _store)
            .ExecuteAsync(new RegisterApplicationVersionCommand(w.Solution.Id, w.Solution.Items.Single().Id, "2.0", "next"), Ct);
        var unserved = solution.Versions.Single(v => v.Label == "2.0").Id;

        await Assert.ThrowsAsync<DomainException>(() => Define().ExecuteAsync(
            new DefineContextConfigurationCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id, [], unserved), Ct));
    }

    [Fact]
    public async Task Forwarding_uses_the_resolved_workload_through_the_client_port()
    {
        var w = await BuildWorldAsync();
        await EntitleAsync(w);
        await MakeAvailable().ExecuteAsync(new MakeSolutionAvailableCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id), Ct);
        await Define().ExecuteAsync(new DefineContextConfigurationCommand(w.Solution.Id, ContextLevel.Tenant, w.Tenant.Id, [], null, w.Workload.Id), Ct);
        var client = new RecordingWorkloadClient();

        var response = await new ForwardRequest(Target(), client)
            .ExecuteAsync(new ForwardRequestCommand(w.Solution.Id, ContextLevel.Facility, w.Facility.Id, "GET", "status"), Ct);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(new Uri("https://legacy.example.org/base"), client.Last!.BaseAddress);
        Assert.Equal("status", client.Last.RelativePath);
    }
}
