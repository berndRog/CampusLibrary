# Testing-Strategie

Dieses Dokument beschreibt die Testing-Strategie im Projekt `CampusLibrary`.

Das Ziel ist nicht nur, die fachliche Korrektheit zu prüfen. Die Tests sollen auch die unterschiedlichen Testebenen für Studierende sichtbar machen. Das Projekt trennt deshalb Domain Tests, Application UseCase Tests, Application Integration Tests, Infrastructure Tests, Controller-/API-End-to-End-Tests und manuelle HTTP-Dateien.

Für Teil 3 wird bewusst entschieden: Controller-Mock-Tests werden nicht als breite zusätzliche Testebene verwendet. Die Controller bleiben dünn. Die fachliche Logik wird in Domain- und UseCase-Tests geprüft, Persistence und Projektionen werden in Infrastructure Tests geprüft, und der öffentliche HTTP-Vertrag wird über `WebApplicationFactory` und `HttpClient` getestet.

In Teil 3 wurde die Anwendung von einem fachlichen Modul auf zwei fachliche Module erweitert. Die Anwendung enthält nun das Readers-Modul und das Catalog-Modul. Das bestehende Readers-Verhalten bleibt stabil, während das neue Catalog-Verhalten ergänzt und durch Tests abgesichert wird.

Teil 3 ist deshalb nicht hauptsächlich ein Refactoring-Schritt. Teil 3 ist ein Erweiterungsschritt. Die Testsuite prüft beides:

```text
bestehendes Readers-Verhalten funktioniert weiterhin
neues Catalog-Verhalten funktioniert korrekt
```

Das Catalog-Modul führt zusätzliche Domain-Konzepte ein:

```text
Book
Author
BookItem
IsbnVo
Book-zu-BookItem 1:n-Beziehung
Book-zu-Author m:n-Beziehung
```

Die Tests prüfen außerdem, dass die Architekturregeln aus Teil 2 weiterhin gelten, wenn ein zweites Modul ergänzt wird.

## Überblick

Das aktuelle Testprojekt ist:

```text
CampusLibraryApiTest
```

Der produktive Code ist auf mehrere Projekte aufgeteilt:

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
Controller-/API-End-to-End-Tests mit WebApplicationFactory und HttpClient
Manuelle HTTP-Dateien für didaktische API-Tests
```

Controller-Mock-Tests werden bewusst nicht als eigene Testebene aufgeführt.

Der Grund ist, dass die Controller keine fachliche Logik enthalten sollen. Sie nehmen HTTP-Eingaben entgegen, rufen UseCases oder ReadModels auf und übersetzen Ergebnisse in HTTP-Antworten. Dieses Verhalten ist über echte HTTP-Requests nützlicher zu testen als über isolierte Controller-Mocks.

Alle automatisierten Tests werden ausgeführt mit:

```bash
dotnet test
```

Am Ende von Teil 3 sollte das finale Ergebnis von `dotnet test` in README und Projektdokumentation übernommen werden. Die wichtigste Abschlussbedingung ist:

```text
0 fehlgeschlagen
0 übersprungen
```

Wenn zusätzliche Controller-/API-End-to-End-Tests für Authors und Books ergänzt werden, ist die Gesamtzahl der Tests höher als die frühere Teil-3-Zahl von 155.

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
Validierung von Value Objects
Beziehungsregeln
Aktiv-/Inaktiv-Zustand
```

Die Domain Layer verwendet kein EF Core, kein ASP.NET Core, keine Repositories, keine Controller, kein Swagger und kein HTTP.

Das wichtigste Ziel ist zu prüfen, dass Aggregates, Entities und Value Objects ihre eigenen Invarianten schützen.

