using CipAdminCenter.Domain.Catalogue;
using CipAdminCenter.Domain.Commercial;
using CipAdminCenter.Domain.Configuration;
using CipAdminCenter.Domain.Deployment;
using CipAdminCenter.Domain.Entitlements;
using CipAdminCenter.Domain.Organisations;
using CipAdminCenter.Domain.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CipAdminCenter.Infrastructure.Persistence;

// Fluent mappings for the Domain model. All persistence concerns stay in Infrastructure.

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("customers").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.OwnsOne(x => x.ExternalReference, o => ExternalReference(o));
    }

    internal static void ExternalReference<T>(OwnedNavigationBuilder<T, ExternalOrganisationReference> o) where T : class
    {
        o.Property(x => x.Scheme).HasMaxLength(100).HasColumnName("external_scheme");
        o.Property(x => x.Identifier).HasMaxLength(200).HasColumnName("external_identifier");
    }
}

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.OwnsOne(x => x.ExternalReference, o => CustomerConfiguration.ExternalReference(o));
    }
}

internal sealed class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> b)
    {
        b.ToTable("facilities").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.OwnsOne(x => x.ExternalReference, o => CustomerConfiguration.ExternalReference(o));
    }
}

internal sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("units").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasOne<Facility>().WithMany().HasForeignKey(x => x.FacilityId).OnDelete(DeleteBehavior.Restrict);
        b.OwnsOne(x => x.ExternalReference, o => CustomerConfiguration.ExternalReference(o));
    }
}

internal sealed class SolutionConfiguration : IEntityTypeConfiguration<Solution>
{
    public void Configure(EntityTypeBuilder<Solution> b)
    {
        b.ToTable("solutions").HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasMany(x => x.Items).WithOne().HasForeignKey("SolutionId").OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Versions).WithOne().HasForeignKey("SolutionId").OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class SolutionItemConfiguration : IEntityTypeConfiguration<SolutionItem>
{
    public void Configure(EntityTypeBuilder<SolutionItem> b)
    {
        b.ToTable("solution_items").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30);
    }
}

