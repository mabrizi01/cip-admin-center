using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Deployment;

namespace CipAdminCenter.Application.Deployment;

public sealed record WorkloadDto(
    Guid Id,
    Guid SolutionId,
    string Name,
    WorkloadType Type,
    ImplementationGeneration Generation,
    Uri BaseAddress,
    IReadOnlyList<Guid> ServedVersionIds);

public sealed record RegisterWorkloadCommand(
    Guid SolutionId,
    string Name,
    WorkloadType Type,
    ImplementationGeneration Generation,
    Uri BaseAddress,
    IReadOnlyList<Guid>? ServedVersionIds = null);

public sealed class RegisterWorkload(ISolutionRepository solutions, IWorkloadRepository workloads, IUnitOfWork unitOfWork)
{
    public async Task<WorkloadDto> ExecuteAsync(RegisterWorkloadCommand command, CancellationToken ct)
    {
        var solution = await solutions.RequireAsync(command.SolutionId, ct);
        var workload = Workload.Create(solution.Id, command.Name, command.Type, command.Generation, command.BaseAddress);
        foreach (var id in command.ServedVersionIds ?? [])
        {
            var versionId = new ApplicationVersionId(id);
            if (!solution.HasVersion(versionId))
            {
                throw new DomainException($"Application version '{id}' is not registered for the Solution.");
            }

            workload.AssociateVersion(versionId);
        }

        await workloads.AddAsync(workload, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new WorkloadDto(
            workload.Id.Value,
            workload.SolutionId.Value,
            workload.Name,
            workload.Type,
            workload.Generation,
            workload.BaseAddress,
            [.. workload.ServedVersions.Select(a => a.ApplicationVersionId.Value)]);
    }
}
