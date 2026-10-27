using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Deployment;

/// <summary>The application version and/or workload chosen for an organisational context. At least one must be given.</summary>
public sealed record DeploymentSelection
{
    public ApplicationVersionId? VersionId { get; }

    public WorkloadId? WorkloadId { get; }

    public DeploymentSelection(ApplicationVersionId? versionId, WorkloadId? workloadId)
    {
        if (versionId is null && workloadId is null)
        {
            throw new DomainException("A deployment selection requires an application version or a workload.");
        }

        VersionId = versionId;
        WorkloadId = workloadId;
    }
}
