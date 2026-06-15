# Testing Strategy

This document describes the testing strategy used in the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes. The project therefore separates domain tests, application use case tests, infrastructure integration tests, and controller/API tests.

In Part 3, the application has been extended from one functional module to two functional modules. The application now contains the Readers module and the Catalog module. The existing Readers behavior remains stable, while the new Catalog behavior is added and verified by tests.

Part 3 is therefore not mainly a refactoring step. It is an extension step. The test suite verifies both:

```text
existing Readers behavior still works
new Catalog behavior works correctly
```

The Catalog module introduces additional domain concepts:

```text
Book
Author
BookItem
IsbnVo
Book-to-BookItem one-to-many relationship
Book-to-Author many-to-many relationship
```

The tests also verify that the architectural rules from Part 2 still hold when a second module is added.

## Overview

The current test project is:

```text
CampusLibraryApiTest
```

The production code is split across several projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
```

The tests cover the following areas:

```text
Domain tests
Application use case tests with mocks
Application integration tests with SQLite and UnitOfWork
Infrastructure tests for repositories and read models
Controller/API tests with WebApplicationFactory
```

At the current project state, all tests pass:

```text
Test summary: total: 155, failed: 0, succeeded: 155, skipped: 0
```

Run all tests with:

```bash
dotnet test
```

## Test Levels

## 1. Domain Tests

Domain tests verify the behavior of domain objects without infrastructure.

In the Readers module, typical examples are:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

In the Catalog module, typical examples are:

```text
Book.Create(...)
Book.AddBookItem(...)
Book.AssignAuthor(...)
Book.Deactivate(...)

Author.Create(...)
Author.Deactivate(...)

BookItem.Create(...)
IsbnVo.Create(...)
```

Domain tests focus on business rules:

```text
required values
valid value ranges
normalization
invalid input
domain errors
aggregate invariants
value object validation
relationship rules
active/inactive state
```

The domain layer does not use EF Core, ASP.NET Core, repositories, controllers, Swagger, or HTTP.

The main goal is to verify that aggregates, entities and value objects protect their own invariants.

In the modular monolith structure, these tests mainly verify code from:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_2_BuildingBlocks
```

## Catalog Domain Tests

The Catalog domain tests are important because Part 3 introduces a richer domain model than Part 2.

The tests verify, for example:

```text
a Book can be created with a valid title and ISBN
a Book cannot be created with an invalid ISBN
a BookItem can be added to a Book
a BookItem starts with status Available
an Author can be assigned to a Book
the same Author cannot be assigned twice to the same Book
a Book can be deactivated
an Author can be deactivated
CreatedAt and UpdatedAt must be valid UTC timestamps
```

These tests make the aggregate boundary visible.

For example, a `BookItem` is added through the `Book` aggregate:

```text
Book.AddBookItem(...)
```

The test therefore checks not only the `BookItem`, but also that the `Book` aggregate protects the consistency of its own object graph.

## 2. Application Use Case Tests with Mocks

Application use case tests verify the orchestration logic of use cases.

In the Readers module, typical examples are:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDelete
```

In the Catalog module, typical examples are:

```text
BookUcCreate
BookUcAddBookItem
BookUcAssignAuthor
BookUcDeactivate

AuthorUcCreate
AuthorUcDeactivate
```

These tests use mocks or test doubles for ports such as:

```text
IReaderRepository
IBookRepository
IAuthorRepository
IUnitOfWork
IClock
ILogger<T>
```

The purpose is to check that the use case coordinates the workflow correctly:

```text
validate basic input
resolve optional ids
load aggregates
create value objects
check uniqueness
call domain methods
save changes
return DTOs or errors
```

For example, `BookUcCreate` checks whether the ISBN already exists before creating a new Book.

`BookUcAddBookItem` checks whether the inventory number already exists before adding a new physical book item.

`BookUcAssignAuthor` loads both the Book and the Author before calling the domain method that assigns the Author to the Book.

These tests are mostly independent from EF Core and HTTP. They focus on application logic inside the Core modules.

## What Mock-Based Use Case Tests Should Verify

Use case tests with mocks should verify both success and failure paths.

Typical success checks are:

```text
the correct repository method is called
the domain method is called through the aggregate
UnitOfWork is called once on success
the returned DTO contains the expected data
```

Typical failure checks are:

```text
invalid input returns a domain error
missing aggregate returns NotFound
duplicate data returns Conflict
UnitOfWork is not called on failure
no unnecessary repository call is made after an early failure
```

The purpose is not to test EF Core. The purpose is to test the application workflow.

## 3. Application Integration Tests

Application integration tests use real infrastructure parts where useful.

They verify that use cases work together with:

```text
real repository implementation
real UnitOfWork
SQLite test database
EF Core tracking
real EF Core mappings
```

This is useful because some bugs only appear when EF Core, the repository, and the UnitOfWork interact.

These tests are slower than pure domain tests, but they give more confidence that application and persistence work together.

In Part 3, these tests are especially important because there are now two Core modules and one shared Infrastructure project.

The intended dependency direction is still:

```text
Core modules define ports.
Infrastructure implements ports.
Tests verify that both work together correctly.
```

## Catalog Application Integration Tests

Catalog integration tests verify that Catalog use cases work correctly with the real persistence adapters.

Typical examples are:

```text
creating an Author persists the Author
creating a Book persists the Book and ISBN
adding a BookItem persists the BookItem
assigning an Author persists the Book-Author relationship
deactivating a Book updates IsActive
deactivating an Author updates IsActive
```

These tests are important because Part 3 contains relationships that must be mapped correctly by EF Core.

For example, assigning an Author to a Book involves:

```text
Book aggregate
Author aggregate
Book.Authors navigation
BookAuthorJoin table in Infrastructure
UnitOfWork
SQLite database
```

A pure domain test can verify the domain rule.

An integration test verifies that the relationship is actually persisted.

## 4. Infrastructure Tests

Infrastructure tests verify the persistence adapters.

Typical areas are:

```text
ReaderRepositoryEf
ReaderReadModelEf