internal sealed class ApplicationVersionConfiguration : IEntityTypeConfiguration<ApplicationVersion>
{
    public void Configure(EntityTypeBuilder<ApplicationVersion> b)
    {
        b.ToTable("application_versions").HasKey(x => x.Id);
        b.Property(x => x.Label).HasMaxLength(100).IsRequired();
        b.Property(x => x.Variant).HasMaxLength(100).IsRequired();
        b.HasOne<SolutionItem>().WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class WorkloadConfiguration : IEntityTypeConfiguration<Workload>
{
    public void Configure(EntityTypeBuilder<Workload> b)
    {
        b.ToTable("workloads").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Generation).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.BaseAddress).HasMaxLength(500).IsRequired();
        b.HasOne<Solution>().WithMany().HasForeignKey(x => x.SolutionId).OnDelete(DeleteBehavior.Restrict);
        b.OwnsMany(x => x.ServedVersions, o =>
        {
            o.ToTable("workload_versions");
            o.WithOwner().HasForeignKey("WorkloadId");
            o.Property<int>("Id").ValueGeneratedOnAdd();
            o.HasKey("Id");
            o.Property(x => x.ApplicationVersionId).HasColumnName("application_version_id");
        });
        b.Navigation(x => x.ServedVersions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class AgreementConfiguration : IEntityTypeConfiguration<Agreement>
{
    public void Configure(EntityTypeBuilder<Agreement> b)
    {
        b.ToTable("agreements").HasKey(x => x.Id);
        b.Property(x => x.Reference).HasMaxLength(100).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.OwnsMany(x => x.Conditions, o =>
        {
            o.ToTable("agreement_conditions");
            o.WithOwner().HasForeignKey("AgreementId");
            o.Property<int>("Id").ValueGeneratedOnAdd();
            o.HasKey("Id");
            o.Property(x => x.Code).HasMaxLength(100);
            o.Property(x => x.Description).HasMaxLength(2000);
        });
        b.Navigation(x => x.Conditions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class SolutionPackageConfiguration : IEntityTypeConfiguration<SolutionPackage>
{
    public void Configure(EntityTypeBuilder<SolutionPackage> b)
    {
        b.ToTable("solution_packages").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.OwnsMany(x => x.Solutions, o =>
        {
            o.ToTable("solution_package_solutions");
            o.WithOwner().HasForeignKey("SolutionPackageId");
            o.Property<int>("Id").ValueGeneratedOnAdd();
            o.HasKey("Id");
            o.Property(x => x.SolutionId).HasColumnName("solution_id");
        });
        b.Navigation(x => x.Solutions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class BillingProfileConfiguration : IEntityTypeConfiguration<BillingProfile>
{
    public void Configure(EntityTypeBuilder<BillingProfile> b)
    {
        b.ToTable("billing_profiles").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> b)
    {
        b.ToTable("purchases").HasKey(x => x.Id);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Solution>().WithMany().HasForeignKey(x => x.SolutionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("subscriptions").HasKey(x => x.Id);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Solution>().WithMany().HasForeignKey(x => x.SolutionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class EntitlementConfiguration : IEntityTypeConfiguration<Entitlement>
{
    public void Configure(EntityTypeBuilder<Entitlement> b)
    {
        b.ToTable("entitlements").HasKey(x => x.Id);
        b.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.ComplexProperty(x => x.Scope, s =>
        {
            s.Property(x => x.Kind).HasConversion<string>().HasMaxLength(30).HasColumnName("scope_kind");
            s.Property(x => x.SolutionId).HasColumnName("scope_solution_id");
            s.Property(x => x.ApplicationId).HasColumnName("scope_application_id");
            s.Property(x => x.FunctionalityCode).HasMaxLength(100).HasColumnName("scope_functionality_code");
        });
        b.HasIndex(x => x.CustomerId);
    }
}

internal sealed class SolutionAvailabilityConfiguration : IEntityTypeConfiguration<SolutionAvailability>
{
    public void Configure(EntityTypeBuilder<SolutionAvailability> b)
    {
        b.ToTable("solution_availabilities").HasKey(x => x.Id);
        b.HasOne<Solution>().WithMany().HasForeignKey(x => x.SolutionId).OnDelete(DeleteBehavior.Restrict);
        b.ComplexProperty(x => x.Context, c =>
        {
            c.Property(x => x.Level).HasConversion<string>().HasMaxLength(30).HasColumnName("context_level");
            c.Property(x => x.Id).HasColumnName("context_id");
        });
        b.HasIndex(x => x.SolutionId);
    }
}

internal sealed class ContextConfigurationConfiguration : IEntityTypeConfiguration<ContextConfiguration>
{
    public void Configure(EntityTypeBuilder<ContextConfiguration> b)
    {
        b.ToTable("context_configurations").HasKey(x => x.Id);
        b.HasOne<Solution>().WithMany().HasForeignKey(x => x.SolutionId).OnDelete(DeleteBehavior.Restrict);
        b.ComplexProperty(x => x.Context, c =>
        {
            c.Property(x => x.Level).HasConversion<string>().HasMaxLength(30).HasColumnName("context_level");
            c.Property(x => x.Id).HasColumnName("context_id");
        });
        b.Ignore(x => x.Selection);
        b.HasIndex(x => x.SolutionId);
        b.OwnsMany(x => x.Entries, o =>
        {
            o.ToTable("context_configuration_entries");
            o.WithOwner().HasForeignKey("ContextConfigurationId");
            o.Property<int>("Id").ValueGeneratedOnAdd();
            o.HasKey("Id");
            o.Property(x => x.Key).HasMaxLength(200);
            o.Property(x => x.Value).HasMaxLength(4000);
        });
        b.Navigation(x => x.Entries).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class GatewayDefinitionConfiguration : IEntityTypeConfiguration<GatewayDefinition>
{
    public void Configure(EntityTypeBuilder<GatewayDefinition> b)
    {
        b.ToTable("gateway_definitions").HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasOne<Solution>().WithMany().HasForeignKey(x => x.SolutionId).OnDelete(DeleteBehavior.Restrict);
        b.OwnsMany(x => x.AccessPoints, o =>
        {
            o.ToTable("gateway_access_points");
            o.WithOwner().HasForeignKey("GatewayDefinitionId");
            o.Property<int>("Id").ValueGeneratedOnAdd();
            o.HasKey("Id");
            o.Property(x => x.Name).HasMaxLength(200);
            o.Property(x => x.PathPrefix).HasMaxLength(200);
        });
        b.Navigation(x => x.AccessPoints).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
