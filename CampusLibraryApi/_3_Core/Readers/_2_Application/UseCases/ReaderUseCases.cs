using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._1_Ports;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

// Facade for all Reader write use cases.
// Controllers depend on this facade instead of depending on every single use case.
public sealed class ReaderUseCases(
   ReaderUcCreate createUc,
   ReaderUcUpdate updateUc,
   ReaderUcDelete deleteUc
) : IReaderUseCases {

   public Task<Result<ReaderDto>> CreateAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   ) => createUc.ExecuteAsync(
      dto: dto,
      ct: ct
   );

   public Task<Result<ReaderDto>> UpdateAsync(
      Guid id,
      ReaderUpdateDto dto,
      CancellationToken ct
   ) => updateUc.ExecuteAsync(
      id: id,
      dto: dto,
      ct: ct
   );

   public Task<Result> DeleteAsync(
      Guid id,
      CancellationToken ct
   ) => deleteUc.ExecuteAsync(
      id: id,
      ct: ct
   );
}

/*
Didaktik
--------

ReaderUseCases ist die konkrete Fassade für die schreibenden Use Cases
des Readers-Moduls.

Ohne diese Fassade müsste der Controller jeden einzelnen Use Case als
eigene Abhängigkeit kennen. Mit der Fassade hängt der Controller nur von
IReaderUseCases ab.

Dadurch entsteht eine klare Trennung:

IReaderReadModel
- Query-Seite
- GET-Endpunkte
- liefert DTOs aus Leseabfragen

IReaderUseCases
- Command-Seite
- POST, PUT, DELETE
- koordiniert fachliche Änderungen

Die Fassade enthält selbst keine Fachlogik. Sie delegiert nur an die
konkreten Use Cases.

Lernziele
---------

- Fassade als Vereinfachung für Controller-Abhängigkeiten verstehen
- Command- und Query-Seite im Controller sichtbar trennen
- konkrete Use Cases hinter einem Port kapseln
- spätere Erweiterbarkeit des Moduls vorbereiten
*/
