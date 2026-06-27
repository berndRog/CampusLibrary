# Architecture: CampusLibrary Part 2 — Readers Modular Monolith

This document describes the architecture of Part 2 of the `CampusLibraryApi` teaching project.

Part 2 transforms the Readers application from Part 1 into a **project-based modular monolith**. The application is still deployed as one application and uses one database, but the architectural areas are now separated into projects.

The business scope is intentionally unchanged and limited to one module:

```text
Readers
```

The purpose of Part 2 is not to introduce more business functionality. Its purpose is to make architectural boundaries visible through project references and to prepare the solution for later modules such as Catalog and Loans.

The current test status is:

```text
Test summary: total: 70, failed: 0, succeeded: 70, skipped: 0
```

## Architectural Goal

Part 2 demonstrates the following teaching goals:

- how a structured one-project monolith can be refactored into a modular monolith
- how projects can act as architectural boundaries
- how dependency direction can be expressed through project references
- how the domain model stays independent from EF Core and ASP.NET Core
- how command use cases and query read models can be separated
- how tests protect behavior during structural refactoring
- how a solution can be prepared for additional modules without adding them yet

The central question of this part is:

```text
How can a clean one-project monolith be moved into a project-based modular monolith
without changing the visible business behavior?
```

## Current Project Structure

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure

CampusLibraryApi_1_Web
└─ _1_Web
   └─ Controllers
      └─ ReadersController.cs

CampusLibraryApi_2_BuildingBlocks
└─ _2_BuildingBlocks
   ├─ Result.cs
   ├─ _1_Ports
   │  ├─ IClock.cs
   │  └─ IUnitOfWork.cs
   └─ _3_Domain
      ├─ Entities
      │  ├─ Entity.cs
      │  └─ AggregateRoot.cs
      └─ Errors

CampusLibraryApi_3_Core_Readers
└─ _3_Core
   └─ Readers
      ├─ DiReaderModule.cs
      ├─ DiReaders.cs
      ├─ _1_Ports
      │  ├─ Inbound
      │  │  └─ IReaderUseCases.cs
      │  └─ Outbound
      │     ├─ IReaderDbContext.cs
      │     ├─ IReaderReadModel.cs
      │     └─ IReaderRepository.cs
      ├─ _2_Application
      │  ├─ Dtos
      │  ├─ Mappings
      │  └─ UseCases
      │     ├─ ReaderUcCreate.cs
      │     ├─ ReaderUcUpdate.cs
      │     ├─ ReaderUcDeactivate.cs
      │     └─ ReaderUseCases.cs
      └─ _3_Domain
         ├─ Entities
         │  └─ Reader.cs
         ├─ Errors
         │  └─ ReaderErrors.cs
         └─ ValueObjects
            ├─ AddressVo.cs
            └─ EmailVo.cs

CampusLibraryApi_4_Infrastructure
└─ _4_Infrastructure
   └─ Persistence
      ├─ Converters
      │  └─ UtcDateTimeConverter.cs
      ├─ Database
      │  ├─ AppDbContext.cs
      │  └─ UnitOfWorkEf.cs
      └─ Readers
         ├─ ConfigReader.cs
         ├─ ReaderDbContextEf.cs
         ├─ ReaderReadModelEf.cs
         └─ ReaderRepositoryEf.cs

CampusLibraryApiTest
└─ Tests for domain, application, infrastructure and API behavior
```

## Why This Is a Modular Monolith

The application is still a monolith because it is deployed as one application:

```text
one deployable application
one process
one database
```

It is modular because the code is separated into projects with explicit dependency rules:

```text
Web/API
BuildingBlocks
Core_Readers
Infrastructure
Tests
```

The important change compared with Part 1 is:

```text
Part 1: architectural boundaries are represented by folders.
Part 2: architectural boundaries are represented by projects.
```

This makes unintended dependencies harder to introduce.

## Project Responsibilities

### CampusLibraryApi

`CampusLibraryApi` is the executable application project and acts as the composition root.

It is responsible for:

- hosting the ASP.NET Core application
- loading configuration
- configuring middleware
- configuring API versioning and Swagger
- composing Web, Core and Infrastructure services

### CampusLibraryApi_1_Web

`CampusLibraryApi_1_Web` contains the HTTP API layer.

In Part 2 it contains the `ReadersController`.

The controller is responsible for:

- routing
- model binding
- HTTP status codes
- ProblemDetails mapping
- calling use cases for commands
- calling read models for queries

The controller does not contain business rules.

### CampusLibraryApi_2_BuildingBlocks

`CampusLibraryApi_2_BuildingBlocks` contains shared abstractions that are independent from a concrete module.

Examples:

```text
Result<T>
IClock
IUnitOfWork
Entity
AggregateRoot
Domain errors
```

BuildingBlocks must remain small and stable. It should contain only concepts that are really shared across modules.

### CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` contains the Readers module.

