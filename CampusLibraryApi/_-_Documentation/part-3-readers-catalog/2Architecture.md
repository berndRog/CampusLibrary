# Architecture: CampusLibrary Part 3 — Readers + Catalog Modular Monolith

This document describes the architecture of Part 3 of the CampusLibraryApi.

Part 3 extends the project-based modular monolith from Part 2 by adding a second functional business module: Catalog.

Part 2 introduced stronger architectural boundaries by moving the structured Readers monolith into separate projects for Web/API, BuildingBlocks, Core, Infrastructure and Tests.

Part 3 keeps that modular structure and adds a richer domain model with books, authors, physical book items, an ISBN value object, a one-to-many relationship and a many-to-many relationship.

This means:

* one deployable application
* multiple projects
* one database
* two business modules: Readers and Catalog
* stronger modular boundaries through project references
* a richer domain model in the Catalog module
* unchanged Readers behavior
* existing and new tests remain green

The current test suite contains:

```text
155 tests
0 failed
0 skipped
```

## Architectural Goal

The architecture of Part 3 is intended to make the following concepts visible in teaching:

* how to add a second business module to a modular monolith
* how to keep existing module behavior stable while extending the system
* how to model a richer domain with aggregates, entities and value objects
* how to model a one-to-many relationship inside an aggregate
* how to model a many-to-many relationship without turning the join table into a domain entity
* how to separate domain relationships from persistence details
* how to keep Core modules independent from EF Core and database configuration
* how to separate write-oriented use cases from read-oriented read models
* how to test domain, application, infrastructure and API behavior across modules
* how to document the HTTP API with Swagger/OpenAPI

Part 3 therefore answers this question:

```text
How can a second business module with richer domain relationships be added to a project-based modular monolith without breaking the existing architecture?
```

## Current Project Structure

Current state with the modules Readers and Catalog:

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure
   ├─ DiSwagger.cs
   └─ other application-level registrations

CampusLibraryApi_1_Web
└─ Controllers
   ├─ ReadersController.cs
   ├─ AuthorsController.cs
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
      └─ DomainError.cs

CampusLibraryApi_3_Core_Readers
├─ _1_Ports
├─ _2_Application
└─ _3_Domain

CampusLibraryApi_3_Core_Catalog
├─ _1_Ports
│  └─ Outbound
│     ├─ IBookRepository.cs
│     ├─ IAuthorRepository.cs
│     ├─ IBookReadModel.cs
│     ├─ IAuthorReadModel.cs
│     └─ ICatalogDbContext.cs
│
├─ _2_Application
│  ├─ Dtos
│  │  ├─ AuthorCreateDto.cs
│  │  ├─ AuthorDto.cs
│  │  ├─ BookAssignAuthorDto.cs
│  │  ├─ BookCreateDto.cs
│  │  ├─ BookDetailDto.cs
│  │  ├─ BookDto.cs
│  │  ├─ BookItemAddDto.cs
│  │  ├─ BookItemDto.cs
│  │  ├─ BookListItemDto.cs
│  │  └─ BookSearchDto.cs
│  ├─ Mappings
│  └─ UseCases
│     ├─ AuthorUcCreate.cs
│     ├─ AuthorUcDeactivate.cs
│     ├─ AuthorUseCases.cs
│     ├─ BookUcCreate.cs
│     ├─ BookUcAddBookItem.cs
│     ├─ BookUcAssignAuthor.cs
│     ├─ BookUcDeactivate.cs
│     └─ BookUseCases.cs
│
└─ _3_Domain
   ├─ Entities
   │  ├─ Author.cs
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
   ├─ Catalog
   │  └─ BookAuthorJoin.cs
   ├─ Configurations
   │  ├─ ConfigAuthor.cs
   │  ├─ ConfigBook.cs
   │  ├─ ConfigBookItem.cs
   │  └─ ConfigReader.cs
   ├─ Database
   │  ├─ AppDbContext.cs
   │  └─ UnitOfWorkEf.cs
   ├─ ReadModels
   │  ├─ ReaderReadModelEf.cs
   │  ├─ AuthorReadModelEf.cs
   │  └─ BookReadModelEf.cs
   ├─ Repositories
   │  ├─ ReaderRepositoryEf.cs
   │  ├─ AuthorRepositoryEf.cs
   │  └─ BookRepositoryEf.cs
   └─ Seed.cs

