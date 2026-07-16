# Testing-Strategie – CampusLibrary Teil 6

Englische Version: [4Testing.md](4Testing.md)

Offizieller Branch:

```text
part-6/authn-provision-profile
```

## Ziel

Teil 6 erweitert die Teststrategie um technische Identität, Provisioning, Profilzustände, Bearer-Token-Durchsatz und Subject-basierte `/me`-Workflows.

Die Tests sollen zwei Dinge gleichzeitig beweisen:

```text
Die fachlichen Änderungen funktionieren.
Die Architektur bleibt von HTTP-, Claim- und JWT-Details entkoppelt.
```

Der zuletzt vollständig verifizierte Stand war:

```text
238 Tests
0 fehlgeschlagen
0 übersprungen
```

Nach dem finalen Merge sollte erneut ausgeführt werden:

```bash
dotnet clean
dotnet build
dotnet test
```

## Testebenen

```text
Domain Tests
Application UseCase Mock Tests
Application Integration Tests
Infrastructure Repository Tests
Infrastructure ReadModel Tests
Controller-/API-End-to-End-Tests
manuelle HTTP-Skripte
```

Controller-Mock-Tests werden nicht als breite eigene Testebene verwendet. Controller sollen dünne HTTP-Adapter bleiben. Der Application Workflow wird in UseCase-Tests geprüft, der öffentliche HTTP-Vertrag über `WebApplicationFactory` und `HttpClient`.

## Identity-Tests

### `IdentitySubject.Check(...)`

Die Prüfung sollte mindestens folgende Fälle abdecken:

```text
nicht authentifiziert                → IdentityUnauthenticated
kein Reader-Account                  → AccessNotAllowed
Subject leer                         → SubjectRequired
Subject länger als 200 Zeichen       → InvalidIdentitySubject
Username leer                        → IdentityEmailRequired
CreatedAt default                    → TimestampInvalid
gültige Reader-Identität             → Success(subject)
```

`AdminRights` wird nicht fachlich geprüft. Der Wert gehört zum technischen Kompatibilitätsvertrag.

### Fake Identity Gateway

UseCase- und Integrationstests verwenden einen kontrollierbaren `IIdentityGateway` beziehungsweise Fake. Dadurch können Identity-Szenarien ohne echten IdentityAccessServer und ohne JWT-Bibliothek getestet werden.

Typische Testidentitäten:

```text
Reader mit gültigem Subject
Employee
nicht authentifizierter Benutzer
Reader mit fehlendem Subject
Reader mit unvollständigem Profil
```

## Reader-Provisioning

Tests für `ReaderUcCreateMeProvision` prüfen unter anderem:

```text
gültige technische Reader-Identität erzeugt einen fachlichen Reader
Subject wird dauerhaft gespeichert
Username wird als initiale E-Mail verwendet
optionale Test-ID wird reproduzierbar übernommen
doppelte Provisionierung wird abgelehnt
Employee darf keinen Reader provisionieren
ungültige Identity-Daten werden vor Persistence abgewiesen
UnitOfWork wird nur bei Erfolg gespeichert
```

## Initiale Profilvervollständigung

Tests für `ReaderUcUpdateMeProfile` prüfen:

```text
Reader wird über Subject gefunden
Vorname, Nachname und Adresse werden gesetzt
IsProfileCompleted wird true
nicht provisionierter Reader liefert NotFound
bereits vollständiges Profil wird nach der festgelegten Regel behandelt
Validation Errors werden weitergegeben
```

## Spätere Self-Service-Aktualisierung

Tests für `ReaderUcUpdateMe` prüfen:

```text
keine Reader-ID wird vom Client benötigt
Reader wird über Identity Subject gefunden
Lastname kann geändert werden
Email kann geändert werden
Adresse kann geändert werden
null lässt bestehende Werte unverändert
fremde Reader können nicht über eine ID gewählt werden
```

Ein wichtiger Regressionstest bestätigt, dass die Reader-Zuordnung nach einer E-Mail-Änderung weiterhin über `Subject` funktioniert.

## Loan-Tests

### Borrow Me

Tests prüfen:

```text
Reader ist authentifiziert
Reader-Account ist erforderlich
Reader ist provisioniert
Profil ist vollständig
Reader ist aktiv
BookItem existiert
BookItem ist ausleihbar
BookItem besitzt keinen laufenden Loan
Loan erhält ReaderId aus der Subject-Zuordnung
optionale Loan-Test-ID wird übernommen
```

### Select Me

ReadModel- und API-Tests prüfen, dass `/loans/me` nur Loans des aktuellen Readers liefert.

### Renew Me

Tests prüfen:

