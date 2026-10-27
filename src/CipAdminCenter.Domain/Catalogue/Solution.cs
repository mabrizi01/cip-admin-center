using CipAdminCenter.Domain.Common;

namespace CipAdminCenter.Domain.Catalogue;

/// <summary>A modular healthcare Solution in the product catalogue, composed of Applications, Gateways and Components.</summary>
public sealed class Solution
{
    private readonly List<SolutionItem> _items = [];
    private readonly List<ApplicationVersion> _versions = [];

    private Solution() { }

    public SolutionId Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<SolutionItem> Items => _items;

    public IReadOnlyCollection<ApplicationVersion> Versions => _versions;

    public static Solution Create(string code, string name) => new()
    {
        Id = SolutionId.New(),
        Code = Guard.NotBlank(code, nameof(code)).ToUpperInvariant(),
        Name = Guard.NotBlank(name, nameof(name)),
    };

    public SolutionItem AddItem(SolutionItemKind kind, string name)
    {
        var item = SolutionItem.Create(kind, name);
        if (_items.Any(i => i.Kind == kind && string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"The Solution already contains a {kind} named '{item.Name}'.");
        }

        _items.Add(item);
        return item;
    }

    public ApplicationVersion RegisterVersion(SolutionItemId applicationId, string label, string? variant = null)
    {
        var application = _items.FirstOrDefault(i => i.Id == applicationId)
            ?? throw new DomainException("The Application is not part of the Solution.");
        if (application.Kind != SolutionItemKind.Application)
        {
            throw new DomainException("Versions can only be registered for Applications.");
        }

        var version = ApplicationVersion.Create(applicationId, label, variant);
        if (_versions.Any(v => v.ApplicationId == applicationId && v.Label == version.Label && v.Variant == version.Variant))
        {
            throw new DomainException("This application version is already registered.");
        }

        _versions.Add(version);
        return version;
    }

    public bool HasVersion(ApplicationVersionId versionId) => _versions.Any(v => v.Id == versionId);
}
