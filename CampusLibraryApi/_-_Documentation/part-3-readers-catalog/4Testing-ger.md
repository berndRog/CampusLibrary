# Teststrategie

Dieses Dokument beschreibt die Teststrategie, die im Projekt `CampusLibrary` verwendet wird.

Das Ziel besteht nicht nur darin, Korrektheit zu prüfen. Die verschiedenen Testebenen sollen auch für die Lehre sichtbar werden. Das Projekt trennt daher Domain Tests, Application UseCase Tests, Infrastructure Integration Tests und Controller-/API-Tests.

In Teil 3 wurde die Anwendung von einem funktionsfähigen Modul auf zwei funktionsfähige Module erweitert. Die Anwendung enthält jetzt das Readers-Modul und das Catalog-Modul. Das bestehende Readers-Verhalten bleibt stabil, während das neue Catalog-Verhalten ergänzt und durch Tests abgesichert wird.

Teil 3 ist daher nicht hauptsächlich ein Refactoring-Schritt. Teil 3 ist ein Erweiterungsschritt. Die Testsuite prüft beides:

```text
bestehendes Readers-Verhalten funktioniert weiterhin
neues Catalog-Verhalten funktioniert korrekt
```

Das Catalog-Modul führt zusätzliche fachliche Konzepte ein:

```text
Book
Author
BookItem
IsbnVo
Book-zu-BookItem 1:n-Beziehung
Book-zu-Author m:n-Beziehung
```

Die Tests prüfen außerdem, dass die Architekturregeln aus Teil 2 weiterhin gelten, obwohl ein zweites Modul hinzugefügt wurde.

## Überblick

Das aktuelle Testprojekt ist:

```text
CampusLibraryApiTest
```

Der Produktivcode ist auf mehrere Projekte verteilt:

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_4_Infrastructure
```

Die Tests decken folgende Bereiche ab:

```text
Domain Tests
Application UseCase Tests mit Mocks
Application Integration Tests mit SQLite und UnitOfWork
Infrastructure Tests für Repositories und ReadModels
Controller/API Tests mit WebApplicationFactory
```

Im aktuellen Projektstand sind alle Tests grün:

```text
Test summary: total: 155, failed: 0, succeeded: 155, skipped: 0
```

Alle Tests ausführen:

```bash
dotnet test
```

## Testebenen

## 1. Domain Tests

Domain Tests prüfen das Verhalten von Domain-Objekten ohne Infrastructure.

Im Readers-Modul sind typische Beispiele:

```text
Reader.Create(...)
Reader.UpdateProfile(...)
EmailVo.Create(...)
AddressVo.Create(...)
```

Im Catalog-Modul sind typische Beispiele:

```text
Book.Create(...)
Book.AddBookItem(...)
Book.AssignAuthor(...)
Book.Deactivate(...)

Author.Create(...)
Author.Deactivate(...)

BookItem.Create(...)
IsbnVo.Create(...)
```

Domain Tests konzentrieren sich auf fachliche Regeln:

```text
Pflichtwerte
gültige Wertebereiche
Normalisierung
ungültige Eingaben
Domain Errors
Aggregate-Invarianten
Value-Object-Validierung
Beziehungsregeln
aktiver/inaktiver Zustand
```

Die Domain-Schicht verwendet kein EF Core, kein ASP.NET Core, keine Repositories, keine Controller, kein Swagger und kein HTTP.

Das Hauptziel ist zu prüfen, ob Aggregates, Entities und Value Objects ihre eigenen Invarianten schützen.

In der Struktur des modularen Monolithen prüfen diese Tests hauptsächlich Code aus:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_2_BuildingBlocks
```

## Catalog Domain Tests

Die Catalog Domain Tests sind besonders wichtig, weil Teil 3 ein reichhaltigeres Domain Model einführt als Teil 2.

Die Tests prüfen zum Beispiel:

