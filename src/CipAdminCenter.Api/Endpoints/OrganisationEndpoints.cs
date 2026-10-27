using CipAdminCenter.Api.Contracts;
using CipAdminCenter.Application.Organisations;

namespace CipAdminCenter.Api.Endpoints;

internal static class OrganisationEndpoints
{
    public static void MapOrganisationEndpoints(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/").WithTags("Organisations");

        group.MapPost("/customers", async (CreateOrganisationRequest r, CreateCustomer useCase, CancellationToken ct) =>
        {
            var customer = await useCase.ExecuteAsync(new CreateCustomerCommand(r.Name, r.ExternalReference.ToDto()), ct);
            return Results.Created($"/api/v1/customers/{customer.Id}", customer);
        });

        group.MapGet("/customers/{customerId:guid}", async (Guid customerId, GetCustomer useCase, CancellationToken ct) =>
            Results.Ok(await useCase.ExecuteAsync(customerId, ct)));

        group.MapPost("/customers/{customerId:guid}/tenants", async (Guid customerId, CreateOrganisationRequest r, CreateTenant useCase, CancellationToken ct) =>
            Results.Created(string.Empty, await useCase.ExecuteAsync(new CreateTenantCommand(customerId, r.Name, r.ExternalReference.ToDto()), ct)));

        group.MapPost("/tenants/{tenantId:guid}/facilities", async (Guid tenantId, CreateOrganisationRequest r, CreateFacility useCase, CancellationToken ct) =>
            Results.Created(string.Empty, await useCase.ExecuteAsync(new CreateFacilityCommand(tenantId, r.Name, r.ExternalReference.ToDto()), ct)));

        group.MapPost("/facilities/{facilityId:guid}/units", async (Guid facilityId, CreateOrganisationRequest r, CreateUnit useCase, CancellationToken ct) =>
            Results.Created(string.Empty, await useCase.ExecuteAsync(new CreateUnitCommand(facilityId, r.Name, r.ExternalReference.ToDto()), ct)));
    }
}
