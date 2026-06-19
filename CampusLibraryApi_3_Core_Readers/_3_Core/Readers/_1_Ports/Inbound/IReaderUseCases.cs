using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;

// Facade port for all Reader write use cases.
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

   // Delete an existing Reader aggregate.
   Task<Result> DeleteAsync(
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
Fassade ReaderUseCases delegiert dann an die einzelnen Use Cases.

Abgrenzung:

IReaderReadModel
- Query-Seite
- GET-Endpunkte
- liefert DTOs aus Leseabfragen

IReaderUseCases
- Command-Seite
- POST, PUT, DELETE
- koordiniert fachliche Änderungen

Lernziele
---------

- Fassade als Vereinfachung für Controller-Abhängigkeiten verstehen
- Query-Seite und Command-Seite im Controller trennen
- konkrete Use Cases hinter einem Port kapseln
- spätere Erweiterbarkeit eines Moduls vorbereiten
*/
