# ADR-0007: Defer authentication and authorisation

Status: Accepted

## Context

No requirements define identity providers, roles or permissions.

## Decision

Introduce only `IPlatformAccessGuard` in Application (called by context-scoped use cases) with a permissive placeholder in Infrastructure. Do not define roles, permission matrices or identity infrastructure until requirements exist.

## Consequences

The extension point exists, but the platform must not be exposed beyond development until a real guard replaces the placeholder.