CampusLibraryApiTest
└─ Tests for domain, value objects, use cases, repositories, read models and controller/API scenarios
```

## Why This Is Still a Modular Monolith

Part 3 is still a monolith because the application is deployed as one application.

There is still:

```text
one deployable application
one database
one runtime process
```

However, it is modular because the solution is split into separate projects and business modules with explicit dependency rules.

The important difference from Part 2 is this:

```text
Part 2: one business module, Readers.
Part 3: two business modules, Readers and Catalog.
```

With two modules, the modular structure becomes more meaningful. The architecture is no longer only preparation for future modules. It now has to support real module separation.

## Project Responsibilities

Part 3 uses the following main projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

Each project has a clear responsibility.

## CampusLibraryApi

`CampusLibraryApi` is the executable application project.

It contains the composition root of the application.

Typical responsibilities are:

* configure the application host
* load configuration
* register controllers
* register Swagger/OpenAPI
* register API versioning
* register modules
* register infrastructure
* build and run the application

`CampusLibraryApi` wires the application together.

It may reference all other production projects because it is responsible for composing the running application.

It must not contain domain logic.

## CampusLibraryApi_1_Web

`CampusLibraryApi_1_Web` contains the HTTP API surface.

In Part 3, this includes:

```text
ReadersController
AuthorsController
BooksController
```

The Web project is responsible for translating HTTP requests into application calls.

Typical responsibilities are:

* define routes
* receive DTOs
* call read models for GET requests
* call use cases for write requests
* translate Result errors into HTTP responses
* return DTOs or ProblemDetails
* document success and error responses for Swagger/OpenAPI

The Web project does not contain business rules.

For example, the controller does not decide whether an ISBN is valid. That belongs to the Catalog domain model.

## CampusLibraryApi_2_BuildingBlocks

`CampusLibraryApi_2_BuildingBlocks` contains reusable architectural building blocks.

Typical contents are:

* Result
* DomainError
* WebErrorStatus
* Entity
* AggregateRoot
* IClock
* IUnitOfWork

These types are not specific to Readers or Catalog.

They are reusable concepts for all current and future modules.

The important rule is:

```text
BuildingBlocks must not depend on a concrete business module.
```

BuildingBlocks are general architectural elements. They are not the place for reader-specific, catalog-specific or loan-specific business logic.

## CampusLibraryApi_3_Core_Readers

`CampusLibraryApi_3_Core_Readers` is the first business module.

It contains the reader-specific domain model, application use cases, DTOs, mappings and ports.

The Readers module remains stable in Part 3.

The important rule is:

```text
Adding Catalog must not require changing the Readers domain model.
```

Readers is still responsible for readers only.

It does not know books, authors, book items or catalog persistence details.

## CampusLibraryApi_3_Core_Catalog

`CampusLibraryApi_3_Core_Catalog` is the second business module.

It contains the catalog-specific domain model, application use cases, DTOs, mappings and ports.

The Catalog module is structured internally into:

```text
_1_Ports
_2_Application
_3_Domain
```

The important rule is:

```text
The Catalog core module does not depend on Web or Infrastructure.
```

This keeps the Catalog module independent from HTTP, EF Core, SQLite and Swagger.

## Catalog Domain

The domain part of the Catalog module contains:

* Book
* Author
* BookItem
* IsbnVo
* BookItemStatus
* CatalogErrors

The Catalog domain contains business rules and domain validation.

It does not know:

* controllers
* EF Core
* HTTP
* Swagger
* database details
* dependency injection

The domain model should be understandable without knowing how the data is stored or how HTTP requests are received.

## Book as Aggregate Root

`Book` is an Aggregate Root.

It represents the bibliographic work.

A book has:

* title
* optional subtitle
* ISBN
* authors
* book items
* active state

The aggregate is created through a factory method:

```csharp
Book.Create(...)
```

It is changed through explicit domain methods, for example:

```csharp
Book.AddBookItem(...)
Book.AssignAuthor(...)
Book.Deactivate(...)
```

This avoids uncontrolled changes through public setters.

The didactic rule is:

```text
Domain state should be changed through explicit domain methods, not by setting properties from the outside.
```

## Author as Aggregate Root

`Author` is an Aggregate Root.

It represents a person who can be assigned to books.

An author has:

* firstname
* lastname
* display name
* active state

Authors are not physically deleted in the Catalog module.

They are deactivated through a domain method:

```csharp
Author.Deactivate(...)
```

This changes the business state instead of deleting the database row.

## BookItem as Entity

`BookItem` is an Entity inside the Book aggregate.

It represents a physical item or copy of a book.

A book item has:

* inventory number
* status

The current status is represented by an enum:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

The enum is stored as an integer in the database.

This keeps the database compact and stable while the code expresses the meaning through the enum names.

## ISBN as Value Object

`IsbnVo` is a Value Object.

It encapsulates validation and normalization rules for ISBN values.

The goal is to avoid spreading ISBN validation logic across controllers, use cases and repositories.

The didactic rule is:

```text
If a value has business meaning and rules, model it as a value object.
```

## Catalog Relationships

Part 3 introduces two important relationship types.

## Book to BookItem: One-to-Many

The relationship between `Book` and `BookItem` is one-to-many.

```text
Book 1 --- n BookItem
```

This relationship belongs inside the Book aggregate.

A book item is added through the Book aggregate:

```csharp
Book.AddBookItem(...)
```

The didactic meaning is:

```text
Book is responsible for consistency inside its aggregate boundary.
BookItem belongs to Book.
BookItem is not managed independently through its own use case facade.
```

## Book to Author: Many-to-Many

The relationship between `Book` and `Author` is many-to-many.

```text
Book n --- m Author
```

The domain exposes this relationship through:

```csharp
Book.Authors
```

A book is assigned to an author through:

```csharp
Book.AssignAuthor(...)
```

The database stores the relationship through a join table.

The join type is implemented in Infrastructure:

```text
BookAuthorJoin
```

The important design decision is:

```text
BookAuthorJoin is an infrastructure detail.
BookAuthorJoin is not a domain entity.
BookAuthorJoin has no own business identity.
```

The database relationship uses the composite key:

```text
BookId + AuthorId
```

This makes the persistence model explicit without polluting the domain model with a technical join entity.

## Why BookAuthorJoin Is Not a Domain Entity

A domain entity should have its own business identity and business meaning.

In this project, the relationship between Book and Author is important, but the join row itself has no separate domain lifecycle.

There is no separate business concept such as "AuthorshipAssignment" with its own attributes and rules.

Therefore:

```text
Book and Author are domain concepts.
The Book-Author relationship is a domain relationship.
The join table is a persistence mechanism.
```

The infrastructure join class exists only because EF Core needs to map the many-to-many table explicitly.

## Deactivate Instead of Delete

In the Catalog module, Books and Authors are not physically deleted.

Instead, they are deactivated:

```text
IsActive = false
```

This has two consequences:

```text
Repositories can still load the aggregate.
Read models decide what is visible in normal queries.
```

Normal lists and searches return only active Books and Authors.

This distinction is important:

```text
Deactivate changes business state.
Delete removes data physically.
```

The Catalog module uses Deactivate to preserve historical and referential information.

## Repositories and Read Models

Part 3 keeps a clear distinction between repositories and read models.

## Repositories

Repositories are used on the write side.

They load aggregates for use cases.

Examples:

```text
IBookRepository
IAuthorRepository
```

Typical repository responsibilities are:

* add an aggregate
* add multiple aggregates for seed or tests
* find an aggregate by id
* check uniqueness rules
* keep EF Core tracking for write workflows

Repositories return domain objects.

They are not optimized for display.

## Read Models

Read models are used on the read side.

They return DTOs for display, search and selection.

Examples:

```text
IBookReadModel
IAuthorReadModel
```

Typical read model responsibilities are:

* query active books
* query active authors
* search books
* search authors
* project database data into DTOs
* use AsNoTracking for read-only queries

Read models return DTOs, not domain objects.

The didactic rule is:

```text
Repositories load aggregates for changes.
Read models return DTOs for queries.
```

## Use Cases and Read Models

Part 3 keeps the same write/read separation as Part 2.

```text
Use Case  = write-oriented application workflow
ReadModel = read-oriented DB-to-DTO projection
```

Therefore:

```text
GET requests               → ReadModel
POST / PUT / PATCH / DELETE → Use Case
```

For Catalog:

```text
GET /camplib/v1/books
→ BooksController
→ IBookReadModel.SelectAllAsync

