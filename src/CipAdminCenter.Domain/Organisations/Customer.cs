using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

/// <summary>Contracting party. Distinct from a Tenant: a Customer may own several Tenants.</summary>
public sealed class Customer
{
    private Customer() { }

    public CustomerId Id { get; private set; }

    public string Name { get; private set; } = null!;

    public ExternalOrganisationReference? ExternalReference { get; private set; }

    public static Customer Create(string name, ExternalOrganisationReference? externalReference = null) => new()
    {
        Id = CustomerId.New(),
        Name = Guard.NotBlank(name, nameof(name)),
        ExternalReference = externalReference,
    };
}
