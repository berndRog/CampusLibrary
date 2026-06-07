using CampusLibraryApi._2_Shared;
using CampusLibraryApi._3_Core.Readers._2_Application.Dtos;
namespace CampusLibraryApi._3_Core.Readers._1_Ports;

public interface IReaderReadModel {
   Task<Result<ReaderDto>> FindByIdAsync(
      Guid id, 
      CancellationToken ct = default
   );
   
   Task<Result<ReaderDto>> FindBySubjectAsync(
      string subject, 
      CancellationToken ct
   );

   Task<Result<IReadOnlyList<ReaderDto>>> SelectAllAsync(CancellationToken ct);
}