POST /camplib/v1/books
→ BooksController
→ IBookUseCases.CreateAsync

POST /camplib/v1/books/{bookId}/items
→ BooksController
→ IBookUseCases.AddBookItemAsync

POST /camplib/v1/books/{bookId}/authors
→ BooksController
→ IBookUseCases.AssignAuthorAsync

PATCH /camplib/v1/books/{bookId}/deactivate
→ BooksController
→ IBookUseCases.DeactivateAsync
```

This distinction is important for teaching.

GET requests should not accidentally become domain workflows. They query data and return DTOs.

Write requests protect domain consistency.

## Application Layer in Catalog

The application part of the Catalog module coordinates use cases.

It contains:

* DTOs
* use cases
* mapping helpers
* use case facades

Examples:

* BookUcCreate
* BookUcAddBookItem
* BookUcAssignAuthor
* BookUcDeactivate
* BookUseCases
* AuthorUcCreate
* AuthorUcDeactivate
* AuthorUseCases

Use cases are responsible for workflows.

Typical responsibilities of a use case are:

* validate basic input
* resolve optional ids
* load aggregates
* create value objects
* check uniqueness rules through repositories
* call domain methods
* save changes through IUnitOfWork
* return DTOs

Use cases should not contain detailed domain rules if those rules belong in the domain model.

## Use Case Facades

The module exposes facade interfaces for command use cases:

```text
IBookUseCases
IAuthorUseCases
```

These interfaces contain only write operations.

They do not contain query operations.

The rule is:

```text
Commands belong to UseCases.
Queries belong to ReadModels.
```

This avoids turning the use case facade into a generic service interface for everything the controller needs.

The controller may depend on both:

```text
IBookUseCases for commands.
IBookReadModel for queries.
```

This is intentional.

The didactic rule is:

```text
Not everything a controller needs is a use case.
```

## DTOs in Catalog

The Catalog module uses different DTOs for different use cases.

Examples:

```text
BookCreateDto       → input for creating a book
BookDto             → result of write operations
BookDetailDto       → detailed read model result
BookListItemDto     → list and search result
BookItemAddDto      → input for adding a physical book item
BookItemDto         → representation of a physical book item
BookAssignAuthorDto → input for assigning an author to a book
AuthorCreateDto     → input for creating an author
AuthorDto           → author representation
```

The important point is:

```text
DTOs are shaped for use cases and queries.
They are not forced to mirror the domain model exactly.
```

For example, `BookDetailDto` contains authors, book items and calculated counts.

`BookListItemDto` contains compact data for list and search results.

## BookAssignAuthorDto

The endpoint for assigning an author to a book is:

```text
POST /camplib/v1/books/{bookId}/authors
```

The book id comes from the route.

Therefore, the request body only needs the author id:

```csharp
public sealed record BookAssignAuthorDto(
   Guid AuthorId
);
```

There is no BookAuthor id.

The join table is an infrastructure detail and has no API identity.

## Infrastructure in Part 3

`CampusLibraryApi_4_Infrastructure` implements technical details for all current modules.

This includes:

* EF Core configurations
* AppDbContext
* repositories
* read models
* UnitOfWorkEf
* seed data
* join table mapping

The Infrastructure project may know EF Core.

The Core modules must not know EF Core.

The dependency direction remains:

```text
Core modules define ports.
Infrastructure implements ports.
```

## DbContext Access

There is one shared technical database and one shared EF Core DbContext.

Each module defines its own logical DbContext port.

Readers defines:

```text
IReadersDbContext
```

Catalog defines:

```text
ICatalogDbContext
```

`AppDbContext` implements both interfaces.

This allows each Core module to depend only on the part of the DbContext it needs.

The didactic idea is:

```text
Even with one physical DbContext, modules can define their own logical view of the database.
```

## EF Core Configuration

EF Core configuration belongs to Infrastructure.

Examples:

```text
ConfigReader
ConfigAuthor
ConfigBook
ConfigBookItem
```

The domain model should not contain EF Core-specific configuration.

The many-to-many relationship between Book and Author is configured in Infrastructure through `BookAuthorJoin`.

The Book to BookItem relationship is configured as a one-to-many relationship.

The BookItem status enum is stored as an integer.

This keeps the database compact while the code remains expressive.

## Dependency Rules

The most important project dependency rules are:

```text
BuildingBlocks does not depend on any business module.

