# Testing Strategy — Part 5

This document describes the testing strategy for Part 5 of the `CampusLibrary` project.

Part 5 adds `CampusLibraryClient`, a Blazor SSR client without real authentication. The existing backend tests from Part 4 remain important. The new focus is manual and exploratory client/API testing.

German version: [4Testing-ger.md](4Testing-ger.md)

## Known status

After the BookItem identity change, the following was reported:

```text
dotnet build
Build succeeded

dotnet test
196 total, 0 failed, 0 skipped
```

Important:

```text
dotnet test currently mainly verifies the API.
Pure client changes are verified through dotnet build and browser tests.
```

## Test projects and applications

Automated test project:

```text
CampusLibraryApiTest
```

Client project:

```text
CampusLibraryClient
```

Backend projects:

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

Part 5 keeps the automated backend test levels from Part 4:

```text
Domain tests
Value object tests
Use case mock tests
Use case integration tests
Repository integration tests
ReadModel integration tests
cross-module contract integration tests
Controller/API end-to-end tests
manual HTTP files
```

Part 5 adds:

```text
Blazor client build test
manual Client/API smoke tests
manual UI perspective tests through DevIdentity
```

## 1. Backend regression tests

Run this when API, DTOs, use cases, read models, Seed/TestSeed or tests were changed:

```bash
dotnet test
```

Important backend test areas:

```text
Readers deactivate behavior
Catalog workflows for Book and BookItem
Loans workflows Borrow, Renew and Return
ReadModel projections
cross-module contracts
API status codes and ProblemDetails responses
```

## 2. Client build

Run this after client changes:

```bash
dotnet build
```

This verifies:

```text
CampusLibraryClient compiles
Razor components compile
DTOs match the current API contract
DI registration is consistent
prepared Auth files do not break the no-auth mode
```

## 3. Start manual Client + API tests

Start the API:

```bash
dotnet run --project CampusLibraryApi
```

Start the client:

```bash
dotnet run --project CampusLibraryClient
```

Client address:

```text
https://localhost:6040
```

API BaseUrl in the client:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## 4. Smoke tests

### TC-P5-CLIENT-001 — Client starts

Steps:

```text
1. Start CampusLibraryApi.
2. Start CampusLibraryClient.
3. Open the client in the browser.
```

Expected result:

```text
The home page is displayed.
No real login is required.
The horizontal Bootstrap navigation is visible.
```

### TC-P5-CLIENT-002 — Navigation works

Steps:

```text
1. Open Home.
2. Open Catalog.
3. Open Readers if the employee profile is active.
4. Open Loans.
```

Expected result:

```text
All visible pages can be opened.
The active menu item is recognizable.
The layout remains stable.
```

### TC-P5-CLIENT-003 — Auth is inactive

Precondition:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "DevIdentityEnabled": true,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Expected result:

```text
No login redirect occurs.
No AccessTokenHandler is required for API calls.
DevIdentity only controls the UI perspective.
```

## 5. DevIdentity tests

### TC-P5-IDENTITY-001 — Employee perspective

Precondition:

```json
{
  "DevIdentity": {
    "ActiveProfile": "EmployeeAdmin"
  }
}
```

Expected result:

```text
Navigation shows Home, Catalog, Readers, Loans, Logout.
Catalog shows employee actions.
Readers page is visible.
```

### TC-P5-IDENTITY-002 — Reader perspective

Precondition:

```json
{
  "DevIdentity": {
    "ActiveProfile": "ReaderRita"
  }
}
```

Expected result:

```text
Navigation shows Home, Catalog, Loans, Logout.
Catalog shows a borrow action when an item is available.
ReaderId is available for Borrow.
```

## 6. Readers client tests

### TC-P5-READERS-001 — Load readers list

Steps:

```text
1. Activate EmployeeAdmin.
2. Start CampusLibraryApi with seed data.
3. Open /readers.
```

Expected result:

```text
The Readers page displays reader rows.
The table displays name, email and status.
Subject is not displayed.
```

### TC-P5-READERS-002 — No reader creation in Part 5

Steps:

```text
1. Open /readers.
2. Look for a Create reader action.
```

Expected result:

```text
There is no UI function for creating a reader.
Reader provisioning is planned for later AuthN/AuthZ parts.
```

## 7. Catalog client tests

### TC-P5-CATALOG-001 — Load books list

Steps:

```text
1. Start CampusLibraryApi with seed data.
2. Open /catalog/books.
```

Expected result:

