using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;
using Microsoft.EntityFrameworkCore;

namespace CipAdminCenter.Infrastructure.Persistence.Repositories;

internal sealed class OrganisationRepository(CipDbContext db) : IOrganisationRepository
{
    public async Task AddCustomerAsync(Customer customer, CancellationToken ct) => await db.Customers.AddAsync(customer, ct);

    public Task<Customer?> GetCustomerAsync(CustomerId id, CancellationToken ct) => db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddTenantAsync(Tenant tenant, CancellationToken ct) => await db.Tenants.AddAsync(tenant, ct);

    public Task<Tenant?> GetTenantAsync(TenantId id, CancellationToken ct) => db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddFacilityAsync(Facility facility, CancellationToken ct) => await db.Facilities.AddAsync(facility, ct);

    public Task<Facility?> GetFacilityAsync(FacilityId id, CancellationToken ct) => db.Facilities.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddUnitAsync(Unit unit, CancellationToken ct) => await db.Units.AddAsync(unit, ct);

    public async Task<OrganisationPath?> GetPathAsync(ContextRef context, CancellationToken ct)
    {
        switch (context.Level)
        {
            case ContextLevel.Tenant:
                var tenant = await GetTenantAsync(new TenantId(context.Id), ct);
                return tenant is null ? null : OrganisationPath.ForTenant(tenant);
            case ContextLevel.Facility:
                var facility = await GetFacilityAsync(new FacilityId(context.Id), ct);
                if (facility is null)
                {
                    return null;
                }

                var facilityTenant = await GetTenantAsync(facility.TenantId, ct);
                return facilityTenant is null ? null : OrganisationPath.ForFacility(facilityTenant, facility);
            case ContextLevel.Unit:
                var unit = await db.Units.FirstOrDefaultAsync(x => x.Id == new UnitId(context.Id), ct);
                if (unit is null)
                {
                    return null;
                }

                var unitFacility = await GetFacilityAsync(unit.FacilityId, ct);
                var unitTenant = unitFacility is null ? null : await GetTenantAsync(unitFacility.TenantId, ct);
                return unitFacility is null || unitTenant is null ? null : OrganisationPath.ForUnit(unitTenant, unitFacility, unit);
            default:
                return null;
        }
    }
}

internal sealed class SolutionRepository(CipDbContext db) : ISolutionRepository
{
    public async Task AddAsync(Solution solution, CancellationToken ct) => await db.Solutions.AddAsync(solution, ct);

    public Task<Solution?> GetAsync(SolutionId id, CancellationToken ct) =>
        db.Solutions.Include(s => s.Items).Include(s => s.Versions).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) => db.Solutions.AnyAsync(s => s.Code == code, ct);
}

internal sealed class WorkloadRepository(CipDbContext db) : IWorkloadRepository
{
    public async Task AddAsync(Workload workload, CancellationToken ct) => await db.Workloads.AddAsync(workload, ct);

    public async Task<IReadOnlyList<Workload>> ListBySolutionAsync(SolutionId solutionId, CancellationToken ct) =>
        await db.Workloads.Where(w => w.SolutionId == solutionId).ToListAsync(ct);
}

internal sealed class AgreementRepository(CipDbContext db) : IAgreementRepository
{
    public async Task AddAsync(Agreement agreement, CancellationToken ct) => await db.Agreements.AddAsync(agreement, ct);

    public Task<Agreement?> GetAsync(AgreementId id, CancellationToken ct) => db.Agreements.FirstOrDefaultAsync(a => a.Id == id, ct);
}

internal sealed class EntitlementRepository(CipDbContext db) : IEntitlementRepository
{
    public async Task AddAsync(Entitlement entitlement, CancellationToken ct) => await db.Entitlements.AddAsync(entitlement, ct);

    public async Task<IReadOnlyList<Entitlement>> ListForCustomerAsync(CustomerId customerId, CancellationToken ct) =>
        await db.Entitlements.Where(e => e.CustomerId == customerId).ToListAsync(ct);
}

internal sealed class SolutionAvailabilityRepository(CipDbContext db) : ISolutionAvailabilityRepository
{
    public async Task AddAsync(SolutionAvailability availability, CancellationToken ct) => await db.SolutionAvailabilities.AddAsync(availability, ct);

    public async Task<IReadOnlyList<SolutionAvailability>> ListAsync(SolutionId solutionId, IReadOnlyCollection<ContextRef> contexts, CancellationToken ct)
    {
        var ids = contexts.Select(c => c.Id).ToList();
        var candidates = await db.SolutionAvailabilities.Where(a => a.SolutionId == solutionId && ids.Contains(a.Context.Id)).ToListAsync(ct);
        return [.. candidates.Where(a => contexts.Contains(a.Context))];
    }
}

internal sealed class ContextConfigurationRepository(CipDbContext db) : IContextConfigurationRepository
{
    public async Task AddAsync(ContextConfiguration configuration, CancellationToken ct) => await db.ContextConfigurations.AddAsync(configuration, ct);

    public async Task<IReadOnlyList<ContextConfiguration>> ListAsync(SolutionId solutionId, IReadOnlyCollection<ContextRef> contexts, CancellationToken ct)
    {
        var ids = contexts.Select(c => c.Id).ToList();
        var candidates = await db.ContextConfigurations
            .Include(c => c.Entries)
            .Where(c => c.SolutionId == solutionId && ids.Contains(c.Context.Id))
            .ToListAsync(ct);
        return [.. candidates.Where(c => contexts.Contains(c.Context))];
    }
}
