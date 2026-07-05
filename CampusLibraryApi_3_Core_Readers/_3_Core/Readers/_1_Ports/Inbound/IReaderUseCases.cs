using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;

// Facade port for Reader command use cases.
// The web layer depends on this interface instead of concrete use case classes.
public interface IReaderUseCases {

   // Create a new fully populated Reader aggregate.
   Task<Result<ReaderDto>> CreateAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   );

   // Update mutable Reader profile data through the administrative flow.
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

   // Creates the fachlicher Reader shell for the current technical user.
   // The operation is idempotent: if the reader already exists, it is returned.
   Task<Result<ReaderProvisionDto>> CreateProvisionAsync(
      string? id,
      CancellationToken ct
   );

   // Completes or updates firstname/lastname of the current reader.
   Task<Result<ReaderDto>> UpdateProfileAsync(
      ReaderProfileUpdateDto dto,
      CancellationToken ct
   );
}

/*
Didaktik
--------

IReaderUseCases ist die Fassade für die schreibende Seite des Readers-Moduls.

Part 6 ergänzt Self-Service-Operationen für den aktuell angemeldeten Reader:

- FindMeAsync
- ProvisionMeAsync
- UpdateMyProfileAsync

Diese Operationen unterscheiden sich bewusst von der klassischen
Reader-Verwaltung. Subject und Email kommen aus dem IdentityAccessServer und
nicht aus einem Formular.

Die Fassade enthält weiterhin nur Command-/Self-Service-Anwendungsfälle. Für
allgemeine Listen- und Suchabfragen bleibt IReaderReadModel zuständig.

Lernziele
---------

- technische Anmeldung und fachliches Provisioning trennen
- Self-Service-UseCases von administrativer Reader-Verwaltung unterscheiden
- Controller-Abhängigkeiten über eine Fassade klein halten
*/
