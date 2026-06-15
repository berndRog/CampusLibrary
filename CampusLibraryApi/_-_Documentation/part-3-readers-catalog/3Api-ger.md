# API-Dokumentation

Dieses Dokument beschreibt die öffentliche HTTP API der aktuellen `CampusLibraryApi`.

Swagger/OpenAPI ist die verbindliche technische API-Beschreibung. Dieses Dokument ergänzt Swagger um eine didaktische Übersicht für Studierende.

## Base URL

In der Entwicklungsumgebung ist die API erreichbar unter:

```text
https://localhost:8010
http://localhost:8012
```

Der aktuelle API-Präfix lautet:

```text
/camplib/v1
```

Die Version ist Teil der URL.

Beispiel:

```http
GET /camplib/v1/readers
```

## Swagger

Die Swagger UI ist im Development-Modus erreichbar unter:

```text
https://localhost:8010/swagger
```

Das generierte OpenAPI-Dokument beschreibt:

```text
Routen
Request Bodies
Response Bodies
Status Codes
ProblemDetails Responses
DTO Schemas
```

Die aktuelle API enthält drei Endpoint-Gruppen:

```text
Readers
Authors
Books
```

## Module

Die aktuelle API enthält zwei fachliche Module:

```text
Readers-Modul
Catalog-Modul
```

Das Readers-Modul verwaltet Bibliotheksnutzer.

Das Catalog-Modul verwaltet Books, Authors und physische BookItems.

# Readers-Modul

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer der CampusLibrary-Domäne.

Ein Reader ist nicht dasselbe wie ein technisches Benutzerkonto.

Die Referenz auf die technische Identität wird abgebildet durch:

```text
Subject
```

## Reader-Routen

## Alle Reader abrufen

```http
GET /camplib/v1/readers
```

Liefert alle Reader.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "10000000-0000-0000-0000-000000000000",
    "subject": "a00090ad-d9df-486a-8757-4a649e26a54e",
    "firstname": "Erika",
    "lastname": "Mustermann",
    "email": "erika.mustermann@t-online.de",
    "addressDto": {
      "street": "Hauptstr. 23",
      "postalCode": "29556",
      "city": "Suderburg",
      "country": "DE"
    }
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Einen Reader anhand der Id abrufen

```http
GET /camplib/v1/readers/{id}
```

Beispiel:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
401 Unauthorized
403 Forbidden
404 Not Found
```

## Einen Reader anhand der Email abrufen

```http
GET /camplib/v1/readers/email?email={email}
```

Beispiel:

```http
GET /camplib/v1/readers/email?email=erika.mustermann@t-online.de
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

## Einen Reader anlegen

```http
POST /camplib/v1/readers
```

Request Body:

```json
{
  "firstname": "Edgar",
  "lastname": "Engel",
  "email": "e.engel@freenet.de",
  "addressDto": {
    "street": "Am Markt 14",
    "postalCode": "04109",
    "city": "Leipzig",
    "country": "DE"
  },
  "subject": "70000000-0007-0000-0000-000000000000",
  "id": null
}
```

Erfolgreiche Antwort:

```http
201 Created
```

Response Body:

```json
{
  "id": "generated-or-provided-id",
  "subject": "70000000-0007-0000-0000-000000000000",
  "firstname": "Edgar",
  "lastname": "Engel",
  "email": "e.engel@freenet.de",
  "addressDto": {
    "street": "Am Markt 14",
    "postalCode": "04109",
    "city": "Leipzig",
    "country": "DE"
  }
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## ReaderCreateDto

```csharp
public sealed record ReaderCreateDto(
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto,
   string Subject,
   string? Id
);
```

`Id` ist optional.

Das ist Absicht.

Die Id kann bei normaler API-Nutzung weggelassen werden. Der UseCase erzeugt dann eine neue Id.

Für Lehre, Tests oder deterministische Seed-Daten kann die Id angegeben werden.

Deshalb ist `Id` sowohl technisch als auch fachlich nullable.

## Einen Reader aktualisieren

```http
PUT /camplib/v1/readers/{id}
```

Beispiel:

```http
PUT /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Request Body, um nur den Nachnamen zu ändern:

```json
{
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Request Body, um nur die Email zu ändern:

```json
{
  "lastname": null,
  "email": "e.meier@gmx.de",
  "addressDto": null
}
```

Request Body, um nur die Adresse zu ändern:

```json
{
  "lastname": null,
  "email": null,
  "addressDto": {
    "street": "Schillerstr. 1",
    "postalCode": "30123",
    "city": "Hannover",
    "country": "DE"
  }
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

## ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Alle Properties sind bewusst nullable.

Das ist für partielle Updates erforderlich.

Die Bedeutung von `null` lautet:

```text
Lastname = null   -> keine Änderung
Email = null      -> keine Änderung
AddressDto = null -> keine Änderung
```

Das ist technisch und fachlich erforderlich.

`Firstname` ist bewusst nicht Teil des Update DTOs.

`Subject` ist ebenfalls bewusst nicht Teil des Update DTOs.

Die technische Identitätsreferenz wird durch normale Profiländerungen nicht geändert.

## Einen Reader löschen

```http
DELETE /camplib/v1/readers/{id}
```

Beispiel:

```http
DELETE /camplib/v1/readers/30000000-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
204 No Content
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

## AddressDto

```csharp
public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);
```

Die Adresse wird in der Application-Schicht in ein `AddressVo` umgewandelt.

Das Value Object führt seine eigene Validierung aus.

# Catalog-Modul

Das Catalog-Modul verwaltet Books, Authors und physische BookItems.

Das Modul führt ein fachlich reichhaltigeres Domain Model ein als das Readers-Modul.

Es enthält:

```text
Book
Author
BookItem
IsbnVo
```

Ein `Book` beschreibt das bibliografische Werk.

Ein `Author` beschreibt eine Person, die Books zugeordnet werden kann.

Ein `BookItem` beschreibt ein physisches Exemplar eines Books.

Ein `IsbnVo` schützt die ISBN-Validierungsregeln.

# Author-Routen

## Alle aktiven Authors abrufen

```http
GET /camplib/v1/authors
```

Liefert alle aktiven Authors.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000000",
    "firstname": "Robert",
    "lastname": "Martin",
    "displayName": "Robert Martin",
    "isActive": true
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Einen aktiven Author anhand der Id abrufen

```http
GET /camplib/v1/authors/{id}
```

Beispiel:

```http
GET /camplib/v1/authors/a0000001-0000-0000-0000-000000000000
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Inaktive Authors werden von diesem normalen Read-Endpunkt nicht zurückgegeben.

## Aktive Authors suchen

```http
GET /camplib/v1/authors/search?searchText={searchText}
```

Beispiel:

```http
GET /camplib/v1/authors/search?searchText=Martin
```

Sucht aktive Authors nach Firstname, Lastname oder DisplayName.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "a0000001-0000-0000-0000-000000000000",
    "firstname": "Robert",
    "lastname": "Martin",
    "displayName": "Robert Martin",
    "isActive": true
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

Wenn kein Author zum Suchtext passt, liefert der Endpunkt eine leere Liste.

## Einen Author anlegen

```http
POST /camplib/v1/authors
```

Request Body:

```json
{
  "firstname": "Robert",
  "lastname": "Martin",
  "id": null
}
```

Erfolgreiche Antwort:

```http
201 Created
```

Response Body:

