using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Tests;

public class ConfigurationTests
{
    private static readonly SolutionId Solution = SolutionId.New();
    private static readonly ContextRef Tenant = ContextRef.For(TenantId.New());
    private static readonly ContextRef Facility = ContextRef.For(FacilityId.New());
    private static readonly ContextRef Unit = ContextRef.For(UnitId.New());

    private static ContextConfiguration Config(ContextRef context, params (string Key, string Value, bool Allow)[] entries)
    {
        var c = ContextConfiguration.Create(Solution, context);
        foreach (var (k, v, a) in entries)
        {
            c.SetEntry(k, v, a);
        }

        return c;
    }

    [Fact]
    public void Values_are_inherited_from_ancestors()
    {
        var effective = ConfigurationResolver.Resolve([Config(Tenant, ("theme", "blue", true)), Config(Unit)]);

        var value = effective.Values["theme"];
        Assert.Equal("blue", value.Value);
        Assert.Equal(Tenant, value.Source);
        Assert.False(value.IsOverride);
    }

    [Fact]
    public void Local_values_override_inherited_values_when_permitted()
    {
        var effective = ConfigurationResolver.Resolve([
            Config(Tenant, ("theme", "blue", true)),
            Config(Facility),
            Config(Unit, ("theme", "green", true))]);

        var value = effective.Values["theme"];
        Assert.Equal("green", value.Value);
        Assert.Equal(Unit, value.Source);
        Assert.True(value.IsOverride);
    }

    [Fact]
    public void Locked_values_cannot_be_overridden()
    {
        var tenant = Config(Tenant, ("mode", "strict", false));
        var unit = Config(Unit, ("mode", "lax", true));

        Assert.Equal("strict", ConfigurationResolver.Resolve([tenant, unit]).Values["mode"].Value);
        Assert.Throws<DomainException>(() => ConfigurationResolver.EnsureOverridePermitted([tenant], "mode"));
        ConfigurationResolver.EnsureOverridePermitted([tenant], "other");
    }

    [Fact]
    public void Most_specific_selection_wins()
    {
        var tenant = Config(Tenant);
        tenant.Select(new DeploymentSelection(ApplicationVersionId.New(), null));
        var facility = Config(Facility);
        var workload = WorkloadId.New();
        facility.Select(new DeploymentSelection(null, workload));

        var effective = ConfigurationResolver.Resolve([tenant, facility, Config(Unit)]);

        Assert.Equal(workload, effective.Selection!.Selection.WorkloadId);
        Assert.Equal(Facility, effective.Selection.Source);
    }

    [Fact]
    public void Selection_requires_version_or_workload() =>
        Assert.Throws<DomainException>(() => new DeploymentSelection(null, null));
}
