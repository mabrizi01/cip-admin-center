# ADR-0004: Containerised, cloud-portable deployment

Status: Accepted

## Context

Azure may be the first cloud, but AWS, GCP and other OCI platforms must remain possible.

## Decision

Ship OCI images (Dockerfiles for API and web), configure via environment variables, expose `/health`, and provide a compose file for local development with PostgreSQL. No Kubernetes, no cloud-specific services in Domain/Application. Any future cloud service goes behind an Application port implemented in Infrastructure/Integration. CI is a generic build-and-test workflow without a deployment target.

## Consequences

The application is portable by default; deployment automation is added later per target.
