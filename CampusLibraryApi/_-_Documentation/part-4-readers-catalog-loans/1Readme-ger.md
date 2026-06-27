# CampusLibrary — Teil 4: Readers + Catalog + Loans

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Englische Version: [1Readme.md](1Readme.md)

## Aktueller Stand

Diese Version enthält drei fachliche Module:

* Readers
* Catalog
* Loans

Teil 4 erweitert den Readers+Catalog-Modular-Monolith um ein Loans-Modul. Readers und Books verwenden `IsActive`. BookItems und Loans verwenden Statuswerte. Eine aktuell offene Ausleihe hat `LoanStatus.Borrowed`.

Finales automatisiertes Testergebnis für diesen Teil:

```text
202 Tests
0 fehlgeschlagen
0 übersprungen
Build succeeded
```

## Aktueller Branch

```text
part-4/readers-catalog-loans
```

## Projektstruktur

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

## Architekturidee

Die Lösung ist ein projektbasierter modularer Monolith.

Die API wird als eine ASP.NET-Core-Anwendung deployt und verwendet eine Datenbank. Der Code bleibt modular, weil jede fachliche Fähigkeit ihr Domänenmodell, ihre Ports, Use Cases und Tests besitzt.

Zentrale Abhängigkeitsregel:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure implementiert Outbound Ports der Core-Module.
Das ausführbare API-Projekt verdrahtet alle Module.
```

## Module

## Readers

Readers verwaltet Bibliotheksleserinnen und -leser.

Wichtige Konzepte:

```text
Reader-Aggregate
Reader-Value-Objects
Reader.IsActive
Reader-Deaktivierung
Reader-Repository
Reader-ReadModel
ReadersController
```

Ein Reader wird deaktiviert statt physisch gelöscht. Normale Lese-Endpunkte liefern nur aktive Reader. `with-inactive`-Endpunkte beziehen inaktive Reader mit ein.

## Catalog

Catalog verwaltet Books und physische BookItems.

Wichtige Konzepte:

```text
Book-Aggregate
BookItem-Entity
IsbnVo
Book.IsActive
BookItemStatus
AuthorsText
Book-ReadModel
BooksController
```

Ein Book repräsentiert das bibliografische Werk. Ein BookItem repräsentiert ein physisches Exemplar.

Es gibt kein Author-Aggregate und keine Author-API. Autoren werden in `Book.AuthorsText` gespeichert. Die Suche nach Autorennachnamen parst diesen komma-separierten Text.

## Loans

Loans verwaltet Ausleihe, Verlängerung und Rückgabe von BookItems.

Wichtige Konzepte:

```text
Loan-Aggregate
LoanPeriodVo
LoanStatus
LoanRules
Loan-Repository
Loan-ReadModel
Loan-Use-Cases
LoansController
```

`LoanStatus` lautet:

```csharp
public enum LoanStatus {
   Borrowed = 1,
   Returned = 2,
   Cancelled = 3
}
```

Loans verwenden kein `IsActive`. Eine Loan ist aktuell offen, wenn ihr Status `Borrowed` ist und `ReturnedAt` den Wert `null` hat.

## Modulübergreifende Contracts

Das Loans-Modul darf nicht direkt auf Readers- oder Catalog-Tabellen zugreifen.

Stattdessen verwendet es Contracts aus BuildingBlocks:

```text
IReaderLoanContract
IBookItemLoanContract
ReaderLoanInfoDto
BookItemLoanInfoDto
```

Die Implementierungen liegen in Infrastructure.

Besitzregel:

```text
Readers besitzt Readers.
Catalog besitzt Books und BookItems.
Loans besitzt Loans.
```

## Loan-Workflows

### Borrow

```text
POST /camplib/v1/loans
```

Der Borrow-Use-Case:

* validiert den Request
* fragt Readers, ob der Reader ausleihen darf
* fragt Catalog, ob das BookItem grundsätzlich ausleihbar ist
* prüft, ob das BookItem bereits ausgeliehen ist
* erzeugt ein LoanPeriodVo mit LoanRules.StandardLoanDays
* erzeugt eine Loan mit LoanStatus.Borrowed
* speichert das Loan-Aggregate

### Renew

```text
PATCH /camplib/v1/loans/{id}/renew
```

Der Renew-Use-Case:

* lädt das Loan-Aggregate
* prüft die fachlichen Regeln
* verlängert das Fälligkeitsdatum um LoanRules.StandardRenewalDays
* erhöht den RenewalCount

### Return at desk

```text
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Der ReturnAtDesk-Use-Case:

* lädt das Loan-Aggregate
* setzt `ReturnedAt`
* ändert den Status auf Returned

## API-Überblick

Endpoint-Gruppen:

```text
Readers
Books
Loans
```

Wichtige Loan-Endpunkte:

```text
GET   /camplib/v1/loans
GET   /camplib/v1/loans/{id}
POST  /camplib/v1/loans
PATCH /camplib/v1/loans/{id}/renew
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Es gibt bewusst keine Route `/loans/active`. Loans verwenden `LoanStatus.Borrowed`, nicht `IsActive`.

## Testing

Teil 4 enthält automatisierte Tests über alle relevanten Schichten:

* Domain-Tests
* Value-Object-Tests
* Use-Case-Mock-Tests
* Use-Case-Integrationstests
* Repository-Integrationstests
* ReadModel-Integrationstests
* Contract-Integrationstests zwischen Modulen
* Controller/API-End-to-End-Tests
* manuelle `.http`-Dateien

Alle Tests ausführen:

```bash
dotnet test
```

## Manuelle HTTP-Dateien

Empfohlene Reihenfolge:

```text
1. Readers.http oder 01_Seed_Readers.http
2. Books.http oder 02_Seed_Books.http
3. Loans.http oder 03_Seed_Loans.http
```

Für größere Lehreinheiten sollten Seed-Aufbau und API-Verhaltenstests getrennt werden:

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