```text
ein Book kann mit gültigem Title und gültiger ISBN erzeugt werden
ein Book kann nicht mit ungültiger ISBN erzeugt werden
ein BookItem kann einem Book hinzugefügt werden
ein BookItem startet mit dem Status Available
ein Author kann einem Book zugeordnet werden
derselbe Author kann demselben Book nicht doppelt zugeordnet werden
ein Book kann deaktiviert werden
ein Author kann deaktiviert werden
CreatedAt und UpdatedAt müssen gültige UTC-Zeitstempel sein
```

Diese Tests machen die Aggregate-Grenze sichtbar.

Ein `BookItem` wird zum Beispiel über das `Book`-Aggregate hinzugefügt:

```text
Book.AddBookItem(...)
```

Der Test prüft daher nicht nur das `BookItem`, sondern auch, ob das `Book`-Aggregate die Konsistenz seines eigenen Objektgraphen schützt.

## 2. Application UseCase Tests mit Mocks

Application UseCase Tests prüfen die Orchestrierungslogik der UseCases.

Im Readers-Modul sind typische Beispiele:

```text
ReaderUcCreate
ReaderUcUpdate
ReaderUcDelete
```

Im Catalog-Modul sind typische Beispiele:

```text
BookUcCreate
BookUcAddBookItem
BookUcAssignAuthor
BookUcDeactivate

AuthorUcCreate
AuthorUcDeactivate
```

Diese Tests verwenden Mocks oder Test Doubles für Ports wie:

```text
IReaderRepository
IBookRepository
IAuthorRepository
IUnitOfWork
IClock
ILogger<T>
```

Ziel ist zu prüfen, ob der UseCase den Workflow korrekt koordiniert:

```text
grundlegende Eingaben prüfen
optionale Ids auflösen
Aggregates laden
Value Objects erzeugen
Eindeutigkeit prüfen
Domain-Methoden aufrufen
Änderungen speichern
DTOs oder Fehler zurückgeben
```

`BookUcCreate` prüft zum Beispiel, ob die ISBN bereits existiert, bevor ein neues Book erzeugt wird.

`BookUcAddBookItem` prüft, ob die InventoryNumber bereits existiert, bevor ein neues physisches BookItem hinzugefügt wird.

`BookUcAssignAuthor` lädt sowohl das Book als auch den Author, bevor die Domain-Methode aufgerufen wird, die den Author dem Book zuordnet.

Diese Tests sind weitgehend unabhängig von EF Core und HTTP. Sie konzentrieren sich auf Application-Logik innerhalb der Core-Module.

## Was mockbasierte UseCase Tests prüfen sollten

UseCase Tests mit Mocks sollten sowohl Erfolgs- als auch Fehlerpfade prüfen.

Typische Prüfungen im Erfolgsfall sind:

```text
die richtige Repository-Methode wird aufgerufen
die Domain-Methode wird über das Aggregate aufgerufen
UnitOfWork wird bei Erfolg genau einmal aufgerufen
das zurückgegebene DTO enthält die erwarteten Daten
```

Typische Prüfungen im Fehlerfall sind:

```text
ungültige Eingaben liefern einen Domain Error
fehlendes Aggregate liefert NotFound
doppelte Daten liefern Conflict
UnitOfWork wird bei Fehlern nicht aufgerufen
nach einem frühen Fehler erfolgen keine unnötigen Repository-Aufrufe
```

Ziel ist nicht, EF Core zu testen. Ziel ist, den Application Workflow zu testen.

## 3. Application Integration Tests

Application Integration Tests verwenden echte Infrastructure-Bestandteile, wenn das sinnvoll ist.

Sie prüfen, ob UseCases korrekt zusammenarbeiten mit:

```text
echter Repository-Implementierung
echtem UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
echten EF-Core-Mappings
```

Das ist nützlich, weil manche Fehler erst auftreten, wenn EF Core, Repository und UnitOfWork gemeinsam verwendet werden.

Diese Tests sind langsamer als reine Domain Tests, geben aber mehr Sicherheit, dass Application und Persistence korrekt zusammenspielen.

In Teil 3 sind diese Tests besonders wichtig, weil es jetzt zwei Core-Module und ein gemeinsames Infrastructure-Projekt gibt.

