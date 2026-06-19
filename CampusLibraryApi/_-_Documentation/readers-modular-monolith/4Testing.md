# Testing Strategy

This document describes the testing strategy used in the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes.

## Overview

The current test project is:

```text
CampusLibraryApiTest
```

The tests cover:

```text
Domain tests
Application use case tests with mocks
Application integration tests with SQLite and UnitOfWork
Infrastructure tests for repositories and read models
Controller/end-to-end tests with WebApplicationFactory
```

Current status:

```text
Test summary: total: 72, failed: 0, succeeded: 72, skipped: 0
```

Run all tests with:

```bash
dotnet test
```

## 1. Domain Tests

Domain tests verify the behavior of domain objects without infrastructure.

Typical examples:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Domain tests focus on business rules, normalization, invalid input, domain errors and soft delete state changes.

## 2. Application Use Case Tests with Mocks

Application use case tests verify the orchestration logic of use cases.

Typical examples:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
```

These tests use mocks for ports such as:

```text
IReaderRepository
IUnitOfWork
IClock
```

`ReaderUcDeactivate` checks the id, loads the reader, calls `Reader.Deactivate(...)`, saves only if the domain operation succeeds, and returns domain errors such as `InvalidId`, `ReaderNotFound` or `IsAlreadyDeactivated`.

## 3. Application Integration Tests

Application integration tests use real infrastructure parts where useful:

```text
real repository implementation
real UnitOfWork
SQLite test database
EF Core tracking
```

For deactivation, integration tests verify that the reader is no longer visible through normal read model queries, but can still be found through `WithInactive` queries.

## 4. Infrastructure Tests

Infrastructure tests verify persistence adapters:

```text
ReaderRepositoryEf
ReaderReadModelEf
AppDbContext
EF Core mappings
SQLite behavior
migrations
```

For the current soft delete behavior, read model tests verify two different views:

```text
normal queries       -> only active readers
WithInactive queries -> active and inactive readers
```

## 5. Controller / End-to-End Tests

Controller tests use `WebApplicationFactory<Program>` and call the API through HTTP.

They verify routing, model binding, controller actions, status codes, JSON serialization, ProblemDetails mapping, dependency injection and database integration.

The current Reader controller tests cover:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

The `DELETE` endpoint is tested as a deactivation endpoint, not as a physical database delete.

## Deactivation Tests

The central rule is:

```text
Deactivate changes the reader state.
Read models decide visibility.
```

The tests verify:

```text
Reader.Deactivate(...) succeeds for active readers
Reader.Deactivate(...) fails for already inactive readers
ReaderUcDeactivate returns InvalidId for Guid.Empty
ReaderUcDeactivate returns ReaderNotFound for unknown readers
ReaderUcDeactivate returns IsAlreadyDeactivated for inactive readers
normal read model queries hide inactive readers
WithInactive read model queries still return inactive readers
```

## Didactic Goals

The test suite is intended to help students understand:

```text
separation of test levels
domain testing without infrastructure
mock-based use case testing
integration testing with SQLite
controller testing through HTTP
test data reuse through seed objects
why fake clocks are useful
how partial updates should be tested
how soft delete behavior should be tested
how active and inactive query views differ
why migrations must be updated when persistent domain state changes
```
