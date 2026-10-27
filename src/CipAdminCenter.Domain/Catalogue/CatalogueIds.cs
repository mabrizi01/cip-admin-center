using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Catalogue;

public readonly record struct SolutionId(Guid Value) : IGuidId
{
    public static SolutionId New() => new(Guid.NewGuid());
}

public readonly record struct SolutionItemId(Guid Value) : IGuidId
{
    public static SolutionItemId New() => new(Guid.NewGuid());
}

public readonly record struct ApplicationVersionId(Guid Value) : IGuidId
{
    public static ApplicationVersionId New() => new(Guid.NewGuid());
}
