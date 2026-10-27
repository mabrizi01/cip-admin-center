using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

/// <summary>
/// A validated chain Customer -> Tenant [-> Facility [-> Unit]]. It is the only way other modules
/// navigate the hierarchy, so boundary checks live in one place.
/// </summary>
public sealed class OrganisationPath
{
    private readonly ContextRef[] _chain;

    private OrganisationPath(CustomerId customerId, ContextRef[] chain)
    {
        CustomerId = customerId;
        _chain = chain;
    }

    public CustomerId CustomerId { get; }

    /// <summary>Contexts ordered from the broadest (Tenant) to the most specific.</summary>
    public IReadOnlyList<ContextRef> Chain => _chain;

    public ContextRef Context => _chain[^1];

    public static OrganisationPath ForTenant(Tenant tenant) =>
        new(tenant.CustomerId, [ContextRef.For(tenant.Id)]);

    public static OrganisationPath ForFacility(Tenant tenant, Facility facility)
    {
        if (facility.TenantId != tenant.Id)
        {
            throw new DomainException("The Facility does not belong to the Tenant.");
        }

        return new(tenant.CustomerId, [ContextRef.For(tenant.Id), ContextRef.For(facility.Id)]);
    }

    public static OrganisationPath ForUnit(Tenant tenant, Facility facility, Unit unit)
    {
        if (facility.TenantId != tenant.Id)
        {
            throw new DomainException("The Facility does not belong to the Tenant.");
        }

        if (unit.FacilityId != facility.Id)
        {
            throw new DomainException("The Unit does not belong to the Facility.");
        }

        return new(tenant.CustomerId, [ContextRef.For(tenant.Id), ContextRef.For(facility.Id), ContextRef.For(unit.Id)]);
    }
}
