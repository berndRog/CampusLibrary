# CampusLibrary

Lehrprojekt für eine modular aufgebaute, DDD-orientierte ASP.NET-Core-Web-API.

Diese Version ist **Teil 2 – Readers Modular Monolith**. Sie übernimmt die Reader-Funktionalität aus Teil 1 und überführt die Architektur von einem ordnerbasierten Monolithen in einen projektbasierten modularen Monolithen.

Der fachliche Umfang bleibt bewusst klein: Die Anwendung enthält nur das **Readers**-Modul. Ziel dieses Teils ist es, Modulgrenzen, Abhängigkeitsrichtung, Ports, Adapter, Repositories, ReadModels und Tests sichtbar zu machen, bevor in späteren Teilen weitere Module hinzukommen.

## Aktueller Stand

Das Projekt enthält aktuell:

- nur das Readers-Modul
- ASP.NET Core Web API
- API-Versionierung
- Swagger/OpenAPI-Dokumentation
- SQLite-Persistenz mit EF Core
- Repository- und ReadModel-Infrastruktur
- UseCases für Create, Update und Deactivate
- Controller-/End-to-End-Tests mit echter SQLite-Testdatenbank
- modulare Projektstruktur mit Web, BuildingBlocks, Core_Readers, Infrastructure und Tests

Das Reader-Verhalten wurde an das aktuelle Modell der späteren Projektteile angepasst:

- `Reader` ist ein Aggregate Root.
- `Reader` besitzt ein `IsActive`-Flag.
- Reader werden nicht physisch gelöscht.
- Die frühere Delete-Operation ist fachlich ein **Deactivate**.
- Normale Leseabfragen liefern nur aktive Reader.
- Spezielle ReadModel-Abfragen können deaktivierte Reader einschließen.
- Command-UseCases sind von Query-ReadModels getrennt.

Der aktuelle Teststand lautet:

```text
Test summary: total: 70, failed: 0, succeeded: 70, skipped: 0
```

## Version

```text
v2-readers-modular-monolith
```

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

Das BuildingBlocks-Projekt enthält gemeinsame Abstraktionen und Basistypen, die unabhängig von einem konkreten Fachmodul sind.

Das Readers-Core-Projekt enthält das readerspezifische Domain Model, DTOs, Mappings, Application UseCases und Ports.

Das Infrastructure-Projekt implementiert EF-Core-Persistenz, Repositories, ReadModels, Datenbankkonfiguration und UnitOfWork.

Das Testprojekt prüft das Verhalten über Domain-, Application-, Infrastructure- und API-Grenzen hinweg.

Die wichtigste Abhängigkeitsregel lautet:

```text
Core-Module hängen nicht von Web/API oder Infrastructure ab.
Infrastructure hängt von Core-Modulen ab, weil sie deren Outbound Ports implementiert.
Das ausführbare API-Projekt ist der Composition Root und verdrahtet alle Module miteinander.
```

## Was noch nicht enthalten ist

Teil 2 enthält bewusst noch nicht:

- Catalog-Modul
- Books
- BookItems
- Loans
- Authentifizierung und Autorisierung
- modulübergreifende Contracts

Diese Themen werden in späteren Teilen der Lehrreihe eingeführt.
