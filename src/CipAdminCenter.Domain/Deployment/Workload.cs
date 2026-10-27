using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Deployment;

public readonly record struct WorkloadId(Guid Value) : IGuidId
{
    public static WorkloadId New() => new(Guid.NewGuid());
}

public enum WorkloadType
{
    MonolithicApplication = 1,
    WebApplication = 2,
    MobileBackend = 3,
    RestApi = 4,
    Container = 5,
}

/// <summary>Legacy and current implementations evolve in parallel.</summary>
public enum ImplementationGeneration
{
    Legacy = 1,
    Current = 2,
}

public sealed record WorkloadVersionAssociation(ApplicationVersionId ApplicationVersionId);

/// <summary>
/// Deployment metadata for a technical deployment unit serving a Solution. CIP Admin Center records
/// and associates workloads; it does not orchestrate them.
/// </summary>
public sealed class Workload
{
    private readonly List<WorkloadVersionAssociation> _servedVersions = [];

    private Workload() { }

    public WorkloadId Id { get; private set; }

    public SolutionId SolutionId { get; private set; }

    public string Name { get; private set; } = null!;

    public WorkloadType Type { get; private set; }

    public ImplementationGeneration Generation { get; private set; }

    public Uri BaseAddress { get; private set; } = null!;

    public IReadOnlyCollection<WorkloadVersionAssociation> ServedVersions => _servedVersions;

    public static Workload Create(SolutionId solutionId, string name, WorkloadType type, ImplementationGeneration generation, Uri baseAddress)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException($"Unknown workload type '{type}'.");
        }

        if (!Enum.IsDefined(generation))
        {
            throw new DomainException($"Unknown generation '{generation}'.");
        }

        if (baseAddress is null || !baseAddress.IsAbsoluteUri || (baseAddress.Scheme != Uri.UriSchemeHttp && baseAddress.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException("The workload base address must be an absolute http(s) URI.");
        }

        return new()
        {
            Id = WorkloadId.New(),
            SolutionId = solutionId,
            Name = Guard.NotBlank(name, nameof(name)),
            Type = type,
            Generation = generation,
            BaseAddress = baseAddress,
        };
    }

    public void AssociateVersion(ApplicationVersionId versionId)
    {
        if (!Serves(versionId))
        {
            _servedVersions.Add(new WorkloadVersionAssociation(versionId));
        }
    }

    public bool Serves(ApplicationVersionId versionId) => _servedVersions.Any(a => a.ApplicationVersionId == versionId);
}
