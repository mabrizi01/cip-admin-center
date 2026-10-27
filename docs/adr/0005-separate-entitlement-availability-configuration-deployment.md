# ADR-0005: Separate entitlement, availability, configuration and deployment

Status: Accepted

## Context

These are often conflated, but a right to use something, where it is offered, how it is configured and what serves it change independently.

## Decision

Model four distinct concepts: `Entitlement` (commercial right of a Customer, scoped to Solution/Application/functionality), `SolutionAvailability` (per Tenant/Facility/Unit context), `ContextConfiguration` (per Solution and context, with inheritance and lockable overrides, plus a version/workload selection) and `Workload` (deployment metadata with version associations). Relationships are enforced by use cases/policies, not by merging models: availability requires an active entitlement (`AvailabilityPolicy`), target resolution requires availability, then effective configuration, then `WorkloadSelector`. Entitlement alone never makes a Solution available.

## Consequences

The platform can answer where a Solution is actually available without treating the concepts as one. More types to maintain.
