# Testing Strategy — Part 5

This document describes the testing strategy for Part 5 of the `CampusLibrary` project.

Part 5 adds `CampusLibraryClient`, a Blazor SSR client without active authentication. The existing backend tests from Part 4 remain important. The new focus is manual and exploratory client/API testing.

German version: [4Testing-ger.md](4Testing-ger.md)

## Known build status

The current Part 5 start state was verified with:

```bash
dotnet build
```

Result:

```text
Build succeeded
```

## Test projects and applications

Automated test project:

```text
CampusLibraryApiTest
```

Backend production projects:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
```

Client project:

```text
CampusLibraryClient
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
Cross-module contract integration tests
Controller/API end-to-end tests
Manual HTTP files
```

Part 5 adds a new visible level:

```text
Manual Client + API tests
```

The first client version intentionally does not require a full automated UI test setup. The goal is to make client/API interaction visible and understandable.

## 1. Backend regression tests

Run all automated tests:

```bash
dotnet test
```

These tests verify that the API still works after adding the client project.

Important backend test areas:

```text
Readers deactivate behavior
Catalog book and book item workflows
Loans borrow, renew and return workflows
ReadModel projections
Cross-module contracts
API status codes and ProblemDetails responses
```

## 2. Full solution build

Run:

```bash
dotnet build
```

This verifies:

```text
all backend projects compile
CampusLibraryClient compiles
project references are consistent
Razor components compile
Auth preparation does not break the no-auth mode
```

This is especially relevant in Part 5 because the client contains prepared AuthN/AuthZ files that are not active yet.

## 3. Manual Client + API tests

Manual Client + API tests use a running CampusLibraryApi and a running CampusLibraryClient.

Start the API first. Then start the client.

Example:

```bash
dotnet run --project CampusLibraryApi
dotnet run --project CampusLibraryClient
```

The client must point to the correct API URL:

```json
{
  "CampusLibraryApi": {
    "BaseUrl": "https://localhost:8010/"
  }
}
```

## 4. Client smoke tests

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
No login is required.
Navigation is visible.
```

### TC-P5-CLIENT-002 — Navigation works

Steps:

```text
1. Open the client.
2. Navigate to Readers.
3. Navigate to Catalog / Books.
4. Navigate to Loans.
```

Expected result:

```text
All pages can be opened without authentication.
The layout remains stable.
No AuthorizeView blocks the user.
```

### TC-P5-CLIENT-003 — Auth is inactive

Precondition:

```json
{
  "Features": {
    "AuthNEnabled": false,
    "ApiAccessTokenEnabled": false,
    "AuthZEnabled": false
  }
}
```

Steps:

```text
1. Start the client.
2. Open Readers, Books and Loans.
```

Expected result:

```text
No login redirect occurs.
No AccessTokenHandler is required for API calls.
No role or policy decision hides the pages.
```

## 5. Readers client tests

### TC-P5-READERS-001 — Load readers list

Steps:

```text
1. Start CampusLibraryApi with seed data.
2. Start CampusLibraryClient.
3. Open /readers.
```

Expected result:

```text
The Readers page displays reader rows.
The table shows firstname, lastname, email, subject and status.
```

API call:

```http
GET /camplib/v1/readers?includeInactive=false
```

### TC-P5-READERS-002 — API unavailable shows error

Steps:

```text
1. Start CampusLibraryClient.
2. Stop CampusLibraryApi.
3. Open /readers or click Reload.
```

Expected result:

```text
The page does not crash.
ErrorAlert displays a network/API error.
```

## 6. Catalog client tests

### TC-P5-CATALOG-001 — Load books list

Steps:

```text
1. Start CampusLibraryApi with seed data.
2. Start CampusLibraryClient.
3. Open /catalog/books.
```

Expected result:

```text
The Books page displays book rows.
The table shows title, subtitle, authors, ISBN, item counts and status.
```

API call:

```http
GET /camplib/v1/books?includeInactive=false
```

### TC-P5-CATALOG-002 — Search books by title

Steps:

```text
1. Open /catalog/books.
2. Select Title.
3. Enter a known title search text.
4. Click Search.
```

Expected result:

```text
The table displays matching books.
If no book matches, an empty result message is shown.
```

API call:

```http
GET /camplib/v1/books/search?searchField=Title&searchText={text}&includeInactive=false
```

### TC-P5-CATALOG-003 — Search books by author last name

Steps:

```text
1. Open /catalog/books.
2. Select Author last name.
3. Enter a known author last name.
4. Click Search.
```

Expected result:

```text
The table displays books whose AuthorsText contains a matching author last name.
```

## 7. Loans client tests

### TC-P5-LOANS-001 — Load borrowed loans

Steps:

```text
1. Start CampusLibraryApi with seed data.
2. Start CampusLibraryClient.
3. Open /loans.
```

Expected result:

```text
The Loans page displays currently borrowed loans.
Rows show reader, title, inventory number, loan date, due date, status and overdue flag.
```

API call:

```http
GET /camplib/v1/loans
```

### TC-P5-LOANS-002 — Renew loan

Steps:

```text
1. Open /loans.
2. Click Renew on a renewable borrowed loan.
```

Expected result:

```text
The API renews the loan.
The list reloads.
The due date and/or renewal count are updated according to the API response and projection.
If the loan cannot be renewed, ErrorAlert displays the API error.
```

API call:

```http
PATCH /camplib/v1/loans/{id}/renew
```

### TC-P5-LOANS-003 — Return loan at desk

Steps:

```text
1. Open /loans.
2. Click Return on a borrowed loan.
```

Expected result:

```text
The API marks the loan as returned.
The list reloads.
The returned loan no longer appears in the borrowed loans list.
If the loan cannot be returned, ErrorAlert displays the API error.
```

API call:

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

## 8. Error handling tests

### TC-P5-ERROR-001 — ProblemDetails is displayed

Steps:

```text
1. Trigger a known API validation or conflict error through a client action.
2. Observe the page.
```

Expected result:

```text
The error is shown through ErrorAlert.
The page remains usable.
```

### TC-P5-ERROR-002 — Invalid API base URL

Steps:

```text
1. Set CampusLibraryApi:BaseUrl to an invalid URL.
2. Start the client.
3. Open a page that loads data.
```

Expected result:

```text
The page displays a network error.
The client application does not crash.
```

## 9. Regression rule for prepared Auth

Because Part 5 contains prepared but inactive AuthN/AuthZ code, every build should verify:

```text
AuthNEnabled=false keeps the client anonymous.
ApiAccessTokenEnabled=false keeps API calls token-free.
AuthZEnabled=false keeps navigation unrestricted.
```

If an Auth-related change causes the no-auth client flow to require login, it belongs to a later part and should not be activated in Part 5.

## 10. Future automated client tests

Later parts may add automated UI/component tests.

Possible candidates:

```text
component tests for ErrorAlert
client tests with fake HttpMessageHandler
navigation smoke tests
Playwright tests for full browser workflows
```

For Part 5, manual Client + API tests are sufficient and didactically useful because they show the HTTP boundary directly.