Readers depends on BuildingBlocks.

Catalog depends on BuildingBlocks.

Infrastructure depends on BuildingBlocks, Readers and Catalog.

Web depends on Readers, Catalog and BuildingBlocks.

The executable API project wires all projects together.

Tests may reference all projects that are required for testing.
```

A simplified dependency direction is:

```text
CampusLibraryApi_2_BuildingBlocks
        ↑
        │
CampusLibraryApi_3_Core_Readers
        ↑
        │
CampusLibraryApi_4_Infrastructure

CampusLibraryApi_2_BuildingBlocks
        ↑
        │
CampusLibraryApi_3_Core_Catalog
        ↑
        │
CampusLibraryApi_4_Infrastructure
```

The Web/API side calls into modules through ports and use case facades.

The Infrastructure side implements outbound ports defined by the modules.

The Core modules remain independent from Web and Infrastructure.

## Write Side

Write workflows go through use cases.

```text
Controller
→ Use Case Facade
→ Concrete Use Case
→ Domain / Aggregate
→ Repository
→ EF Core
→ UnitOfWork
```

Example for creating a book:

```text
POST /camplib/v1/books
→ BooksController
→ IBookUseCases.CreateAsync
→ BookUcCreate
→ IsbnVo.Create(...)
→ Book.Create(...)
→ IBookRepository
→ BookRepositoryEf
→ UnitOfWorkEf
```

Example for assigning an author:

```text
POST /camplib/v1/books/{bookId}/authors
→ BooksController
→ IBookUseCases.AssignAuthorAsync
→ BookUcAssignAuthor
→ IBookRepository.FindByIdAsync(...)
→ IAuthorRepository.FindByIdAsync(...)
→ Book.AssignAuthor(...)
→ UnitOfWorkEf
```

Example for adding a book item:

```text
POST /camplib/v1/books/{bookId}/items
→ BooksController
→ IBookUseCases.AddBookItemAsync
→ BookUcAddBookItem
→ IBookRepository.FindByIdAsync(...)
→ Book.AddBookItem(...)
→ UnitOfWorkEf
```

Example for deactivating a book:

```text
PATCH /camplib/v1/books/{bookId}/deactivate
→ BooksController
→ IBookUseCases.DeactivateAsync
→ BookUcDeactivate
→ IBookRepository.FindByIdAsync(...)
→ Book.Deactivate(...)
→ UnitOfWorkEf
```

## Read Side

Read workflows go through read models.

```text
Controller
→ ReadModel
→ DbContext
→ DTO
```

Example for book search:

```text
GET /camplib/v1/books/search?searchField=Title&searchText=clean
→ BooksController
→ IBookReadModel.SearchAsync
→ BookReadModelEf
→ AppDbContext
→ BookListItemDto
```

Example for author search:

```text
GET /camplib/v1/authors/search?searchText=Martin
→ AuthorsController
→ IAuthorReadModel.SearchAsync
→ AuthorReadModelEf
→ AppDbContext
→ AuthorDto
```

The read side does not load aggregates for normal query responses.

It projects database data into DTOs.

## API Versioning and Swagger

The API uses versioned routes.

Current routes use:

```text
/camplib/v1
```

The current HTTP API contains endpoints for:

* Readers
* Authors
* Books

Swagger/OpenAPI is configured for documentation and manual testing.

The controllers contain XML comments and response annotations.

Swagger documents:

* successful responses
* ProblemDetails error responses
* 400 Bad Request
* 401 Unauthorized
* 403 Forbidden
* 404 Not Found
* 409 Conflict

Swagger is not the architecture itself. It documents the HTTP surface of the application.

The architecture rule remains:

```text
Swagger documents the API.
Controllers translate HTTP.
Use cases write.
Read models read.
```

## Current HTTP API

The current HTTP API supports the following endpoint groups.

## Readers

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

## Authors

```text
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

