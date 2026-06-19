# Architecture: CampusLibrary Part 2 — Readers Modular Monolith

This document describes the architecture of **Part 2** of the `CampusLibraryApi`.

Part 2 continues the `Readers` module from Part 1 and moves the internal architectural boundaries into separate projects. The application is still one deployable modular monolith, but the project structure now makes dependencies and module boundaries more explicit.

```text
one deployable application
multiple projects
one database
one first domain module: Readers
```

## Architectural Goal

Part 2 is intended to make the following concepts visible in teaching:

- how a one-project monolith can evolve into a project-based modular monolith
- how to separate Web, BuildingBlocks, Core, Infrastructure and Tests into projects
- how to keep the domain model independent from technical infrastructure
- how to model a first domain module with explicit ports
- how to distinguish write-oriented use cases from read-oriented read models
- how to model soft delete behavior through a domain operation
- how to prepare the codebase for additional modules such as Catalog, Loans and AuthN/AuthZ

## Solution Structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

The important dependency principle is:

```text
Core defines ports.
Infrastructure implements ports.
Web calls ports.
Domain does not depend on Web or Infrastructure.
```

## Project Responsibilities

### CampusLibraryApi

Application entry project. It configures and starts the application. It should not contain domain logic.

### CampusLibraryApi_1_Web

Contains HTTP controllers. `ReadersController` translates HTTP requests into calls to `IReaderReadModel` and `IReaderUseCases`.

### CampusLibraryApi_2_BuildingBlocks

Contains reusable building blocks such as `Result`, `DomainError`, `Entity`, `AggregateRoot`, `IClock` and `IUnitOfWork`.

### CampusLibraryApi_3_Core_Readers

Contains the Readers module Core:

```text
_1_Ports
_2_Application
_3_Domain
```

The Core defines abstractions such as `IReaderRepository`, `IReaderReadModel`, `IReaderUseCases` and `IReaderDbContext`. It also contains the domain model: `Reader`, `EmailVo`, `AddressVo` and `ReaderErrors`.

### CampusLibraryApi_4_Infrastructure

Contains technical implementations such as `AppDbContext`, `UnitOfWorkEf`, `ReaderRepositoryEf`, `ReaderReadModelEf`, EF Core configurations, migrations and seed data.

### CampusLibraryApiTest

Contains tests for domain behavior, use case orchestration, repository behavior, read model behavior and controller/end-to-end behavior.

## The Readers Module

The module contains:

- `Reader` as Aggregate Root
- `EmailVo` and `AddressVo` as Value Objects
- `ReaderErrors` as domain errors
- `ReaderUcCreate`, `ReaderUcUpdate`, `ReaderUcDeactivate`
- `ReaderUseCases` as write-side facade
- `IReaderRepository` for the write side
- `IReaderReadModel` for the read side
- `ReaderRepositoryEf` and `ReaderReadModelEf` as EF Core implementations

## Current HTTP API

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

`DELETE /camplib/v1/readers/{id}` triggers a deactivation. It does not physically remove the reader from the database.

## Reader as Aggregate Root

`Reader` owns the consistency rules for reader profile data and reader lifecycle state.

It is created and changed through domain methods:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
```

> Domain state should be changed through explicit domain methods, not by setting properties from the outside.

## Use Cases and Read Models

```text
Use Case  = write-oriented application workflow
ReadModel = read-oriented DB-to-DTO projection
```

```text
GET                  -> ReadModel
POST / PUT / DELETE  -> Use Case
```

## Write Side

```text
Controller
→ Use Case
→ Domain / Aggregate
→ Repository
→ EF Core
→ UnitOfWork
```

Example for deactivate:

```text
DELETE /camplib/v1/readers/{id}
→ ReadersController
→ ReaderUseCases.DeactivateAsync
→ ReaderUcDeactivate
→ Reader.Deactivate(...)
→ IReaderRepository
→ ReaderRepositoryEf
→ UnitOfWorkEf
```

## Soft Delete and `IsActive`

A reader is not physically deleted from the database. Instead, `Reader.Deactivate(...)` sets `IsActive` to `false`.

```text
Deactivate = domain operation
Inactive   = state after deactivation
DELETE     = HTTP verb used to trigger the operation
```

Normal read model queries return only active readers.

Special read model queries include inactive readers:

```text
FindByIdWithInactiveAsync
SelectAllWithInactiveAsync
```

This preserves historical information for later modules such as `Loans`, while still giving normal clients a clean active-reader view.

## Read Side

Read models project database data directly into DTOs and typically use `.AsNoTracking()`.

Normal read methods return only active readers. Methods with `WithInactive` in the name return active and inactive readers.

## Migrations

Part 2 uses EF Core migrations in the Infrastructure project.

Because the reader lifecycle now contains `IsActive`, the database model and migrations must reflect this property.

> If the domain model changes persistent state, the database schema and migrations must be updated as well.

## Testing Architecture

The current test suite verifies:

```text
Reader domain behavior
Email and address validation
Create use case
Update use case
Deactivate use case
Soft delete behavior through IsActive
Repository behavior
Read model projections
HTTP controller behavior
```

Latest known test status:

```text
72 tests
0 failed
```

## Architecture Rules

1. The application is still one deployable monolith.
2. The solution is split into separate projects.
3. Web translates HTTP and contains no domain logic.
4. Core contains domain and application logic.
5. Domain does not know Web, Infrastructure, EF Core, or Swagger.
6. Core defines ports.
7. Infrastructure implements Core ports.
8. Use cases write domain state.
9. Deactivation is a write use case and changes domain state.
10. Normal read model queries return only active readers.
11. `WithInactive` read model queries return active and inactive readers.
12. EF Core configuration and migrations belong to Infrastructure.

## Didactic Rule of Thumb

```text
Use cases write.
Read models read.
Deactivate changes state.
Read models decide visibility.
```