BookRepositoryEf
AuthorRepositoryEf
BookReadModelEf
AuthorReadModelEf

AppDbContext
EF Core mappings
SQLite behavior
```

The repository is part of the write side.

The read model is part of the query side.

This separation is intentional:

```text
Repository -> domain-oriented write access
ReadModel  -> DTO-oriented query access
```

Infrastructure tests help verify that entities, value objects, conversions, relationships and queries work correctly with the database.

In the project-based structure, these tests mainly verify code from:

```text
CampusLibraryApi_4_Infrastructure
```

together with domain types and ports from:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_2_BuildingBlocks
```

## Repository Tests

Repository tests verify write-side persistence behavior.

For Readers, typical repository behavior is:

```text
add Reader
find Reader by id
find Reader by email
check subject uniqueness
remove Reader
```

For Catalog, typical repository behavior is:

```text
add Author
find Author by id
check author name uniqueness

add Book
find Book by id
check ISBN uniqueness
check inventory number uniqueness
load Book with Authors
load Book with BookItems
```

Repositories work with domain objects.

They are not responsible for optimized DTO projections.

## ReadModel Tests

ReadModel tests verify query-side projections.

ReadModels return DTOs directly.

Typical Reader read model tests verify:

```text
select all readers
find reader by id
find reader by email
```

Typical Catalog read model tests verify:

```text
select all active authors
find active author by id
search active authors

select all active books
find active book by id
search active books by title
search active books by author name
search active books by ISBN
select active books by author id
```

ReadModel tests are also responsible for verifying read-side visibility rules.

For example:

```text
inactive Books are not returned by normal Book read models
inactive Authors are not returned by normal Author read models
```

This is different from repository behavior.

Repositories may still load inactive aggregates because use cases may need them.

## Repository vs ReadModel Tests

Part 3 makes the distinction between repository tests and read model tests especially important.

Repositories test the write-side persistence model.

ReadModels test the read-side query model.

For Catalog, this distinction becomes visible with deactivation.

Repository tests should verify:

```text
a deactivated Book is still stored
a deactivated Book can still be loaded as an aggregate
IsActive is false

a deactivated Author is still stored
a deactivated Author can still be loaded as an aggregate
IsActive is false
```

ReadModel tests should verify:

```text
inactive Books are hidden from normal Book lists
inactive Books are hidden from normal Book search results
inactive Authors are hidden from normal Author lists
inactive Authors are hidden from normal Author search results
```

The didactic rule is:

```text
Repositories load aggregates for changes.
ReadModels decide what is visible in queries.
```

## 5. Controller / API Tests

Controller tests use:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
```

These tests start the ASP.NET Core application in a test host and call the API through HTTP.

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
Swagger-compatible API behavior
```

The Reader controller tests cover:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

The Author controller tests cover:

```text
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

The Book controller tests cover:

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=...&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

These tests are closest to real API usage.

In Part 3, controller/API tests also verify that the two modules are wired correctly by the executable API project.

They therefore check not only controller behavior, but also the composition of:

```text
Web
Readers module
Catalog module
Infrastructure
BuildingBlocks
```

## Test Database

The tests use SQLite through the test infrastructure.

The test database is created by:

```text
TestDatabase
TestBaseFactory
```

The factory replaces selected production services:

```text
AppDbContext
IUnitOfWork
IClock
TestSeed
Authentication
```

A fake clock is used to make timestamps deterministic.

This is important because the domain expects UTC timestamps.

Example:

```csharp
public DateTime TestCreatedAt { get; set; } =
   new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);
```

The fake clock is especially useful for aggregates that store audit timestamps:

```text
CreatedAt
UpdatedAt
```

## Test Seed

The test seed provides stable demo and test data.

Typical Readers are:

```text
Reader1
Reader2
Reader3
Reader4
Reader5
Reader6
ReaderRegister
```

Typical Catalog data includes:

```text
Author1
Author2
Author3

Book1
Book2
Book3

