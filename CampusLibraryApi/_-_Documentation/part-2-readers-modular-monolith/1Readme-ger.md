# CampusLibrary

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Das Projekt zeigt, wie ein kleiner modularer Monolith in getrennte Projekte für Web/API, Building Blocks, Core-Module, Infrastructure und Tests aufgeteilt werden kann, ohne dass das Domain Model von technischen Persistenzdetails abhängig wird.

## Aktueller Stand

Das Projekt enthält aktuell das erste funktionsfähige Modul:

* Readers-Modul
* ASP.NET Core Web API
* API-Versionierung
* Swagger/OpenAPI-Dokumentation
* SQLite-Persistenz mit EF Core
* Repository- und ReadModel-Infrastruktur
* UseCases für Create, partielles Update und Delete
* Controller-/End-to-End-Tests mit einer echten SQLite-Testdatenbank

Der ursprüngliche Monolith wurde in einen projektbasierten modularen Monolithen überführt. Gemeinsame Abstraktionen und Basistypen wurden nach `BuildingBlocks` verschoben. Das `Readers`-Modul ist jetzt ein eigenständiges Core-Modul, während technische Persistenzdetails im Infrastructure-Projekt liegen.

Die Testsuite enthält aktuell 66 Tests. Diese prüfen Domainlogik, Value Objects, UseCases, Repositories, ReadModels und Controller-/End-to-End-Szenarien.

## Versionen

* `v1-readers-monolith`
  Erste abgeschlossene Version mit dem Readers-Modul innerhalb einer einfachen monolithischen Projektstruktur.

* `v2-readers-modular-monolith`
  Refaktorierte Version mit einer projektbasierten modularen Monolith-Struktur.

## Aktueller Branch

```text
part-2/readers-modular-monolith
```

## Projektstruktur

```text
CampusLibraryApi
CampusLibraryApi_1_Web
CampusLibraryApi_2_BuildingBlocks
CampusLibraryApi_3_Core_Readers
CampusLibraryApi_4_Infrastructure
CampusLibraryApiTest
```

## Architekturidee

Die Web/API-Schicht stellt die HTTP-Endpunkte bereit.

Das Core-Modul enthält das readerspezifische Domain Model, die Application UseCases und die Ports.

Das BuildingBlocks-Projekt enthält gemeinsame Abstraktionen, die unabhängig von einem konkreten Fachmodul sind.

Das Infrastructure-Projekt implementiert technische Details wie EF-Core-Persistenz, Repositories und ReadModels.

Das Testprojekt prüft das Verhalten über Domain-, Application-, Infrastructure- und API-Grenzen hinweg.

Die wichtigste Abhängigkeitsregel lautet:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure hängt von Core-Modulen ab, weil sie deren Outbound Ports implementiert.
Das API-Projekt ist der Composition Root und verdrahtet alle Module miteinander.
```
