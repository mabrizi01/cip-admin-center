using System.ComponentModel.DataAnnotations;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;

namespace CipAdminCenter.Api.Contracts;

// Transport models. They are validated here and mapped to Application commands; no business rules live in this layer.

public sealed record ExternalReferenceRequest
{
    [Required, StringLength(100)] public string Scheme { get; init; } = string.Empty;

    [Required, StringLength(200)] public string Identifier { get; init; } = string.Empty;
}

public sealed record CreateOrganisationRequest
{
    [Required, StringLength(200)] public string Name { get; init; } = string.Empty;

    public ExternalReferenceRequest? ExternalReference { get; init; }
}

public sealed record RegisterSolutionRequest
{
    [Required, StringLength(50)] public string Code { get; init; } = string.Empty;

    [Required, StringLength(200)] public string Name { get; init; } = string.Empty;
}

public sealed record AddSolutionItemRequest
{
    public SolutionItemKind Kind { get; init; }

    [Required, StringLength(200)] public string Name { get; init; } = string.Empty;
}

public sealed record RegisterApplicationVersionRequest
{
    [Required, StringLength(100)] public string Label { get; init; } = string.Empty;

    [StringLength(100)] public string? Variant { get; init; }
}

public sealed record RegisterWorkloadRequest
{
    [Required, StringLength(200)] public string Name { get; init; } = string.Empty;

    public WorkloadType Type { get; init; }

    public ImplementationGeneration Generation { get; init; }

    [Required] public Uri? BaseAddress { get; init; }

    public Guid[]? ServedVersionIds { get; init; }
}

public sealed record AgreementConditionRequest
{
    [Required, StringLength(100)] public string Code { get; init; } = string.Empty;

    [Required, StringLength(2000)] public string Description { get; init; } = string.Empty;
}

public sealed record RecordAgreementRequest
{
    [Required, StringLength(100)] public string Reference { get; init; } = string.Empty;

    [Required, StringLength(200)] public string Title { get; init; } = string.Empty;

    public DateOnly ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public AgreementConditionRequest[]? Conditions { get; init; }
}

public sealed record RecordEntitlementRequest
{
    public EntitlementScopeKind ScopeKind { get; init; }

    public Guid SolutionId { get; init; }

    public Guid? ApplicationId { get; init; }

    [StringLength(100)] public string? FunctionalityCode { get; init; }

    public DateOnly ValidFrom { get; init; }

    public DateOnly? ValidTo { get; init; }

    public Guid? AgreementId { get; init; }
}

public sealed record ConfigurationEntryRequest
{
    [Required, StringLength(200)] public string Key { get; init; } = string.Empty;

    [Required, StringLength(4000)] public string Value { get; init; } = string.Empty;

    public bool AllowOverride { get; init; } = true;
}

public sealed record DefineConfigurationRequest
{
    public ConfigurationEntryRequest[]? Entries { get; init; }

    public Guid? SelectedVersionId { get; init; }

    public Guid? SelectedWorkloadId { get; init; }
}
