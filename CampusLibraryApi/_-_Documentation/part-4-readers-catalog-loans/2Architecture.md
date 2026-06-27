# Architecture: CampusLibrary Part 4 — Readers + Catalog + Loans

This document describes the architecture of Part 4 of `CampusLibraryApi`.

The application is a project-based modular monolith with three business modules: Readers, Catalog and Loans. It is deployed as one ASP.NET Core application and uses one database.

Final automated test result:

```text
202 tests
0 failed
0 skipped
Build succeeded
```

## Architectural goal

Part 4 makes the following concepts visible for teaching:

* project-based modular monolith
* module boundaries through project references
* independent Core modules
* shared BuildingBlocks
* aggregates, entities and value objects
* write-side use cases and read-side read models
* repositories for aggregate loading
* cross-module contracts without direct table access
* Infrastructure as implementation of outbound ports
* HTTP API as adapter
* automated tests across all relevant layers

## Current project structure

```text
CampusLibraryApi
├─ Program.cs
├─ appsettings.json
└─ Configure

CampusLibraryApi_1_Web
└─ Controllers
   ├─ ReadersController.cs
   ├─ BooksController.cs
   └─ LoansController.cs

CampusLibraryApi_2_BuildingBlocks
├─ Result.cs
├─ _1_Ports
│  ├─ IClock.cs
│  ├─ IUnitOfWork.cs
│  └─ Contracts
│     ├─ IReaderLoanContract.cs
│     └─ IBookItemLoanContract.cs
├─ _2_Application
│  └─ Contracts
│     ├─ ReaderLoanInfoDto.cs
│     └─ BookItemLoanInfoDto.cs
└─ _3_Domain
   ├─ Entities
   │  ├─ Entity.cs
   │  └─ AggregateRoot.cs
   └─ Errors

CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Dependency direction

```text
Web/API
  -> Core modules
  -> BuildingBlocks

Infrastructure
  -> Core modules
  -> BuildingBlocks

Core modules
  -> BuildingBlocks
```

Core modules do not reference Web or Infrastructure.

## Module ownership

Each module owns its own data and concepts.

```text
Readers owns Reader.
Catalog owns Book and BookItem.
Loans owns Loan.
```

Loans must not directly access Reader, Book or BookItem tables. It asks the owning modules through contracts.

## Cross-module contracts

Part 4 introduces module-to-module collaboration without breaking ownership.

The contracts are placed in BuildingBlocks:

```text
IReaderLoanContract
IBookItemLoanContract
ReaderLoanInfoDto
BookItemLoanInfoDto
```

Infrastructure implements these contracts:

```text
ReaderLoanContractEf
BookItemLoanContractEf
```

The Loans module uses them in `LoanUcBorrow`.

## Write model and read model

Write side:

```text
Controller -> UseCase -> Repository -> Aggregate -> UnitOfWork
```

Read side:

```text
Controller -> ReadModel -> DTO projection
```

Repositories return aggregates. ReadModels return DTOs.

## Loans domain model

`Loan` is an aggregate root.

It contains:

```text
Id
ReaderId
BookItemId
LoanPeriodVo
ReturnedAt
LoanStatus
RenewalCount
CreatedAt
UpdatedAt
```

`LoanPeriodVo` contains:

```text
LoanDate
DueDate
```

`LoanStatus` is:

```csharp
public enum LoanStatus {
   Borrowed = 1,
   Returned = 2,
   Cancelled = 3
}
```

## IsActive versus Status

Part 4 deliberately distinguishes these concepts:

```text
Reader / Book:
- IsActive
- deactivation hides records from normal read models

BookItem / Loan:
- Status
- status describes the business lifecycle
```

A Loan is not active/inactive. It is borrowed, returned or cancelled.

## Loan use cases

### Borrow

`LoanUcBorrow` coordinates three module responsibilities:

```text
Readers: Is the reader allowed to borrow?
Catalog: Is the book item loanable?
Loans: Is the book item already borrowed?
```

The use case creates the `LoanPeriodVo` from `LoanRules.StandardLoanDays` and creates a `Loan` with status `Borrowed`.

### Renew

`LoanUcRenew` loads a Loan aggregate and asks the domain to renew it. The domain checks:

```text
loan must be borrowed
loan must not be overdue
loan must not exceed LoanRules.MaxRenewals
new due date must be after current due date
```

### Return at desk

`LoanUcReturnAtDesk` loads a Loan aggregate and records the actual return timestamp from `IClock`.

## Loan read model

`ILoanReadModel` returns API-oriented DTOs:

```text
FindByIdAsync       -> LoanDetailDto
FindAllBorrowedAsync -> IReadOnlyList<LoanListItemDto>
```

The read model enriches Loan data with Reader and BookItem information using contracts.

It may also calculate display-oriented values such as:

```text
IsOverdue
CanRenew
```

The rules for these values should stay aligned with the domain policies.

## Infrastructure

Infrastructure contains:

* EF Core configurations
* `AppDbContext`
* repositories
* read models
* contract implementations
* unit of work
* clock implementation
* seed data

There are no foreign-key constraints from Loans to Readers or BookItems. Module ownership is expressed through code boundaries and contracts.

## Teaching focus of Part 4

Part 4 demonstrates how a new module collaborates with existing modules without taking ownership of their tables or aggregates.

Students see:

* module ownership
* cross-module contracts
* aggregate consistency
* status-based workflows
* read-side enrichment
* controller/API testing
* manual API workflows
