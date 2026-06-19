# CampusLibrary

Teaching project for a modular DDD-oriented ASP.NET Core Web API.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

The current version contains two functional modules:

* Readers module
* Catalog module

The application provides:

* ASP.NET Core Web API
* API versioning
* Swagger/OpenAPI documentation
* SQLite persistence with EF Core
* repository and read model infrastructure
* write-side use cases
* query-side read models
* controller/API tests with `WebApplicationFactory` and `HttpClient`
* manual `.http` files for didactic API testing

Final automated test result:

```text
139 tests
0 failed
0 skipped
```

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

The solution is a project-based modular monolith.

The Web/API layer exposes HTTP endpoints. The Core modules contain domain model, use cases, DTOs and ports. BuildingBlocks contains reusable abstractions. Infrastructure implements EF Core persistence, repositories, read models and database configuration. Tests verify behavior across domain, application, infrastructure and API boundaries.

Central dependency rule:

```text
Core modules do not depend on Web/API or Infrastructure.
Infrastructure implements outbound ports defined by Core modules.
The executable API project wires all modules together.
```

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
* deactivate a reader
* query active readers
* query readers including inactive readers
* find readers by id or email

A Reader is deactivated by changing its business state. Normal read endpoints show active readers. Special `with-inactive` endpoints include inactive readers.

## Catalog module

The Catalog module manages the library catalog.

It contains:

* Book aggregate
* BookItem entity
* ISBN value object
* Book use cases
* Book read model
* Book repository
* Books controller
* Catalog tests

A Book represents the bibliographic work. A BookItem represents a physical copy of a book.

## Catalog domain model

## Book

`Book` is an aggregate root.

A book contains:

* author text
* title
* optional subtitle
* ISBN
* physical book items
* active state

The author text is stored as one string.

Examples:

```text
Robert C. Martin
Martin Fowler, Kent Beck
Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides
```

The domain validates that at least one author name is provided.

## BookItem

`BookItem` is an entity inside the `Book` aggregate.

It contains:

* inventory number
* status

The status is modeled as an enum:

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

A new book item starts with status `Available`.

## ISBN value object

`IsbnVo` protects the rule that a book must have a valid ISBN.

The domain should not work with arbitrary strings when a value has a specific business meaning.

## Relationship: Book to BookItem

The relationship between `Book` and `BookItem` is one-to-many.

```text
Book 1 --- n BookItem
```

A `BookItem` belongs to a `Book`. It is added through the `Book` aggregate.

## Commands and queries

Use cases change the state of the system.

```text
ReaderUseCases
- CreateAsync
- UpdateAsync
- DeactivateAsync

BookUseCases
- CreateAsync
- AddBookItemAsync
- DeactivateAsync
```

Read models return data for display, search and selection.

```text
ReaderReadModel
- FindByIdAsync
- FindByEmailAsync
- SelectAllAsync
- FindByIdWithInactiveAsync
- SelectAllWithInactiveAsync

BookReadModel
- FindByIdAsync
- SelectAllAsync
- SearchAsync
```

Central distinction:

```text
Use cases change state.
Read models read and project data.
Repositories load aggregates.
Controllers translate HTTP requests and responses.
```

## Catalog search

Books can be searched by one explicit search field:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` searches the author text by lastname rule. The author text is split by commas. Each author token is split by spaces. The last word of each author token is treated as the lastname.

Examples:

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

## API endpoints

Base route:

```text
/camplib/v1
```

Readers:

```http
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Books:

```http
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
PATCH /camplib/v1/books/{bookId}/deactivate
```

## Manual HTTP files

For manual API tests, reset or delete the database first.

Execution order:

```text
1. Books.http
2. Readers.http
```

## Testing

Run all automated tests:

```bash
dotnet test
```

Final result:

```text
139 tests
0 failed
0 skipped
```

The tests cover domain, value objects, use cases, repositories, read models, controller/API behavior and manual HTTP scenarios.
