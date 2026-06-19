# Testing Strategy

This document describes the testing strategy used in the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes.

The current test suite verifies the Readers module and the Catalog module.

Final automated test result:

```text
Test summary: total: 139, failed: 0, succeeded: 139, skipped: 0
Build succeeded
```

## Test project

```text
CampusLibraryApiTest
```

Production projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
```

## Test levels

The test suite covers:

```text
Domain tests
Value object tests
Use case mock tests
Use case integration tests
Repository integration tests
ReadModel integration tests
Controller/API end-to-end tests
Manual HTTP files
```

Run all tests:

```bash
dotnet test
```

## 1. Domain tests

Domain tests verify domain objects without infrastructure.

Readers examples:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Catalog examples:

```text
Book.Create(...)
Book.AddBookItem(...)
Book.Deactivate(...)
BookItem.Create(...)
IsbnVo.Create(...)
```

Domain tests focus on:

```text
required values
normalization
invalid input
domain errors
aggregate invariants
value object validation
active/inactive state
UTC timestamps
```

## Catalog domain tests

Catalog domain tests verify:

```text
Book can be created with valid AuthorsText, title and ISBN
Book cannot be created without valid AuthorsText
Book cannot be created with invalid ISBN
AuthorsText is normalized
BookItem can be added to Book
BookItem starts with status Available
duplicate inventory numbers are rejected
Book can be deactivated
CreatedAt and UpdatedAt use UTC timestamps
```

## 2. Use case mock tests

Use case mock tests verify application workflow orchestration.

Readers examples:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDeactivate
```

Catalog examples:

```text
BookUcCreate
BookUcAddBookItem
BookUcDeactivate
```

Typical mocked ports:

```text
IReaderRepository
IBookRepository
IUnitOfWork
IClock
ILogger<T>
```

Use case mock tests verify:

```text
input validation
optional id handling
repository calls
uniqueness checks
domain method calls
UnitOfWork calls
returned DTOs
error results
```

## 3. Use case integration tests

Use case integration tests verify use cases with real persistence adapters.

They use:

```text
real repository implementation
real UnitOfWork
SQLite test database
EF Core tracking
real EF Core mappings
```

Catalog integration examples:

```text
creating a Book persists Book and ISBN
creating a Book persists AuthorsText
creating a Book without AuthorsText fails
adding a BookItem persists the BookItem
adding a duplicate inventory number fails
deactivating a Book updates IsActive
```

## 4. Infrastructure tests

Infrastructure tests verify persistence adapters.

Typical areas:

```text
ReaderRepositoryEf
ReaderReadModelEf
BookRepositoryEf
BookReadModelEf
AppDbContext
EF Core mappings
SQLite behavior
```

Repositories belong to the write side and return domain objects.

ReadModels belong to the read side and return DTOs.

```text
Repository -> aggregate-oriented write access
ReadModel  -> DTO-oriented query access
```

## Repository tests

Readers repository tests verify:

```text
add Reader
find Reader by id
find Reader by email
check subject uniqueness
load deactivated Reader as aggregate
```

Catalog repository tests verify:

```text
add Book
find Book by id
check ISBN uniqueness
check inventory number uniqueness
load Book with BookItems
load deactivated Book as aggregate
```

## ReadModel tests

Reader read model tests verify:

```text
select all active readers
select all readers including inactive readers
find active reader by id
find reader by id including inactive readers
find reader by email
```

Catalog read model tests verify:

```text
select all active books
find active book by id
search active books by title
search active books by author lastname
search active books by ISBN
hide inactive books from normal queries
```

## Catalog search tests

Book search supports:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` uses the lastname rule for AuthorsText.

Examples:

```text
Robert C. Martin -> Martin
Martin Fowler -> Fowler
Kent Beck -> Beck
```

A regression test verifies that a search for `Martin` returns `Clean Code`, because its author text contains `Robert C. Martin`. It does not return `Refactoring`, because its author text contains `Martin Fowler`, where `Fowler` is the lastname.

## 5. Controller/API end-to-end tests

Controller/API end-to-end tests use:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
HttpClient
```

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
HTTP contract from the outside
```

Reader API tests cover:

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

Book API tests cover:

```text
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

Manual HTTP files make API behavior visible for students.

Execution order after a database reset:

```text
1. Books.http
2. Readers.http
```

## Test database

Automated tests use SQLite through the test infrastructure.

The test factory replaces selected services:

```text
AppDbContext
IUnitOfWork
IClock
TestSeed
Authentication
```

A fake clock is used to make timestamps deterministic.

## Test seed

The test seed provides stable demo and test data.

Typical Catalog data:

```text
Book1
Book2
Book3
Book4
BookItems for Books
```

Stable seed data keeps examples consistent across domain tests, integration tests, API tests and manual HTTP files.
