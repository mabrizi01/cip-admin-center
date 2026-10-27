using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Deployment;

namespace CipAdminCenter.Domain.Tests;

public class CatalogueAndDeploymentTests
{
    private static readonly Uri Address = new("https://legacy.example.org");

    private static (Solution Solution, ApplicationVersion Legacy, ApplicationVersion Next) Catalogue()
    {
        var solution = Solution.Create("sol", "Solution");
        var app = solution.AddItem(SolutionItemKind.Application, "App");
        return (solution, solution.RegisterVersion(app.Id, "1.0", "legacy"), solution.RegisterVersion(app.Id, "2.0", "next"));
    }

    [Fact]
    public void Versions_can_only_be_registered_for_applications()
    {
        var solution = Solution.Create("sol", "Solution");
        var gateway = solution.AddItem(SolutionItemKind.Gateway, "Gw");

        Assert.Throws<DomainException>(() => solution.RegisterVersion(gateway.Id, "1.0"));
        Assert.Throws<DomainException>(() => solution.AddItem(SolutionItemKind.Gateway, "gw"));
    }

    [Fact]
    public void Version_selection_resolves_the_workload_serving_it()
    {
        var (solution, legacy, next) = Catalogue();
        var legacyWorkload = Workload.Create(solution.Id, "legacy", WorkloadType.MonolithicApplication, ImplementationGeneration.Legacy, Address);
        legacyWorkload.AssociateVersion(legacy.Id);
        var nextWorkload = Workload.Create(solution.Id, "next", WorkloadType.Container, ImplementationGeneration.Current, new Uri("https://next.example.org"));
        nextWorkload.AssociateVersion(next.Id);

        var selected = WorkloadSelector.Select(new DeploymentSelection(next.Id, null), [legacyWorkload, nextWorkload]);

        Assert.Equal(nextWorkload.Id, selected.Id);
    }

    [Fact]
    public void Explicit_workload_selection_wins_and_current_generation_breaks_ties()
    {
        var (solution, legacy, _) = Catalogue();
        var old = Workload.Create(solution.Id, "old", WorkloadType.WebApplication, ImplementationGeneration.Legacy, Address);
        var current = Workload.Create(solution.Id, "new", WorkloadType.RestApi, ImplementationGeneration.Current, Address);
        old.AssociateVersion(legacy.Id);
        current.AssociateVersion(legacy.Id);

        Assert.Equal(current.Id, WorkloadSelector.Select(new DeploymentSelection(legacy.Id, null), [old, current]).Id);
        Assert.Equal(old.Id, WorkloadSelector.Select(new DeploymentSelection(null, old.Id), [old, current]).Id);
    }

    [Fact]
    public void Selection_fails_when_no_workload_serves_the_version()
    {
        var (solution, legacy, _) = Catalogue();
        var workload = Workload.Create(solution.Id, "w", WorkloadType.RestApi, ImplementationGeneration.Current, Address);

        Assert.Throws<DomainException>(() => WorkloadSelector.Select(new DeploymentSelection(legacy.Id, null), [workload]));
    }

    [Fact]
    public void Workload_requires_an_absolute_http_address() =>
        Assert.Throws<DomainException>(() => Workload.Create(SolutionId.New(), "w", WorkloadType.RestApi, ImplementationGeneration.Current, new Uri("/relative", UriKind.Relative)));
}
