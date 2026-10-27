using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Abstractions;

internal static class Loading
{
    public static async Task<OrganisationPath> RequirePathAsync(this IOrganisationRepository repository, ContextRef context, CancellationToken ct) =>
        await repository.GetPathAsync(context, ct) ?? throw new NotFoundException($"{context.Level} '{context.Id}' was not found.");
}
