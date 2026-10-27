using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Application.Catalogue;

public sealed record SolutionItemDto(Guid Id, SolutionItemKind Kind, string Name);

public sealed record ApplicationVersionDto(Guid Id, Guid ApplicationId, string Label, string Variant);

public sealed record SolutionDto(Guid Id, string Code, string Name, IReadOnlyList<SolutionItemDto> Items, IReadOnlyList<ApplicationVersionDto> Versions);

public sealed record RegisterSolutionCommand(string Code, string Name);

public sealed record AddSolutionItemCommand(Guid SolutionId, SolutionItemKind Kind, string Name);

public sealed record RegisterApplicationVersionCommand(Guid SolutionId, Guid ApplicationId, string Label, string? Variant);

internal static class CatalogueMapping
{
    public static SolutionDto ToDto(this Solution s) => new(
        s.Id.Value,
        s.Code,
        s.Name,
        [.. s.Items.Select(i => new SolutionItemDto(i.Id.Value, i.Kind, i.Name))],
        [.. s.Versions.Select(v => new ApplicationVersionDto(v.Id.Value, v.ApplicationId.Value, v.Label, v.Variant))]);

    public static async Task<Solution> RequireAsync(this ISolutionRepository repository, Guid id, CancellationToken ct) =>
        await repository.GetAsync(new SolutionId(id), ct) ?? throw new NotFoundException($"Solution '{id}' was not found.");
}

public sealed class RegisterSolution(ISolutionRepository solutions, IUnitOfWork unitOfWork)
{
    public async Task<SolutionDto> ExecuteAsync(RegisterSolutionCommand command, CancellationToken ct)
    {
        var solution = Solution.Create(command.Code, command.Name);
        if (await solutions.CodeExistsAsync(solution.Code, ct))
        {
            throw new DomainException($"A Solution with code '{solution.Code}' already exists.");
        }

        await solutions.AddAsync(solution, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return solution.ToDto();
    }
}

public sealed class GetSolution(ISolutionRepository solutions)
{
    public async Task<SolutionDto> ExecuteAsync(Guid solutionId, CancellationToken ct) =>
        (await solutions.RequireAsync(solutionId, ct)).ToDto();
}

/// <summary>Associates an Application, Gateway or Component with a Solution.</summary>
public sealed class AddSolutionItem(ISolutionRepository solutions, IUnitOfWork unitOfWork)
{
    public async Task<SolutionDto> ExecuteAsync(AddSolutionItemCommand command, CancellationToken ct)
    {
        var solution = await solutions.RequireAsync(command.SolutionId, ct);
        solution.AddItem(command.Kind, command.Name);
        await unitOfWork.SaveChangesAsync(ct);
        return solution.ToDto();
    }
}

public sealed class RegisterApplicationVersion(ISolutionRepository solutions, IUnitOfWork unitOfWork)
{
    public async Task<SolutionDto> ExecuteAsync(RegisterApplicationVersionCommand command, CancellationToken ct)
    {
        var solution = await solutions.RequireAsync(command.SolutionId, ct);
        solution.RegisterVersion(new SolutionItemId(command.ApplicationId), command.Label, command.Variant);
        await unitOfWork.SaveChangesAsync(ct);
        return solution.ToDto();
    }
}