In der Struktur des modularen Monolithen prüfen diese Tests hauptsächlich Code aus:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_2_BuildingBlocks
```

## Catalog Domain Tests

Die Catalog Domain Tests sind wichtig, weil Teil 3 ein reichhaltigeres Domain Model einführt als Teil 2.

Die Tests prüfen zum Beispiel:

```text
ein Book kann mit gültigem Titel und gültiger ISBN erzeugt werden
ein Book kann nicht mit ungültiger ISBN erzeugt werden
ein BookItem kann einem Book hinzugefügt werden
ein BookItem startet mit dem Status Available
ein Author kann einem Book zugeordnet werden
derselbe Author kann demselben Book nicht zweimal zugeordnet werden
ein Book kann deaktiviert werden
ein Author kann deaktiviert werden
CreatedAt und UpdatedAt müssen gültige UTC-Zeitpunkte sein
```

Diese Tests machen die Aggregate-Grenze sichtbar.

Ein `BookItem` wird zum Beispiel über das `Book`-Aggregate hinzugefügt:

```text
Book.AddBookItem(...)
```

Der Test prüft deshalb nicht nur das `BookItem`, sondern auch, dass das `Book`-Aggregate die Konsistenz seines eigenen Objektgraphen schützt.

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

Der Zweck ist zu prüfen, ob der UseCase den Ablauf korrekt koordiniert:

```text
grundlegende Eingaben validieren
optionale IDs auflösen
Aggregates laden
Value Objects erzeugen
Eindeutigkeit prüfen
Domain-Methoden aufrufen
Änderungen speichern
DTOs oder Fehler zurückgeben
```

Zum Beispiel prüft `BookUcCreate`, ob die ISBN bereits existiert, bevor ein neues Book erzeugt wird.

`BookUcAddBookItem` prüft, ob die InventoryNumber bereits existiert, bevor ein neues physisches BookItem hinzugefügt wird.

`BookUcAssignAuthor` lädt sowohl das Book als auch den Author, bevor die Domain-Methode aufgerufen wird, die den Author dem Book zuordnet.

Diese Tests sind weitgehend unabhängig von EF Core und HTTP. Sie konzentrieren sich auf die Application-Logik innerhalb der Core-Module.

## Was Mock-basierte UseCase Tests prüfen sollten

UseCase Tests mit Mocks sollten sowohl Erfolgsfälle als auch Fehlerfälle prüfen.

Typische Prüfungen im Erfolgsfall sind:

```text
die richtige Repository-Methode wird aufgerufen
die Domain-Methode wird über das Aggregate aufgerufen
UnitOfWork wird bei Erfolg genau einmal aufgerufen
das zurückgegebene DTO enthält die erwarteten Daten
```

Typische Prüfungen im Fehlerfall sind:

```text
ungültige Eingabe liefert einen Domain Error
fehlendes Aggregate liefert NotFound
doppelte Daten liefern Conflict
UnitOfWork wird bei Fehlern nicht aufgerufen
nach einem frühen Fehler werden keine unnötigen Repository-Aufrufe ausgeführt
```

Der Zweck ist nicht, EF Core zu testen. Der Zweck ist, den Application Workflow zu testen.

## Warum Controller-Mock-Tests keine eigene Testebene sind

Controller-Mock-Tests werden in Teil 3 bewusst nicht als breite zusätzliche Testebene verwendet.

Der Grund ist das beabsichtigte Controller-Design:

```text
Controller nehmen HTTP-Eingaben entgegen.
Controller rufen ReadModels oder UseCases auf.
Controller übersetzen Result<T> in HTTP-Antworten.
Controller sollen keine fachliche Logik enthalten.
```

Wenn Controller dünn sind, wiederholen isolierte Controller-Mock-Tests meistens Verhalten, das an anderer Stelle bereits geprüft wird.

Der Application Workflow wird durch UseCase Tests mit Mocks geprüft.

Das Query-Verhalten wird durch ReadModel Tests geprüft.

Der HTTP-Vertrag wird durch Controller-/API-End-to-End-Tests mit `WebApplicationFactory` und `HttpClient` geprüft.

Controller-Mock-Tests wären nur dann sinnvoll, wenn im Controller selbst relevante Verzweigungen, besondere Statuscode-Entscheidungen, eigene Header-Logik, komplexes Authorization-Verhalten oder manuelles Response-Mapping enthalten wären, das nicht durch eine gemeinsame Hilfsmethode abgedeckt ist.

Für Teil 3 lautet die didaktische Entscheidung deshalb:

```text
Keine breite Controller-Mock-Testebene.
UseCase Tests verwenden Mocks.
Controller-/API-Tests verwenden echtes HTTP über HttpClient.
```

## 3. Application Integration Tests

Application Integration Tests verwenden echte Infrastructure-Bestandteile, wo das sinnvoll ist.

Sie prüfen, ob UseCases zusammenarbeiten mit:

```text
echter Repository-Implementierung
echter UnitOfWork
SQLite-Testdatenbank
EF-Core-Tracking
echten EF-Core-Mappings
```

Das ist nützlich, weil manche Fehler erst sichtbar werden, wenn EF Core, Repository und UnitOfWork zusammenspielen.

Diese Tests sind langsamer als reine Domain Tests, geben aber mehr Sicherheit, dass Application und Persistence korrekt zusammenspielen.

In Teil 3 sind diese Tests besonders wichtig, weil es nun zwei Core-Module und ein gemeinsames Infrastructure-Projekt gibt.

Die beabsichtigte Dependency-Richtung bleibt:

```text
Core-Module definieren Ports.
Infrastructure implementiert Ports.
Tests prüfen, ob beides zusammen korrekt funktioniert.
```

## Catalog Application Integration Tests

Catalog Integration Tests prüfen, ob Catalog UseCases mit den echten Persistence-Adaptern korrekt funktionieren.

Typische Beispiele sind:

```text
das Erzeugen eines Authors persistiert den Author
das Erzeugen eines Books persistiert Book und ISBN
das Hinzufügen eines BookItems persistiert das BookItem
das Zuordnen eines Authors persistiert die Book-Author-Beziehung
das Deaktivieren eines Books aktualisiert IsActive
das Deaktivieren eines Authors aktualisiert IsActive
```

Diese Tests sind wichtig, weil Teil 3 Beziehungen enthält, die von EF Core korrekt gemappt werden müssen.

Zum Beispiel umfasst das Zuordnen eines Authors zu einem Book:

```text
Book-Aggregate
Author-Aggregate
Book.Authors-Navigation
BookAuthorJoin-Tabelle in Infrastructure
UnitOfWork
SQLite-Datenbank
```

Ein reiner Domain Test kann die Domain-Regel prüfen.

Ein Integration Test prüft, ob die Beziehung tatsächlich persistiert wird.

## 4. Infrastructure Tests

Infrastructure Tests prüfen die Persistence-Adapter.

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

Das ReadModel gehört zur Query-Seite.

Diese Trennung ist bewusst gewählt:

```text
Repository -> domain-orientierter Schreibzugriff
ReadModel  -> DTO-orientierter Lesezugriff
```

Infrastructure Tests helfen zu prüfen, ob Entities, Value Objects, Conversions, Beziehungen und Queries korrekt mit der Datenbank funktionieren.

In der projektbasierten Struktur prüfen diese Tests hauptsächlich Code aus:

```text
CampusLibraryApi_4_Infrastructure
```

zusammen mit Domain Types und Ports aus:

```text
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_3_Core_Catalog
CampusLibraryApi_2_BuildingBlocks
```

## Repository Tests

Repository Tests prüfen das Verhalten der schreibseitigen Persistence.

Für Readers sind typische Repository-Verhalten:

```text
Reader hinzufügen
Reader anhand der ID finden
Reader anhand der Email finden
Subject-Eindeutigkeit prüfen
Reader entfernen
```

Für Catalog sind typische Repository-Verhalten:

```text
Author hinzufügen
Author anhand der ID finden
Eindeutigkeit des Author-Namens prüfen