Die beabsichtigte Abhängigkeitsrichtung bleibt:

```text
Core-Module definieren Ports.
Infrastructure implementiert Ports.
Tests prüfen, dass beide korrekt zusammenspielen.
```

## Catalog Application Integration Tests

Catalog Integration Tests prüfen, ob Catalog UseCases korrekt mit den echten Persistenzadaptern funktionieren.

Typische Beispiele sind:

```text
einen Author erzeugen und persistieren
ein Book erzeugen und mit ISBN persistieren
ein BookItem hinzufügen und persistieren
einen Author zuordnen und die Book-Author-Beziehung persistieren
ein Book deaktivieren und IsActive aktualisieren
einen Author deaktivieren und IsActive aktualisieren
```

Diese Tests sind wichtig, weil Teil 3 Beziehungen enthält, die von EF Core korrekt abgebildet werden müssen.

Die Zuordnung eines Authors zu einem Book betrifft zum Beispiel:

```text
Book-Aggregate
Author-Aggregate
Book.Authors-Navigation
BookAuthorJoin-Tabelle in der Infrastructure
UnitOfWork
SQLite-Datenbank
```

Ein reiner Domain Test kann die Domain-Regel prüfen.

Ein Integration Test prüft, ob die Beziehung tatsächlich persistiert wird.

## 4. Infrastructure Tests

Infrastructure Tests prüfen die Persistenzadapter.

Typische Bereiche sind:

```text
ReaderRepositoryEf
ReaderReadModelEf

BookRepositoryEf
AuthorRepositoryEf
BookReadModelEf
AuthorReadModelEf

AppDbContext
EF-Core-Mappings
SQLite-Verhalten
```

Das Repository gehört zur Schreibseite.

Das ReadModel gehört zur Leseseite.

Diese Trennung ist beabsichtigt:

```text
Repository -> domain-orientierter Schreibzugriff
ReadModel  -> DTO-orientierter Lesezugriff
```

Infrastructure Tests helfen zu prüfen, ob Entities, Value Objects, Conversions, Beziehungen und Queries korrekt mit der Datenbank funktionieren.

In der projektbasierten Struktur prüfen diese Tests hauptsächlich Code aus:

```text
CampusLibraryApi_4_Infrastructure
```

zusammen mit Domain-Typen und Ports aus:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_2_BuildingBlocks
```

## Repository Tests

Repository Tests prüfen das Persistenzverhalten auf der Schreibseite.

Für Readers ist typisches Repository-Verhalten:

```text
Reader hinzufügen
Reader anhand der Id finden
Reader anhand der Email finden
Subject-Eindeutigkeit prüfen
Reader entfernen
```

Für Catalog ist typisches Repository-Verhalten:

```text
Author hinzufügen
Author anhand der Id finden
Eindeutigkeit des Author-Namens prüfen

Book hinzufügen
Book anhand der Id finden
ISBN-Eindeutigkeit prüfen
Eindeutigkeit der InventoryNumber prüfen
Book mit Authors laden
Book mit BookItems laden
```

Repositories arbeiten mit Domain-Objekten.

Sie sind nicht für optimierte DTO-Projektionen zuständig.

## ReadModel Tests

ReadModel Tests prüfen Projektionen auf der Leseseite.

ReadModels geben direkt DTOs zurück.

Typische Reader ReadModel Tests prüfen:

```text
alle Reader auswählen
Reader anhand der Id finden
Reader anhand der Email finden
```

Typische Catalog ReadModel Tests prüfen:

```text
alle aktiven Authors auswählen
aktiven Author anhand der Id finden
aktive Authors suchen

