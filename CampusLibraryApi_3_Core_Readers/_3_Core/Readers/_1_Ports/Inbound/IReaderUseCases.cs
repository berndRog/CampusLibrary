using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;

// Facade port for Reader command use cases.
public interface IReaderUseCases {
   // Idempotently creates the fachlicher Reader for the current token subject.
   // The optional id exists only for deterministic tests and manual examples.
   Task<Result<bool>> ProvisionMeAsync(
      string? id,
      CancellationToken ct
   );

   // Completes the initial fachliche profile after provisioning.
   Task<Result<ReaderDto>> UpdateMeProfileAsync(
      ReaderProfileDto dto,
      CancellationToken ct
   );

   // Changes selected mutable fachliche data of the current Reader.
   Task<Result<ReaderDto>> UpdateMeAsync(
      ReaderUpdateDto dto,
      CancellationToken ct
   );

   Task<Result> DeactivateAsync(
      Guid id,
      CancellationToken ct
   );
}
