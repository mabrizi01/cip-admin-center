using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Tests;

/// <summary>In-memory implementation of every persistence port, so use cases are tested without any infrastructure.</summary>
public sealed class InMemoryStore :
    IOrganisationRepository, ISolutionRepository, IWorkloadRepository, IAgreementRepository,
    IEntitlementRepository, ISolutionAvailabilityRepository, IContextConfigurationRepository, IUnitOfWork, IPlatformAccessGuard
{
    public List<Customer> Customers { get; } = [];
    public List<Tenant> Tenants { get; } = [];
    public List<Facility> Facilities { get; } = [];
    public List<Unit> Units { get; } = [];
    public List<Solution> Solutions { get; } = [];
    public List<Workload> Workloads { get; } = [];
    public List<Agreement> Agreements { get; } = [];
    public List<Entitlement> Entitlements { get; } = [];
    public List<SolutionAvailability> Availabilities { get; } = [];
    public List<ContextConfiguration> Configurations { get; } = [];
    public List<PlatformAccessRequest> AccessRequests { get; } = [];
    public int Saves { get; private set; }

    public Task AddCustomerAsync(Customer c, CancellationToken ct) { Customers.Add(c); return Task.CompletedTask; }
    public Task<Customer?> GetCustomerAsync(CustomerId id, CancellationToken ct) => Task.FromResult(Customers.FirstOrDefault(x => x.Id == id));
    public Task AddTenantAsync(Tenant t, CancellationToken ct) { Tenants.Add(t); return Task.CompletedTask; }
    public Task<Tenant?> GetTenantAsync(TenantId id, CancellationToken ct) => Task.FromResult(Tenants.FirstOrDefault(x => x.Id == id));
    public Task AddFacilityAsync(Facility f, CancellationToken ct) { Facilities.Add(f); return Task.CompletedTask; }
    public Task<Facility?> GetFacilityAsync(FacilityId id, CancellationToken ct) => Task.FromResult(Facilities.FirstOrDefault(x => x.Id == id));
    public Task AddUnitAsync(Unit u, CancellationToken ct) { Units.Add(u); return Task.CompletedTask; }

    public Task<OrganisationPath?> GetPathAsync(ContextRef context, CancellationToken ct)
    {
        OrganisationPath? path = null;
        switch (context.Level)
        {
            case ContextLevel.Tenant when Tenants.FirstOrDefault(t => t.Id.Value == context.Id) is { } t:
                path = OrganisationPath.ForTenant(t);
                break;
            case ContextLevel.Facility when Facilities.FirstOrDefault(f => f.Id.Value == context.Id) is { } f:
                path = OrganisationPath.ForFacility(Tenants.First(t => t.Id == f.TenantId), f);
                break;
            case ContextLevel.Unit when Units.FirstOrDefault(u => u.Id.Value == context.Id) is { } u:
                var facility = Facilities.First(x => x.Id == u.FacilityId);
                path = OrganisationPath.ForUnit(Tenants.First(t => t.Id == facility.TenantId), facility, u);
                break;
        }

        return Task.FromResult(path);
    }

    public Task AddAsync(Solution s, CancellationToken ct) { Solutions.Add(s); return Task.CompletedTask; }
    public Task<Solution?> GetAsync(SolutionId id, CancellationToken ct) => Task.FromResult(Solutions.FirstOrDefault(x => x.Id == id));
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) => Task.FromResult(Solutions.Any(s => s.Code == code));

    public Task AddAsync(Workload w, CancellationToken ct) { Workloads.Add(w); return Task.CompletedTask; }
    public Task<IReadOnlyList<Workload>> ListBySolutionAsync(SolutionId id, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Workload>>([.. Workloads.Where(w => w.SolutionId == id)]);

    public Task AddAsync(Agreement a, CancellationToken ct) { Agreements.Add(a); return Task.CompletedTask; }
    public Task<Agreement?> GetAsync(AgreementId id, CancellationToken ct) => Task.FromResult(Agreements.FirstOrDefault(x => x.Id == id));

    public Task AddAsync(Entitlement e, CancellationToken ct) { Entitlements.Add(e); return Task.CompletedTask; }
    public Task<IReadOnlyList<Entitlement>> ListForCustomerAsync(CustomerId id, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Entitlement>>([.. Entitlements.Where(e => e.CustomerId == id)]);

    public Task AddAsync(SolutionAvailability a, CancellationToken ct) { Availabilities.Add(a); return Task.CompletedTask; }
    Task<IReadOnlyList<SolutionAvailability>> ISolutionAvailabilityRepository.ListAsync(SolutionId id, IReadOnlyCollection<ContextRef> contexts, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<SolutionAvailability>>([.. Availabilities.Where(a => a.SolutionId == id && contexts.Contains(a.Context))]);

    public Task AddAsync(ContextConfiguration c, CancellationToken ct) { Configurations.Add(c); return Task.CompletedTask; }
    Task<IReadOnlyList<ContextConfiguration>> IContextConfigurationRepository.ListAsync(SolutionId id, IReadOnlyCollection<ContextRef> contexts, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ContextConfiguration>>([.. Configurations.Where(c => c.SolutionId == id && contexts.Contains(c.Context))]);

    public Task SaveChangesAsync(CancellationToken ct) { Saves++; return Task.CompletedTask; }

    public Task EnsureAccessAsync(PlatformAccessRequest request, CancellationToken ct) { AccessRequests.Add(request); return Task.CompletedTask; }
}

public sealed class FakeClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

public sealed class RecordingWorkloadClient : IWorkloadClient
{
    public WorkloadRequest? Last { get; private set; }

    public Task<WorkloadResponse> SendAsync(WorkloadRequest request, CancellationToken ct)
    {
        Last = request;
        return Task.FromResult(new WorkloadResponse(200, "ok", "text/plain"));
    }
}
