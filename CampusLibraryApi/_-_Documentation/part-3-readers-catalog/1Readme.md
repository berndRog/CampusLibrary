# CampusLibrary

Teaching project for a modular DDD-oriented ASP.NET Core Web API.

The project demonstrates how a small modular monolith can be structured into separate projects for Web/API, Building Blocks, Core modules, Infrastructure and Tests while keeping the domain model independent from technical persistence details.

German version: [1readme-ger.md](1readme-ger.md)

## Current status

The project currently contains two functional modules:

* Readers module
* Catalog module
* ASP.NET Core Web API
* API versioning
* Swagger/OpenAPI documentation
* SQLite persistence with EF Core
* Repository and ReadModel infrastructure
* Use cases for write-side workflows
* ReadModels for query-side projections
* Controller/API tests with a real SQLite test database

The initial monolith has been refactored into a project-based modular monolith. Shared abstractions and base types are located in `BuildingBlocks`. The `Readers` and `Catalog` modules are independent core modules, while technical persistence details are implemented in the Infrastructure project.

The test suite currently contains:

```text
155 tests
0 failed
0 skipped
```

## Versions

* `v1-readers-monolith`
  First completed version with the Readers module inside a single monolithic project structure.

* `v2-readers-modular-monolith`
  Refactored version with a project-based modular monolith structure.

* `v3-readers-catalog`
  Adds the Catalog module with books, authors, book items, ISBN value object, read models, use cases, repositories, controllers and Swagger documentation.

## Current branch

```text
part-3/readers-catalog
```

## Project structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architectural idea

The Web/API layer exposes the HTTP endpoints.

The Core modules contain the domain model, application use cases, DTOs and ports of a business module.

The BuildingBlocks project contains shared abstractions that are independent of a specific business module.

The Infrastructure project implements technical details such as EF Core persistence, repositories, read models and database configuration.

The Test project verifies the behavior across domain, application, infrastructure and API boundaries.

The most important dependency rule is:

```text
Core modules do not depend on Web/API or Infrastructure.
Infrastructure depends on Core modules because it implements their outbound ports.
The API project acts as the composition root and wires all modules together.
```

## Modules

## Readers module

The Readers module manages library readers.

It contains:

* Reader aggregate
* Reader value objects
* Reader use cases
* Reader repository port
* Reader read model port
* Reader controller
* Reader tests

Typical operations are:

* create a reader
* update reader profile data
* delete a reader
* query readers
* find readers by id or email

The Readers module is intentionally simple and acts as the starting point for the architecture.

## Catalog module

The Catalog module manages the library catalog.

It contains:

* Book aggregate
* Author aggregate
* BookItem entity
* ISBN value object
* Book and Author use cases
* Book and Author read models
* Book and Author repositories
* Book and Author controllers
* Catalog tests

The Catalog module introduces richer domain modeling compared to the Readers module.

## Catalog domain model

### Book

`Book` is an aggregate root.

A book represents the bibliographic work and contains:

* title
* optional subtitle
* ISBN
* authors
* book items
* active state

A book can have many authors.

A book can have many physical book items.

### Author

`Author` is an aggregate root.

An author contains:

* firstname
* lastname
* display name
* active state

Authors are not physically deleted in the Catalog module. They are deactivated by setting `IsActive` to `false`.

### BookItem

`BookItem` is an entity inside the `Book` aggregate.

A book item represents a physical copy of a book.

It contains:

* inventory number
* status

The book item status is modeled as an enum:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

The enum is stored as an integer in the database. This keeps persistence compact and stable while the code still expresses the meaning through the enum names.

### ISBN value object

`IsbnVo` is a value object.

It protects the domain rule that a book must have a valid ISBN. The domain should not work with arbitrary strings when a value has a specific business meaning.

## Relationships

### Book to BookItem

The relationship between `Book` and `BookItem` is one-to-many.

```text
Book 1 --- n BookItem
```

A `BookItem` belongs to a `Book`. It is added through the `Book` aggregate.

