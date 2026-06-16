# Testing Strategy

This document describes the testing strategy used in the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes. The project therefore separates domain tests, application use case tests, application integration tests, infrastructure tests, controller/API end-to-end tests, and manual HTTP files.

A conscious decision is made in Part 3: controller mock tests are not used as a broad additional test level. Controllers are kept thin. The business logic is tested in domain and use case tests, persistence and projections are tested in infrastructure tests, and the public HTTP contract is tested through `WebApplicationFactory` and `HttpClient`.

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
Controller/API end-to-end tests with WebApplicationFactory and HttpClient
Manual HTTP files for didactic API testing
```

Controller mock tests are intentionally not listed as a separate test level.

The reason is that the controllers should contain no business logic. They receive HTTP input, call use cases or read models, and translate results into HTTP responses. This behavior is more useful to test through real HTTP requests than through isolated controller mocks.

Run all automated tests with:

```bash
dotnet test
```

At the end of Part 3, the final `dotnet test` result should be copied into the README and project documentation. The important final condition is:

```text
0 failed
0 skipped
```

If additional controller/API end-to-end tests for Authors and Books are added, the total test count will be higher than the earlier Part 3 count of 155.

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

## Why Controller Mock Tests Are Not a Separate Test Level

Controller mock tests are deliberately not used as a broad additional test level in Part 3.

The reason is the intended controller design:

```text
Controllers receive HTTP input.
Controllers call read models or use cases.
Controllers translate Result<T> into HTTP responses.
Controllers should not contain business logic.
```

If controllers are thin, isolated controller mock tests usually repeat behavior that is already covered elsewhere.

The application workflow is tested by use case tests with mocks.

The query behavior is tested by read model tests.

The HTTP contract is tested by controller/API end-to-end tests with `WebApplicationFactory` and `HttpClient`.

Controller mock tests would only be useful if the controller itself contained relevant branching logic, special status-code decisions, custom header logic, complex authorization behavior, or manual response mapping that is not covered by a shared helper.

For Part 3, the didactic decision is therefore:

```text
No broad controller mock test layer.
UseCase tests use mocks.
Controller/API tests use real HTTP through HttpClient.
```

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
search active authors by lastname

select all active books
find active book by id
search active books by title
search active books by author lastname
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

## Catalog Search Tests

Part 3 includes catalog search tests for Authors and Books.

Author search uses the Author lastname:

```text
GET /camplib/v1/authors/search?searchText=Martin
```

This should find `Robert C. Martin`, but not `Martin Fowler`, because `Martin` is only the firstname of `Martin Fowler`.

Book search supports the following search fields:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` searches only the lastname of assigned Authors.

The firstname is not searched.

This avoids accidental matches.

For example:

```text
AuthorLastName = Martin -> Clean Code
AuthorLastName = Fowler -> Refactoring and Design Patterns
```

A specific regression test should verify that:

```text
AuthorLastName = Martin
```

returns:

```text
Clean Code
```

but does not return:

```text
Refactoring
Design Patterns
```

because in those books, `Martin` is only the firstname of `Martin Fowler`.

This test makes the fachliche search decision visible:

```text
In catalog search, the author lastname is the relevant search criterion.
The firstname should not create accidental matches.
```

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

## 5. Controller / API End-to-End Tests

Controller/API end-to-end tests use:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
HttpClient
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
HTTP contract from the outside
```

The Reader controller/API tests cover:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

The Author controller/API end-to-end tests should cover the public HTTP behavior of the `AuthorsController`, for example:

```text
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

The Book controller/API end-to-end tests should cover the public HTTP behavior of the `BooksController`, for example:

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

These tests are closest to real API usage.

In Part 3, controller/API end-to-end tests also verify that the two modules are wired correctly by the executable API project.

They therefore check not only controller behavior, but also the composition of:

```text
Web
Readers module
Catalog module
Infrastructure
BuildingBlocks
```

These tests do not test individual classes.

They test the public HTTP contract of the application.

The didactic rule is:

```text
Domain tests verify business rules.
UseCase tests verify workflows.
Repository and ReadModel tests verify persistence and projections.
HttpClient tests verify that the API works from the outside.
```

## Manual HTTP Files

In addition to automated tests, Part 3 also contains manual HTTP files for didactic API testing.

These files are used after the database has been deleted or reset.

The intended execution order is:

```text
1. Authors.http
2. Books.http
3. Readers.http
```

`Seed.cs` defines the stable ids.

The `.http` files create the corresponding data through the public API.

```text
Authors.http creates the Authors.
Books.http creates the Books, uses the existing Authors, assigns Authors to Books and adds BookItems.
Readers.http creates or verifies Reader data.
```

This is intentional.

The manual HTTP files should not invent unrelated ad-hoc ids for relationships.

They should use the stable ids from `Seed.cs`, so that tests, documentation and manual API usage describe the same examples.

The didactic rule is:

```text
Seed.cs defines stable example data.
The .http files create this data through the public API.
Manual API tests should be reproducible after a database reset.
```

## Test Database

The automated tests use SQLite through the test infrastructure.

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
Author4
Author5

Book1
Book2
Book3
Book4

Books with Authors
Books with BookItems
```

The tests should prefer seed data over manually constructed ad hoc data.

This keeps the examples consistent and easier to understand for students.

For Catalog tests, seed data is also useful because relationships should be built from the same tracked object graph where necessary.

For example, books with authors should use existing Author instances instead of creating duplicate Author objects with the same ids.

The same principle is used by the manual HTTP files.

`Seed.cs` defines stable ids, while the `.http` files create the corresponding records through the public API.

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

Controller/API end-to-end tests verify:

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

Controller/API end-to-end test:
Does the public HTTP API behave correctly from the outside?

Manual HTTP file:
Can students reproduce and inspect the API behavior manually after a database reset?
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

For manual API testing, reset the database and execute:

```text
Authors.http
Books.http
Readers.http
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
controller/API testing through HTTP
test data reuse through seed objects
why fake clocks are useful
how partial updates should be tested
how tests protect architectural refactorings
how tests protect architectural extensions
how a modular monolith can still be tested end-to-end
how relationships can be tested across domain and infrastructure
how read models differ from repositories
how deactivation differs from deletion
how catalog search by author lastname avoids accidental matches
why controller mock tests are not needed when controllers are thin
how manual HTTP files can reproduce API behavior after a database reset
```

The tests are therefore not only a safety net, but also part of the learning material.

## Didactic Rule of Thumb

Each test level has its own purpose:

```text
Domain tests protect business rules.
Use case tests protect application workflows.
Infrastructure tests protect persistence behavior.
Controller/API end-to-end tests protect the public HTTP API.
HttpClient tests protect the public HTTP contract.
Manual HTTP files make API behavior visible and reproducible for students.
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

For catalog search, the most important rule is:

```text
Author search and Book search use author lastname.
Firstname is not searched, because it would create accidental matches.
```

For controller tests, the most important rule is:

```text
UseCase tests use mocks.
Controller/API tests use HttpClient.
Thin controllers do not need a broad controller mock test layer.
```

For manual API tests, the most important rule is:

```text
Seed.cs defines stable example data.
The .http files create this data through the public API.
```
