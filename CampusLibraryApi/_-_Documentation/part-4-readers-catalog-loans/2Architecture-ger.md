# Architektur: CampusLibrary Teil 4 — Readers + Catalog + Loans

Dieses Dokument beschreibt die Architektur von Teil 4 der `CampusLibraryApi`.

Die Anwendung ist ein projektbasierter modularer Monolith mit drei fachlichen Modulen: Readers, Catalog und Loans. Sie wird als eine ASP.NET-Core-Anwendung deployt und verwendet eine Datenbank.

Finales automatisiertes Testergebnis:

```text
202 Tests
0 fehlgeschlagen
0 übersprungen
Build succeeded
```

## Architekturziel

Teil 4 macht folgende Konzepte für den Unterricht sichtbar:

* projektbasierter modularer Monolith
* Modulgrenzen über Projektreferenzen
* unabhängige Core-Module
* gemeinsame BuildingBlocks
* Aggregates, Entities und Value Objects
* schreibende Use Cases und lesende ReadModels
* Repositories zum Laden von Aggregaten
* modulübergreifende Contracts ohne direkten Tabellenzugriff
* Infrastructure als Implementierung von Outbound Ports
* HTTP-API als Adapter
* automatisierte Tests über relevante Schichten hinweg

## Aktuelle Projektstruktur

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

## Abhängigkeitsrichtung

```text
Web/API
  -> Core-Module
  -> BuildingBlocks

Infrastructure
  -> Core-Module
  -> BuildingBlocks

Core-Module
  -> BuildingBlocks
```

Core-Module referenzieren weder Web noch Infrastructure.

## Modulbesitz

Jedes Modul besitzt seine eigenen Daten und Konzepte.

```text
Readers besitzt Reader.
Catalog besitzt Book und BookItem.
Loans besitzt Loan.
```

Loans darf nicht direkt auf Reader-, Book- oder BookItem-Tabellen zugreifen. Es fragt die besitzenden Module über Contracts.

## Modulübergreifende Contracts

Teil 4 führt Zusammenarbeit zwischen Modulen ein, ohne Besitzgrenzen zu verletzen.

Die Contracts liegen in BuildingBlocks:

```text
IReaderLoanContract
IBookItemLoanContract
ReaderLoanInfoDto
BookItemLoanInfoDto
```

Infrastructure implementiert diese Contracts:

```text
ReaderLoanContractEf
BookItemLoanContractEf
```

Das Loans-Modul verwendet sie in `LoanUcBorrow`.

## Write Model und Read Model

Schreibseite:

```text
Controller -> UseCase -> Repository -> Aggregate -> UnitOfWork
```

Leseseite:

```text
Controller -> ReadModel -> DTO-Projektion
```

Repositories geben Aggregate zurück. ReadModels geben DTOs zurück.

## Loans-Domänenmodell

`Loan` ist ein Aggregate Root.

Es enthält:

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

`LoanPeriodVo` enthält:

```text
LoanDate
DueDate
```

`LoanStatus` lautet:

```csharp
public enum LoanStatus {
   Borrowed = 1,
   Returned = 2,
   Cancelled = 3
}
```

## IsActive versus Status

Teil 4 unterscheidet diese Konzepte bewusst:

```text
Reader / Book:
- IsActive
- Deaktivierung blendet Datensätze aus normalen ReadModels aus

BookItem / Loan:
- Status
- Status beschreibt den fachlichen Lebenszyklus
```

Eine Loan ist nicht aktiv/inaktiv. Sie ist ausgeliehen, zurückgegeben oder storniert.

## Loan-Use-Cases

### Borrow

`LoanUcBorrow` koordiniert drei Modulverantwortungen:

```text
Readers: Darf der Reader ausleihen?
Catalog: Ist das BookItem grundsätzlich ausleihbar?
Loans: Ist das BookItem bereits ausgeliehen?
```

Der Use Case erzeugt das `LoanPeriodVo` aus `LoanRules.StandardLoanDays` und erzeugt eine `Loan` mit Status `Borrowed`.

### Renew

`LoanUcRenew` lädt ein Loan-Aggregate und lässt die Domäne die Verlängerung prüfen. Die Domäne prüft:

```text
Loan muss Borrowed sein
Loan darf nicht überfällig sein
LoanRules.MaxRenewals darf nicht überschritten sein
neues DueDate muss nach aktuellem DueDate liegen
```

### Return at desk

`LoanUcReturnAtDesk` lädt ein Loan-Aggregate und setzt den tatsächlichen Rückgabezeitpunkt aus `IClock`.

## Loan-ReadModel

`ILoanReadModel` liefert API-orientierte DTOs:

```text
FindByIdAsync         -> LoanDetailDto
FindAllBorrowedAsync  -> IReadOnlyList<LoanListItemDto>
```

Das ReadModel reichert Loan-Daten mit Reader- und BookItem-Informationen über Contracts an.

Es kann außerdem anzeigeorientierte Werte berechnen:

```text
IsOverdue
CanRenew
```

Die Regeln für diese Werte sollten mit den Domain-Policies übereinstimmen.

## Infrastructure

Infrastructure enthält:

* EF-Core-Konfigurationen
* `AppDbContext`
* Repositories
* ReadModels
* Contract-Implementierungen
* Unit of Work
* Clock-Implementierung
* Seed-Daten

Es gibt keine Foreign-Key-Constraints von Loans zu Readers oder BookItems. Modulbesitz wird über Codegrenzen und Contracts ausgedrückt.

## Didaktischer Fokus von Teil 4

Teil 4 zeigt, wie ein neues Modul mit bestehenden Modulen zusammenarbeitet, ohne deren Tabellen oder Aggregate zu besitzen.

Studierende sehen:

* Modulbesitz
* modulübergreifende Contracts
* Aggregate-Konsistenz
* statusbasierte Workflows
* leseseitige Anreicherung
* Controller/API-Tests
* manuelle API-Workflows
