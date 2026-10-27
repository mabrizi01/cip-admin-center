using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Deployment;

/// <summary>Chooses the workload that serves a <see cref="DeploymentSelection"/>.</summary>
public static class WorkloadSelector
{
    public static Workload Select(DeploymentSelection selection, IEnumerable<Workload> solutionWorkloads)
    {
        var workloads = solutionWorkloads.ToList();

        if (selection.WorkloadId is { } workloadId)
        {
            var explicitWorkload = workloads.FirstOrDefault(w => w.Id == workloadId)
                ?? throw new DomainException("The selected workload is not registered for the Solution.");
            if (selection.VersionId is { } v && !explicitWorkload.Serves(v))
            {
                throw new DomainException("The selected workload does not serve the selected application version.");
            }

            return explicitWorkload;
        }

        var versionId = selection.VersionId!.Value;
        var candidates = workloads.Where(w => w.Serves(versionId)).ToList();
        return candidates.Count switch
        {
            0 => throw new DomainException("No workload serves the selected application version."),
            1 => candidates[0],
            _ => candidates.Where(w => w.Generation == ImplementationGeneration.Current).ToList() is [var single]
                ? single
                : throw new DomainException("Several workloads serve the selected application version; select a workload explicitly."),
        };
    }
}
