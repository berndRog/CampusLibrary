# Testing Strategy: CampusLibrary Part 2

This document describes the testing strategy used in **Part 2 – Readers Modular Monolith**.

The goal is not only to verify correctness. The test suite also makes the architectural layers and testing levels visible for teaching purposes.

In Part 2, the application has been refactored from a folder-based monolith into a project-based modular monolith. The business scope is still limited to the Readers module.

The current test status is:

```text
Test summary: total: 70, failed: 0, succeeded: 70, skipped: 0
```

Run all tests with:

```bash
dotnet test
```

## Tested Production Projects

The production code is split across these projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
```

The test project is:

```text
CampusLibraryApiTest
```

## Test Levels

The tests cover the following levels:

```text
Domain tests
Application use case tests with mocks
Application integration tests with SQLite and UnitOfWork
Infrastructure tests for repositories and read models
Controller/end-to-end tests with WebApplicationFactory
```

## 1. Domain Tests

Domain tests verify domain behavior without Infrastructure and without ASP.NET Core.

Typical examples:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

The focus is on business rules:

```text
required values
valid value ranges
normalization
partial updates
soft-deactivation
invalid state transitions
domain errors
```

The domain layer does not depend on EF Core, controllers, repositories or HTTP.

The main teaching point is:

```text
Aggregates and value objects protect their own invariants.
```

## 2. Application Use Case Tests with Mocks

Application use case tests verify orchestration logic.

Typical use cases:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
ReaderUseCases
```

These tests use mocks or test doubles for ports such as:

```text
IReaderRepository
IUnitOfWork
IClock
```

They verify that the use case coordinates the workflow correctly:

```text
validate input
create value objects
load aggregates
check uniqueness
call domain methods
save changes
return DTOs or domain errors
```

The important teaching point is:

```text
UseCases orchestrate. They do not contain persistence code and they do not expose HTTP behavior.
```

## 3. Application Integration Tests

Application integration tests verify use cases together with real infrastructure components.

They use:

```text
real Repository implementation
real UnitOfWork
SQLite test database
EF Core tracking
Fake clock for deterministic timestamps
```

These tests are useful because some problems only appear when Application, Repository, DbContext and UnitOfWork interact.

In Part 2 this is especially relevant because the use cases live in the Readers core project while repository and UnitOfWork implementations live in the Infrastructure project.

The dependency direction remains:

```text
Core defines ports.
Infrastructure implements ports.
Tests verify that both work together.
```

## 4. Infrastructure Tests

Infrastructure tests verify persistence adapters.

Typical tested components:

```text
ReaderRepositoryEf
ReaderReadModelEf
ReaderDbContextEf
AppDbContext
ConfigReader
UtcDateTimeConverter
```

The repository is part of the write side:

```text
ReaderRepositoryEf -> Reader aggregate
```

The read model is part of the query side:

```text
ReaderReadModelEf -> ReaderDto
```

The current Reader read model behavior is important:

```text
normal queries return only active readers
special queries can include inactive readers
```

Infrastructure tests verify that this behavior works with a real SQLite database.

## 5. Controller / End-to-End Tests

Controller/end-to-end tests use the ASP.NET Core test host.

Typical infrastructure:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
Test authentication
SQLite test database
```

These tests call the API through HTTP and verify:

```text
routing
model binding
status codes
JSON serialization
ProblemDetails mapping
dependency injection
database integration
```

The current Reader controller tests cover the main API behavior:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

The `DELETE` endpoint is tested as a deactivate operation. It must not physically remove the reader.

## Test Database

The tests use SQLite.

The test infrastructure creates a test database and replaces selected runtime services.

Typical replacements are:

```text
AppDbContext
IUnitOfWork
IClock
Test seed data
Authentication
```

A fake clock is used so that `CreatedAt` and `UpdatedAt` values can be tested deterministically.

## Important Behavior Under Test

The most important current Reader behavior is:

```text
Create reader
Update reader
Deactivate reader
Reject duplicate email/subject where applicable
Keep deactivated readers in the database
Hide deactivated readers from normal read queries
Return deactivated readers only through explicit with-inactive queries
```

## Why the Test Count Changed

The current test suite contains 70 tests.

Older documentation mentioned 66 tests. During the alignment with the current Reader model, tests were updated and one duplicate historical delete test was removed.

The relevant result is the current verified state:

```text
70 total
0 failed
0 skipped
```

## Teaching Value

Part 2 is useful for teaching because the tests show that architectural refactoring can be performed safely.

Students can compare:

```text
Part 1: one project, folder-based structure
Part 2: several projects, explicit module boundaries
```

The behavior remains centered on Readers, but the architecture now prepares the project for later modules.
