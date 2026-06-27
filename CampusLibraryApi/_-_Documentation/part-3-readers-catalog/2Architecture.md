# Architecture: CampusLibrary Part 3 — Readers + Catalog Modular Monolith

This document describes the architecture of Part 3 of `CampusLibraryApi`.

The application is a project-based modular monolith with two business modules: Readers and Catalog. It is deployed as one ASP.NET Core application and uses one database.

Final automated test result:

```text
139 tests
0 failed
0 skipped
```

## Architectural goal

The architecture makes the following concepts visible for teaching:

* project-based modular monolith
* module boundaries through project references
* independent Core modules
* shared BuildingBlocks
* aggregates, entities and value objects
* one-to-many relationship inside an aggregate
* write-side use cases and read-side read models
* repositories for aggregate loading
* Infrastructure as implementation of outbound ports
* HTTP API as adapter
* automated tests across all relevant layers

## Current project structure

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure

CampusLibraryApi_1_Web
└─ Controllers
   ├─ ReadersController.cs
   └─ BooksController.cs

CampusLibraryApi_2_BuildingBlocks
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
├─ _1_Ports
├─ _2_Application
└─ _3_Domain

CampusLibraryApi_3_Core_Catalog
├─ _1_Ports
│  └─ Outbound
│     ├─ IBookRepository.cs
│     ├─ IBookReadModel.cs
│     └─ ICatalogDbContext.cs
├─ _2_Application
│  ├─ Dtos
│  ├─ Enums
│  ├─ Mappings
│  └─ UseCases
└─ _3_Domain
   ├─ Entities
   │  ├─ Book.cs
   │  └─ BookItem.cs
   ├─ Enums
   │  └─ BookItemStatus.cs
   ├─ Errors
   │  └─ CatalogErrors.cs
   └─ ValueObjects
      └─ IsbnVo.cs

CampusLibraryApi_4_Infrastructure
└─ Persistence
   ├─ Configurations
   ├─ Database
   ├─ ReadModels
   ├─ Repositories
   └─ Seed.cs

CampusLibraryApiTest
```

## Modular monolith

The application has one deployable unit, one database and one runtime process. The code is modular because business capabilities are separated into projects and modules.

```text
Readers contains reader-specific code.
Catalog contains catalog-specific code.
BuildingBlocks contains reusable architectural types.
Infrastructure implements technical adapters.
Web exposes the HTTP API.
```

## Dependency direction

```text
Web/API
  -> Core modules
  -> BuildingBlocks

Infrastructure
  -> Core modules
  -> BuildingBlocks

Core modules
  -> BuildingBlocks
```

Core modules do not reference Web or Infrastructure.

## Ports and adapters

The Core modules define ports. Infrastructure implements outbound ports.

Examples:

```text
IReaderRepository    -> ReaderRepositoryEf
IReaderReadModel     -> ReaderReadModelEf
IBookRepository      -> BookRepositoryEf
IBookReadModel       -> BookReadModelEf
IReaderDbContext     -> AppDbContext
ICatalogDbContext    -> AppDbContext
```

Controllers call use case facades or read models. They do not call EF Core directly.

## Write model and read model

The project intentionally separates write-side behavior and query-side projections.

Write side:

```text
Controller -> UseCase -> Repository -> Aggregate -> UnitOfWork
```

Read side:

```text
Controller -> ReadModel -> DTO projection
```

Repositories return aggregates. ReadModels return DTOs.

## Readers module

Readers owns the Reader aggregate and related value objects.

Reader uses `IsActive` to support deactivation. Normal read model queries return only active readers. Additional `with-inactive` queries include inactive readers.

Typical use cases:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
```

## Catalog module

Catalog owns Books and BookItems.

Book is an aggregate root. BookItem is an entity inside the Book aggregate.

Important modeling decisions:

```text
Book uses IsActive.
BookItem uses BookItemStatus.
There is no Author aggregate.
Authors are represented as Book.AuthorsText.
```

This keeps Part 3 focused on one aggregate with a one-to-many child entity.

## IsActive versus Status

The project uses two different modeling concepts:

```text
Reader / Book:
- IsActive
- deactivation hides records from normal read models

BookItem:
- Status
- describes the state of a physical copy
```

This distinction becomes important when Loans are introduced in Part 4.

## Infrastructure

Infrastructure implements persistence and technical adapters.

It contains:

* EF Core configurations
* `AppDbContext`
* repositories
* read models
* unit of work
* clock implementation
* seed data

The same database is used by all modules, but module ownership is expressed through ports and code boundaries.

## Composition root

The executable `CampusLibraryApi` project wires everything together.

It registers:

* controllers
* API versioning
* Swagger/OpenAPI
* Core modules
* Infrastructure
* EF Core and SQLite

## Teaching focus of Part 3

Part 3 shows how a second module is added to the modular monolith without introducing distributed systems complexity.

Students see:

* two independent Core modules
* one shared runtime
* one shared database
* clear module ownership
* aggregate boundaries
* read/write separation
* API and integration testing
