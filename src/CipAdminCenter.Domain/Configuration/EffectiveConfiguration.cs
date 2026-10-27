using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Configuration;

public sealed record EffectiveValue(string Value, ContextRef Source, bool IsOverride, bool AllowOverride);

public sealed record EffectiveSelection(DeploymentSelection Selection, ContextRef Source);

public sealed record EffectiveConfiguration(IReadOnlyDictionary<string, EffectiveValue> Values, EffectiveSelection? Selection);

public static class ConfigurationResolver
{
    /// <param name="chain">Configurations of the contexts on the path, ordered from Tenant to the most specific context. Missing levels are simply absent.</param>
    public static EffectiveConfiguration Resolve(IEnumerable<ContextConfiguration> chain)
    {
        var values = new Dictionary<string, EffectiveValue>();
        EffectiveSelection? selection = null;

        foreach (var configuration in chain)
        {
            foreach (var entry in configuration.Entries)
            {
                var existing = values.GetValueOrDefault(entry.Key);
                if (existing is { AllowOverride: false })
                {
                    continue;
                }

                values[entry.Key] = new EffectiveValue(entry.Value, configuration.Context, existing is not null, entry.AllowOverride);
            }

            if (configuration.Selection is { } s)
            {
                selection = new EffectiveSelection(s, configuration.Context);
            }
        }

        return new EffectiveConfiguration(values, selection);
    }

    /// <summary>Rejects an override of a key that an ancestor has locked.</summary>
    public static void EnsureOverridePermitted(IEnumerable<ContextConfiguration> ancestors, string key)
    {
        var locked = ancestors.FirstOrDefault(a => a.Entries.Any(e => e.Key == key && !e.AllowOverride));
        if (locked is not null)
        {
            throw new DomainException($"'{key}' is locked by the {locked.Context.Level} configuration and cannot be overridden.");
        }
    }
}