### Book to Author

The relationship between `Book` and `Author` is many-to-many.

```text
Book n --- m Author
```

The domain exposes this relationship through `Book.Authors`.

The database stores the relationship through an infrastructure-level join table.

```text
BookAuthorJoin is an infrastructure detail.
BookAuthorJoin is not a domain entity.
BookAuthorJoin uses the composite key BookId + AuthorId.
```

## Commands and Queries

The project separates write-side commands from read-side queries.

### Use cases

Use cases change the state of the system.

Examples:

```text
ReaderUseCases
- CreateAsync
- UpdateAsync
- DeleteAsync

BookUseCases
- CreateAsync
- AddBookItemAsync
- AssignAuthorAsync
- DeactivateAsync

AuthorUseCases
- CreateAsync
- DeactivateAsync
```

Use cases work with repositories, domain objects and the unit of work.

### Read models

Read models return data for display, search and selection.

Examples:

```text
ReaderReadModel
- FindByIdAsync
- FindByEmailAsync
- SelectAllAsync

BookReadModel
- FindByIdAsync
- SelectAllAsync
- SearchAsync
- SelectByAuthorIdAsync

AuthorReadModel
- FindByIdAsync
- SelectAllAsync
- SearchAsync
```

Read models return DTOs, not domain objects.

The central distinction is:

```text
Use cases change state.
Read models read and project data.
Repositories load aggregates.
Controllers translate HTTP requests and responses.
```

## Deactivate instead of Delete

In the Catalog module, books and authors are not physically deleted.

Instead, they are deactivated:

```text
IsActive = false
```

Repositories can still load the aggregate.

Read models decide what is visible in normal queries.

Normal lists and searches return only active books and authors.

## API endpoints

The API is versioned.

Current API version:

```text
v1
```

Base route:

```text
/camplib/v1
```

### Readers

```http
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

### Authors

```http
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

### Books

```http
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Swagger and error handling

The controllers contain XML comments and Swagger response annotations.

The API documents success and error responses explicitly.

Typical error responses are:

* `400 Bad Request`
* `401 Unauthorized`
* `403 Forbidden`
* `404 Not Found`
* `409 Conflict`

Errors are returned as `ProblemDetails`.

The controllers intentionally map domain errors to HTTP responses explicitly. This makes the relationship between a domain error and the resulting HTTP status code visible for teaching purposes.

## Testing

Run all tests:

```bash
dotnet test
```

Current test result:

```text
155 tests
0 failed
0 skipped
```

The test suite covers:

* domain tests
* value object tests
* use case mock tests
* use case integration tests
* repository integration tests
* read model integration tests
* controller/API tests

## Running the application

```bash
dotnet run --project CampusLibraryApi
```

## Migrations

Create a migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project CampusLibraryApi_4_Infrastructure/CampusLibraryApi_4_Infrastructure.csproj \
  --startup-project CampusLibraryApi/CampusLibraryApi.csproj
```

Update the database:

```bash
dotnet ef database update \
  --project CampusLibraryApi_4_Infrastructure/CampusLibraryApi_4_Infrastructure.csproj \
  --startup-project CampusLibraryApi/CampusLibraryApi.csproj
```

## Key teaching points

```text
Controllers are HTTP adapters.
Use cases change state.
Read models return data for display and search.
Repositories load aggregates.
Domain objects protect business rules.
DTOs cross application boundaries.
Infrastructure implements technical details.
```

Important rules:

```text
Core modules do not depend on Infrastructure.
Queries belong to read models.
Commands belong to use cases.
Deactivate is not delete.
The domain shows the business relationship.
Infrastructure shows the persistence mechanism.
```

## Next step

The next planned module is the Loans module.

The main business goal is:

```text
A reader borrows a book item.
```

This will introduce relationships between modules and new design questions:

* Should Loan be its own aggregate?
* How should one module reference data from another module?
* Which data is referenced directly?
* Which data should be stored as a snapshot?
* How should cross-module rules be checked?
