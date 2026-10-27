# ADR-0006: Initial frontend scaffolding strategy

Status: Accepted

## Context

The priority is the administrative platform foundation, not the full administration experience.

## Decision

Provide a minimal Next.js/React/TypeScript app that shows API health and one use case (create Customer), using a small typed client and a Next.js rewrite (`/cip-api/*` -> `CIP_API_BASE_URL`) to avoid CORS configuration. No UI framework, state library or authentication yet.

## Consequences

Demonstrates API consumption cheaply. The UI will be built as administrative flows are specified.
