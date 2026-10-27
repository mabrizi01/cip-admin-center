using CipAdminCenter.Api.Contracts;
using CipAdminCenter.Application.Configuration;
using CipAdminCenter.Application.Entitlements;
using CipAdminCenter.Application.Routing;

namespace CipAdminCenter.Api.Endpoints;

/// <summary>Endpoints scoped to one Solution in one organisational context: /contexts/{level}/{contextId}/solutions/{solutionId}/...</summary>
internal static class ContextEndpoints
{
    public static void MapContextEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/contexts/{level}/{contextId:guid}/solutions/{solutionId:guid}");

        group.MapPut("/availability", async (string level, Guid contextId, Guid solutionId, MakeSolutionAvailable useCase, CancellationToken ct) =>
            EndpointHelpers.TryParseLevel(level, out var l)
                ? Results.Ok(await useCase.ExecuteAsync(new MakeSolutionAvailableCommand(solutionId, l, contextId), ct))
                : EndpointHelpers.InvalidLevel(level))
            .WithTags("Entitlements and Availability");

        group.MapPut("/configuration", async (string level, Guid contextId, Guid solutionId, DefineConfigurationRequest r, DefineContextConfiguration useCase, CancellationToken ct) =>
        {
            if (!EndpointHelpers.TryParseLevel(level, out var l))
            {
                return EndpointHelpers.InvalidLevel(level);
            }

            var entries = r.Entries?.Select(e => new ConfigurationEntryDto(e.Key, e.Value, e.AllowOverride)).ToList() ?? [];
            await useCase.ExecuteAsync(new DefineContextConfigurationCommand(solutionId, l, contextId, entries, r.SelectedVersionId, r.SelectedWorkloadId), ct);
            return Results.NoContent();
        }).WithTags("Contextual Configuration");

        group.MapGet("/configuration/effective", async (string level, Guid contextId, Guid solutionId, ResolveEffectiveConfiguration useCase, CancellationToken ct) =>
            EndpointHelpers.TryParseLevel(level, out var l)
                ? Results.Ok(await useCase.ExecuteAsync(solutionId, l, contextId, ct))
                : EndpointHelpers.InvalidLevel(level))
            .WithTags("Contextual Configuration");

        group.MapGet("/target", async (string level, Guid contextId, Guid solutionId, ResolveTarget useCase, CancellationToken ct) =>
            EndpointHelpers.TryParseLevel(level, out var l)
                ? Results.Ok(await useCase.ExecuteAsync(solutionId, l, contextId, ct))
                : EndpointHelpers.InvalidLevel(level))
            .WithTags("Access and Routing");
    }
}
