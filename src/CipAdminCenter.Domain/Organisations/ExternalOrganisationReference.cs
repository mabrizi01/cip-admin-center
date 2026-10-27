using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

/// <summary>Maps a CIP organisational role to a real healthcare organisation known in an external register (scheme + identifier).</summary>
public sealed record ExternalOrganisationReference
{
    public string Scheme { get; private set; }

    public string Identifier { get; private set; }

    public ExternalOrganisationReference(string scheme, string identifier)
    {
        Scheme = Guard.NotBlank(scheme, nameof(scheme));
        Identifier = Guard.NotBlank(identifier, nameof(identifier));
    }
}
