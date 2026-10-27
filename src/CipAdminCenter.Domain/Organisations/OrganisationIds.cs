using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

public readonly record struct CustomerId(Guid Value) : IGuidId
{
    public static CustomerId New() => new(Guid.NewGuid());
}

public readonly record struct TenantId(Guid Value) : IGuidId
{
    public static TenantId New() => new(Guid.NewGuid());
}

public readonly record struct FacilityId(Guid Value) : IGuidId
{
    public static FacilityId New() => new(Guid.NewGuid());
}

public readonly record struct UnitId(Guid Value) : IGuidId
{
    public static UnitId New() => new(Guid.NewGuid());
}