Book hinzufügen
Book anhand der ID finden
ISBN-Eindeutigkeit prüfen
InventoryNumber-Eindeutigkeit prüfen
Book mit Authors laden
Book mit BookItems laden
```

Repositories arbeiten mit Domain-Objekten.

Sie sind nicht für optimierte DTO-Projektionen verantwortlich.

## ReadModel Tests

ReadModel Tests prüfen query-seitige Projektionen.

ReadModels liefern DTOs direkt zurück.

Typische Reader ReadModel Tests prüfen:

```text
alle Reader auswählen
Reader anhand der ID finden
Reader anhand der Email finden
```

Typische Catalog ReadModel Tests prüfen:

```text
alle aktiven Authors auswählen
aktiven Author anhand der ID finden
aktive Authors anhand des Nachnamens suchen

alle aktiven Books auswählen
aktives Book anhand der ID finden
aktive Books anhand des Titels suchen
aktive Books anhand des Author-Nachnamens suchen
aktive Books anhand der ISBN suchen
aktive Books anhand der Author-ID auswählen
```

ReadModel Tests sind außerdem dafür verantwortlich, die Sichtbarkeitsregeln der Leseseite zu prüfen.

Zum Beispiel:

```text
inaktive Books werden von normalen Book ReadModels nicht zurückgegeben
inaktive Authors werden von normalen Author ReadModels nicht zurückgegeben
```

Das unterscheidet sich vom Repository-Verhalten.

Repositories dürfen inaktive Aggregates weiterhin laden, weil UseCases sie möglicherweise benötigen.

## Tests für die Catalog-Suche

Teil 3 enthält Tests für die Suche nach Authors und Books im Catalog.

Die Author-Suche verwendet den Nachnamen des Authors:

```text
GET /camplib/v1/authors/search?searchText=Martin
```

Diese Suche soll `Robert C. Martin` finden, aber nicht `Martin Fowler`, weil `Martin` bei `Martin Fowler` nur der Vorname ist.

Die Book-Suche unterstützt folgende Suchfelder:

```text
Title
AuthorLastName
Isbn
```

`AuthorLastName` sucht ausschließlich im Nachnamen der zugeordneten Authors.

Der Vorname wird nicht durchsucht.

Dadurch werden zufällige Treffer vermieden.

Zum Beispiel:

```text
AuthorLastName = Martin -> Clean Code
AuthorLastName = Fowler -> Refactoring und Design Patterns
```

Ein gezielter Regressionstest sollte prüfen, dass:

```text
AuthorLastName = Martin
```

nur dieses Book liefert:

```text
Clean Code
```

aber nicht:

```text
Refactoring
Design Patterns
```

Denn bei diesen Books ist `Martin` nur der Vorname von `Martin Fowler`.

Dieser Test macht die fachliche Suchentscheidung sichtbar:

```text
In der Katalogsuche ist der Author-Nachname das relevante Suchkriterium.
Der Vorname soll keine zufälligen Treffer erzeugen.
```

## Repository Tests vs. ReadModel Tests

Teil 3 macht die Unterscheidung zwischen Repository Tests und ReadModel Tests besonders wichtig.

Repositories testen das schreibseitige Persistence Model.

ReadModels testen das leseseitige Query Model.

Für Catalog wird dieser Unterschied besonders bei Deactivation sichtbar.

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
inaktive Books werden in normalen Book-Listen ausgeblendet
inaktive Books werden in normalen Book-Suchergebnissen ausgeblendet
inaktive Authors werden in normalen Author-Listen ausgeblendet
inaktive Authors werden in normalen Author-Suchergebnissen ausgeblendet
```

