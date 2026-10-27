using CipAdminCenter.Api.Contracts;
using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Application.Commercial;
using CipAdminCenter.Application.Deployment;
using CipAdminCenter.Application.Entitlements;

namespace CipAdminCenter.Api.Endpoints;

internal static class CatalogueEndpoints
{
    public static void MapCatalogueEndpoints(this IEndpointRouteBuilder api)
    {
        var solutions = api.MapGroup("/solutions").WithTags("Product Catalogue");

        solutions.MapPost("/", async (RegisterSolutionRequest r, RegisterSolution useCase, CancellationToken ct) =>
        {
            var solution = await useCase.ExecuteAsync(new RegisterSolutionCommand(r.Code, r.Name), ct);
            return Results.Created($"/api/v1/solutions/{solution.Id}", solution);
        });

        solutions.MapGet("/{solutionId:guid}", async (Guid solutionId, GetSolution useCase, CancellationToken ct) =>
            Results.Ok(await useCase.ExecuteAsync(solutionId, ct)));

        solutions.MapPost("/{solutionId:guid}/items", async (Guid solutionId, AddSolutionItemRequest r, AddSolutionItem useCase, CancellationToken ct) =>
            Results.Ok(await useCase.ExecuteAsync(new AddSolutionItemCommand(solutionId, r.Kind, r.Name), ct)));

        solutions.MapPost("/{solutionId:guid}/items/{applicationId:guid}/versions",
            async (Guid solutionId, Guid applicationId, RegisterApplicationVersionRequest r, RegisterApplicationVersion useCase, CancellationToken ct) =>
                Results.Ok(await useCase.ExecuteAsync(new RegisterApplicationVersionCommand(solutionId, applicationId, r.Label, r.Variant), ct)));

        solutions.MapPost("/{solutionId:guid}/workloads", async (Guid solutionId, RegisterWorkloadRequest r, RegisterWorkload useCase, CancellationToken ct) =>
            Results.Ok(await useCase.ExecuteAsync(new RegisterWorkloadCommand(solutionId, r.Name, r.Type, r.Generation, r.BaseAddress!, r.ServedVersionIds), ct)))
            .WithTags("Deployment Registry");

        var customers = api.MapGroup("/customers/{customerId:guid}");

        customers.MapPost("/agreements", async (Guid customerId, RecordAgreementRequest r, RecordAgreement useCase, CancellationToken ct) =>
            Results.Ok(await useCase.ExecuteAsync(
                new RecordAgreementCommand(
                    customerId,
                    r.Reference,
                    r.Title,
                    r.ValidFrom,
                    r.ValidTo,
                    r.Conditions?.Select(c => new AgreementConditionDto(c.Code, c.Description)).ToList()),
                ct)))
            .WithTags("Commercial Management");

        customers.MapPost("/entitlements", async (Guid customerId, RecordEntitlementRequest r, RecordEntitlement useCase, CancellationToken ct) =>
            Results.Ok(await useCase.ExecuteAsync(
                new RecordEntitlementCommand(customerId, r.ScopeKind, r.SolutionId, r.ApplicationId, r.FunctionalityCode, r.ValidFrom, r.ValidTo, r.AgreementId),
                ct)))
            .WithTags("Entitlements and Availability");
    }
}
