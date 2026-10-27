# CIP Admin Center

CIP Admin Center is a **StarHealth** software platform for the governance, configuration and deployment management of modular healthcare software **Solutions**. It is administrative and infrastructural: it provides common governance, configuration, entitlement, access-mediation and deployment-management capabilities **without** containing the clinical business logic of the Solutions it manages.

> The acronym "CIP" is not expanded anywhere: no expansion has been provided. The product name is "CIP Admin Center".

Organisations are modelled as **Customer → Tenant → Facility → Unit**. Customer and Tenant are distinct concepts: a Customer is the contracting party; Tenants, Facilities and Units are the contexts in which Solutions are made available and configured.

## Functional capabilities (initial foundation)

1. **Product catalogue** – Solutions composed of Applications, Gateways and Components; application versions with implementation variants (e.g. legacy / next).
2. **Organisations** – the hierarchy above, with an optional external reference mapping a CIP role to a real organisation.
3. **Commercial management** – Agreements (free-text conditions), Solution Packages, Billing Profiles (Customer-owned, optionally tied to a Tenant). No invoicing, payments or pricing.
4. **Entitlements and availability** – Purchases, Subscriptions, Entitlements (Solution / Application / functionality) and the organisational availability of Solutions.
5. **Contextual configuration** – simple key/value settings at Tenant, Facility and Unit level with inheritance, controlled (lockable) overrides, and selection of the application version or workload.
6. **Access and routing** – Gateway/access-point definitions plus a port (`IWorkloadClient`) and HTTP adapter to forward a request to the configured workload.
7. **Platform access** – an extension point (`IPlatformAccessGuard`) for future authentication/authorisation. No identity infrastructure yet.
8. **Deployment registry** – workload metadata (type, legacy/current generation, address) and version-to-workload associations. Metadata only: no orchestration.

The four concepts below are deliberately **separate** (see ADR-0005):

| Concept | Question it answers | Where |
| --- | --- | --- |
| Entitlement | May the Customer use it (commercial right)? | `Domain/Entitlements` |
| Availability | In which contexts is the Solution offered? | `Domain/Entitlements` (`SolutionAvailability`) |
| Configuration | How is it configured in that context? | `Domain/Configuration` |
| Deployment | Which workload serves that context? | `Domain/Deployment` |

## Technology stack

- C# / .NET 10, ASP.NET Core minimal APIs, OpenAPI (`Microsoft.AspNetCore.OpenApi`)
- PostgreSQL with Entity Framework Core (Npgsql provider), code-based migrations
- React, Next.js, TypeScript (scaffold only)
- Docker (OCI) images and Docker Compose for local development; xUnit for tests

## Architecture: modular monolith

One deployable API process. Modules are **logical** boundaries organised by business capability as folders/namespaces inside each layer project (no microservices, no message broker, no Kubernetes).

| Module | Responsibility |
| --- | --- |
| Catalogue | Solutions, items (Application/Gateway/Component), application versions |
| Organisations | Customer, Tenant, Facility, Unit, `OrganisationPath` (validated hierarchy), `ContextRef` |
| Commercial | Agreements, Solution Packages, Billing Profiles |
| Entitlements | Purchases, Subscriptions, Entitlements, Solution availability |
| Configuration | Context configuration, inheritance, overrides, effective-configuration resolution |
| Routing | Gateway definitions, access points, target resolution/forwarding use cases |
| Deployment | Workloads, version-to-workload associations, workload selection |
| Platform access | `IPlatformAccessGuard` extension point |

### Layers and dependency rules

```
Api ──► Application ──► Domain
 │          ▲
 ├──► Infrastructure ─┘   (implements Application ports, uses EF Core / PostgreSQL)
 └──► Integration  ───┘   (implements Application ports for external systems/workloads)
```

- **Domain** – business concepts and invariants. References nothing (no EF Core, ASP.NET Core, cloud or vendor SDKs; no gateway implementation details; no clinical logic).
- **Application** – use cases (plain classes with `ExecuteAsync`, no mediator) and the ports they need (`I*Repository`, `IUnitOfWork`, `IWorkloadClient`, `IPlatformAccessGuard`). Depends on Domain only. Commands/results use primitives and DTOs.
- **Infrastructure** – EF Core `CipDbContext`, fluent mappings, repositories, migrations, health check, options validation.
- **Integration** – adapters for external systems and workloads (`HttpWorkloadClient`). No business rules.
- **Api** – transport models, validation, endpoints, OpenAPI, composition root. No business logic.

Project references enforce this: Domain has none; Application → Domain; Infrastructure and Integration → Application; Api → Application, Infrastructure, Integration.

## Repository structure

```
src/
  CipAdminCenter.Domain/          modules: Organisations, Catalogue, Deployment, Commercial, Entitlements, Configuration, Routing
  CipAdminCenter.Application/     use cases + ports, same module folders
  CipAdminCenter.Infrastructure/  Persistence (DbContext, mappings, repositories, Migrations), Access placeholder
  CipAdminCenter.Integration/     HttpWorkloadClient
  CipAdminCenter.Api/             Program.cs, Endpoints/, Contracts/, Dockerfile
tests/
  CipAdminCenter.Domain.Tests/
  CipAdminCenter.Application.Tests/                  in-memory fakes of the ports
  CipAdminCenter.Infrastructure.IntegrationTests/    real PostgreSQL
  CipAdminCenter.Integration.Tests/                  HTTP adapter with a stub handler
  CipAdminCenter.Api.IntegrationTests/               real API host on in-memory SQLite
web/cip-admin-center-web/         Next.js scaffold
docs/adr/                         architecture decision records
docs/architecture/                API routes and notes
```

