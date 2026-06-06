using CampusLibraryApi._2_Shared;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReaderReadModel {
   Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct);
   Task<Result<ReaderDto>> FindByIdAsync(Guid id, CancellationToken ct);
   Task<Result<ReaderDto>> FindBySubjectAsync(string subject, CancellationToken ct);
}
