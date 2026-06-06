using CampusLibrary.Api.Readers.Application.Dtos;
using CampusLibrary.Api.Shared;

namespace CampusLibrary.Api.Readers.Application.Ports;

public interface IReaderReadModel {
   Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct);
   Task<Result<ReaderDto>> FindByIdAsync(Guid id, CancellationToken ct);
   Task<Result<ReaderDto>> FindBySubjectAsync(string subject, CancellationToken ct);
}
