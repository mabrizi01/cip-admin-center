using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Application.Configuration;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Routing;

public sealed record ResolvedTargetDto(Guid WorkloadId, string WorkloadName, Uri BaseAddress, Guid? VersionId);

public sealed record ForwardRequestCommand(Guid SolutionId, ContextLevel Level, Guid ContextId, string Method, string RelativePath, string? Body = null, string? ContentType = null);

/// <summary>Determines which workload serves a Solution in a context: availability, then effective configuration, then workload selection.</summary>
public sealed class ResolveTarget(
    IOrganisationRepository organisations,
    ISolutionAvailabilityRepository availabilities,
    IContextConfigurationRepository configurations,
    IWorkloadRepository workloads,
    IPlatformAccessGuard accessGuard)
{
    public async Task<ResolvedTargetDto> ExecuteAsync(Guid solutionId, ContextLevel level, Guid contextId, CancellationToken ct)
    {
        var context = ContextRef.Create(level, contextId);
        await accessGuard.EnsureAccessAsync(new PlatformAccessRequest("routing.resolve", context, solutionId), ct);

        var id = new SolutionId(solutionId);
        var path = await organisations.RequirePathAsync(context, ct);

        if (!AvailabilityPolicy.IsAvailable(path, await availabilities.ListAsync(id, path.Chain, ct)))
        {
            throw new DomainException("The Solution is not available in this context.");
        }

        var effective = ConfigurationResolver.Resolve(await configurations.LoadChainAsync(id, path.Chain, ct));
        var selection = effective.Selection?.Selection
            ?? throw new DomainException("No application version or workload is configured for this context.");

        var workload = WorkloadSelector.Select(selection, await workloads.ListBySolutionAsync(id, ct));
        return new ResolvedTargetDto(workload.Id.Value, workload.Name, workload.BaseAddress, selection.VersionId?.Value);
    }
}

/// <summary>Resolves the configured workload and forwards a request to it through the <see cref="IWorkloadClient"/> port. Callers never see the implementation.</summary>
public sealed class ForwardRequest(ResolveTarget resolveTarget, IWorkloadClient client)
{
    public async Task<WorkloadResponse> ExecuteAsync(ForwardRequestCommand command, CancellationToken ct)
    {
        var target = await resolveTarget.ExecuteAsync(command.SolutionId, command.Level, command.ContextId, ct);
        return await client.SendAsync(new WorkloadRequest(target.BaseAddress, command.Method, command.RelativePath, command.Body, command.ContentType), ct);
    }
}