alle aktiven Books auswählen
aktives Book anhand der Id finden
aktive Books nach Title suchen
aktive Books nach AuthorName suchen
aktive Books nach ISBN suchen
aktive Books anhand der AuthorId auswählen
```

ReadModel Tests sind außerdem dafür verantwortlich, Sichtbarkeitsregeln auf der Leseseite zu prüfen.

Zum Beispiel:

```text
inaktive Books werden von normalen Book ReadModels nicht zurückgegeben
inaktive Authors werden von normalen Author ReadModels nicht zurückgegeben
```

Das unterscheidet sich vom Repository-Verhalten.

Repositories dürfen inaktive Aggregates weiterhin laden, weil UseCases sie möglicherweise benötigen.

## Repository Tests vs. ReadModel Tests

Teil 3 macht die Unterscheidung zwischen Repository Tests und ReadModel Tests besonders sichtbar.

Repositories testen das Persistenzmodell der Schreibseite.

ReadModels testen das Query-Modell der Leseseite.

Bei Catalog wird diese Unterscheidung besonders durch Deactivation sichtbar.

Repository Tests sollten prüfen:

```text
ein deaktiviertes Book ist weiterhin gespeichert
ein deaktiviertes Book kann weiterhin als Aggregate geladen werden
IsActive ist false

ein deaktivierter Author ist weiterhin gespeichert
ein deaktivierter Author kann weiterhin als Aggregate geladen werden
IsActive ist false
```

ReadModel Tests sollten prüfen:

```text
inaktive Books werden in normalen Book-Listen verborgen
inaktive Books werden in normalen Book-Suchergebnissen verborgen
inaktive Authors werden in normalen Author-Listen verborgen
inaktive Authors werden in normalen Author-Suchergebnissen verborgen
```

Die didaktische Regel lautet:

```text
Repositories laden Aggregates für Änderungen.
ReadModels entscheiden, was in Queries sichtbar ist.
```

## 5. Controller- / API-Tests

Controller Tests verwenden:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
```

Diese Tests starten die ASP.NET-Core-Anwendung in einem Testhost und rufen die API über HTTP auf.

Sie prüfen:

```text
Routing
Model Binding
Controller Actions
Status Codes
JSON-Serialisierung
ProblemDetails-Mapping
Dependency Injection
Datenbankintegration
Swagger-kompatibles API-Verhalten
```

Die Reader Controller Tests decken folgende Endpunkte ab:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Die Author Controller Tests decken folgende Endpunkte ab:

```text
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

Die Book Controller Tests decken folgende Endpunkte ab:

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=...&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

Diese Tests sind am nächsten an der realen API-Nutzung.

In Teil 3 prüfen Controller-/API-Tests außerdem, ob beide Module korrekt durch das ausführbare API-Projekt verdrahtet werden.

Sie prüfen daher nicht nur Controller-Verhalten, sondern auch die Zusammensetzung von:

```text
Web
Readers-Modul
Catalog-Modul
Infrastructure
BuildingBlocks
```

## Testdatenbank

Die Tests verwenden SQLite über die Testinfrastruktur.

Die Testdatenbank wird erzeugt durch:

```text
TestDatabase
TestBaseFactory
```

Die Factory ersetzt ausgewählte Produktivdienste:

```text
AppDbContext
IUnitOfWork
IClock
TestSeed
Authentication
```

Eine Fake Clock wird verwendet, um Zeitstempel deterministisch zu machen.

Das ist wichtig, weil die Domain UTC-Zeitstempel erwartet.

Beispiel:

```csharp
public DateTime TestCreatedAt { get; set; } =
   new(2025, 01, 01, 00, 00, 00, DateTimeKind.Utc);
```

Die Fake Clock ist besonders nützlich für Aggregates, die Audit-Zeitstempel speichern:

```text
CreatedAt
UpdatedAt
```

## Test Seed

Der Test Seed stellt stabile Demo- und Testdaten bereit.

Typische Readers sind:

```text
Reader1
Reader2
Reader3
Reader4
Reader5
Reader6
ReaderRegister
```

Typische Catalog-Daten sind:

```text
Author1
Author2
Author3

Book1
Book2
Book3