```json
{
  "id": "generated-or-provided-id",
  "firstname": "Robert",
  "lastname": "Martin",
  "displayName": "Robert Martin",
  "isActive": true
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## AuthorCreateDto

```csharp
public sealed record AuthorCreateDto(
   string Firstname,
   string Lastname,
   string? Id
);
```

`Id` ist optional.

Die Id kann bei normaler API-Nutzung weggelassen werden. Der UseCase erzeugt dann eine neue Id.

Für Lehre, Tests oder deterministische Seed-Daten kann die Id angegeben werden.

## Einen Author deaktivieren

```http
PATCH /camplib/v1/authors/{id}/deactivate
```

Beispiel:

```http
PATCH /camplib/v1/authors/a0000001-0000-0000-0000-000000000000/deactivate
```

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
{
  "id": "a0000001-0000-0000-0000-000000000000",
  "firstname": "Robert",
  "lastname": "Martin",
  "displayName": "Robert Martin",
  "isActive": false
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Deactivate ist nicht dasselbe wie Delete.

Der Author bleibt in der Datenbank gespeichert.

Normale ReadModels entscheiden, ob inaktive Authors sichtbar sind.

## AuthorDto

```csharp
public sealed record AuthorDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string DisplayName,
   bool IsActive
);
```

`AuthorDto` wird sowohl für Read-Antworten als auch für Antworten schreibender UseCases verwendet.

# Book-Routen

## Alle aktiven Books abrufen

```http
GET /camplib/v1/books
```

Liefert alle aktiven Books als kompakte Listeneinträge.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": null,
    "isbn": "9780132350884",
    "authors": [
      "Robert Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

## Ein aktives Book anhand der Id abrufen

```http
GET /camplib/v1/books/{id}
```

Beispiel:

```http
GET /camplib/v1/books/b0000001-0000-0000-0000-000000000000
```

Liefert eine ausführliche Book-Darstellung.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "authors": [
    {
      "id": "a0000001-0000-0000-0000-000000000000",
      "firstname": "Robert",
      "lastname": "Martin",
      "displayName": "Robert Martin",
      "isActive": true
    }
  ],
  "bookItems": [
    {
      "id": "i0000001-0000-0000-0000-000000000000",
      "bookId": "b0000001-0000-0000-0000-000000000000",
      "inventoryNumber": "CL-0001",
      "status": 1
    }
  ],
  "totalBookItems": 1,
  "availableBookItems": 1,
  "isActive": true,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-01T00:00:00Z"
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Inaktive Books werden von diesem normalen Read-Endpunkt nicht zurückgegeben.

## Aktive Books suchen

```http
GET /camplib/v1/books/search?searchField={searchField}&searchText={searchText}
```

Beispiele:

```http
GET /camplib/v1/books/search?searchField=Title&searchText=Clean
GET /camplib/v1/books/search?searchField=AuthorName&searchText=Martin
GET /camplib/v1/books/search?searchField=Isbn&searchText=9780132350884
```

Sucht aktive Books anhand genau eines Suchkriteriums.

Unterstützte Suchfelder:

```text
Title
AuthorName
Isbn
```

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": null,
    "isbn": "9780132350884",
    "authors": [
      "Robert Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

Wenn kein Book zum Suchkriterium passt, liefert der Endpunkt eine leere Liste.

## Aktive Books zu einem Author abrufen

```http
GET /camplib/v1/books/by-author/{authorId}
```

Beispiel:

```http
GET /camplib/v1/books/by-author/a0000001-0000-0000-0000-000000000000
```

Liefert alle aktiven Books, die einem Author zugeordnet sind.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "b0000001-0000-0000-0000-000000000000",
    "title": "Clean Code",
    "subtitle": null,
    "isbn": "9780132350884",
    "authors": [
      "Robert Martin"
    ],
    "totalBookItems": 2,
    "availableBookItems": 2
  }
]
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
```

Wenn dem Author kein aktives Book zugeordnet ist, liefert der Endpunkt eine leere Liste.

## Ein Book anlegen

```http
POST /camplib/v1/books
```

Request Body:

```json
{
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "id": null
}
```

Erfolgreiche Antwort:

```http
201 Created
```

Response Body:

```json
{
  "id": "generated-or-provided-id",
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": true
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
409 Conflict
```

## BookCreateDto

```csharp
public sealed record BookCreateDto(
   string Title,
   string? Subtitle,
   string Isbn,
   string? Id
);
```

`Id` ist optional.

Die Id kann bei normaler API-Nutzung weggelassen werden. Der UseCase erzeugt dann eine neue Id.

Für Lehre, Tests oder deterministische Seed-Daten kann die Id angegeben werden.

Die ISBN wird durch `IsbnVo` validiert.

