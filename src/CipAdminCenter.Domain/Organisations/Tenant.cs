using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

/// <summary>Operational and configuration boundary below a Customer.</summary>
public sealed class Tenant
{
    private Tenant() { }

    public TenantId Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public string Name { get; private set; } = null!;

    public ExternalOrganisationReference? ExternalReference { get; private set; }

    public static Tenant Create(CustomerId customerId, string name, ExternalOrganisationReference? externalReference = null) => new()
    {
        Id = TenantId.New(),
        CustomerId = customerId.Value == Guid.Empty ? throw new DomainException("A Tenant requires a Customer.") : customerId,
        Name = Guard.NotBlank(name, nameof(name)),
        ExternalReference = externalReference,
    };
}