Books with Authors
Books with BookItems
```

Tests sollten Seed-Daten gegenüber manuell konstruierten Ad-hoc-Daten bevorzugen.

Dadurch bleiben Beispiele konsistent und für Studierende leichter verständlich.

Für Catalog Tests sind Seed-Daten außerdem hilfreich, weil Beziehungen bei Bedarf aus demselben getrackten Objektgraphen aufgebaut werden sollten.

Books mit Authors sollten zum Beispiel vorhandene Author-Instanzen verwenden, statt doppelte Author-Objekte mit denselben Ids zu erzeugen.

## Tests für partielle Updates

`ReaderUpdateDto` unterstützt partielle Updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Die Bedeutung von `null` lautet:

```text
Lastname = null   -> aktuellen Nachnamen beibehalten
Email = null      -> aktuelle Email beibehalten
AddressDto = null -> aktuelle Adresse beibehalten
```

Nur angegebene Werte werden geändert.

Ein leerer oder nur aus Leerzeichen bestehender Nachname ist nicht dasselbe wie `null`.

```text
null       -> keine Änderung
""         -> ungültiger Wert
"   "      -> ungültiger Wert
"Meier"   -> gültige Änderung
```

Diese Unterscheidung ist für die Semantik partieller Updates wichtig.

Die Tests sollten daher beide Fälle abdecken:

```text
Feld fehlt oder ist null           -> keine Änderung
Feld ist angegeben, aber ungültig  -> Validierungsfehler
```

## Tests für Catalog-Beziehungen

Teil 3 ergänzt Beziehungen, die explizit getestet werden müssen.

## Book zu BookItem

Ein Book kann mehrere BookItems besitzen.

Das ist eine 1:n-Beziehung:

```text
Book 1 --- n BookItem
```

Die Tests sollten prüfen:

```text
ein BookItem kann einem bestehenden Book hinzugefügt werden
ein BookItem benötigt eine InventoryNumber
die InventoryNumber muss eindeutig sein
ein neues BookItem startet mit dem Status Available
das Hinzufügen eines BookItems aktualisiert das Book-Aggregate
die Beziehung wird durch EF Core persistiert
ReadModels zeigen die Gesamtzahl und die verfügbaren BookItems
```

## Book zu Author

Ein Book kann mehrere Authors haben.

Ein Author kann mehreren Books zugeordnet sein.

Das ist eine m:n-Beziehung:

```text
Book n --- m Author
```

Die Tests sollten prüfen:

```text
ein bestehender Author kann einem bestehenden Book zugeordnet werden
derselbe Author kann demselben Book nicht doppelt zugeordnet werden
das Zuordnen eines Authors aktualisiert das Book-Aggregate
die Beziehung wird über die BookAuthorJoin-Tabelle persistiert
ReadModels geben Authors in stabiler Reihenfolge zurück
Books können anhand einer AuthorId ausgewählt werden
```

Die technische Join-Tabelle wird nicht als eigenes Domain-Konzept getestet.

Sie wird über das Persistenzverhalten getestet.

Die didaktische Regel lautet:

```text
Die Domain zeigt die Beziehung.
Infrastructure persistiert die Beziehung.
```

## Deactivation Tests

Teil 3 verwendet Deactivation für Catalog Books und Authors.

Deactivate ist nicht dasselbe wie Delete.

```text
IsActive = false
```

Die Tests sollten beide Seiten dieser Entscheidung prüfen.

Repository- und UseCase-Tests prüfen:

```text
das Aggregate existiert weiterhin
IsActive ist false
die Änderung ist persistiert
UpdatedAt ist aktualisiert
```

ReadModel Tests prüfen:

```text
inaktive Books werden in normalen Book Queries nicht zurückgegeben
inaktive Authors werden in normalen Author Queries nicht zurückgegeben
```

Controller-/API-Tests prüfen:

```text
PATCH /books/{bookId}/deactivate liefert 200 OK
PATCH /authors/{id}/deactivate liefert 200 OK
normale GET-Endpunkte liefern deaktivierte Ressourcen nicht mehr zurück
```

Diese Trennung macht die Designentscheidung in den Tests sichtbar.

## Warum unterschiedliche Testarten?

Jede Testart beantwortet eine andere Frage.

```text
Domain Test:
Funktioniert die fachliche Regel?

UseCase Mock Test:
Ruft der Application Workflow die richtigen Ports auf und behandelt er Fehler korrekt?

