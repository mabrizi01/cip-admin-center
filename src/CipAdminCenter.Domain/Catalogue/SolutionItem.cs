using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Catalogue;

/// <summary>Functional role of an item inside a Solution. Technical deployment is modelled separately (see Workload).</summary>
public enum SolutionItemKind
{
    Application = 1,
    Gateway = 2,
    Component = 3,
}

/// <summary>An Application, Gateway or Component belonging to a Solution.</summary>
public sealed class SolutionItem
{
    private SolutionItem() { }

    public SolutionItemId Id { get; private set; }

    public SolutionItemKind Kind { get; private set; }

    public string Name { get; private set; } = null!;

    internal static SolutionItem Create(SolutionItemKind kind, string name)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new DomainException($"Unknown item kind '{kind}'.");
        }

        return new() { Id = SolutionItemId.New(), Kind = kind, Name = Guard.NotBlank(name, nameof(name)) };
    }
}
