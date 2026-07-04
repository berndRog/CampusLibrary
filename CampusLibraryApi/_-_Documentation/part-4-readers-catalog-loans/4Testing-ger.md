# Teststrategie — Teil 4

Dieses Dokument beschreibt die Teststrategie in Teil 4 des Projekts `CampusLibrary`.

Ziel ist nicht nur die Prüfung der Korrektheit, sondern auch die Sichtbarkeit der unterschiedlichen Testebenen für den Unterricht.

Teil 4 prüft die Module Readers, Catalog und Loans.

Finales automatisiertes Testergebnis:

```text
Test summary: total: 196, failed: 0, succeeded: 196, skipped: 0
Build succeeded
```

## Testprojekt

```text
CampusLibraryApiTest
```

Produktionsprojekte:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_3_Core_Loan
CampusLibraryApi_4_Infrastructure
```

## Testebenen

Die Testsuite deckt ab:

```text
Domain-Tests
Value-Object-Tests
Use-Case-Mock-Tests
Use-Case-Integrationstests
Repository-Integrationstests
ReadModel-Integrationstests
Modulübergreifende Contract-Integrationstests
Controller/API-End-to-End-Tests
Manuelle HTTP-Dateien
```

Alle Tests ausführen:

```bash
dotnet test
```

## 1. Domain-Tests

Domain-Tests prüfen Domänenobjekte ohne Infrastructure.

Readers-Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
Reader.Deactivate(...)
```

Catalog-Beispiele:

```text
Book.Create(...)
Book.AddBookItem(...)
Book.Deactivate(...)
BookItem.Create(...)
IsbnVo.Create(...)
```

Loans-Beispiele:

```text
Loan.Create(...)
Loan.Renew(...)
Loan.ReturnAtDesk(...)
Loan.IsOverdue(...)
Loan.CanRenew(...)
LoanPeriodVo.Create(...)
```

Domain-Tests konzentrieren sich auf:

```text
Pflichtwerte
Normalisierung
ungültige Eingaben
Domain-Fehler
Aggregate-Invarianten
Statusübergänge
Value-Object-Validierung
UTC-Zeitstempel
```

## 2. Use-Case-Tests

Use-Case-Tests prüfen die Orchestrierung von Application Workflows.

Loan-Beispiele:

```text
LoanUcBorrow
LoanUcRenew
LoanUcReturnAtDesk
```

Diese Tests prüfen:

```text
Contract-Aufrufe zu Readers und Catalog
Repository-Aufrufe
UnitOfWork-Aufrufe
Fehlerweitergabe
Mapping von Aggregate zu DTO
```

## 3. Use-Case-Integrationstests

Use-Case-Integrationstests führen Use Cases mit echter Infrastructure-Verdrahtung und In-Memory-Datenbank aus.

Loan-Beispiele:

```text
BorrowAsync_ok_persists_loan_to_database
BorrowAsync_book_item_already_borrowed_fails
RenewAsync_ok_persists_new_due_date_and_renewal_count
ReturnAtDeskAsync_ok_persists_returned_status_and_returned_at
```

## 4. Repository-Integrationstests

Repository-Integrationstests prüfen das Laden und Speichern von Aggregates über EF Core.

Loan-Repository-Beispiele:

```text
FindByIdAsync
FindBorrowedByBookItemIdAsync
FindBorrowedByReaderIdAsync
Add
AddRange
```

Die Begrifflichkeit lautet bewusst `Borrowed` und nicht `Active`, weil Loans `LoanStatus.Borrowed` statt `IsActive` verwenden.

## 5. ReadModel-Integrationstests

Loan-ReadModel-Tests prüfen lesende Projektionen, die über Contracts angereichert werden.

Beispiele:

```text
FindByIdAsync -> LoanDetailDto
FindAllBorrowedAsync -> IReadOnlyList<LoanListItemDto>
```

Die ReadModel-Tests müssen Readers, Books/BookItems und Loans einfügen, weil das Loan-ReadModel `IReaderLoanContract` und `IBookItemLoanContract` zur DTO-Anreicherung verwendet.

## 6. Modulübergreifende Contract-Integrationstests

Contract-Tests prüfen, dass die Infrastructure-Implementierungen lesende Informationen korrekt über Modulgrenzen bereitstellen.

Beispiele:

```text
ReaderLoanContractIntT
BookItemLoanContractIntT
```

Diese Tests prüfen:

```text
Reader existiert und darf ausleihen
Reader nicht gefunden
Reader nicht aktiv
BookItem existiert
BookItem nicht gefunden
BookItem nicht ausleihbar
```

## 7. Controller/API-End-to-End-Tests

Controller/API-Tests verwenden `WebApplicationFactory` und `HttpClient`.

Loan-API-Beispiele:

```text
GET    /camplib/v1/loans
GET    /camplib/v1/loans/{id}
POST   /camplib/v1/loans
PATCH  /camplib/v1/loans/{id}/renew
PATCH  /camplib/v1/loans/{id}/return-at-desk
```

Tests prüfen:

```text
Statuscodes
JSON-Antwortkörper
Created-Antworten und Location Header
Routing
Validierungsfehler
Konfliktfehler
Not-Found-Fehler
```

Wichtige Testregel:

```text
Zuerst den HTTP-Statuscode prüfen.
Danach JSON lesen.
```

Damit werden 404/500-Fehler nicht durch JSON-Parsing-Exceptions verdeckt.

## 8. Manuelle HTTP-Dateien

Manueller Ablauf in Teil 4:

```text
1. Datenbank zurücksetzen/löschen
2. Readers.http ausführen
3. Books.http ausführen
4. Loans.http ausführen
```

Empfohlene Lehrstruktur:

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

Dadurch werden Setup und eigentliche Tests getrennt.

## Didaktischer Wert

Die Testsuite zeigt, dass verschiedene Testarten verschiedene Fragen beantworten:

```text
Domain-Tests: Ist die Regel korrekt?
Use-Case-Tests: Ist der Workflow korrekt?
Repository-Tests: Funktioniert die Persistenz?
ReadModel-Tests: Ist die lesende Projektion korrekt?
Contract-Tests: Werden Modulgrenzen eingehalten?
API-Tests: Ist der HTTP-Vertrag korrekt?
Manuelle HTTP-Dateien: Können Studierende die API selbst erkunden?
```
