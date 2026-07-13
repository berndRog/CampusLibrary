using CampusLibraryClient.Api.Dtos;
using CampusLibraryClient.Core;

namespace CampusLibraryClient.Api.Contracts;

public interface IReaderClient {

   Task<Result<IEnumerable<ReaderDto>>> GetAllAsync(
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<ReaderDto>> GetByIdAsync(
      Guid id,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<ReaderDto>> GetByEmailAsync(
      string email,
      bool includeInactive = false,
      CancellationToken ct = default
   );

   Task<Result<ReaderDto>> GetMeAsync(
      CancellationToken ct = default
   );

   Task<Result<ReaderProvisionMeDto>> ProvisionMeAsync(
      string? id = null,
      CancellationToken ct = default
   );

   Task<Result<ReaderDto>> UpdateMeProfileAsync(
      ReaderProfileMeDto dto,
      CancellationToken ct = default
   );

   Task<Result<ReaderDto>> UpdateMeAsync(
      ReaderUpdateMeDto dto,
      CancellationToken ct = default
   );

   Task<Result<bool>> DeactivateAsync(
      Guid id,
      CancellationToken ct = default
   );
}
