using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Commercial;

public sealed record AgreementConditionDto(string Code, string Description);

public sealed record AgreementDto(
    Guid Id,
    Guid CustomerId,
    string Reference,
    string Title,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    IReadOnlyList<AgreementConditionDto> Conditions);

public sealed record RecordAgreementCommand(
    Guid CustomerId,
    string Reference,
    string Title,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    IReadOnlyList<AgreementConditionDto>? Conditions = null);

public sealed class RecordAgreement(IOrganisationRepository organisations, IAgreementRepository agreements, IUnitOfWork unitOfWork)
{
    public async Task<AgreementDto> ExecuteAsync(RecordAgreementCommand command, CancellationToken ct)
    {
        var customerId = new CustomerId(command.CustomerId);
        _ = await organisations.GetCustomerAsync(customerId, ct)
            ?? throw new NotFoundException($"Customer '{command.CustomerId}' was not found.");

        var agreement = Agreement.Create(customerId, command.Reference, command.Title, command.ValidFrom, command.ValidTo);
        foreach (var condition in command.Conditions ?? [])
        {
            agreement.AddCondition(condition.Code, condition.Description);
        }

        await agreements.AddAsync(agreement, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return new AgreementDto(
            agreement.Id.Value,
            agreement.CustomerId.Value,
            agreement.Reference,
            agreement.Title,
            agreement.ValidFrom,
            agreement.ValidTo,
            [.. agreement.Conditions.Select(c => new AgreementConditionDto(c.Code, c.Description))]);
    }
}
