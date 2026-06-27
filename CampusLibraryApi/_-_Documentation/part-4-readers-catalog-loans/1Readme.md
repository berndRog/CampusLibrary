# CampusLibrary — Part 4: Readers + Catalog + Loans

Teaching project for a modular, DDD-oriented ASP.NET Core Web API.

German version: [1Readme-ger.md](1Readme-ger.md)

## Current status

This version contains three functional modules:

* Readers
* Catalog
* Loans

Part 4 extends the Readers + Catalog modular monolith with a Loans module. Readers and Books use `IsActive`. BookItems and Loans use status values. A currently open loan has `LoanStatus.Borrowed`.

Final automated test result for this part:

```text
202 tests
0 failed
0 skipped
Build succeeded
```

## Current branch

```text
part-4/readers-catalog-loans
```

## Project structure

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architectural idea

The solution is a project-based modular monolith.

The API is deployed as one ASP.NET Core application and uses one database. The code is still modular because each business capability owns its domain model, ports, use cases and tests.

Central dependency rule:

```text
Core modules do not depend on Web/API or Infrastructure.
Infrastructure implements outbound ports defined by Core modules.
The executable API project wires all modules together.
```

## Modules

## Readers

Readers manages library readers.

Important concepts:

```text
Reader aggregate
Reader value objects
Reader.IsActive
Reader deactivate workflow
Reader repository
Reader read model
ReadersController
```

A Reader is deactivated instead of physically deleted. Normal read endpoints return active readers only. `with-inactive` endpoints include inactive readers.

## Catalog

Catalog manages books and physical book items.

Important concepts:

```text
Book aggregate
BookItem entity
IsbnVo
Book.IsActive
BookItemStatus
AuthorsText
Book read model
BooksController
```

A Book represents the bibliographic work. A BookItem represents a physical copy.

There is no Author aggregate and no Author API. Authors are stored in `Book.AuthorsText`. Author-last-name search parses the comma-separated author text.

## Loans

Loans manages borrowing, renewal and return of book items.

Important concepts:

```text
Loan aggregate
LoanPeriodVo
LoanStatus
LoanRules
Loan repository
Loan read model
Loan use cases
LoansController
```

`LoanStatus` is:

```csharp
public enum LoanStatus {
   Borrowed = 1,
   Returned = 2,
   Cancelled = 3
}
```

Loans do not use `IsActive`. A Loan is currently open when its status is `Borrowed` and `ReturnedAt` is `null`.

## Cross-module contracts

The Loans module must not directly access Readers or Catalog tables.

Instead it uses contracts from BuildingBlocks:

```text
IReaderLoanContract
IBookItemLoanContract
ReaderLoanInfoDto
BookItemLoanInfoDto
```

The implementations live in Infrastructure.

Ownership rule:

```text
Readers owns Readers.
Catalog owns Books and BookItems.
Loans owns Loans.
```

## Loan workflows

### Borrow

```text
POST /camplib/v1/loans
```

The Borrow use case:

* validates the request
* asks Readers whether the reader may borrow
* asks Catalog whether the book item is loanable
* checks whether the book item is already borrowed
* creates a LoanPeriodVo using LoanRules.StandardLoanDays
* creates a Loan with LoanStatus.Borrowed
* stores the Loan aggregate

### Renew

```text
PATCH /camplib/v1/loans/{id}/renew
```

The Renew use case:

* loads the Loan aggregate
* checks the domain rules
* extends the due date by LoanRules.StandardRenewalDays
* increments the renewal count

### Return at desk

```text
PATCH /camplib/v1/loans/{id}/return-at-desk
```

The ReturnAtDesk use case:

* loads the Loan aggregate
* records `ReturnedAt`
* changes status to Returned

## API overview

Endpoint groups:

```text
Readers
Books
Loans
```

Important Loan endpoints:

```text
GET   /camplib/v1/loans
GET   /camplib/v1/loans/{id}
POST  /camplib/v1/loans
PATCH /camplib/v1/loans/{id}/renew
PATCH /camplib/v1/loans/{id}/return-at-desk
```

There is intentionally no `/loans/active` route. Loans use `LoanStatus.Borrowed`, not `IsActive`.

## Testing

Part 4 includes automated tests across all relevant layers:

* domain tests
* value object tests
* use case mock tests
* use case integration tests
* repository integration tests
* read model integration tests
* cross-module contract integration tests
* controller/API end-to-end tests
* manual `.http` files

Run all tests:

```bash
dotnet test
```

## Manual HTTP files

Recommended order:

```text
1. Readers.http or 01_Seed_Readers.http
2. Books.http or 02_Seed_Books.http
3. Loans.http or 03_Seed_Loans.http
```

For larger teaching units, seed setup and API behavior tests should be separated:

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
