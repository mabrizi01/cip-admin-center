namespace CipAdminCenter.Domain.Common;

/// <summary>Marker for strongly typed, Guid-backed identifiers. Implemented as small record structs per concept.</summary>
public interface IGuidId
{
    Guid Value { get; }
}
