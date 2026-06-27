# Testing Strategy — Part 3

This document describes the testing strategy used in Part 3 of the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes.

Part 3 verifies the Readers module and the Catalog module.

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
status values
UTC timestamps
```

## 2. Use case mock tests

Use case mock tests verify application workflow orchestration without a real database.

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

These tests verify:

```text
repository calls
read model checks
unit of work calls
error propagation
mapping from aggregate to DTO
```

## 3. Use case integration tests

Use case integration tests run use cases with real infrastructure wiring and an in-memory database.

They verify that:

```text
use cases persist changes correctly
repositories and unit of work work together
read models can observe persisted changes
business conflicts are detected
```

## 4. Repository integration tests

Repository integration tests verify loading and storing aggregates through EF Core.

Repositories return aggregates, not DTOs.

Examples:

```text
IReaderRepository
IBookRepository
```

## 5. ReadModel integration tests

ReadModel tests verify query-side projections.

ReadModels return DTOs and may hide inactive records from normal queries.

Examples:

```text
IReaderReadModel
IBookReadModel
```

Important behavior:

```text
normal reader queries return active readers only
with-inactive reader queries include inactive readers
normal book queries return active books only
book search ignores inactive books
```

## 6. Controller/API end-to-end tests

Controller/API tests use `WebApplicationFactory` and `HttpClient`.

They verify the HTTP behavior of the public API:

```text
status codes
JSON response bodies
Created responses and Location headers
routing
validation errors
conflict errors
not found errors
```

Examples:

```text
ReadersControllerE2eT
BooksControllerE2eT
```

## 7. Manual HTTP files

Manual HTTP files are used for demonstration and exploratory testing.

Part 3 manual flow:

```text
1. Reset/delete database
2. Run Readers.http
3. Run Books.http
```

Recommended improvement for larger teaching units:

```text
01_Seed_Readers.http
02_Seed_Books.http
11_Readers_Api.http
12_Books_Api.http
91_Readers_Destructive.http
92_Books_Destructive.http
```

This separates setup from actual tests.

## Didactic value

The test suite shows that different kinds of tests answer different questions:

```text
Domain tests: Is the rule correct?
Use case tests: Is the workflow correct?
Repository tests: Is persistence correct?
ReadModel tests: Is the query projection correct?
API tests: Is the HTTP contract correct?
Manual HTTP files: Can students explore the API manually?
```
