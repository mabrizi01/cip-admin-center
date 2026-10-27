using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Organisations;

/// <summary>Organisational levels at which Solutions are made available and configured. Customer is deliberately not a context.</summary>
public enum ContextLevel
{
    Tenant = 1,
    Facility = 2,
    Unit = 3,
}

/// <summary>Reference to a single organisational context (a Tenant, a Facility or a Unit).</summary>
public readonly record struct ContextRef(ContextLevel Level, Guid Id)
{
    public static ContextRef For(TenantId id) => new(ContextLevel.Tenant, id.Value);

    public static ContextRef For(FacilityId id) => new(ContextLevel.Facility, id.Value);

    public static ContextRef For(UnitId id) => new(ContextLevel.Unit, id.Value);

    public static ContextRef Create(ContextLevel level, Guid id)
    {
        if (!Enum.IsDefined(level))
        {
            throw new DomainException($"Unknown context level '{level}'.");
        }

        return new ContextRef(level, Guard.NotEmpty(id, nameof(id)));
    }
}
