# API-Dokumentation — Teil 4

Dieses Dokument beschreibt die öffentliche HTTP-API von Teil 4 der `CampusLibraryApi`.

Swagger/OpenAPI ist die maßgebliche technische API-Beschreibung. Dieses Dokument bietet eine didaktische Übersicht für Studierende.

## Base URL

Development-URLs:

```text
https://localhost:8010
http://localhost:8012
```

API-Präfix:

```text
/camplib/v1
```

Endpoint-Gruppen:

```text
Readers
Books
Loans
```

Swagger UI ist im Development-Modus verfügbar:

```text
https://localhost:8010/swagger
```

## Manuelle HTTP-Dateien

Für manuelle API-Tests sollte die Datenbank zuerst zurückgesetzt oder gelöscht werden.

Empfohlene Reihenfolge:

```text
1. Readers.http
2. Books.http
3. Loans.http
```

Für größere Übungen sollten Seed-Dateien und Verhaltenstests getrennt werden:

```text
01_Seed_Readers.http
02_Seed_Books.http
03_Seed_Loans.http
11_Readers_Api.http
12_Books_Api.http
13_Loans_Api.http
```

# Readers API

Die Readers API ist gegenüber Teil 3 unverändert.

Wichtige Endpunkte:

```http
GET    /camplib/v1/readers
GET    /camplib/v1/readers/with-inactive
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/{id}/with-inactive
GET    /camplib/v1/readers/email?email={email}
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

`DELETE /readers/{id}` deaktiviert einen Reader.

# Books API

Die Books API verwendet weiterhin Books und BookItems. Ein BookItem wird nur noch über seine eindeutige `Id` identifiziert. Es gibt keine separate `InventoryNumber` mehr.

Wichtige Endpunkte:

```http
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
PATCH /camplib/v1/books/{bookId}/deactivate
```

Es gibt keine Author API.

Beim Hinzufügen eines BookItems kann der Client optional eine Id liefern. Die Inventarnummer wird nicht mehr als eigenes Feld übertragen.

```csharp
public sealed record BookItemAddDto(
   string? Id
);

public sealed record BookItemDto(
   Guid Id,
   Guid BookId,
   int Status
);
```

In der UI darf die `BookItemId` weiterhin als Inventarnummer angezeigt werden.

# Loans API

Loans beschreiben den Ausleih-Lebenszyklus konkreter BookItems.

Loans verwenden `LoanStatus`, nicht `IsActive`.

```text
Borrowed = 1
Returned = 2
Cancelled = 3
```

DTOs geben den Status als numerischen API-Wert aus. Die Domäne verwendet intern weiterhin das Enum `LoanStatus`.

Aktuelle DTO-Regel: `InventoryNumber` ist aus den Loan-DTOs entfernt. Die eindeutige Exemplaridentität ist `BookItemId`. Zusätzlich enthalten Reader-bezogene Loan-DTOs jetzt `Email`.

```csharp
public sealed record ReaderLoanInfoDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   bool IsActive
);

public sealed record LoanListItemDto(
   Guid Id,

   Guid ReaderId,
   string Firstname,
   string Lastname,

   Guid BookItemId,

   string Title,
   string? Subtitle,

   DateTime LoanDate,
   DateTime DueDate,

   int Status,
   bool IsOverdue
);

public sealed record LoanDetailDto(
   Guid Id,

   Guid ReaderId,
   string Firstname,
   string Lastname,
   string Email,

   Guid BookItemId,

   Guid BookId,
   string AuthorsText,
   string Title,
   string? Subtitle,
   string Isbn,
   bool BookIsActive,
   bool IsAvailableForLoan,

   DateTime LoanDate,
   DateTime DueDate,
   DateTime? ReturnedAt,

   int Status,
   int RenewalCount,

   bool IsOverdue,
   bool CanRenew
);
```

## Ausgeliehene Loans abrufen

```http
GET /camplib/v1/loans
```

Liefert alle aktuell ausgeliehenen Loans.

Response-Typ:

```text
IReadOnlyList<LoanListItemDto>
```

Erfolgreiche Antwort:

```http
200 OK
```

Es gibt bewusst keine Route `/loans/active`.

## Eine Loan nach Id abrufen

```http
GET /camplib/v1/loans/{id}
```

Liefert eine Detailprojektion mit Reader- und BookItem-Daten.

Response-Typ:

```text
LoanDetailDto
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
```

## BookItem ausleihen

```http
POST /camplib/v1/loans
Content-Type: application/json
```

Beispiel:

```json
{
  "id": "a1000001-0000-0000-0000-000000000000",
  "readerId": "10000000-0000-0000-0000-000000000000",
  "bookItemId": "be000001-0000-0000-0000-000000000000"
}
```

Mögliche Antworten:

```http
201 Created
400 Bad Request
404 Not Found
409 Conflict
```

Der Client liefert keine Leihdauer. Das Fälligkeitsdatum wird aus `LoanRules.StandardLoanDays` abgeleitet.

## Loan verlängern

```http
PATCH /camplib/v1/loans/{id}/renew
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

Eine Loan kann nur verlängert werden, wenn sie ausgeliehen, nicht überfällig und unterhalb der maximalen Anzahl von Verlängerungen ist.

## Loan am Service Desk zurückgeben

```http
PATCH /camplib/v1/loans/{id}/return-at-desk
```

Mögliche Antworten:

```http
200 OK
400 Bad Request
404 Not Found
409 Conflict
```

Der Rückgabezeitpunkt wird vom Application Service über `IClock` gesetzt.

## Typischer manueller Ablauf

```http
POST  /camplib/v1/loans
GET   /camplib/v1/loans/{id}
PATCH /camplib/v1/loans/{id}/renew
PATCH /camplib/v1/loans/{id}/return-at-desk
GET   /camplib/v1/loans
```

Nach der Rückgabe erscheint die Loan nicht mehr in `GET /loans`, weil dieser Endpunkt aktuell ausgeliehene Loans listet.
