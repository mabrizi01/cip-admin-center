using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Tests;

public class EntitlementAndAvailabilityTests
{
    private static readonly DateOnly Today = new(2025, 6, 1);

    private static (OrganisationPath Path, ContextRef Tenant, ContextRef Unit) Path()
    {
        var tenant = Tenant.Create(CustomerId.New(), "T");
        var facility = Facility.Create(tenant.Id, "F");
        var unit = Unit.Create(facility.Id, "U");
        return (OrganisationPath.ForUnit(tenant, facility, unit), ContextRef.For(tenant.Id), ContextRef.For(unit.Id));
    }

    [Fact]
    public void Entitlement_alone_does_not_make_a_solution_available()
    {
        var (path, _, _) = Path();
        var solution = SolutionId.New();
        var entitlement = Entitlement.Grant(path.CustomerId, EntitlementScope.ForSolution(solution), Today, null);

        AvailabilityPolicy.EnsureEntitled([entitlement], solution, Today);

        Assert.False(AvailabilityPolicy.IsAvailable(path, []));
    }

    [Fact]
    public void Availability_requires_an_active_entitlement()
    {
        var solution = SolutionId.New();
        var customer = CustomerId.New();
        var expired = Entitlement.Grant(customer, EntitlementScope.ForSolution(solution), Today.AddDays(-10), Today.AddDays(-1));
        var otherSolution = Entitlement.Grant(customer, EntitlementScope.ForSolution(SolutionId.New()), Today, null);

        Assert.Throws<DomainException>(() => AvailabilityPolicy.EnsureEntitled([expired, otherSolution], solution, Today));
    }

    [Fact]
    public void Entitlement_can_be_scoped_to_application_or_functionality()
    {
        var solution = SolutionId.New();
        var functionality = Entitlement.Grant(CustomerId.New(), EntitlementScope.ForFunctionality(solution, "reporting"), Today, null);

        Assert.Equal(EntitlementScopeKind.Functionality, functionality.Scope.Kind);
        Assert.Throws<DomainException>(() => EntitlementScope.ForFunctionality(solution, " "));
        Assert.Throws<DomainException>(() => Entitlement.Grant(CustomerId.New(), EntitlementScope.ForSolution(solution), Today, Today.AddDays(-1)));
    }

    [Fact]
    public void Availability_is_inherited_and_the_most_specific_record_wins()
    {
        var (path, tenant, unit) = Path();
        var solution = SolutionId.New();
        var atTenant = SolutionAvailability.Enable(solution, tenant);

        Assert.True(AvailabilityPolicy.IsAvailable(path, [atTenant]));

        var disabledAtUnit = SolutionAvailability.Enable(solution, unit);
        disabledAtUnit.Disable();
        Assert.False(AvailabilityPolicy.IsAvailable(path, [atTenant, disabledAtUnit]));
    }
}
