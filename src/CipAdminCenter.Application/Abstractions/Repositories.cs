using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Abstractions;

// Persistence ports required by the initial use cases. Implemented in Infrastructure.

public interface IOrganisationRepository
{
    Task AddCustomerAsync(Customer customer, CancellationToken ct);
    Task<Customer?> GetCustomerAsync(CustomerId id, CancellationToken ct);
    Task AddTenantAsync(Tenant tenant, CancellationToken ct);
    Task<Tenant?> GetTenantAsync(TenantId id, CancellationToken ct);
    Task AddFacilityAsync(Facility facility, CancellationToken ct);
    Task<Facility?> GetFacilityAsync(FacilityId id, CancellationToken ct);
    Task AddUnitAsync(Unit unit, CancellationToken ct);

    /// <summary>Loads and validates the hierarchy path of a context; null when the context does not exist.</summary>
    Task<OrganisationPath?> GetPathAsync(ContextRef context, CancellationToken ct);
}

public interface ISolutionRepository
{
    Task AddAsync(Solution solution, CancellationToken ct);
    Task<Solution?> GetAsync(SolutionId id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
}

public interface IWorkloadRepository
{
    Task AddAsync(Workload workload, CancellationToken ct);
    Task<IReadOnlyList<Workload>> ListBySolutionAsync(SolutionId solutionId, CancellationToken ct);
}

public interface IAgreementRepository
{
    Task AddAsync(Agreement agreement, CancellationToken ct);
    Task<Agreement?> GetAsync(AgreementId id, CancellationToken ct);
}

public interface IEntitlementRepository
{
    Task AddAsync(Entitlement entitlement, CancellationToken ct);
    Task<IReadOnlyList<Entitlement>> ListForCustomerAsync(CustomerId customerId, CancellationToken ct);
}

public interface ISolutionAvailabilityRepository
{
    Task AddAsync(SolutionAvailability availability, CancellationToken ct);

    /// <summary>Availability records of the Solution whose context is one of the given contexts.</summary>
    Task<IReadOnlyList<SolutionAvailability>> ListAsync(SolutionId solutionId, IReadOnlyCollection<ContextRef> contexts, CancellationToken ct);
}

public interface IContextConfigurationRepository
{
    Task AddAsync(ContextConfiguration configuration, CancellationToken ct);

    /// <summary>Configurations of the Solution whose context is one of the given contexts. Tracked for update.</summary>
    Task<IReadOnlyList<ContextConfiguration>> ListAsync(SolutionId solutionId, IReadOnlyCollection<ContextRef> contexts, CancellationToken ct);
}
