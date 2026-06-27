# API-Dokumentation: CampusLibrary Teil 2

Dieses Dokument beschreibt die öffentliche HTTP-API von **Teil 2 – Readers Modular Monolith**.

Swagger/OpenAPI ist die maßgebliche technische API-Beschreibung. Dieses Dokument ergänzt eine didaktische Übersicht für Studierende.

## Basis-URL

In der Entwicklung ist die API typischerweise erreichbar unter:

```text
https://localhost:8010
http://localhost:8012
```

Der aktuelle API-Präfix lautet:

```text
/camplib/v1
```

Beispiel:

```http
GET /camplib/v1/readers
```

## Swagger

Die Swagger UI ist im Development-Modus verfügbar unter:

```text
https://localhost:8010/swagger
```

Swagger beschreibt:

- Routen
- Request Bodies
- Response Bodies
- Statuscodes
- ProblemDetails Responses
- DTO-Schemas

## Modulumfang

Teil 2 enthält nur das **Readers**-Modul.

Es gibt noch kein Catalog-Modul, keine Books, keine BookItems und keine Loans.

Ein Reader repräsentiert einen fachlichen Bibliotheksnutzer der CampusLibrary-Domäne. Ein Reader ist nicht dasselbe wie ein technischer Login-Account.

Die Verbindung zu einer technischen Identität wird dargestellt durch:

```text
Subject
```

## Reader-Verhalten

Reader werden nicht physisch gelöscht.

Ein Reader besitzt ein `IsActive`-Flag.

Normale Query-Endpunkte liefern nur aktive Reader. Zusätzliche Endpunkte können deaktivierte Reader einschließen.

Die öffentliche API verwendet weiterhin HTTP-Semantik. Der Endpunkt:

```http
DELETE /camplib/v1/readers/{id}
```

bedeutet daher fachlich:

```text
Reader deaktivieren.
```

Er löscht den Datensatz nicht physisch aus der Datenbank.

## Reader-Routen

### Alle aktiven Reader abrufen

```http
GET /camplib/v1/readers
```

Liefert alle aktiven Reader.

Erfolgreiche Antwort:

```http
200 OK
```

Response Body:

```json
[
  {
    "id": "10000000-0000-0000-0000-000000000000",
    "subject": "70000000-0007-0000-0000-000000000000",
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

### Alle Reader inklusive deaktivierter Reader abrufen

```http
GET /camplib/v1/readers/with-inactive
```

Liefert aktive und deaktivierte Reader.

Dieser Endpunkt ist nützlich für Administration, Tests und die didaktische Unterscheidung zwischen normalen Abfragen und expliziten Abfragen inklusive inaktiver Daten.

Erfolgreiche Antwort:

```http
200 OK
```

### Einen aktiven Reader über Id abrufen

```http
GET /camplib/v1/readers/{id}
```

Beispiel:

```http
GET /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Liefert den Reader nur, wenn er aktiv ist.

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
404 Not Found
```

### Einen Reader über Id inklusive deaktivierter Reader abrufen

```http
GET /camplib/v1/readers/{id}/with-inactive
```

Liefert den Reader auch dann, wenn er deaktiviert ist.

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
404 Not Found
```

### Einen aktiven Reader über Email abrufen

```http
GET /camplib/v1/readers/email?email={email}
```

Beispiel:

```http
GET /camplib/v1/readers/email?email=erika.mustermann@t-online.de
```

Liefert den Reader nur, wenn er aktiv ist.

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
404 Not Found
```

### Reader anlegen

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

Mögliche Fehlerantworten:

```http
400 Bad Request
409 Conflict
```

### Reader aktualisieren

```http
PUT /camplib/v1/readers/{id}
```

Beispiel:

```http
PUT /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Request Body, wenn nur der Nachname geändert werden soll:

```json
{
  "firstname": null,
  "lastname": "Meier",
  "email": null,
  "addressDto": null
}
```

Request Body, wenn nur die Email geändert werden soll:

```json
{
  "firstname": null,
  "lastname": null,
  "email": "e.meier@gmx.de",
  "addressDto": null
}
```

Erfolgreiche Antwort:

```http
200 OK
```

Mögliche Fehlerantworten:

```http
400 Bad Request
404 Not Found
409 Conflict
```

### Reader deaktivieren

```http
DELETE /camplib/v1/readers/{id}
```

Beispiel:

```http
DELETE /camplib/v1/readers/10000000-0000-0000-0000-000000000000
```

Dieser Endpunkt deaktiviert den Reader.

Er löscht den Datensatz nicht physisch.

Erfolgreiche Antwort:

```http
204 No Content
```

Mögliche Fehlerantworten:

```http
400 Bad Request
404 Not Found
409 Conflict
```

Ein deaktivierter Reader erscheint nicht mehr in normalen Leseabfragen:

```http
GET /camplib/v1/readers
GET /camplib/v1/readers/{id}
```

Ein deaktivierter Reader kann weiterhin über Endpunkte eingesehen werden, die inaktive Reader ausdrücklich einschließen:

```http
GET /camplib/v1/readers/with-inactive
GET /camplib/v1/readers/{id}/with-inactive
```

## DTOs

### AddressDto

```csharp
public sealed record AddressDto(
   string Street,
   string PostalCode,
   string City,
   string? Country
);
```

### ReaderCreateDto

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

`Id` ist optional. Im normalen API-Betrieb kann der UseCase eine neue Id erzeugen. Für Tests und deterministische Seed-Szenarien kann eine Id explizit übergeben werden.

### ReaderUpdateDto

```csharp
public sealed record ReaderUpdateDto(
   string? Firstname,
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

`ReaderUpdateDto` unterstützt partielle Updates. Felder mit `null` werden nicht geändert.

### ReaderDto

```csharp
public sealed record ReaderDto(
   Guid Id,
   string Subject,
   string Firstname,
   string Lastname,
   string Email,
   AddressDto AddressDto
);
```

## ProblemDetails

Fachliche und validierungsbezogene Fehler werden über ProblemDetails auf HTTP-Antworten abgebildet.

Typische Zuordnungen sind:

```text
400 Bad Request -> ungültige Eingaben
404 Not Found   -> Reader nicht gefunden
409 Conflict    -> doppelte Daten oder ungültiger Zustandsübergang
```

Der Controller wirft für normale Domain-Fehler keine fachlichen Exceptions. UseCases und ReadModels liefern `Result<T>`-Werte, die in HTTP-Antworten übersetzt werden.

## Didaktische Hinweise

Teil 2 ist bewusst klein gehalten.

Die API zeigt:

- wie ein Controller Command-UseCases aufruft
- wie ein Controller ReadModels für Queries aufruft
- wie Domain Errors zu HTTP ProblemDetails werden
- wie sich Soft-Deactivation von physischem Löschen unterscheidet
- wie ein modularer Monolith trotzdem eine einfache HTTP-API anbieten kann
