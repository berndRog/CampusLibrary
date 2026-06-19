using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._1_Ports;

// Facade port for Reader command use cases.
// The web layer depends on this interface instead of concrete use case classes.
public interface IReaderUseCases {

   // Create a new Reader aggregate.
   Task<Result<ReaderDto>> CreateAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   );

   // Update mutable Reader profile data.
   Task<Result<ReaderDto>> UpdateAsync(
      Guid id,
      ReaderUpdateDto dto,
      CancellationToken ct
   );

   // Deactivate an existing Reader aggregate.
   // This is a soft delete: the Reader remains stored,
   // but is hidden from normal read model queries.
   Task<Result> DeactivateAsync(
      Guid id,
      CancellationToken ct
   );
}

/*
Didaktik
--------

IReaderUseCases ist die Fassade für die schreibende Seite des Readers-Moduls.

Controller sollen nicht jeden einzelnen Use Case als eigene Abhängigkeit
kennen. Stattdessen hängen sie nur von IReaderUseCases ab. Die konkrete
Fassade ReaderUseCases delegiert intern an die einzelnen Use Cases.

Diese Fassade enthält nur Command-Operationen, also Operationen, die den
Zustand des Systems verändern:

- Reader anlegen
- Reader ändern
- Reader deaktivieren

Abgrenzung:

IReaderReadModel
- Query-Seite
- GET-Endpunkte
- liest Daten
- liefert DTOs aus Leseabfragen
- filtert normale Abfragen auf aktive Reader

IReaderUseCases
- Command-Seite
- POST, PUT, DELETE bzw. fachliche Änderungsoperationen
- lädt Aggregate über Repositories
- ruft fachliche Methoden auf dem Aggregate auf
- speichert Änderungen über UnitOfWork

DeactivateAsync löscht den Reader nicht physisch aus der Datenbank.
Stattdessen wird IsActive auf false gesetzt. Das ist ein Soft Delete.

Der Reader bleibt dadurch für historische Zusammenhänge erhalten. Das ist
wichtig, sobald spätere Module wie Loans hinzukommen: Eine frühere Ausleihe
soll weiterhin nachvollziehbar bleiben, auch wenn der Reader im normalen
Leserbestand nicht mehr angezeigt wird.

Dass DeactivateAsync nur Result und kein Result<ReaderDto> zurückgibt, ist
bewusst gewählt. Die Operation bestätigt nur, ob die fachliche Änderung
erfolgreich war. Ein aktualisiertes DTO ist für diesen Command nicht nötig.

Lernziele
---------

- Fassade als Vereinfachung für Controller-Abhängigkeiten verstehen
- Query-Seite und Command-Seite im Controller trennen
- konkrete Use Cases hinter einem Port kapseln
- Soft Delete als fachliche Deaktivierung modellieren
- IsActive als Sichtbarkeitsregel in ReadModels verstehen
- spätere Erweiterbarkeit eines Moduls vorbereiten
*/