Die didaktische Regel lautet:

```text
Repositories laden Aggregates für Änderungen.
ReadModels entscheiden, was in Queries sichtbar ist.
```

## 5. Controller- / API-End-to-End-Tests

Controller-/API-End-to-End-Tests verwenden:

```text
WebApplicationFactory<Program>
TestBaseFactory
TestBaseEndToEnd
TestAuthHandler
HttpClient
```

Diese Tests starten die ASP.NET-Core-Anwendung in einem Testhost und rufen die API über HTTP auf.

Sie prüfen:

```text
Routing
Model Binding
Controller Actions
Statuscodes
JSON-Serialisierung
ProblemDetails-Mapping
Dependency Injection
Datenbankintegration
Swagger-kompatibles API-Verhalten
HTTP-Vertrag von außen
```

Die Reader Controller-/API-Tests decken ab:

```text
GET    /camplib/v1/readers
GET    /camplib/v1/readers/{id}
GET    /camplib/v1/readers/email?email=...
POST   /camplib/v1/readers
PUT    /camplib/v1/readers/{id}
DELETE /camplib/v1/readers/{id}
```

Die Author Controller-/API-End-to-End-Tests sollten das öffentliche HTTP-Verhalten des `AuthorsController` abdecken, zum Beispiel:

```text
GET   /camplib/v1/authors
GET   /camplib/v1/authors/{id}
GET   /camplib/v1/authors/search?searchText=...
POST  /camplib/v1/authors
PATCH /camplib/v1/authors/{id}/deactivate
```

