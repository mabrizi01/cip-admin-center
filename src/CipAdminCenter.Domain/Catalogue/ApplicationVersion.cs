using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Catalogue;

/// <summary>A version of an Application. The variant distinguishes parallel implementations of the same software (e.g. "legacy", "next").</summary>
public sealed class ApplicationVersion
{
    private ApplicationVersion() { }

    public ApplicationVersionId Id { get; private set; }

    public SolutionItemId ApplicationId { get; private set; }

    public string Label { get; private set; } = null!;

    public string Variant { get; private set; } = string.Empty;

    internal static ApplicationVersion Create(SolutionItemId applicationId, string label, string? variant) => new()
    {
        Id = ApplicationVersionId.New(),
        ApplicationId = applicationId,
        Label = Guard.NotBlank(label, nameof(label)),
        Variant = variant?.Trim() ?? string.Empty,
    };
}
