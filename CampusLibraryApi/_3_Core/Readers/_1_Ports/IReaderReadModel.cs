using CampusLibrary.Api._2_Shared;
using CampusLibrary.Api._3_Core.Readers.Application.Dtos;
namespace CampusLibrary.Api._3_Core.Readers.Application.Ports;

public interface IReaderReadModel {
   Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct);
   Task<Result<ReaderDto>> FindByIdAsync(Guid id, CancellationToken ct);
   Task<Result<ReaderDto>> FindBySubjectAsync(string subject, CancellationToken ct);
}
