# ADR-0001: Use a modular monolith

Status: Accepted

## Context

The platform needs clear capability boundaries (catalogue, organisations, commercial, entitlements, configuration, routing, platform access, deployment registry) but no requirement justifies independent deployment, scaling or ownership of those capabilities.

## Decision

Build one deployable API process. Modules are logical boundaries, organised as folders/namespaces by business capability inside each layer project. Modules interact through explicit Application use cases and the small shared `ContextRef`/`OrganisationPath` types of the Organisations module. No microservices, no message broker.

## Consequences

Simple local development, deployment and transactions. Module boundaries rely on discipline and tests; they can later be extracted if a concrete need appears.
