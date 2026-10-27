# ADR-0002: Separate Domain, Application, Infrastructure, Integration and API

Status: Accepted

## Context

Business rules must not depend on PostgreSQL, ASP.NET Core, cloud SDKs or gateway implementation details.

## Decision

Five projects with enforced references: Domain (none); Application -> Domain; Infrastructure -> Application; Integration -> Application; Api -> Application, Infrastructure, Integration. Application defines ports (repositories, `IUnitOfWork`, `IWorkloadClient`, `IPlatformAccessGuard`) that Infrastructure and Integration implement. Application commands use primitives/DTOs. Use cases are plain classes; no mediator framework. Repositories are per-module and only for real use cases (no generic repository).

## Consequences

Domain and Application are testable without infrastructure. The cost is some mapping code and more projects, kept to five.
