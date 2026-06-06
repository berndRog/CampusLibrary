using CampusLibrary.Api._2_Shared;
using CampusLibrary.Api._3_Core.Readers.Application.Dtos;
using CampusLibrary.Api._3_Core.Readers.Domain;
namespace CampusLibrary.Api._3_Core.Readers.Application.Ports;

public interface IReaderUseCases {
   Task<Result<ReaderDto>> ExecuteAsync(
      ReaderCreateDto dto,
      CancellationToken ct
   );

}