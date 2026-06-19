# Testing Strategy

This document describes the testing strategy used in the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes. The project therefore separates domain tests, application use case tests, infrastructure integration tests, and controller/end-to-end tests.

## Overview

The current test project is:

```text
CampusLibraryApiTest
```

The tests cover the following areas:

```text
Domain tests
Application use case tests with mocks
Application integration tests with SQLite and UnitOfWork
Infrastructure tests for repositories and read models
Controller/end-to-end tests with WebApplicationFactory
```

At the current project state, all tests pass:

```text
Test summary: total: 72, failed: 0, succeeded: 72, skipped: 0
```

Run all tests with:

```bash
dotnet test
```

## Test Levels

### 1. Domain Tests

Domain tests verify the behavior of domain objects without infrastructure.

Typical examples:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Domain tests focus on business rules:

```text
required values
valid value ranges
normalization
invalid input
partial updates
domain errors
soft delete state changes
```

The domain layer does not use EF Core, ASP.NET Core, repositories, controllers, or HTTP.

The main goal is to verify that aggregates and value objects protect their own invariants.

For the current reader lifecycle, domain tests also verify that a reader can be deactivated and that an already deactivated reader cannot be deactivated again.

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

The purpose is to check that the use case coordinates the workflow correctly:

```text
load aggregate
validate input
create value objects
check uniqueness
call domain methods
save changes only after successful domain operations
return DTOs or errors
```

For example, `ReaderUcUpdate` checks whether a new email address is already used by another reader before updating the aggregate.

`ReaderUcDeactivate` checks the id, loads the reader, calls `Reader.Deactivate(...)`, saves only if the domain operation succeeds, and returns domain errors such as `ReaderNotFound` or `IsAlreadyDeactivated`.

## 3. Application Integration Tests

Application integration tests use real infrastructure parts where useful.

They verify that use cases work together with:

```text
real repository implementation
real UnitOfWork
SQLite test database
EF Core tracking
```

This is useful because some bugs only appear when EF Core, the repository, and the UnitOfWork interact.

These tests are slower than pure domain tests, but they give more confidence that application and persistence work together.

For deactivation, integration tests verify that the reader is no longer visible through normal read model queries, but can still be found through `WithInactive` queries.

## 4. Infrastructure Tests

Infrastructure tests verify the persistence adapters.

Typical areas:

```text
ReaderRepositoryEf
ReaderReadModelEf
AppDbContext
EF Core mappings
SQLite behavior
```

The repository is part of the write side.

The read model is part of the query side.

This separation is intentional:

```text
Repository -> domain-oriented write access
ReadModel  -> DTO-oriented query access
```

Infrastructure tests help verify that entities, value objects, conversions, and queries work correctly with the database.

For the current soft delete behavior, read model tests verify two different views:

```text
normal queries       -> only active readers
WithInactive queries -> active and inactive readers
```

## 5. Controller / End-to-End Tests

Controller tests use:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
```

These tests start the ASP.NET Core application in a test host and call the API through HTTP.

They verify:

```text
routing
model binding
controller actions
status codes
JSON serialization
ProblemDetails mapping
dependency injection
database integration
```

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

These tests are closest to real API usage.

The `DELETE` endpoint is tested as a deactivation endpoint, not as a physical database delete.

## Test Database

The tests use SQLite through the test infrastructure.

The test database is created by:

```text
TestDatabase
TestBaseFactory
```

The factory replaces selected production services:

```text
AppDbContext
IUnitOfWork
IClock
TestSeed
Authentication
```

A fake clock is used to make timestamps deterministic.

This is important because the domain expects UTC timestamps.

Example:

```csharp
public DateTime TestCreatedAt { get; set; } =
   new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);
```

## Test Seed

The test seed provides stable demo and test data.

Typical readers:

```text
Reader1
Reader2
Reader3
Reader4
Reader5
Reader6
ReaderRegister
```

The tests should prefer seed data over manually constructed ad hoc data.

This keeps the examples consistent and easier to understand for students.

## Partial Update Tests

`ReaderUpdateDto` supports partial updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

The meaning of `null` is:

```text
Lastname = null   -> keep current lastname
Email = null      -> keep current email
AddressDto = null -> keep current address
```

Only provided values are changed.

An empty or whitespace lastname is not the same as `null`.

```text
null       -> no change
""         -> invalid value
"   "      -> invalid value
"Meier"   -> valid change
```

This distinction is important for partial update semantics.

## Deactivation Tests

The current project uses soft delete behavior for readers.

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

This makes the difference between physical delete and domain-level deactivation visible.

## Why Different Test Types?

Each test type answers a different question.

```text
Domain test:
Does the business rule work?

Use case mock test:
Does the application workflow call the right ports and handle errors?

Integration test:
Does the use case work with real persistence?

Infrastructure test:
Does EF Core store and load the data correctly?

Controller/E2E test:
Does the API behave correctly from the outside?
```

Together, these tests form a teaching-oriented test strategy.

## Recommended Workflow

During development:

```bash
dotnet test
```

For API changes, also run the application and inspect Swagger:

```bash
dotnet run --project CampusLibraryApi
```

Swagger is available in development mode at:

```text
https://localhost:8010/swagger
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
```

The tests are therefore not only a safety net, but also part of the learning material.
