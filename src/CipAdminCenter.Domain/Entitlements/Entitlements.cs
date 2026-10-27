using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Entitlements;

public readonly record struct PurchaseId(Guid Value) : IGuidId
{
    public static PurchaseId New() => new(Guid.NewGuid());
}

public readonly record struct SubscriptionId(Guid Value) : IGuidId
{
    public static SubscriptionId New() => new(Guid.NewGuid());
}

public readonly record struct EntitlementId(Guid Value) : IGuidId
{
    public static EntitlementId New() => new(Guid.NewGuid());
}

public readonly record struct SolutionAvailabilityId(Guid Value) : IGuidId
{
    public static SolutionAvailabilityId New() => new(Guid.NewGuid());
}

public sealed class Purchase
{
    private Purchase() { }

    public PurchaseId Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public AgreementId? AgreementId { get; private set; }

    public SolutionId SolutionId { get; private set; }

    public DateOnly PurchasedOn { get; private set; }

    public static Purchase Record(CustomerId customerId, SolutionId solutionId, DateOnly purchasedOn, AgreementId? agreementId = null) => new()
    {
        Id = PurchaseId.New(),
        CustomerId = customerId,
        SolutionId = solutionId,
        PurchasedOn = purchasedOn,
        AgreementId = agreementId,
    };
}

public sealed class Subscription
{
    private Subscription() { }

    public SubscriptionId Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public AgreementId? AgreementId { get; private set; }

    public SolutionId SolutionId { get; private set; }

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidTo { get; private set; }

    public static Subscription Start(CustomerId customerId, SolutionId solutionId, DateOnly validFrom, DateOnly? validTo, AgreementId? agreementId = null)
    {
        if (validTo is { } to && to < validFrom)
        {
            throw new DomainException("The Subscription end date must not precede its start date.");
        }

        return new()
        {
            Id = SubscriptionId.New(),
            CustomerId = customerId,
            SolutionId = solutionId,
            ValidFrom = validFrom,
            ValidTo = validTo,
            AgreementId = agreementId,
        };
    }
}

public enum EntitlementScopeKind
{
    Solution = 1,
    Application = 2,
    Functionality = 3,
}

/// <summary>What an Entitlement grants the right to use: a Solution, one of its Applications, or a specific functionality.</summary>
public readonly record struct EntitlementScope(EntitlementScopeKind Kind, SolutionId SolutionId, SolutionItemId? ApplicationId, string? FunctionalityCode)
{
    public static EntitlementScope ForSolution(SolutionId solutionId) => new(EntitlementScopeKind.Solution, solutionId, null, null);

    public static EntitlementScope ForApplication(SolutionId solutionId, SolutionItemId applicationId) =>
        new(EntitlementScopeKind.Application, solutionId, applicationId, null);

    public static EntitlementScope ForFunctionality(SolutionId solutionId, string functionalityCode) =>
        new(EntitlementScopeKind.Functionality, solutionId, null, Guard.NotBlank(functionalityCode, nameof(functionalityCode)));
}

/// <summary>The commercial right of a Customer to use something. It says nothing about where it is available or how it is configured.</summary>
public sealed class Entitlement
{
    private Entitlement() { }

    public EntitlementId Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public EntitlementScope Scope { get; private set; }

    public AgreementId? AgreementId { get; private set; }

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidTo { get; private set; }

    public static Entitlement Grant(CustomerId customerId, EntitlementScope scope, DateOnly validFrom, DateOnly? validTo, AgreementId? agreementId = null)
    {
        if (customerId.Value == Guid.Empty)
        {
            throw new DomainException("An Entitlement requires a Customer.");
        }

        if (validTo is { } to && to < validFrom)
        {
            throw new DomainException("The Entitlement end date must not precede its start date.");
        }

        return new()
        {
            Id = EntitlementId.New(),
            CustomerId = customerId,
            Scope = scope,
            ValidFrom = validFrom,
            ValidTo = validTo,
            AgreementId = agreementId,
        };
    }

    public bool IsActiveOn(DateOnly date) => date >= ValidFrom && (ValidTo is null || date <= ValidTo);
}

/// <summary>A Solution made available in one organisational context (Tenant, Facility or Unit). Independent from entitlement and configuration.</summary>
public sealed class SolutionAvailability
{
    private SolutionAvailability() { }

    public SolutionAvailabilityId Id { get; private set; }

    public SolutionId SolutionId { get; private set; }

    public ContextRef Context { get; private set; }

    public bool IsEnabled { get; private set; }

    public static SolutionAvailability Enable(SolutionId solutionId, ContextRef context) => new()
    {
        Id = SolutionAvailabilityId.New(),
        SolutionId = solutionId,
        Context = context,
        IsEnabled = true,
    };

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;
}

public static class AvailabilityPolicy
{
    /// <summary>A Solution can only be made available to a Customer's contexts while the Customer holds an active entitlement within that Solution.</summary>
    public static void EnsureEntitled(IEnumerable<Entitlement> customerEntitlements, SolutionId solutionId, DateOnly today)
    {
        if (!customerEntitlements.Any(e => e.Scope.SolutionId == solutionId && e.IsActiveOn(today)))
        {
            throw new DomainException("The Customer holds no active entitlement for this Solution.");
        }
    }

    /// <summary>
    /// A Solution is available in a context when the most specific availability record along the path
    /// (Unit, then Facility, then Tenant) is enabled. No record means not available.
    /// </summary>
    public static bool IsAvailable(OrganisationPath path, IEnumerable<SolutionAvailability> records)
    {
        var byContext = records.ToDictionary(r => r.Context);
        foreach (var context in path.Chain.Reverse())
        {
            if (byContext.TryGetValue(context, out var record))
            {
                return record.IsEnabled;
            }
        }

        return false;
    }
}
