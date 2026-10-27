using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Configuration;

public readonly record struct ContextConfigurationId(Guid Value) : IGuidId
{
    public static ContextConfigurationId New() => new(Guid.NewGuid());
}

/// <summary>
/// A simple key/value setting. When <see cref="AllowOverride"/> is false, contexts below this one
/// cannot override the value. Keys defined below a context that already defines them are overrides.
/// </summary>
public sealed record ConfigurationEntry(string Key, string Value, bool AllowOverride);

/// <summary>Configuration of one Solution in one organisational context (Tenant, Facility or Unit).</summary>
public sealed class ContextConfiguration
{
    private readonly List<ConfigurationEntry> _entries = [];

    private ContextConfiguration() { }

    public ContextConfigurationId Id { get; private set; }

    public SolutionId SolutionId { get; private set; }

    public ContextRef Context { get; private set; }

    public IReadOnlyCollection<ConfigurationEntry> Entries => _entries;

    public ApplicationVersionId? SelectedVersionId { get; private set; }

    public WorkloadId? SelectedWorkloadId { get; private set; }

    public DeploymentSelection? Selection =>
        SelectedVersionId is null && SelectedWorkloadId is null ? null : new DeploymentSelection(SelectedVersionId, SelectedWorkloadId);

    public static ContextConfiguration Create(SolutionId solutionId, ContextRef context) => new()
    {
        Id = ContextConfigurationId.New(),
        SolutionId = solutionId,
        Context = context,
    };

    public void SetEntry(string key, string value, bool allowOverride = true)
    {
        key = Guard.NotBlank(key, nameof(key));
        _entries.RemoveAll(e => e.Key == key);
        _entries.Add(new ConfigurationEntry(key, value ?? throw new DomainException("A configuration value is required."), allowOverride));
    }

    public void Select(DeploymentSelection? selection)
    {
        SelectedVersionId = selection?.VersionId;
        SelectedWorkloadId = selection?.WorkloadId;
    }
}