It owns the Reader domain model and defines the ports needed by the module.

The module contains:

- Reader aggregate
- Email and address value objects
- reader DTOs
- mappings
- command use cases
- inbound port `IReaderUseCases`
- outbound ports `IReaderRepository`, `IReaderReadModel`, `IReaderDbContext`

The Core project does not depend on Infrastructure or Web.

### CampusLibraryApi_4_Infrastructure

`CampusLibraryApi_4_Infrastructure` implements technical details.

For the Readers module it provides:

- EF Core configuration for Reader
- EF Core DbContext integration
- Reader repository implementation
- Reader read model implementation
- UnitOfWork implementation
- UTC DateTime conversion

Infrastructure depends on Core because it implements the outbound ports defined there.

## Dependency Direction

The intended dependency direction is:

```text
CampusLibraryApi
   ├─ CampusLibraryApi_1_Web
   ├─ CampusLibraryApi_2_BuildingBlocks
   ├─ CampusLibraryApi_3_Core_Readers
   └─ CampusLibraryApi_4_Infrastructure

CampusLibraryApi_1_Web
   ├─ CampusLibraryApi_2_BuildingBlocks
   └─ CampusLibraryApi_3_Core_Readers

CampusLibraryApi_3_Core_Readers
   └─ CampusLibraryApi_2_BuildingBlocks

CampusLibraryApi_4_Infrastructure
   ├─ CampusLibraryApi_2_BuildingBlocks
   └─ CampusLibraryApi_3_Core_Readers
```

The important rule is:

```text
Core defines ports.
Infrastructure implements ports.
Web calls the public application and read model ports.
```

## Reader Domain Model

`Reader` is an aggregate root.

It represents a business reader of the library, not a technical user account.

The Reader stores profile data and references the technical identity through a subject value.

A Reader has an `IsActive` flag.

This is used for soft-deactivation:

```text
active reader      -> participates in normal read queries
inactive reader    -> remains stored but is hidden from normal read queries
```

Readers are not physically deleted in Part 2.

## Deactivate Instead of Delete

The former delete behavior has been replaced by a domain-level deactivate behavior.

This is important for teaching because it separates the HTTP verb from the business meaning:

```text
HTTP DELETE /readers/{id}
```

can still be used as a public API operation, but internally it calls:

```text
ReaderUcDeactivate
Reader.Deactivate(...)
```

The database row remains available for auditability, later references, and more realistic business behavior.

## Use Cases and Read Models

Part 2 separates command behavior from query behavior.

Command use cases:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
ReaderUseCases
```

Read models:

```text
ReaderReadModelEf
IReaderReadModel
```

The rule is:

```text
UseCases change state.
ReadModels answer queries.
```

`IReaderUseCases` is therefore an inbound command port.

`IReaderReadModel` is an outbound query port implemented by Infrastructure.

## Repository and ReadModel

The repository works with aggregates and belongs to the write side:

```text
ReaderRepositoryEf -> Reader aggregate
```

The read model returns DTOs and belongs to the query side:

```text
ReaderReadModelEf -> ReaderDto
```

This keeps write behavior and read projections separate.

## Persistence

Part 2 uses EF Core and SQLite.

The Reader table stores the Reader aggregate state, including:

- Id
- Subject
- Firstname
- Lastname
- Email
- Address data
- IsActive
- CreatedAt
- UpdatedAt

UTC DateTime handling is centralized through `UtcDateTimeConverter`.

## What Comes Later

Part 2 deliberately does not introduce additional modules.

Later parts add:

```text
Part 3: Catalog with Book and BookItem
Part 4: Loans and cross-module collaboration
Part 5: Authentication and Authorization
```

Part 2 prepares the architecture for that progression without adding those topics yet.