```text
The catalog table is displayed.
Column structure: Action | Title | Authors | ISBN | Items | Status.
Title and subtitle are shown together in the Title column.
Action comes first.
Items displays borrowed / total.
```

### TC-P5-CATALOG-002 — Search by title

Steps:

```text
1. Open /catalog/books.
2. Select search field Title.
3. Enter search text.
4. Click Search.
```

Expected result:

```text
Matching books are displayed.
If nothing matches, an empty result message is shown.
```

### TC-P5-CATALOG-003 — Search by author last name

Steps:

```text
1. Open /catalog/books.
2. Select search field Author last name.
3. Enter a known last name.
4. Click Search.
```

Expected result:

```text
Books with a matching author last name are displayed.
```

### TC-P5-CATALOG-004 — Create book

Precondition:

```text
EmployeeAdmin is active.
```

Steps:

```text
1. Open /catalog/books.
2. Click Create book.
3. Enter title, optional subtitle, authors and ISBN.
4. Save the book.
```

Expected result:

```text
The book is created through POST /camplib/v1/books.
A success message is displayed.
The first item can be added afterwards.
```

### TC-P5-CATALOG-005 — Add item to active book

Precondition:

```text
EmployeeAdmin is active.
An active book exists.
```

Steps:

```text
1. Open /catalog/books.
2. Click Add item on an active book.
3. Execute Add item.
```

Expected result:

```text
POST /camplib/v1/books/{bookId}/items is called.
The API generates a unique BookItem.Id.
The UI displays this id as inventory number.
```

### TC-P5-CATALOG-006 — Deactivate book

Precondition:

```text
EmployeeAdmin is active.
An active book exists.
```

Steps:

```text
1. Open /catalog/books.
2. Click Deactivate on an active book.
3. Confirm the action.
```

Expected result:

```text
PATCH /camplib/v1/books/{bookId}/deactivate is called.
The book is inactive afterwards.
Inactive books do not offer adding new items.
```

## 8. Borrow tests

### TC-P5-BORROW-001 — Borrow book as reader

Precondition:

```text
ReaderRita is active.
The book has at least one actually available item.
```

Steps:

```text
1. Open /catalog/books.
2. Click Borrow on an available book.
3. Select inventory number.
4. Complete the loan.
```

Expected result:

```text
POST /camplib/v1/loans is called.
The request contains ReaderId and BookItemId.
BookItemId is displayed as inventory number in the UI.
After success, the client navigates to /my/loans.
```

### TC-P5-BORROW-002 — Unavailable books cannot be borrowed

Steps:

```text
1. Activate ReaderRita.
2. Open /catalog/books.
3. Check a book without an available item.
```

Expected result:

```text
No Borrow button is offered.
The UI considers BookItem status and currently borrowed BookItemIds.
```

## 9. Loans client tests

### TC-P5-LOANS-001 — Load loans list

Steps:

```text
1. Start CampusLibraryApi with seed data.
2. Open /loans.
```

Expected result:

```text
The list displays borrowed loans.
The UI displays reader, title, inventory number, loan date, due date, status and overdue flag.
Inventory number is the BookItemId.
```

### TC-P5-LOANS-002 — Open loan details

Steps:

```text
1. Open /loans.
2. Open Details for a loan.
```

Expected result:

```text
The detail page displays book data, inventory number, reader data including email and loan data.
Renew and Return are located on the detail page.
```

### TC-P5-LOANS-003 — Renew loan

Steps:

```text
1. Open loan details.
2. If renewable, click Renew.
```

Expected result:

```text
PATCH /camplib/v1/loans/{id}/renew is called.
The detail data is updated or an API error is displayed.
```

### TC-P5-LOANS-004 — Return loan

Steps:

```text
1. Open loan details.
2. Click Return.
```

Expected result:

```text
PATCH /camplib/v1/loans/{id}/return-at-desk is called.
The loan is marked as returned or an API error is displayed.
```

## 10. Error handling

### TC-P5-ERROR-001 — API not reachable

Steps:

```text
1. Start CampusLibraryClient.
2. Stop CampusLibraryApi.
3. Open /catalog/books or /readers.
```

Expected result:

```text
The page does not crash.
ErrorAlert displays a network/API error.
```

## 11. Regression rules

```text
Create reader does not belong to Part 5.
Subject is not displayed in the readers list.
InventoryNumber must not return as a DTO property.
BookItemId may be labeled as inventory number in the UI.
Action comes first in the catalog table.
Title and subtitle are shown together in the Title column.
DevIdentity is not real authentication.
```
