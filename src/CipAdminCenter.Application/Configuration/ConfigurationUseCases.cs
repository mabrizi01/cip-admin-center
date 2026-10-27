using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Configuration;

public sealed record ConfigurationEntryDto(string Key, string Value, bool AllowOverride = true);

public sealed record DefineContextConfigurationCommand(
    Guid SolutionId,
    ContextLevel Level,
    Guid ContextId,
    IReadOnlyList<ConfigurationEntryDto> Entries,
    Guid? SelectedVersionId = null,
    Guid? SelectedWorkloadId = null);

public sealed record EffectiveValueDto(string Value, ContextLevel SourceLevel, Guid SourceId, bool IsOverride);

public sealed record EffectiveSelectionDto(Guid? VersionId, Guid? WorkloadId, ContextLevel SourceLevel, Guid SourceId);

public sealed record EffectiveConfigurationDto(
    Guid SolutionId,
    ContextLevel Level,
    Guid ContextId,
    IReadOnlyDictionary<string, EffectiveValueDto> Values,
    EffectiveSelectionDto? Selection);

internal static class ConfigurationLoading
{
    public static async Task<IReadOnlyList<ContextConfiguration>> LoadChainAsync(
        this IContextConfigurationRepository repository, SolutionId solutionId, IReadOnlyList<ContextRef> chain, CancellationToken ct)
    {
        var found = await repository.ListAsync(solutionId, chain, ct);
        return [.. chain.Select(c => found.FirstOrDefault(f => f.Context == c)).OfType<ContextConfiguration>()];
    }
}

public sealed class DefineContextConfiguration(
    IOrganisationRepository organisations,
    ISolutionRepository solutions,
    IWorkloadRepository workloads,
    IContextConfigurationRepository configurations,
    IPlatformAccessGuard accessGuard,
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(DefineContextConfigurationCommand command, CancellationToken ct)
    {
        var context = ContextRef.Create(command.Level, command.ContextId);
        await accessGuard.EnsureAccessAsync(new PlatformAccessRequest("configuration.define", context, command.SolutionId), ct);

        var solution = await solutions.RequireAsync(command.SolutionId, ct);
        var path = await organisations.RequirePathAsync(context, ct);
        var chain = await configurations.LoadChainAsync(solution.Id, path.Chain, ct);
        var ancestors = chain.Where(c => c.Context != context).ToList();

        var configuration = chain.FirstOrDefault(c => c.Context == context);
        var isNew = configuration is null;
        configuration ??= ContextConfiguration.Create(solution.Id, context);

        foreach (var entry in command.Entries)
        {
            ConfigurationResolver.EnsureOverridePermitted(ancestors, entry.Key);
            configuration.SetEntry(entry.Key, entry.Value, entry.AllowOverride);
        }

        if (command.SelectedVersionId is not null || command.SelectedWorkloadId is not null)
        {
            var selection = new DeploymentSelection(
                command.SelectedVersionId is { } v ? new ApplicationVersionId(v) : null,
                command.SelectedWorkloadId is { } w ? new WorkloadId(w) : null);
            if (selection.VersionId is { } versionId && !solution.HasVersion(versionId))
            {
                throw new DomainException("The selected application version is not registered for the Solution.");
            }

            // Fails when the selection cannot be served by a registered workload of the Solution.
            WorkloadSelector.Select(selection, await workloads.ListBySolutionAsync(solution.Id, ct));
            configuration.Select(selection);
        }

        if (isNew)
        {
            await configurations.AddAsync(configuration, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}

public sealed class ResolveEffectiveConfiguration(
    IOrganisationRepository organisations,
    IContextConfigurationRepository configurations,
    IPlatformAccessGuard accessGuard)
{
    public async Task<EffectiveConfigurationDto> ExecuteAsync(Guid solutionId, ContextLevel level, Guid contextId, CancellationToken ct)
    {
        var context = ContextRef.Create(level, contextId);
        await accessGuard.EnsureAccessAsync(new PlatformAccessRequest("configuration.resolve", context, solutionId), ct);

        var path = await organisations.RequirePathAsync(context, ct);
        var chain = await configurations.LoadChainAsync(new SolutionId(solutionId), path.Chain, ct);
        var effective = ConfigurationResolver.Resolve(chain);

        return new EffectiveConfigurationDto(
            solutionId,
            level,
            contextId,
            effective.Values.ToDictionary(kv => kv.Key, kv => new EffectiveValueDto(kv.Value.Value, kv.Value.Source.Level, kv.Value.Source.Id, kv.Value.IsOverride)),
            effective.Selection is { } s
                ? new EffectiveSelectionDto(s.Selection.VersionId?.Value, s.Selection.WorkloadId?.Value, s.Source.Level, s.Source.Id)
                : null);
    }
}