Die Book Controller-/API-End-to-End-Tests sollten das öffentliche HTTP-Verhalten des `BooksController` abdecken, zum Beispiel:

```text
GET   /camplib/v1/books
GET   /camplib/v1/books/{id}
GET   /camplib/v1/books/search?searchField=Title&searchText=...
GET   /camplib/v1/books/search?searchField=AuthorLastName&searchText=...
GET   /camplib/v1/books/search?searchField=Isbn&searchText=...
GET   /camplib/v1/books/by-author/{authorId}
POST  /camplib/v1/books
POST  /camplib/v1/books/{bookId}/items
POST  /camplib/v1/books/{bookId}/authors
PATCH /camplib/v1/books/{bookId}/deactivate
```

Diese Tests sind am nächsten an der realen API-Nutzung.

In Teil 3 prüfen Controller-/API-End-to-End-Tests außerdem, ob die beiden Module korrekt durch das ausführbare API-Projekt verdrahtet werden.

Sie prüfen deshalb nicht nur das Controller-Verhalten, sondern auch die Zusammensetzung aus:

```text
Web
Readers-Modul
Catalog-Modul
Infrastructure
BuildingBlocks
```

Diese Tests testen keine einzelnen Klassen.

Sie testen den öffentlichen HTTP-Vertrag der Anwendung.

Die didaktische Regel lautet:

```text
Domain Tests prüfen fachliche Regeln.
UseCase Tests prüfen Workflows.
Repository- und ReadModel-Tests prüfen Persistenz und Projektionen.
HttpClient-Tests prüfen, ob die API von außen funktioniert.
```

## Manuelle HTTP-Dateien

Zusätzlich zu den automatisierten Tests enthält Teil 3 manuelle HTTP-Dateien für didaktische API-Tests.

Diese Dateien werden verwendet, nachdem die Datenbank gelöscht oder zurückgesetzt wurde.

Die vorgesehene Ausführungsreihenfolge lautet:

```text
1. Authors.http
2. Books.http
3. Readers.http
```

`Seed.cs` definiert die stabilen IDs.

Die `.http`-Dateien erzeugen die entsprechenden Daten über die öffentliche API.

```text
Authors.http erzeugt die Authors.
Books.http erzeugt die Books, verwendet die vorhandenen Authors, ordnet Authors zu Books zu und fügt BookItems hinzu.
Readers.http erzeugt oder prüft Reader-Daten.
```

Das ist bewusst so gewählt.

Die manuellen HTTP-Dateien sollen für Beziehungen keine beliebigen Ad-hoc-IDs erfinden.

Sie verwenden die stabilen IDs aus `Seed.cs`, damit Tests, Dokumentation und manuelle API-Nutzung dieselben Beispiele beschreiben.

Die didaktische Regel lautet:

```text
Seed.cs definiert stabile Beispieldaten.
Die .http-Dateien erzeugen diese Daten über die öffentliche API.
Manuelle API-Tests sollen nach einem Datenbank-Reset reproduzierbar sein.
```

## Testdatenbank

Die automatisierten Tests verwenden SQLite über die Test-Infrastruktur.

Die Testdatenbank wird erzeugt durch:

```text
TestDatabase
TestBaseFactory
```

Die Factory ersetzt ausgewählte Produktionsservices:

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

Typische Catalog-Daten enthalten:

```text
Author1
Author2
Author3
Author4
Author5

Book1
Book2
Book3
Book4

Books mit Authors
Books mit BookItems
```

Die Tests sollten Seed-Daten gegenüber manuell konstruierten Ad-hoc-Daten bevorzugen.

Das hält die Beispiele konsistent und für Studierende leichter nachvollziehbar.

