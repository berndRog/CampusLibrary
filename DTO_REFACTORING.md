# DTO-Vereinfachung in CampusLibrary Part 6

## Architekturentscheidung

Die öffentlichen DTOs bleiben Eigentum ihres jeweiligen Fachmoduls.

```text
CampusLibraryApi_3_Core_Readers
└── _2_Application/Dtos/ReaderDtos.cs

CampusLibraryApi_3_Core_Catalog
└── _2_Application/Dtos/CatalogDtos.cs

CampusLibraryApi_3_Core_Loan
└── _2_Application/Dtos/LoanDtos.cs
```

Es gibt bewusst **kein** gemeinsames `CampusLibrary.Contracts`-Projekt.
Ein `ReaderDto` gehört dem Readers-Modul, ein `BookDto` dem Catalog-Modul
und ein `LoanDto` dem Loans-Modul.

Der Blazor-Client ist über HTTP getrennt und besitzt deshalb eigene
Transporttypen:

```text
CampusLibraryClient/Api/Dtos
├── ReaderDtos.cs
├── CatalogDtos.cs
└── LoanDtos.cs
```

Diese Duplizierung bildet die HTTP-Grenze sichtbar ab. Der Client referenziert
keines der Core-Module.

## Echte modulübergreifende Contracts

Nur Daten, die ein Bounded Context einem anderen Bounded Context gezielt
veröffentlicht, liegen in BuildingBlocks:

```text
CampusLibraryApi_2_BuildingBlocks
├── _1_Ports/Contracts
│   ├── IReaderLoanContract
│   ├── IBookItemLoanContract
│   ├── ILoanCatalogContract
│   └── ILoanReaderContract
└── _2_Application/Contracts
    ├── ReaderLoanInfoDto.cs
    ├── BookItemLoanInfoDto.cs
    └── CurrentBookItemLoanInfoDto.cs
```

Die zugehörigen Ports liegen weiterhin unter `_1_Ports`.
Diese Typen sind keine HTTP-DTOs und werden nicht vom Blazor-Client verwendet.

## Vereinfachte öffentliche DTOs

### Readers

- `AddressDto`
- `ReaderDto`
- `ReaderCreateDto`
- `ReaderProfileDto`
- `ReaderUpdateDto`

Die drei Self-Service-Abläufe bleiben fachlich getrennt:

- `POST /readers/me/provision` erzeugt den fachlichen Reader-Rumpf aus der
  technischen Identität.
- `PUT /readers/me/profile` schließt das initiale Profil mit Vorname, Nachname
  und Adresse ab. Die initiale E-Mail stammt bereits aus der Provisionierung.
- `PUT /readers/me/update` ändert später ausschließlich Nachname, fachliche
  E-Mail und Adresse. Der Vorname bleibt unveränderbar.

Dazu bleiben auch die Domainmethoden getrennt:

- `UpdateMyProfile(...)` für den initialen Profilabschluss
- `UpdateProfile(...)` für die spätere selektive Änderung

Nullable Felder in `ReaderDto` bedeuten ausschließlich, dass ein
provisioniertes Profil noch unvollständig ist. Nullable Felder in
`ReaderUpdateDto` bedeuten: Dieser Wert bleibt unverändert.

### Catalog

- `BookDto`
- `BookItemDto`
- `BookCreateDto`
- `BookItemAddDto`
- `BookDeactivationInfoDto`
- `BookLoanInfoDto`

`BookDto` wird für Listen- und Detailantworten verwendet.

### Loans

- `LoanDto`
- `LoanCreateDto`
- `LoanBorrowMeDto`

`LoanDto` wird für Listen- und Detailantworten verwendet.

## Optionale Test-IDs

Die optionalen IDs bleiben bewusst erhalten:

- `ReaderCreateDto.Id`
- `BookCreateDto.Id`
- `BookItemAddDto.Id`
- `LoanCreateDto.Id`
- `LoanBorrowMeDto.Id`
- optionaler Query-Parameter `id` bei `/readers/me/provision`

Sie ermöglichen deterministische Tests, ohne fachliche Pflichtfelder zu sein.

## Controller-Fehlerbehandlung

`DomainProblemDetailsFactory` erzeugt ausschließlich die einheitliche Form des
`ProblemDetails`-Objekts. Die Zuordnung von `WebErrorStatus` zu MVC-Ergebnissen
bleibt direkt in jeder Controller-Methode sichtbar.

Damit stehen Swagger-Dokumentation und tatsächliche HTTP-Fehlerbehandlung an
derselben Stelle.
