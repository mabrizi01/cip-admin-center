using CipAdminCenter.Application.Abstractions;
using CipAdminCenter.Domain.Organisations;

namespace CipAdminCenter.Application.Organisations;

public sealed record ExternalReferenceDto(string Scheme, string Identifier);

public sealed record CustomerDto(Guid Id, string Name, ExternalReferenceDto? ExternalReference);

public sealed record TenantDto(Guid Id, Guid CustomerId, string Name, ExternalReferenceDto? ExternalReference);

public sealed record FacilityDto(Guid Id, Guid TenantId, string Name, ExternalReferenceDto? ExternalReference);

public sealed record UnitDto(Guid Id, Guid FacilityId, string Name, ExternalReferenceDto? ExternalReference);

public sealed record CreateCustomerCommand(string Name, ExternalReferenceDto? ExternalReference = null);

public sealed record CreateTenantCommand(Guid CustomerId, string Name, ExternalReferenceDto? ExternalReference = null);

public sealed record CreateFacilityCommand(Guid TenantId, string Name, ExternalReferenceDto? ExternalReference = null);

public sealed record CreateUnitCommand(Guid FacilityId, string Name, ExternalReferenceDto? ExternalReference = null);

internal static class OrganisationMapping
{
    public static ExternalOrganisationReference? ToDomain(this ExternalReferenceDto? dto) =>
        dto is null ? null : new ExternalOrganisationReference(dto.Scheme, dto.Identifier);

    public static ExternalReferenceDto? ToDto(this ExternalOrganisationReference? r) =>
        r is null ? null : new ExternalReferenceDto(r.Scheme, r.Identifier);

    public static CustomerDto ToDto(this Customer c) => new(c.Id.Value, c.Name, c.ExternalReference.ToDto());

    public static TenantDto ToDto(this Tenant t) => new(t.Id.Value, t.CustomerId.Value, t.Name, t.ExternalReference.ToDto());

    public static FacilityDto ToDto(this Facility f) => new(f.Id.Value, f.TenantId.Value, f.Name, f.ExternalReference.ToDto());

    public static UnitDto ToDto(this Unit u) => new(u.Id.Value, u.FacilityId.Value, u.Name, u.ExternalReference.ToDto());
}

public sealed class CreateCustomer(IOrganisationRepository organisations, IUnitOfWork unitOfWork)
{
    public async Task<CustomerDto> ExecuteAsync(CreateCustomerCommand command, CancellationToken ct)
    {
        var customer = Customer.Create(command.Name, command.ExternalReference.ToDomain());
        await organisations.AddCustomerAsync(customer, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return customer.ToDto();
    }
}

public sealed class GetCustomer(IOrganisationRepository organisations)
{
    public async Task<CustomerDto> ExecuteAsync(Guid customerId, CancellationToken ct) =>
        (await organisations.GetCustomerAsync(new CustomerId(customerId), ct)
            ?? throw new NotFoundException($"Customer '{customerId}' was not found.")).ToDto();
}

public sealed class CreateTenant(IOrganisationRepository organisations, IUnitOfWork unitOfWork)
{
    public async Task<TenantDto> ExecuteAsync(CreateTenantCommand command, CancellationToken ct)
    {
        var customerId = new CustomerId(command.CustomerId);
        _ = await organisations.GetCustomerAsync(customerId, ct)
            ?? throw new NotFoundException($"Customer '{command.CustomerId}' was not found.");
        var tenant = Tenant.Create(customerId, command.Name, command.ExternalReference.ToDomain());
        await organisations.AddTenantAsync(tenant, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return tenant.ToDto();
    }
}

public sealed class CreateFacility(IOrganisationRepository organisations, IUnitOfWork unitOfWork)
{
    public async Task<FacilityDto> ExecuteAsync(CreateFacilityCommand command, CancellationToken ct)
    {
        var tenantId = new TenantId(command.TenantId);
        _ = await organisations.GetTenantAsync(tenantId, ct)
            ?? throw new NotFoundException($"Tenant '{command.TenantId}' was not found.");
        var facility = Facility.Create(tenantId, command.Name, command.ExternalReference.ToDomain());
        await organisations.AddFacilityAsync(facility, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return facility.ToDto();
    }
}

public sealed class CreateUnit(IOrganisationRepository organisations, IUnitOfWork unitOfWork)
{
    public async Task<UnitDto> ExecuteAsync(CreateUnitCommand command, CancellationToken ct)
    {
        var facilityId = new FacilityId(command.FacilityId);
        _ = await organisations.GetFacilityAsync(facilityId, ct)
            ?? throw new NotFoundException($"Facility '{command.FacilityId}' was not found.");
        var unit = Unit.Create(facilityId, command.Name, command.ExternalReference.ToDomain());
        await organisations.AddUnitAsync(unit, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return unit.ToDto();
    }
}
