# Testing Strategy — Part 4

This document describes the testing strategy used in Part 4 of the `CampusLibrary` project.

The goal is not only to verify correctness, but also to make the different test levels visible for teaching purposes.

Part 4 verifies the Readers, Catalog and Loans modules.

Final automated test result:

```text
Test summary: total: 202, failed: 0, succeeded: 202, skipped: 0
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
CampusLibraryApi_3_Core_Loan
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
Cross-module contract integration tests
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
```

Catalog examples:

```text
Book.Create(...)
Book.AddBookItem(...)
Book.Deactivate(...)
BookItem.Create(...)
IsbnVo.Create(...)
```

Loans examples:

```text
Loan.Create(...)
Loan.Renew(...)
Loan.ReturnAtDesk(...)
Loan.IsOverdue(...)
Loan.CanRenew(...)
LoanPeriodVo.Create(...)
```

Domain tests focus on:

```text
required values
normalization
invalid input
domain errors
aggregate invariants
status transitions
value object validation
UTC timestamps
```

## 2. Use case tests

Use case tests verify application workflow orchestration.

Loan examples:

```text
LoanUcBorrow
LoanUcRenew
LoanUcReturnAtDesk
```

These tests verify:

```text
contract calls to Readers and Catalog
repository calls
unit of work calls
error propagation
mapping from aggregate to DTO
```

## 3. Use case integration tests

Use case integration tests run use cases with real infrastructure wiring and an in-memory database.

Loan examples:

```text
BorrowAsync_ok_persists_loan_to_database
BorrowAsync_book_item_already_borrowed_fails
RenewAsync_ok_persists_new_due_date_and_renewal_count
ReturnAtDeskAsync_ok_persists_returned_status_and_returned_at
```

## 4. Repository integration tests

Repository integration tests verify loading and storing aggregates through EF Core.

Loan repository examples:

```text
FindByIdAsync
FindBorrowedByBookItemIdAsync
FindBorrowedByReaderIdAsync
Add
AddRange
```

The terminology is intentionally `Borrowed`, not `Active`, because Loans use `LoanStatus.Borrowed` instead of `IsActive`.

## 5. ReadModel integration tests

Loan read model tests verify query-side projections enriched through contracts.

Examples:

```text
FindByIdAsync -> LoanDetailDto
FindAllBorrowedAsync -> IReadOnlyList<LoanListItemDto>
```

The read model tests must insert Readers, Books/BookItems and Loans, because the Loan read model uses `IReaderLoanContract` and `IBookItemLoanContract` to enrich DTOs.

## 6. Cross-module contract integration tests

Contract tests verify that Infrastructure implementations correctly expose read-only information across module boundaries.

Examples:

```text
ReaderLoanContractIntT
BookItemLoanContractIntT
```

These tests verify:

```text
Reader exists and may borrow
Reader not found
Reader not active
BookItem exists
BookItem not found
BookItem not available for loan
```

## 7. Controller/API end-to-end tests

Controller/API tests use `WebApplicationFactory` and `HttpClient`.

Loan API examples:

```text
GET    /camplib/v1/loans
GET    /camplib/v1/loans/{id}
POST   /camplib/v1/loans
PATCH  /camplib/v1/loans/{id}/renew
PATCH  /camplib/v1/loans/{id}/return-at-desk
```

Tests verify:

```text
status codes
JSON response bodies
Created responses and Location headers
routing
validation errors
conflict errors
not found errors
```

Important test rule:

```text
First assert the HTTP status code.
Then read JSON.
```

This avoids hiding 404/500 errors behind JSON parsing exceptions.

## 8. Manual HTTP files

Part 4 manual flow:

```text
1. Reset/delete database
2. Run Readers.http
3. Run Books.http
4. Run Loans.http
```

Recommended teaching structure:

```text
01_Seed_Readers.http
02_Seed_Books.http
03_Seed_Loans.http
11_Readers_Api.http
12_Books_Api.http
13_Loans_Api.http
91_Readers_Destructive.http
92_Books_Destructive.http
93_Loans_Destructive.http
```

This separates setup from actual tests.

## Didactic value

The test suite shows that different kinds of tests answer different questions:

```text
Domain tests: Is the rule correct?
Use case tests: Is the workflow correct?
Repository tests: Is persistence correct?
ReadModel tests: Is the query projection correct?
Contract tests: Are module boundaries respected?
API tests: Is the HTTP contract correct?
Manual HTTP files: Can students explore the API manually?
```