Books with Authors
Books with BookItems
```

The tests should prefer seed data over manually constructed ad hoc data.

This keeps the examples consistent and easier to understand for students.

For Catalog tests, seed data is also useful because relationships should be built from the same tracked object graph where necessary.

For example, books with authors should use existing Author instances instead of creating duplicate Author objects with the same ids.

## Partial Update Tests

`ReaderUpdateDto` supports partial updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

The meaning of `null` is:

```text
Lastname = null   -> keep current lastname
Email = null      -> keep current email
AddressDto = null -> keep current address
```

Only provided values are changed.

An empty or whitespace lastname is not the same as `null`.

```text
null       -> no change
""         -> invalid value
"   "      -> invalid value
"Meier"   -> valid change
```

This distinction is important for partial update semantics.

The tests should therefore cover both cases:

```text
field omitted or null          -> no change
field provided but invalid     -> validation error
```

## Catalog Relationship Tests

Part 3 adds relationships that must be tested explicitly.

## Book to BookItem

A Book can have multiple BookItems.

This is a one-to-many relationship:

```text
Book 1 --- n BookItem
```

The tests should verify:

```text
a BookItem can be added to an existing Book
a BookItem requires an inventory number
the inventory number must be unique
a new BookItem starts with status Available
adding a BookItem updates the Book aggregate
the relationship is persisted by EF Core
read models show total and available BookItem counts
```

## Book to Author

A Book can have multiple Authors.

An Author can be assigned to multiple Books.

This is a many-to-many relationship:

```text
Book n --- m Author
```

The tests should verify:

```text
an existing Author can be assigned to an existing Book
the same Author cannot be assigned twice to the same Book
assigning an Author updates the Book aggregate
the relationship is persisted through the BookAuthorJoin table
read models return authors in a stable order
books can be selected by Author id
```

The technical join table is not tested as a separate domain concept.

It is tested through persistence behavior.

The didactic rule is:

```text
The domain shows the relationship.
Infrastructure persists the relationship.
```

## Deactivation Tests

Part 3 uses deactivation for Catalog Books and Authors.

Deactivate is not the same as delete.

```text
IsActive = false
```

The tests should verify both sides of this decision.

Repository and use case tests verify:

```text
the aggregate still exists
IsActive is false
the change is persisted
UpdatedAt is updated
```

ReadModel tests verify:

```text
inactive Books are not returned in normal Book queries
inactive Authors are not returned in normal Author queries
```

Controller/API tests verify:

```text
PATCH /books/{bookId}/deactivate returns 200 OK
PATCH /authors/{id}/deactivate returns 200 OK
normal GET endpoints no longer return deactivated resources
```

This separation makes the design decision visible in tests.

## Why Different Test Types?

Each test type answers a different question.

```text
Domain test:
Does the business rule work?

Use case mock test:
Does the application workflow call the right ports and handle errors?

Application integration test:
Does the use case work with real persistence?

Infrastructure test:
Does EF Core store, load and project the data correctly?

Controller/API test:
Does the API behave correctly from the outside?
```

Together, these tests form a teaching-oriented test strategy.

## Why the Tests Matter in Part 3

Part 3 adds a second business module.

The expected result is:

```text
The architecture grows.
The existing behavior stays stable.
The new behavior is covered by tests.
```

The test suite is the safety net for this extension.

If all tests remain green after adding the Catalog module, this gives confidence that:

```text
Readers still works
Catalog works
module boundaries still hold
Infrastructure correctly implements ports from multiple modules
the API project wires all modules together correctly
```

The current result is:

```text
155 tests
0 failed
0 skipped
```

## Recommended Workflow

During development:

```bash
dotnet test
```

For API changes, also run the application and inspect Swagger:

```bash
dotnet run --project CampusLibraryApi
```

Swagger is available in development mode at:

```text
https://localhost:8010/swagger
```

For Catalog changes, Swagger is especially useful to inspect:

```text
Authors endpoints
Books endpoints
DTO schemas
ProblemDetails responses
BookItemStatus representation
BookSearchField representation
```

## Version

The current version belongs to Part 3:

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

## Didactic Goals

The test suite is intended to help students understand:

```text
separation of test levels
domain testing without infrastructure
mock-based use case testing
integration testing with SQLite
controller testing through HTTP
test data reuse through seed objects
why fake clocks are useful
how partial updates should be tested
how tests protect architectural refactorings
how tests protect architectural extensions
how a modular monolith can still be tested end-to-end
how relationships can be tested across domain and infrastructure
how read models differ from repositories
how deactivation differs from deletion
```

The tests are therefore not only a safety net, but also part of the learning material.

## Didactic Rule of Thumb

Each test level has its own purpose:

```text
Domain tests protect business rules.
Use case tests protect application workflows.
Infrastructure tests protect persistence behavior.
Controller tests protect the HTTP API.
End-to-end tests protect the full composition.
```

For Part 3, the most important teaching point is:

```text
A modular extension is successful when the architecture grows,
the existing behavior stays stable,
and the new behavior is proven by tests.
```

Another important rule is:

```text
Repository tests prove that aggregates can be stored and loaded.
ReadModel tests prove what the application shows to the outside.
```

For Catalog this is especially visible with deactivation:

```text
Repositories may still load inactive aggregates.
ReadModels hide inactive data from normal queries.
```
