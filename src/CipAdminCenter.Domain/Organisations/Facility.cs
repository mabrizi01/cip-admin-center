using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

public sealed class Facility
{
    private Facility() { }

    public FacilityId Id { get; private set; }

    public TenantId TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public ExternalOrganisationReference? ExternalReference { get; private set; }

    public static Facility Create(TenantId tenantId, string name, ExternalOrganisationReference? externalReference = null) => new()
    {
        Id = FacilityId.New(),
        TenantId = tenantId.Value == Guid.Empty ? throw new DomainException("A Facility requires a Tenant.") : tenantId,
        Name = Guard.NotBlank(name, nameof(name)),
        ExternalReference = externalReference,
    };
}
