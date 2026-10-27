using CipAdminCenter.Domain.Common;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Domain.Tests;

public class OrganisationPathTests
{
    [Fact]
    public void Customer_and_tenant_are_distinct_levels_of_the_hierarchy()
    {
        var customer = Customer.Create("Hospital Group");
        var tenant = Tenant.Create(customer.Id, "North Region");
        var facility = Facility.Create(tenant.Id, "Main Hospital");
        var unit = Unit.Create(facility.Id, "Ward A");

        var path = OrganisationPath.ForUnit(tenant, facility, unit);

        Assert.Equal(customer.Id, path.CustomerId);
        Assert.Equal([ContextRef.For(tenant.Id), ContextRef.For(facility.Id), ContextRef.For(unit.Id)], path.Chain);
        Assert.Equal(ContextRef.For(unit.Id), path.Context);
    }

    [Fact]
    public void Facility_must_belong_to_its_tenant()
    {
        var customer = Customer.Create("C");
        var tenant = Tenant.Create(customer.Id, "T1");
        var otherTenant = Tenant.Create(customer.Id, "T2");
        var facility = Facility.Create(otherTenant.Id, "F");

        Assert.Throws<DomainException>(() => OrganisationPath.ForFacility(tenant, facility));
    }

    [Fact]
    public void Unit_must_belong_to_its_facility()
    {
        var tenant = Tenant.Create(CustomerId.New(), "T");
        var facility = Facility.Create(tenant.Id, "F");
        var otherFacility = Facility.Create(tenant.Id, "F2");
        var unit = Unit.Create(otherFacility.Id, "U");

        Assert.Throws<DomainException>(() => OrganisationPath.ForUnit(tenant, facility, unit));
    }

    [Fact]
    public void Parents_and_names_are_required()
    {
        Assert.Throws<DomainException>(() => Tenant.Create(default, "T"));
        Assert.Throws<DomainException>(() => Facility.Create(default, "F"));
        Assert.Throws<DomainException>(() => Unit.Create(default, "U"));
        Assert.Throws<DomainException>(() => Customer.Create(" "));
    }
}