```text
Loan gehört dem aktuellen Reader
Renewal Count wird erhöht
DueDate wird über Fake Clock reproduzierbar berechnet
fachliche Verlängerungsgrenzen werden eingehalten
fremde Loans liefern AccessNotAllowed oder NotFound gemäß API-Vertrag
```

### Return at Desk

Die Rückgabe löscht den Loan. Tests prüfen:

```text
Loan existiert vor der Rückgabe
Return liefert Erfolg
Loan ist danach nicht mehr im Repository
GET danach liefert 404
BookItem kann anschließend erneut ausgeliehen werden
```

Alte Annahmen wie `Loan.Status`, `ReturnedAt` oder `LoanAlreadyReturned` gehören nicht mehr zum aktuellen Modell.

## Catalog-Tests

Catalog-Tests prüfen weiterhin:

```text
Book-Erzeugung
ISBN-Validierung
AuthorsText
BookItem-Erzeugung
Book-Suche nach Title, AuthorLastName und Isbn
Deaktivierung
Ausblenden inaktiver Books in ReadModels
```

Ein BC-to-BC-Test für `BookUcDeactivate` stellt sicher, dass ein Book mit laufenden Loans nicht deaktiviert wird. Der UseCase verwendet `ILoanCatalogContract`, nicht direkten Zugriff auf die Loan-Tabelle.

## Repository- und ReadModel-Tests

Repositories testen Aggregate und Schreibzustand. ReadModels testen öffentliche Projektionen.

```text
Repository:
Kann das Aggregate gespeichert und für einen UseCase geladen werden?

ReadModel:
Welche Daten zeigt die Anwendung nach außen?
```

ReaderReadModel-Tests müssen Subject-basierte Suche und `IsProfileCompleted` korrekt projizieren.

LoanReadModel-Tests verwenden das aktuelle `LoanDto` ohne alte List-/Detail-Doppeltypen und ohne Returned-Status.

## Controller-/API-End-to-End-Tests

E2E-Tests verwenden:

```text
WebApplicationFactory<Program>
HttpClient
Testdatenbank
Test-Identity-Adapter oder Test-Authentifizierung
```

Sie prüfen:

```text
Routing
JWT-/Identity-Durchsatz bis zum Gateway
Model Binding
Statuscodes
JSON-Serialisierung
ProblemDetails
Dependency Injection
EF-Core-Integration
öffentliche DTO-Strukturen
```

Wichtige Szenarien:

```text
POST /readers/me/provision → 204
GET /readers/me → 200
PUT /readers/me/profile → 200
PUT /readers/me/update → 200
GET /loans/me → 200
POST /loans/me → 201
GET /loans/me/{id} → 200
PATCH /loans/me/{id}/renew → 200
PATCH /loans/{id}/return-at-desk → 204
GET nach Return → 404
```

Tests für nicht authentifizierte oder falsche Rollen prüfen `401` und `403`.

## IdentityAccessServer und manuelle Tests

Automatisierte CampusLibrary-Tests sollen nicht von einem extern gestarteten IdentityAccessServer abhängen. Das hält Tests schnell und reproduzierbar.

Der echte End-to-End-Fluss wird zusätzlich manuell über `.http`-Skripte geprüft:

```text
Development-Benutzer anlegen
Token beziehen
Reader provisionieren
Reader lesen
Profil vervollständigen
Book und BookItem anlegen
Loan ausleihen
Loan verlängern
Loan zurückgeben
```

Die Skripte prüfen erwartete Statuscodes mit Assertions.

## Client-Tests und manuelle Clientprüfung

Der Client muss insbesondere zeigen:

```text
Anmeldung und Abmeldung
angemeldeten Benutzer darstellen
Access Token an API-Requests anhängen
Catalog ohne unnötige ReaderId aufrufen
Reader-Profilstatus anzeigen
Provisioning/Profile-Seiten korrekt navigieren
Loans über /me aufrufen
401/403/ProblemDetails sinnvoll behandeln
```

Die API bleibt die fachliche Autorität. Clientseitige Sichtbarkeit ersetzt keine serverseitige Prüfung.

## Abschlusskriterien

Vor Merge und Tag:

```bash
git diff --cached --check
dotnet clean
dotnet build
dotnet test
```

Danach sollten geprüft werden:

```text
Arbeitsbaum sauber
Branch part-6/authn-provision-profile gepusht
Tag v6-authn-provision-profile zeigt auf den finalen Commit
nur der offizielle Part-6-Branch bleibt veröffentlicht
```

## Didaktische Kernaussage

```text
Authentifizierung liefert eine technische Identität.
Provisioning verbindet sie mit einem fachlichen Reader.
UseCases kennen nur IIdentityGateway.
Tests ersetzen technische Tokenquellen durch kontrollierbare Adapter.
```