## Ein physisches BookItem hinzufügen

```http
POST /camplib/v1/books/{bookId}/items
```

Beispiel:

```http
POST /camplib/v1/books/b0000001-0000-0000-0000-000000000000/items
```

Request Body:

```json
{
  "inventoryNumber": "CL-0001",
  "id": null
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
{
  "id": "generated-or-provided-id",
  "bookId": "b0000001-0000-0000-0000-000000000000",
  "inventoryNumber": "CL-0001",
  "status": 1
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

Ein neu hinzugefügtes BookItem startet mit dem Status:

```text
Available
```

In der aktuellen JSON-Darstellung wird `Available` serialisiert als:

```text
1
```

Der Enum wird bewusst als Integer in der Datenbank gespeichert.

## BookItemAddDto

```csharp
public sealed record BookItemAddDto(
   string InventoryNumber,
   string? Id
);
```

`InventoryNumber` muss eindeutig sein.

`Id` ist optional.

Die Id kann bei normaler API-Nutzung weggelassen werden. Der UseCase erzeugt dann eine neue Id.

Für Lehre, Tests oder deterministische Seed-Daten kann die Id angegeben werden.

## Einen Author einem Book zuordnen

```http
POST /camplib/v1/books/{bookId}/authors
```

Beispiel:

```http
POST /camplib/v1/books/b0000001-0000-0000-0000-000000000000/authors
```

Request Body:

```json
{
  "authorId": "a0000001-0000-0000-0000-000000000000"
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": true
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

Die `bookId` kommt aus der Route.

Die `authorId` kommt aus dem Request Body.

Es gibt keine `BookAuthorId`.

Die Join-Tabelle ist ein Infrastrukturdetail.

## BookAssignAuthorDto

```csharp
public sealed record BookAssignAuthorDto(
   Guid AuthorId
);
```

Dieses DTO enthält nur die Author-Id, weil die Book-Id bereits Teil der Route ist.

## Ein Book deaktivieren

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Beispiel:

```http
PATCH /camplib/v1/books/b0000001-0000-0000-0000-000000000000/deactivate
```

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
{
  "id": "b0000001-0000-0000-0000-000000000000",
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "bookItemCount": 0,
  "isActive": false
}
```

Mögliche Fehlerantworten:

```http
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
```

Deactivate ist nicht dasselbe wie Delete.

Das Book bleibt in der Datenbank gespeichert.

Normale ReadModels entscheiden, ob inaktive Books sichtbar sind.

## BookDto

```csharp
public sealed record BookDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   int BookItemCount,
   bool IsActive
);
```

`BookDto` wird hauptsächlich als Ergebnis schreibender Book-UseCases verwendet.

## BookListItemDto

```csharp
public sealed record BookListItemDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<string> Authors,
   int TotalBookItems,
   int AvailableBookItems
);
```

`BookListItemDto` wird für Listen und Suchergebnisse verwendet.

Es ist für Katalogübersichten optimiert.

## BookDetailDto

```csharp
public sealed record BookDetailDto(
   Guid Id,
   string Title,
   string? Subtitle,
   string Isbn,
   IReadOnlyList<AuthorDto> Authors,
   IReadOnlyList<BookItemDto> BookItems,
   int TotalBookItems,
   int AvailableBookItems,
   bool IsActive,
   DateTime CreatedAt,
   DateTime UpdatedAt
);
```

`BookDetailDto` wird für die Detailansicht eines Books verwendet.

Es enthält mehr Informationen als `BookListItemDto`.

## BookItemDto

```csharp
public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   string InventoryNumber,
   BookItemStatus Status
);
```

`BookItemDto` repräsentiert ein physisches BookItem.

## BookItemStatus

```csharp
public enum BookItemStatus {
   Available = 1,
   Unavailable = 2,
   Lost = 3,
   Damaged = 4
}
```

Die Integer-Darstellung ist aktuell in JSON und im Swagger-Schema sichtbar.

Das ist kompakt und technisch stabil.

Die fachliche Bedeutung bleibt im Code durch die Enum-Namen sichtbar.

## Fehlerantworten

Die API verwendet `ProblemDetails` für Fehlerantworten.

Beispielstruktur:

```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid email.",
  "code": "Reader.InvalidEmail",
  "traceId": "..."
}
```

Der konkrete `code` hängt vom Domain- oder Application-Fehler ab.

Typische Kategorien:

```text
400 Bad Request   -> ungültige Eingabe
401 Unauthorized  -> Authentifizierung erforderlich
403 Forbidden     -> Zugriff verweigert
404 Not Found     -> Ressource nicht gefunden
409 Conflict      -> doppelte oder widersprüchliche Ressource
```

Beispiele:

```text
409 Conflict -> doppelte Reader-Email
409 Conflict -> doppeltes Reader-Subject
409 Conflict -> doppelte Book-ISBN
409 Conflict -> doppelter Author-Name
409 Conflict -> doppelte InventoryNumber
409 Conflict -> Author ist dem Book bereits zugeordnet
```

## Leseseite und Schreibseite

Die Controller verwenden unterschiedliche Application Ports für Lesen und Schreiben.

Readers:

```text
IReaderReadModel
IReaderUseCases
```

Authors:

```text
IAuthorReadModel
IAuthorUseCases
```

Books:

```text
IBookReadModel
IBookUseCases
```

Die Leseseite wird für Queries verwendet:

```text
GET /readers
GET /readers/{id}
GET /readers/email

