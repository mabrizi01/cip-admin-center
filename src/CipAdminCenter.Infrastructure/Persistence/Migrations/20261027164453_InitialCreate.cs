using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CipAdminCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    external_scheme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "solution_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solution_packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "solutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "agreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agreements_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    scope_application_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scope_functionality_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    scope_kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    scope_solution_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entitlements_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    external_scheme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenants_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solution_package_solutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionPackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solution_package_solutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_solution_package_solutions_solution_packages_SolutionPackag~",
                        column: x => x.SolutionPackageId,
                        principalTable: "solution_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "context_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SelectedVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SelectedWorkloadId = table.Column<Guid>(type: "uuid", nullable: true),
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_context_configurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_context_configurations_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gateway_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gateway_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gateway_definitions_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: true),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedOn = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchases_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchases_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solution_availabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solution_availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_solution_availabilities_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solution_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solution_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_solution_items_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: true),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscriptions_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Generation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BaseAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workloads_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agreement_conditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AgreementId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agreement_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agreement_conditions_agreements_AgreementId",
                        column: x => x.AgreementId,
                        principalTable: "agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "billing_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billing_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_billing_profiles_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_billing_profiles_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    external_scheme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_facilities_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "context_configuration_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AllowOverride = table.Column<bool>(type: "boolean", nullable: false),
                    ContextConfigurationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_context_configuration_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_context_configuration_entries_context_configurations_Contex~",
                        column: x => x.ContextConfigurationId,
                        principalTable: "context_configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gateway_access_points",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PathPrefix = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GatewayDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gateway_access_points", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gateway_access_points_gateway_definitions_GatewayDefinition~",
                        column: x => x.GatewayDefinitionId,
                        principalTable: "gateway_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Variant = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SolutionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_versions_solution_items_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "solution_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_application_versions_solutions_SolutionId",
                        column: x => x.SolutionId,
                        principalTable: "solutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workload_versions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    application_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkloadId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workload_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workload_versions_workloads_WorkloadId",
                        column: x => x.WorkloadId,
                        principalTable: "workloads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    external_scheme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_units_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agreement_conditions_AgreementId",
                table: "agreement_conditions",
                column: "AgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_agreements_CustomerId",
                table: "agreements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_application_versions_ApplicationId",
                table: "application_versions",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_application_versions_SolutionId",
                table: "application_versions",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_billing_profiles_CustomerId",
                table: "billing_profiles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_billing_profiles_TenantId",
                table: "billing_profiles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_context_configuration_entries_ContextConfigurationId",
                table: "context_configuration_entries",
                column: "ContextConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_context_configurations_SolutionId",
                table: "context_configurations",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_entitlements_CustomerId",
                table: "entitlements",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_facilities_TenantId",
                table: "facilities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_gateway_access_points_GatewayDefinitionId",
                table: "gateway_access_points",
                column: "GatewayDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_gateway_definitions_SolutionId",
                table: "gateway_definitions",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_purchases_CustomerId",
                table: "purchases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_purchases_SolutionId",
                table: "purchases",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_solution_availabilities_SolutionId",
                table: "solution_availabilities",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_solution_items_SolutionId",
                table: "solution_items",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_solution_package_solutions_SolutionPackageId",
                table: "solution_package_solutions",
                column: "SolutionPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_solutions_Code",
                table: "solutions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_CustomerId",
                table: "subscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_SolutionId",
                table: "subscriptions",
                column: "SolutionId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_CustomerId",
                table: "tenants",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_units_FacilityId",
                table: "units",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_workload_versions_WorkloadId",
                table: "workload_versions",
                column: "WorkloadId");

            migrationBuilder.CreateIndex(
                name: "IX_workloads_SolutionId",
                table: "workloads",
                column: "SolutionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agreement_conditions");

            migrationBuilder.DropTable(
                name: "application_versions");

            migrationBuilder.DropTable(
                name: "billing_profiles");

            migrationBuilder.DropTable(
                name: "context_configuration_entries");

            migrationBuilder.DropTable(
                name: "entitlements");

            migrationBuilder.DropTable(
                name: "gateway_access_points");

            migrationBuilder.DropTable(
                name: "purchases");

            migrationBuilder.DropTable(
                name: "solution_availabilities");

            migrationBuilder.DropTable(
                name: "solution_package_solutions");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "workload_versions");

            migrationBuilder.DropTable(
                name: "agreements");

            migrationBuilder.DropTable(
                name: "solution_items");

            migrationBuilder.DropTable(
                name: "context_configurations");

            migrationBuilder.DropTable(
                name: "gateway_definitions");

            migrationBuilder.DropTable(
                name: "solution_packages");

            migrationBuilder.DropTable(
                name: "facilities");

            migrationBuilder.DropTable(
                name: "workloads");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "solutions");

            migrationBuilder.DropTable(
                name: "customers");
        }
    }
}
