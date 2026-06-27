# CampusLibrary — Part 3: Readers + Catalog

Teaching project for a modular, DDD-oriented ASP.NET Core Web API.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

This version contains two functional modules:

* Readers
* Catalog

It represents the state before the Loans module is introduced. The Catalog module has already been simplified: there is no separate Author aggregate and no many-to-many Book-Author relationship. Author names are stored directly in `Book.AuthorsText`.

Final automated test result for this part:

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

The Web/API layer exposes HTTP endpoints. The Core modules contain the domain model, use cases, DTOs and ports. BuildingBlocks contains reusable abstractions. Infrastructure implements EF Core persistence, repositories, read models and database configuration. Tests verify behavior across domain, application, infrastructure and API boundaries.

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

A Reader uses an `IsActive` flag. Deactivation is a soft-delete style business operation: the reader remains stored, but normal read endpoints hide inactive readers. Special `with-inactive` endpoints include them.

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

A Book represents the bibliographic work. A BookItem represents one physical copy of a book.

## Catalog domain model

### Book

`Book` is an aggregate root.

A Book contains:

* author text (`AuthorsText`)
* title
* optional subtitle
* ISBN
* physical book items
* `IsActive`
* audit timestamps

Books can be deactivated. Normal catalog queries hide inactive books.

### BookItem

`BookItem` is an entity inside the Book aggregate.

It contains:

* id
* book id
* inventory number
* status

BookItems do not use `IsActive`. Their lifecycle is represented by `BookItemStatus`, for example `Available`, `Unavailable`, `Lost` or `Damaged`.

### AuthorsText instead of Author aggregate

Part 3 deliberately does not contain an `Author` aggregate.

The simplified model is:

```text
Book
- AuthorsText
- Title
- Subtitle
- IsbnVo
- BookItems
```

This avoids a second many-to-many relationship before introducing authentication and authorization in a later part.

Author-last-name search is implemented by parsing `AuthorsText`:

```text
"Martin Fowler, Kent Beck"
-> Fowler
-> Beck
```

The last whitespace-separated token of each comma-separated author entry is treated as the last name.

## API overview

Endpoint groups:

```text
Readers
Books
```

Important endpoints:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}

GET    /camplib/v1/books
GET    /camplib/v1/books/{id}
GET    /camplib/v1/books/search?searchField=Title&searchText=...
GET    /camplib/v1/books/search?searchField=Isbn&searchText=...
GET    /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
POST   /camplib/v1/books
POST   /camplib/v1/books/{bookId}/items
PATCH  /camplib/v1/books/{bookId}/deactivate
```

`DELETE /readers/{id}` performs a deactivation, not a physical delete.

## Testing

The test project covers:

* domain tests
* value object tests
* use case mock tests
* use case integration tests
* repository integration tests
* read model integration tests
* controller/API end-to-end tests
* manual `.http` files

Run all tests:

```bash
dotnet test
```

## Manual HTTP files

For reproducible manual tests, reset or delete the database before running the HTTP files.

Recommended execution order for Part 3:

```text
1. Readers.http
2. Books.http
```

For future teaching material it is useful to separate seed setup and actual tests, for example:

```text
01_Seed_Readers.http
02_Seed_Books.http
11_Readers_Api.http
12_Books_Api.http
```
