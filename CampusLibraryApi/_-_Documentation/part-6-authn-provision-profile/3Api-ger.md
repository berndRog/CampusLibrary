# API – CampusLibrary Teil 6

Englische Version: [3Api.md](3Api.md)

Offizieller Branch:

```text
part-6/authn-provision-profile
```

Basisroute der CampusLibrary API:

```text
https://localhost:8010/camplib/v1
```

Swagger im Development-Modus:

```text
https://localhost:8010/swagger
```

## Authentifizierung

Reader-Self-Service-Endpunkte erwarten ein gültiges Access Token:

```http
Authorization: Bearer <access-token>
```

Das Access Token wird vom IdentityAccessServer ausgestellt und von der CampusLibrary API als JWT Bearer validiert.

Wichtige technische Claims sind:

```text
sub                 stabiler technischer Benutzerbezeichner
preferred_username  Username, initial identisch zur E-Mail
created_at          Erstellungszeit der technischen Identität
admin_rights        technischer Kompatibilitätswert
account type/role   reader oder employee
```

Die genaue Claim-Repräsentation wird ausschließlich durch den Web-Adapter in `IIdentityGateway` übersetzt. UseCases arbeiten nicht direkt mit Claims.

## Reader-Endpunkte

### Aktuellen Reader provisionieren

```http
POST /camplib/v1/readers/me/provision
```

Erwarteter Erfolg:

```text
204 No Content
```

Der Request benötigt kein fachliches Reader-Formular. Subject und initialer Username kommen aus dem Access Token. Eine optionale Test-ID darf nur für reproduzierbare Development-/Testabläufe verwendet werden.

Typische Fehler:

```text
401 IdentityUnauthenticated
403 AccessNotAllowed
400 SubjectRequired
400 InvalidIdentitySubject
400 IdentityEmailRequired
400 TimestampInvalid
409 ReaderAlreadyProvisioned
```

### Aktuellen Reader lesen

```http
GET /camplib/v1/readers/me
```

Erfolg:

```text
200 OK
```

Der Reader wird über `IIdentityGateway.Subject` ermittelt.

### Initiales Profil vervollständigen

```http
PUT /camplib/v1/readers/me/profile
Content-Type: application/json
```

Beispiel:

```json
{
  "firstname": "Rita",
  "lastname": "Reader",
  "addressDto": {
    "street": "Bibliotheksweg 99",
    "postalCode": "29556",
    "city": "Suderburg",
    "country": "DE"
  }
}
```

Erfolg:

```text
200 OK
```

`ReaderProfileDto` enthält bewusst keine E-Mail. Die initiale E-Mail stammt aus der technischen Identität.

### Eigenes Profil selektiv aktualisieren

```http
PUT /camplib/v1/readers/me/update
Content-Type: application/json
```

Beispiel:

```json
{
  "lastname": "Reader-Neu",
  "email": "rita.neu@example.org",
  "addressDto": null
}
```

`null` bedeutet: der bestehende Wert bleibt unverändert.

Erfolg:

```text
200 OK
```

Die spätere fachliche E-Mail darf von `preferred_username` abweichen. Die Zuordnung zum Reader bleibt über den Subject bestehen.

## Catalog-Endpunkte

Catalog bleibt in Teil 6 weitgehend wie in Teil 5.

### Books lesen und suchen

```http
GET /camplib/v1/books
GET /camplib/v1/books/{bookId}
GET /camplib/v1/books/search?searchField=Title&searchText=...
GET /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET /camplib/v1/books/search?searchField=Isbn&searchText=...
```

### Book erzeugen

```http
POST /camplib/v1/books
Content-Type: application/json
```

Beispiel:

```json
{
  "authorsText": "Robert C. Martin",
  "title": "Clean Code",
  "subtitle": null,
  "isbn": "9780132350884",
  "id": "00000001-0000-0000-0000-000000000000"
}
```

Erfolg:

```text
201 Created
```

`id` ist optional und dient reproduzierbaren Development-/Testabläufen.

### BookItem hinzufügen