## Books

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Error Handling

Expected business errors are returned through `Result`.

Controllers translate failed results into HTTP responses.

Errors are returned as `ProblemDetails`.

The decision is intentionally visible in the controller.

Example mapping:

```text
BadRequest   → 400
Unauthorized → 401
Forbidden    → 403
NotFound     → 404
Conflict     → 409
```

This duplication is intentional for teaching.

The goal is that students can see which domain error status becomes which HTTP response.

## Testing Architecture

Part 3 keeps and extends the existing test strategy.

Typical test groups are:

* Domain tests
* Value Object tests
* Use case mock tests
* Use case integration tests
* Repository integration tests
* Read model integration tests
* Controller / API tests

The current test suite verifies:

* Reader domain behavior
* Catalog domain behavior
* Email and address validation
* ISBN validation
* create use cases
* update use cases
* deactivate use cases
* book-author assignment
* book item creation
* repository behavior
* read model projections
* inactive data filtering on the read side
* HTTP controller behavior
* Swagger-documented API behavior

The latest known test status for Part 3 is:

```text
155 tests
0 failed
0 skipped
```

The intended result is:

```text
The architecture grows.
Existing behavior stays stable.
New behavior is covered by tests.
```

## Version

Part 3 is represented by the following branch and planned tag:

```text
Branch: part-3/readers-catalog
Tag:    v3-readers-catalog
```

