# Architecture: CampusLibrary Part 3 — Readers + Catalog Modular Monolith

This document describes the architecture of the current CampusLibraryApi.

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

The application has one deployable unit, one database and one runtime process. The code is still modular because business capabilities are separated into projects and modules.

```text
Readers contains reader-specific code.
Catalog contains catalog-specific code.
BuildingBlocks contains reusable architectural types.
Infrastructure implements technical adapters.
Web exposes the HTTP API.
```

## Project responsibilities

## CampusLibraryApi

The executable application project. It is the composition root and wires all modules together.

Responsibilities:

* configure host and middleware
* register controllers
* register Swagger/OpenAPI
* register API versioning
* register Core modules and Infrastructure
* start the application

## CampusLibraryApi_1_Web

The HTTP adapter layer.

Responsibilities:

* define routes
* receive request DTOs
* call read models for GET requests
* call use cases for write requests
* translate `Result<T>` into HTTP responses
* return DTOs or ProblemDetails
* document the API for Swagger/OpenAPI

## CampusLibraryApi_2_BuildingBlocks

Reusable architectural types:

* Result
* DomainError
* Entity
* AggregateRoot
* IClock
* IUnitOfWork

BuildingBlocks are independent from concrete business modules.

## CampusLibraryApi_3_Core_Readers

The Readers business module.

It contains the Reader domain model, application use cases, DTOs, mappings and ports.

## CampusLibraryApi_3_Core_Catalog

The Catalog business module.

It contains the Book domain model, application use cases, DTOs, mappings and ports.

The Catalog Core module is independent from HTTP, EF Core, SQLite and Swagger.

## CampusLibraryApi_4_Infrastructure

The technical adapter layer.

Responsibilities:

* EF Core DbContext
* EF Core configurations
* repository implementations
* read model implementations
* UnitOfWork implementation
* migrations
* seed data

Infrastructure depends on Core modules because it implements their ports.

## Domain model

## Reader

`Reader` is an aggregate root.

A Reader has:

* firstname
* lastname
* email
* address
* subject
* active state
* created timestamp
* updated timestamp

Email and address are modeled with value objects.

## Book

`Book` is an aggregate root.

A Book has:

* authors text
* title
* optional subtitle
* ISBN
* book items
* active state
* created timestamp
* updated timestamp

State changes happen through domain methods:

```csharp
Book.Create(...)
Book.AddBookItem(...)
Book.Deactivate(...)
```

## AuthorsText

`AuthorsText` stores author names as text.

Examples:

```text
Robert C. Martin
Martin Fowler, Kent Beck
Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides
```

For search, the text is interpreted by a lastname rule:

```text
Split by comma.
Split each author token by spaces.
Use the last word as lastname.
```

Examples:

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

## BookItem

`BookItem` is an entity inside the Book aggregate.

It represents a physical copy of a book.

A new BookItem starts with status `Available`.

## IsbnVo

`IsbnVo` is a value object that validates and normalizes ISBN values.

## Relationship: Book to BookItem

```text
Book 1 --- n BookItem
```

The Book aggregate protects consistency for its BookItems.

## Deactivation

Readers and Books use an active state.

```text
IsActive = false
```

Repositories can load aggregates by id. ReadModels decide what is visible in normal queries.

## Repositories and ReadModels

Repositories are used on the write side. They load aggregates and keep EF Core tracking for workflows.

ReadModels are used on the read side. They project database data into DTOs and usually use no tracking.

```text
Repository -> domain-oriented write access
ReadModel  -> DTO-oriented query access
```

## Use cases and read models

```text
GET requests                -> ReadModel
POST / PUT / PATCH / DELETE -> Use Case
```

Catalog examples:

```text
GET /camplib/v1/books
-> IBookReadModel.SelectAllAsync

POST /camplib/v1/books
-> IBookUseCases.CreateAsync

POST /camplib/v1/books/{bookId}/items
-> IBookUseCases.AddBookItemAsync

PATCH /camplib/v1/books/{bookId}/deactivate
-> IBookUseCases.DeactivateAsync
```

## Database model

Current tables:

```text
Readers
Books
BookItems
```

Books columns:

```text
Id
Authors
Title
Subtitle
Isbn
IsActive
CreatedAt
UpdatedAt
```

The `Authors` column stores `Book.AuthorsText`.

BookItems columns:

```text
Id
InventoryNumber
Status
BookId
```

## Dependency rules

```text
BuildingBlocks depends on no business module.
Readers depends on BuildingBlocks.
Catalog depends on BuildingBlocks.
Infrastructure depends on BuildingBlocks, Readers and Catalog.
Web depends on Readers, Catalog and BuildingBlocks.
The executable API project wires all projects together.
Tests may reference all required projects.
```
