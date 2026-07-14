using CampusLibraryApi._2_BuildingBlocks;
using CampusLibraryApi._3_Core.Readers._1_Ports.Inbound;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;

namespace CampusLibraryApi._3_Core.Readers._2_Application.UseCases;

internal sealed class ReaderUseCases(
   ReaderUcCreateMeProvision createMeProvisionUc,
   ReaderUcUpdateMeProfile updateMeProfileUc,
   ReaderUcUpdateMe updateMeUc,
   ReaderUcDeactivate deactivateUc
) : IReaderUseCases {

   public Task<Result<bool>> ProvisionMeAsync(
      string? id,
      CancellationToken ct
   ) => createMeProvisionUc.ExecuteAsync(id, ct);

   public Task<Result<ReaderDto>> UpdateMeProfileAsync(
      ReaderProfileDto dto,
      CancellationToken ct
   ) => updateMeProfileUc.ExecuteAsync(dto, ct);

   public Task<Result<ReaderDto>> UpdateMeAsync(
      ReaderUpdateDto dto,
      CancellationToken ct
   ) => updateMeUc.ExecuteAsync(dto, ct);

   public Task<Result> DeactivateAsync(
      Guid id,
      CancellationToken ct
   ) => deactivateUc.ExecuteAsync(id, ct);
}