Für Catalog Tests sind Seed-Daten außerdem nützlich, weil Beziehungen bei Bedarf aus demselben getrackten Objektgraphen aufgebaut werden sollten.

Zum Beispiel sollten Books mit Authors vorhandene Author-Instanzen verwenden, statt doppelte Author-Objekte mit denselben IDs zu erzeugen.

Dasselbe Prinzip gilt für die manuellen HTTP-Dateien.

`Seed.cs` definiert stabile IDs, während die `.http`-Dateien die entsprechenden Datensätze über die öffentliche API erzeugen.

## Partial Update Tests

`ReaderUpdateDto` unterstützt partielle Updates:

```csharp
public sealed record ReaderUpdateDto(
   string? Lastname,
   string? Email,
   AddressDto? AddressDto
);
```

Die Bedeutung von `null` ist:

```text
Lastname = null   -> bisherigen Nachnamen beibehalten
Email = null      -> bisherige Email beibehalten
AddressDto = null -> bisherige Adresse beibehalten
```

Nur angegebene Werte werden geändert.

Ein leerer oder nur aus Leerzeichen bestehender Nachname ist nicht dasselbe wie `null`.

```text
null       -> keine Änderung
""         -> ungültiger Wert
"   "      -> ungültiger Wert
"Meier"   -> gültige Änderung
```

Diese Unterscheidung ist wichtig für die Semantik partieller Updates.

Die Tests sollten deshalb beide Fälle abdecken:

```text
Feld fehlt oder ist null          -> keine Änderung
Feld ist angegeben, aber ungültig -> Validierungsfehler
```

## Tests für Catalog-Beziehungen

Teil 3 ergänzt Beziehungen, die ausdrücklich getestet werden müssen.

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
ReadModels zeigen die Anzahl aller und verfügbarer BookItems
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
ein vorhandener Author kann einem vorhandenen Book zugeordnet werden
derselbe Author kann demselben Book nicht zweimal zugeordnet werden
das Zuordnen eines Authors aktualisiert das Book-Aggregate
die Beziehung wird über die BookAuthorJoin-Tabelle persistiert
ReadModels geben Authors in stabiler Reihenfolge zurück
Books können anhand einer Author-ID ausgewählt werden
```

Die technische Join-Tabelle wird nicht als eigenes Domain-Konzept getestet.

Sie wird über das Persistence-Verhalten getestet.

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
die Änderung wird persistiert
UpdatedAt wird aktualisiert
```

ReadModel Tests prüfen:

```text
inaktive Books werden in normalen Book Queries nicht zurückgegeben
inaktive Authors werden in normalen Author Queries nicht zurückgegeben
```

Controller-/API-End-to-End-Tests prüfen:

```text
PATCH /books/{bookId}/deactivate liefert 200 OK
PATCH /authors/{id}/deactivate liefert 200 OK
normale GET-Endpunkte liefern deaktivierte Ressourcen nicht mehr zurück
```

Diese Trennung macht die Designentscheidung in Tests sichtbar.

## Warum verschiedene Testarten?

Jede Testart beantwortet eine andere Frage.

```text
Domain Test:
Funktioniert die fachliche Regel?

UseCase Mock Test:
Ruft der Application Workflow die richtigen Ports auf und behandelt Fehler korrekt?

Application Integration Test:
Funktioniert der UseCase mit echter Persistence?

Infrastructure Test:
Speichert, lädt und projiziert EF Core die Daten korrekt?

Controller-/API-End-to-End-Test:
Verhält sich die öffentliche HTTP-API von außen korrekt?

Manuelle HTTP-Datei:
Können Studierende das API-Verhalten nach einem Datenbank-Reset manuell reproduzieren und nachvollziehen?
```

Zusammen bilden diese Tests eine didaktisch orientierte Testing-Strategie.

## Warum die Tests in Teil 3 wichtig sind

Teil 3 ergänzt ein zweites fachliches Modul.

Das erwartete Ergebnis ist:

```text
Die Architektur wächst.
Das bestehende Verhalten bleibt stabil.
Das neue Verhalten ist durch Tests abgesichert.
```

Die Testsuite ist das Sicherheitsnetz für diese Erweiterung.