## Configuration

All configuration uses environment variables / .NET configuration providers. Nothing secret is committed.

| Setting | Purpose |
| --- | --- |
| `Database__ConnectionString` | **Required.** Npgsql connection string. Startup fails (options validation) if missing. |
| `Database__ApplyMigrationsOnStartup` | `true` applies pending EF migrations at startup (local development). Default `false`. |
| `ASPNETCORE_URLS` | Listen address. |

`compose.yaml` reads `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` (required) and `POSTGRES_PORT` from `.env`. Copy `.env.example` to `.env` and set a local password (`.env` is git-ignored). The frontend uses `CIP_API_BASE_URL` (`web/cip-admin-center-web/.env.example`).

## Build and run locally

Prerequisites: .NET 10 SDK, Docker, Node.js 22 (frontend).

**Everything in containers**

```bash
cp .env.example .env        # set POSTGRES_PASSWORD
docker compose up --build   # db :5432, api :8080, web :3000
```

**API on the host, PostgreSQL in a container**

```bash
docker compose up -d db
export Database__ConnectionString="Host=localhost;Port=5432;Database=cip_admin_center;Username=cip;Password=<your POSTGRES_PASSWORD>"
export Database__ApplyMigrationsOnStartup=true
dotnet run --project src/CipAdminCenter.Api --urls http://localhost:8080
```

Then:

- `GET http://localhost:8080/health` – health (includes a PostgreSQL check)
- `GET http://localhost:8080/openapi/v1.json` – OpenAPI document
- Routes are listed in [docs/architecture/api-routes.md](docs/architecture/api-routes.md)

**Frontend**: `cd web/cip-admin-center-web && npm install && npm run dev`.

## Database migrations

Migrations are code-based and live in `src/CipAdminCenter.Infrastructure/Persistence/Migrations`. The design-time factory reads `Database__ConnectionString` (a placeholder is used if unset, which is enough to add migrations).

```bash
dotnet tool install --global dotnet-ef
cd src/CipAdminCenter.Infrastructure
dotnet ef migrations add <Name> -o Persistence/Migrations
# apply: set Database__ApplyMigrationsOnStartup=true, or
dotnet ef database update
```

No data is seeded: no fabricated organisations or commercial data.

## Tests

```bash
dotnet test                      # all projects
```

- Domain tests: hierarchy validation, configuration inheritance and overrides, entitlement vs availability, version/workload selection.
- Application tests: use cases against in-memory fakes of the ports.
- Integration tests: HTTP adapter with a stub handler.
- API tests: the real host with in-memory SQLite (no external services).
- Infrastructure tests need PostgreSQL. **Approach:** an existing server (e.g. the compose `db`) is used; set `CIP_TEST_POSTGRES` to a connection string *without* a database name. Each run creates and drops its own database. Without the variable these tests are skipped. This avoids a Docker-in-test dependency (Testcontainers) at this stage.

```bash
export CIP_TEST_POSTGRES="Host=localhost;Port=5432;Username=cip;Password=<your POSTGRES_PASSWORD>"
dotnet test tests/CipAdminCenter.Infrastructure.IntegrationTests
```

## Containerisation

OCI images for the API (`src/CipAdminCenter.Api/Dockerfile`, multi-stage, non-root runtime user) and the web scaffold (`web/cip-admin-center-web/Dockerfile`). Configuration is injected via environment variables. A baseline GitHub Actions workflow (`.github/workflows/ci.yml`) builds and tests; it assumes no cloud target.

## Cloud portability

The Domain and Application layers know nothing about any cloud. The runtime is a plain container plus a PostgreSQL connection string, so it runs on Azure, AWS, GCP or any OCI-compatible platform. Any future cloud service must sit behind an Application port and be implemented in Infrastructure/Integration.

## Known assumptions

- The Application and Entitlement scope require the item/Solution to exist in the catalogue; an entitlement to a Solution, one of its Applications or a functionality all allow the Solution to be made available to that Customer's contexts.
- Availability is recorded per context and inherited downwards; the most specific record (Unit, then Facility, then Tenant) wins, and a record may disable it. Without a record the Solution is not available.
- Configuration is keyed by (Solution, context). A value can be locked (`allowOverride=false`) to prevent overrides further down. The most specific version/workload selection wins.
- When a version has several workloads, the single `Current` generation workload is chosen; otherwise a workload must be selected explicitly.
- The catalogue models Applications, Gateways and Components as `SolutionItem` with a kind; `GatewayDefinition` (routing) is not linked to a catalogue item yet.
- Domain entities are mapped directly by EF Core fluent configuration in Infrastructure (no separate persistence models). The Domain contains no EF references.

## Intentionally deferred

- Authentication, authorisation, user roles, permission matrices, identity providers (only `IPlatformAccessGuard` and a permissive placeholder exist; **do not deploy beyond development with it**).
- Agreement conditions and commercial rules, pricing, invoicing, payments, accounting.
- A fixed configuration schema (values are plain strings) or any rules engine.
- Gateway protocols, external Solution interfaces, an API gateway product or routing engine (only the port/adapter and a resolution use case exist; there is no public forwarding endpoint).
- Deployment orchestration (metadata and associations only).
- Endpoints for Packages, Billing Profiles, Purchases, Subscriptions and Gateway definitions (domain and persistence exist; use cases come with concrete requirements).
- Clinical logic and patient data – out of scope.
- The administration frontend beyond a scaffold; production CI/CD.