```http
POST /camplib/v1/books/{bookId}/items
Content-Type: application/json
```

Ein `BookItem` ist das physische Exemplar. Die `Id` ist seine Identität; eine zusätzliche Inventory Number wird im aktuellen Modell nicht verwendet.

### Book deaktivieren

```http
PATCH /camplib/v1/books/{bookId}/deactivate
```

Die Deaktivierung wird gegen laufende Loans geprüft. Das Catalog-Modul fragt dazu über einen BC-to-BC-Port beim Loan-Modul nach.

## Loan-Self-Service

### Eigene Loans lesen

```http
GET /camplib/v1/loans/me
```

Erfolg bei keiner Ausleihe:

```text
200 OK
[]
```

### BookItem ausleihen

```http
POST /camplib/v1/loans/me
Content-Type: application/json
```

Beispiel:

```json
{
  "bookItemId": "00000002-0000-0000-0000-000000000000",
  "id": "00000099-0000-0001-0000-000000000000"
}
```

Erfolg:

```text
201 Created
Location: /camplib/v1/loans/me/{loanId}
```

Der Client sendet keine Reader-ID. Der Reader wird aus dem Token-Subject ermittelt.

Vor dem Borrow werden unter anderem geprüft:

```text
Identity ist authentifiziert
Identity ist Reader
Subject ist gültig
Reader ist provisioniert
Reader-Profil ist vollständig
Reader ist aktiv
BookItem existiert und kann ausgeliehen werden
BookItem ist nicht bereits verliehen
```

### Eigenen Loan lesen

```http
GET /camplib/v1/loans/me/{loanId}
```

Der Loan muss zum aktuellen Reader gehören.

### Eigenen Loan verlängern

```http
PATCH /camplib/v1/loans/me/{loanId}/renew
```

Erfolg:

```text
200 OK
```

Die Domain prüft Verlängerungsregeln, erhöht den Renewal Count und berechnet ein neues Fälligkeitsdatum über `IClock`.

### Rückgabe am Schalter

```http
PATCH /camplib/v1/loans/{loanId}/return-at-desk
```

Erfolg:

```text
204 No Content
```

Die Rückgabe löscht den Loan. Danach ist folgendes Ergebnis korrekt:

```http
GET /camplib/v1/loans/me/{loanId}
```

```text
404 Not Found
```

## DTOs

Serverseitige HTTP-DTOs sind nach Modul gruppiert:

```text
ReaderDtos.cs
CatalogDtos.cs
LoanDtos.cs
```

Der Client besitzt eigene Dateien mit denselben Transportstrukturen. Es besteht keine Projekt-Referenz vom Client auf Core-Module.

## Fehlerantworten

Fachliche Fehler werden als `ProblemDetails` zurückgegeben. Die Controller wählen den HTTP-Status explizit.

Beispiel:

```json
{
  "type": "...",
  "title": "Access not allowed",
  "status": 403,
  "detail": "..."
}
```

Typische Zuordnung:

```text
400 Bad Request   Validierung oder ungültige Identity-Daten
401 Unauthorized  keine authentifizierte technische Identität
403 Forbidden     falscher Account-Typ oder kein Zugriff auf fremde Ressource
404 Not Found     Reader, Book, BookItem oder Loan nicht gefunden
409 Conflict      bereits provisioniert, bereits verliehen oder anderer Konflikt
```

## Empfohlener manueller Ablauf

1. IdentityAccessServer starten.
2. CampusLibrary API starten.
3. optional Blazor-Client starten.
4. Development-Benutzer im IdentityAccessServer anlegen.
5. Access Token beziehen.
6. `POST /readers/me/provision` ausführen.
7. `GET /readers/me` prüfen.
8. `PUT /readers/me/profile` ausführen.
9. Books und BookItems anlegen.
10. Loan über `/loans/me` ausleihen und verlängern.
11. Loan am Schalter zurückgeben.

Die `.http`-Dateien verwenden Token-Variablen und prüfen die erwarteten Statuscodes mit Skript-Assertions.