Part 2 remains available as:

```text
Tag: v2-readers-modular-monolith
```

Part 1 remains available as:

```text
Tag: v1-readers-monolith
```

## Planned Evolution

Part 3 is the foundation for the next teaching step.

The planned evolution is:

```text
Part 1: Readers, one-project monolith
Part 2: Readers, project-based modular monolith
Part 3: Readers + Catalog
Part 4: Readers + Catalog + Loans
Part 5: AuthN + AuthZ
```

Part 4 will add a third business module.

That step is important because the architecture will then show relationships between modules.

## Rules for Extending Part 3

New business modules should follow the same structure as Readers and Catalog.

A new core module should have its own project, for example:

```text
CampusLibraryApi_3_Core_Loans
```

Its internal structure should follow the same pattern:

```text
_1_Ports
_2_Application
_3_Domain
```

Infrastructure implements the ports of the core modules.

The important rule remains:

```text
Core modules define ports.
Infrastructure implements ports.
Core modules do not depend on Infrastructure.
```

Web controllers are placed in the Web project.

Controllers contain no domain logic. They translate HTTP requests into calls to use cases or read models.

## Architecture Rules

The application is one deployable application.

The solution is split into multiple projects.

Project boundaries represent architectural boundaries.

Business modules are represented as separate Core projects.

Web translates HTTP and contains no domain logic.

BuildingBlocks contains reusable architectural base types.

Core modules contain domain and application logic.

Domain does not know Web, Infrastructure, EF Core or Swagger.

Use cases write domain state.

Read models read data directly as DTO projections.

Repositories are used on the write side.

Read models are used on the read side.

Infrastructure implements Core ports.

EF Core configuration belongs to Infrastructure.

Join tables are persistence details unless they have their own business identity.

Program.cs wires modules together but contains no domain logic.

Additional modules should follow the same structure as Readers and Catalog.

AuthN/AuthZ will be added later without changing the basic structure.

## Didactic Rule of Thumb

Use cases protect domain rules on the write side.

Read models provide simple DTOs on the read side.

Or shorter:

```text
Use cases write.
Read models read.
```

For Part 3, another rule is important:

```text
The domain shows the business relationship.
Infrastructure shows the persistence mechanism.
```

For the Book-Author relationship this means:

```text
Book.Authors is part of the domain model.
BookAuthorJoin is part of Infrastructure.
```

And for modularization:

```text
Adding a new module should extend the architecture, not weaken the boundaries.
```
