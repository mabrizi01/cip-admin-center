using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

public sealed class Unit
{
    private Unit() { }

    public UnitId Id { get; private set; }

    public FacilityId FacilityId { get; private set; }

    public string Name { get; private set; } = null!;

    public ExternalOrganisationReference? ExternalReference { get; private set; }

    public static Unit Create(FacilityId facilityId, string name, ExternalOrganisationReference? externalReference = null) => new()
    {
        Id = UnitId.New(),
        FacilityId = facilityId.Value == Guid.Empty ? throw new DomainException("A Unit requires a Facility.") : facilityId,
        Name = Guard.NotBlank(name, nameof(name)),
        ExternalReference = externalReference,
    };
}