Wenn nach dem Ergänzen des Catalog-Moduls alle Tests grün bleiben, gibt das Vertrauen, dass:

```text
Readers weiterhin funktioniert
Catalog funktioniert
Modulgrenzen weiterhin eingehalten werden
Infrastructure Ports aus mehreren Modulen korrekt implementiert
das API-Projekt alle Module korrekt verdrahtet
```

## Empfohlener Workflow

Während der Entwicklung:

```bash
dotnet test
```

Bei API-Änderungen sollte zusätzlich die Anwendung gestartet und Swagger geprüft werden:

```bash
dotnet run --project CampusLibraryApi
```

Swagger ist im Development-Modus erreichbar unter:

```text
https://localhost:8010/swagger
```

Für Catalog-Änderungen ist Swagger besonders hilfreich, um folgende Punkte zu prüfen:

```text
Authors-Endpunkte
Books-Endpunkte
DTO-Schemas
ProblemDetails-Antworten
BookItemStatus-Darstellung
BookSearchField-Darstellung
```

Für manuelle API-Tests wird die Datenbank zurückgesetzt und danach ausgeführt:

```text
Authors.http
Books.http
Readers.http
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
Mock-basierte UseCase Tests
Integration Testing mit SQLite
Controller-/API-Testing über HTTP
Wiederverwendung von Testdaten durch Seed-Objekte
warum Fake Clocks nützlich sind
wie partielle Updates getestet werden sollten
wie Tests architektonische Refactorings schützen
wie Tests architektonische Erweiterungen schützen
wie ein modularer Monolith trotzdem end-to-end getestet werden kann
wie Beziehungen über Domain und Infrastructure hinweg getestet werden können
wie sich ReadModels von Repositories unterscheiden
wie sich Deactivation von Deletion unterscheidet
wie Katalogsuche nach Author-Nachname zufällige Treffer vermeidet
warum Controller-Mock-Tests bei dünnen Controllern nicht nötig sind
wie manuelle HTTP-Dateien API-Verhalten nach einem Datenbank-Reset reproduzierbar machen
```

Die Tests sind daher nicht nur ein Sicherheitsnetz, sondern auch Teil des Lernmaterials.

## Didaktische Faustregel

Jede Testebene hat ihren eigenen Zweck:

```text
Domain Tests schützen fachliche Regeln.
UseCase Tests schützen Application Workflows.
Infrastructure Tests schützen Persistence-Verhalten.
Controller-/API-End-to-End-Tests schützen die öffentliche HTTP-API.
HttpClient-Tests schützen den öffentlichen HTTP-Vertrag.
Manuelle HTTP-Dateien machen API-Verhalten für Studierende sichtbar und reproduzierbar.
End-to-End-Tests schützen die vollständige Komposition.
```

Für Teil 3 ist der wichtigste Lehrpunkt:

```text
Eine modulare Erweiterung ist erfolgreich, wenn die Architektur wächst,
das bestehende Verhalten stabil bleibt
und das neue Verhalten durch Tests bewiesen wird.
```

Eine weitere wichtige Regel lautet:

```text
Repository Tests beweisen, dass Aggregates gespeichert und geladen werden können.
ReadModel Tests beweisen, was die Anwendung nach außen zeigt.
```

Für Catalog wird das besonders bei Deactivation sichtbar:

```text
Repositories dürfen inaktive Aggregates weiterhin laden.
ReadModels blenden inaktive Daten in normalen Queries aus.
```

Für die Katalogsuche ist die wichtigste Regel:

```text
Author-Suche und Book-Suche verwenden den Author-Nachnamen.
Firstname wird nicht durchsucht, weil dadurch zufällige Treffer entstehen würden.
```

Für Controller Tests ist die wichtigste Regel:

```text
UseCase Tests verwenden Mocks.
Controller-/API-Tests verwenden HttpClient.
Dünne Controller benötigen keine breite Controller-Mock-Testebene.
```

Für manuelle API-Tests ist die wichtigste Regel:

```text
Seed.cs definiert stabile Beispieldaten.
Die .http-Dateien erzeugen diese Daten über die öffentliche API.
```
