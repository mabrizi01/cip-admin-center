# ADR-0003: Use PostgreSQL and Entity Framework Core

Status: Accepted

## Context

A relational store with code-based migrations is needed, portable across clouds.

## Decision

Use PostgreSQL through EF Core with the Npgsql provider. Fluent mappings, strongly typed id converters, repositories and migrations live in Infrastructure; Domain entities are mapped directly (private constructors/setters, backing fields) without EF references. Migrations are code-based; they are applied on startup only when `Database__ApplyMigrationsOnStartup=true`. Infrastructure tests run against a real PostgreSQL given by `CIP_TEST_POSTGRES` (skipped when unset); Testcontainers was not introduced to avoid extra initial complexity.

## Consequences

Direct mapping avoids duplicate persistence models; if the Domain needs to diverge from the schema, separate persistence models can be added in Infrastructure.
