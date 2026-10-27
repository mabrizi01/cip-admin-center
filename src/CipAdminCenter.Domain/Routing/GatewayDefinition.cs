using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Routing;

public readonly record struct GatewayDefinitionId(Guid Value) : IGuidId
{
    public static GatewayDefinitionId New() => new(Guid.NewGuid());
}

/// <summary>Access point exposed by a Gateway. Transport protocols are intentionally unspecified.</summary>
public sealed record AccessPoint(string Name, string PathPrefix);

/// <summary>Metadata describing the access points a Gateway exposes for a Solution. Runtime routing lives outside the Domain.</summary>
public sealed class GatewayDefinition
{
    private readonly List<AccessPoint> _accessPoints = [];

    private GatewayDefinition() { }

    public GatewayDefinitionId Id { get; private set; }

    public SolutionId SolutionId { get; private set; }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<AccessPoint> AccessPoints => _accessPoints;

    public static GatewayDefinition Create(SolutionId solutionId, string name) =>
        new() { Id = GatewayDefinitionId.New(), SolutionId = solutionId, Name = Guard.NotBlank(name, nameof(name)) };

    public void AddAccessPoint(string name, string pathPrefix)
    {
        var prefix = Guard.NotBlank(pathPrefix, nameof(pathPrefix));
        if (!prefix.StartsWith('/'))
        {
            throw new DomainException("An access point path prefix must start with '/'.");
        }

        _accessPoints.Add(new AccessPoint(Guard.NotBlank(name, nameof(name)), prefix));
    }
}