Application Integration Test:
Funktioniert der UseCase mit echter Persistenz?

Infrastructure Test:
Speichert, lädt und projiziert EF Core die Daten korrekt?

Controller-/API-Test:
Verhält sich die API von außen korrekt?
```

Zusammen bilden diese Tests eine lehrorientierte Teststrategie.

## Warum die Tests in Teil 3 wichtig sind

Teil 3 ergänzt ein zweites fachliches Modul.

Das erwartete Ergebnis lautet:

```text
Die Architektur wächst.
Das bestehende Verhalten bleibt stabil.
Das neue Verhalten ist durch Tests abgesichert.
```

Die Testsuite ist das Sicherheitsnetz für diese Erweiterung.

Wenn nach dem Hinzufügen des Catalog-Moduls alle Tests grün bleiben, gibt das Vertrauen, dass:

```text
Readers weiterhin funktioniert
Catalog funktioniert
Modulgrenzen weiterhin eingehalten werden
Infrastructure Ports aus mehreren Modulen korrekt implementiert
das API-Projekt alle Module korrekt verdrahtet
```

Der aktuelle Stand lautet:

```text
155 Tests
0 failed
0 skipped
```

## Empfohlener Workflow

Während der Entwicklung:

```bash
dotnet test
```

Bei API-Änderungen zusätzlich die Anwendung starten und Swagger prüfen:

```bash
dotnet run --project CampusLibraryApi
```

Swagger ist im Development-Modus erreichbar unter:

```text
https://localhost:8010/swagger
```

Bei Catalog-Änderungen ist Swagger besonders nützlich, um Folgendes zu prüfen:

```text
Authors-Endpunkte
Books-Endpunkte
DTO-Schemas
ProblemDetails-Antworten
BookItemStatus-Darstellung
BookSearchField-Darstellung
```

## Version

Die aktuelle Version gehört zu Teil 3:

```text
Branch: part-3/readers-catalog
Tag:    v3-readers-catalog
```

Teil 2 bleibt verfügbar als:

```text
Tag: v2-readers-modular-monolith
```

Teil 1 bleibt verfügbar als:

```text
Tag: v1-readers-monolith
```

## Didaktische Ziele

Die Testsuite soll Studierenden helfen, folgende Themen zu verstehen:

```text
Trennung von Testebenen
Domain Testing ohne Infrastructure
mockbasiertes Testen von UseCases
Integration Testing mit SQLite
Controller Testing über HTTP
Wiederverwendung von Testdaten über Seed-Objekte
Nutzen von Fake Clocks
Testen partieller Updates
Tests als Schutz bei Architektur-Refactorings
Tests als Schutz bei Architektur-Erweiterungen
End-to-End-Tests trotz modularem Monolithen
Testen von Beziehungen über Domain und Infrastructure hinweg
Unterschied zwischen ReadModels und Repositories
Unterschied zwischen Deactivation und Delete
```

Die Tests sind daher nicht nur ein Sicherheitsnetz, sondern auch Teil des Lernmaterials.

## Didaktische Faustregel

Jede Testebene hat ihren eigenen Zweck:

```text
Domain Tests schützen fachliche Regeln.
UseCase Tests schützen Application Workflows.
Infrastructure Tests schützen Persistenzverhalten.
Controller Tests schützen die HTTP API.
End-to-End-Tests schützen die Gesamtkomposition.
```

Für Teil 3 ist der wichtigste Lehrpunkt:

```text
Eine modulare Erweiterung ist erfolgreich, wenn die Architektur wächst,
das bestehende Verhalten stabil bleibt
und das neue Verhalten durch Tests nachgewiesen wird.
```

Eine weitere wichtige Regel lautet:

```text
Repository Tests weisen nach, dass Aggregates gespeichert und geladen werden können.
ReadModel Tests weisen nach, was die Anwendung nach außen zeigt.
```

Für Catalog ist das besonders bei Deactivation sichtbar:

```text
Repositories dürfen inaktive Aggregates weiterhin laden.
ReadModels verbergen inaktive Daten in normalen Queries.
```