GET /authors
GET /authors/{id}
GET /authors/search

GET /books
GET /books/{id}
GET /books/search
GET /books/by-author/{authorId}
```

Die Schreibseite wird für Commands verwendet:

```text
POST   /readers
PUT    /readers/{id}
DELETE /readers/{id}

POST  /authors
PATCH /authors/{id}/deactivate

POST  /books
POST  /books/{bookId}/items
POST  /books/{bookId}/authors
PATCH /books/{bookId}/deactivate
```

Diese Trennung unterstützt ein klares Lehrmodell:

```text
Queries lesen DTOs.
Commands verändern Aggregates.
Repositories arbeiten mit Domain-Objekten.
ReadModels liefern DTOs direkt zurück.
```

## Wichtige Designentscheidungen

## Book zu BookItem

Ein Book kann mehrere physische BookItems besitzen.

Das wird als 1:n-Beziehung modelliert:

```text
Book 1 --- n BookItem
```

Ein BookItem wird über das `Book`-Aggregate hinzugefügt.

```text
POST /camplib/v1/books/{bookId}/items
```

## Book zu Author

Ein Book kann mehrere Authors haben.

Ein Author kann mehreren Books zugeordnet sein.

Das wird als m:n-Beziehung modelliert:

```text
Book n --- m Author
```

Die API stellt die Zuordnung so bereit:

```text
POST /camplib/v1/books/{bookId}/authors
```

Der Request Body enthält nur die `authorId`.

Die technische Join-Tabelle wird nicht als API-Ressource veröffentlicht.

## Deactivate statt Delete im Catalog

Books und Authors werden nicht physisch gelöscht.

Sie werden deaktiviert.

```text
IsActive = false
```

Repositories können sie weiterhin laden.

ReadModels entscheiden, ob inaktive Daten sichtbar sind.

Normale Kataloglisten und Suchfunktionen liefern nur aktive Books und Authors.

## Didaktische Ziele

Diese API soll zeigen:

```text
REST-artige Endpunkte
API-Versionierung
Swagger/OpenAPI-Dokumentation
DTOs als API-Verträge
Result-basierte Fehlerbehandlung
ProblemDetails-Antworten
Semantik partieller Updates
Trennung von Lese- und Schreibpfaden
Controller-Tests mit WebApplicationFactory
1:n-Beziehung innerhalb eines Aggregates
m:n-Beziehung über Infrastructure-Mapping
Deactivate statt Delete
modulspezifische ReadModels und UseCases
```

Swagger sollte für die technische Erkundung verwendet werden.

Dieses Dokument sollte für die konzeptionelle Erklärung verwendet werden.
