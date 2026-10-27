using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Application.Catalogue;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Entitlements;

public sealed record EntitlementDto(
    Guid Id,
    Guid CustomerId,
    EntitlementScopeKind ScopeKind,
    Guid SolutionId,
    Guid? ApplicationId,
    string? FunctionalityCode,
    DateOnly ValidFrom,
    DateOnly? ValidTo);

public sealed record RecordEntitlementCommand(
    Guid CustomerId,
    EntitlementScopeKind ScopeKind,
    Guid SolutionId,
    Guid? ApplicationId,
    string? FunctionalityCode,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    Guid? AgreementId = null);

public sealed record SolutionAvailabilityDto(Guid SolutionId, ContextLevel Level, Guid ContextId, bool IsAvailable);

public sealed record MakeSolutionAvailableCommand(Guid SolutionId, ContextLevel Level, Guid ContextId);

public sealed class RecordEntitlement(
    IOrganisationRepository organisations,
    ISolutionRepository solutions,
    IAgreementRepository agreements,
    IEntitlementRepository entitlements,
    IUnitOfWork unitOfWork)
{
    public async Task<EntitlementDto> ExecuteAsync(RecordEntitlementCommand command, CancellationToken ct)
    {
        var customerId = new CustomerId(command.CustomerId);
        _ = await organisations.GetCustomerAsync(customerId, ct)
            ?? throw new NotFoundException($"Customer '{command.CustomerId}' was not found.");
        var solution = await solutions.RequireAsync(command.SolutionId, ct);

        AgreementId? agreementId = null;
        if (command.AgreementId is { } rawAgreementId)
        {
            var agreement = await agreements.GetAsync(new AgreementId(rawAgreementId), ct)
                ?? throw new NotFoundException($"Agreement '{rawAgreementId}' was not found.");
            if (agreement.CustomerId != customerId)
            {
                throw new DomainException("The Agreement belongs to a different Customer.");
            }

            agreementId = agreement.Id;
        }

        var scope = command.ScopeKind switch
        {
            EntitlementScopeKind.Solution => EntitlementScope.ForSolution(solution.Id),
            EntitlementScopeKind.Application => EntitlementScope.ForApplication(solution.Id, ResolveApplication(solution, command.ApplicationId)),
            EntitlementScopeKind.Functionality => EntitlementScope.ForFunctionality(solution.Id, command.FunctionalityCode ?? string.Empty),
            _ => throw new DomainException($"Unknown entitlement scope '{command.ScopeKind}'."),
        };

        var entitlement = Entitlement.Grant(customerId, scope, command.ValidFrom, command.ValidTo, agreementId);
        await entitlements.AddAsync(entitlement, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new EntitlementDto(
            entitlement.Id.Value,
            customerId.Value,
            scope.Kind,
            solution.Id.Value,
            scope.ApplicationId?.Value,
            scope.FunctionalityCode,
            entitlement.ValidFrom,
            entitlement.ValidTo);
    }

    private static SolutionItemId ResolveApplication(Solution solution, Guid? applicationId)
    {
        var id = new SolutionItemId(applicationId ?? Guid.Empty);
        if (!solution.Items.Any(i => i.Id == id && i.Kind == SolutionItemKind.Application))
        {
            throw new DomainException("The Application is not part of the Solution.");
        }

        return id;
    }
}

/// <summary>Makes a Solution available in a context. Requires an entitlement, but entitlement alone never makes a Solution available.</summary>
public sealed class MakeSolutionAvailable(
    IOrganisationRepository organisations,
    ISolutionRepository solutions,
    IEntitlementRepository entitlements,
    ISolutionAvailabilityRepository availabilities,
    IPlatformAccessGuard accessGuard,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<SolutionAvailabilityDto> ExecuteAsync(MakeSolutionAvailableCommand command, CancellationToken ct)
    {
        var context = ContextRef.Create(command.Level, command.ContextId);
        await accessGuard.EnsureAccessAsync(new PlatformAccessRequest("solution.make-available", context, command.SolutionId), ct);

        var solution = await solutions.RequireAsync(command.SolutionId, ct);
        var path = await organisations.RequirePathAsync(context, ct);

        var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
        AvailabilityPolicy.EnsureEntitled(await entitlements.ListForCustomerAsync(path.CustomerId, ct), solution.Id, today);

        var existing = (await availabilities.ListAsync(solution.Id, [context], ct)).FirstOrDefault();
        if (existing is null)
        {
            await availabilities.AddAsync(SolutionAvailability.Enable(solution.Id, context), ct);
        }
        else
        {
            existing.Enable();
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new SolutionAvailabilityDto(solution.Id.Value, command.Level, command.ContextId, true);
    }
}
