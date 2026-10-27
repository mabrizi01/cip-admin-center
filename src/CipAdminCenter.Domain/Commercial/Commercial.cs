using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Commercial;

public readonly record struct AgreementId(Guid Value) : IGuidId
{
    public static AgreementId New() => new(Guid.NewGuid());
}

public readonly record struct SolutionPackageId(Guid Value) : IGuidId
{
    public static SolutionPackageId New() => new(Guid.NewGuid());
}

public readonly record struct BillingProfileId(Guid Value) : IGuidId
{
    public static BillingProfileId New() => new(Guid.NewGuid());
}

/// <summary>Free-text contractual condition. Structured commercial rules are intentionally not defined yet.</summary>
public sealed record AgreementCondition(string Code, string Description);

/// <summary>Agreement between StarHealth and a Customer. A Customer may have many.</summary>
public sealed class Agreement
{
    private readonly List<AgreementCondition> _conditions = [];

    private Agreement() { }

    public AgreementId Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public string Reference { get; private set; } = null!;

    public string Title { get; private set; } = null!;

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidTo { get; private set; }

    public IReadOnlyCollection<AgreementCondition> Conditions => _conditions;

    public static Agreement Create(CustomerId customerId, string reference, string title, DateOnly validFrom, DateOnly? validTo)
    {
        if (customerId.Value == Guid.Empty)
        {
            throw new DomainException("An Agreement requires a Customer.");
        }

        if (validTo is { } to && to < validFrom)
        {
            throw new DomainException("The Agreement end date must not precede its start date.");
        }

        return new()
        {
            Id = AgreementId.New(),
            CustomerId = customerId,
            Reference = Guard.NotBlank(reference, nameof(reference)),
            Title = Guard.NotBlank(title, nameof(title)),
            ValidFrom = validFrom,
            ValidTo = validTo,
        };
    }

    public void AddCondition(string code, string description) =>
        _conditions.Add(new AgreementCondition(Guard.NotBlank(code, nameof(code)), Guard.NotBlank(description, nameof(description))));
}

/// <summary>A named group of Solutions that can be offered together.</summary>
public sealed class SolutionPackage
{
    private readonly List<PackageSolution> _solutions = [];

    private SolutionPackage() { }

    public SolutionPackageId Id { get; private set; }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<PackageSolution> Solutions => _solutions;

    public static SolutionPackage Create(string name) => new() { Id = SolutionPackageId.New(), Name = Guard.NotBlank(name, nameof(name)) };

    public void Include(SolutionId solutionId)
    {
        if (_solutions.All(s => s.SolutionId != solutionId))
        {
            _solutions.Add(new PackageSolution(solutionId));
        }
    }
}

public sealed record PackageSolution(SolutionId SolutionId);

/// <summary>Billing Profile owned by a Customer and optionally tied to one of its Tenants. No invoicing logic.</summary>
public sealed class BillingProfile
{
    private BillingProfile() { }

    public BillingProfileId Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public TenantId? TenantId { get; private set; }

    public string Name { get; private set; } = null!;

    public static BillingProfile Create(CustomerId customerId, string name) => new()
    {
        Id = BillingProfileId.New(),
        CustomerId = customerId.Value == Guid.Empty ? throw new DomainException("A Billing Profile requires a Customer.") : customerId,
        Name = Guard.NotBlank(name, nameof(name)),
    };

    public void AssociateTenant(Tenant tenant)
    {
        if (tenant.CustomerId != CustomerId)
        {
            throw new DomainException("The Tenant belongs to a different Customer.");
        }

        TenantId = tenant.Id;
    }
}
